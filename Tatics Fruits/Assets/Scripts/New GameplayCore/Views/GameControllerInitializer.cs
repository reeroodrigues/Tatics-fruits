using System;
using DefaultNamespace.New_GameplayCore;
using New_GameplayCore.GameState;
using New_GameplayCore.Services;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace New_GameplayCore.Views
{
    public class GameControllerInitializer : MonoBehaviour
    {
        [SerializeField] private LevelConfigSO levelConfig;
        [SerializeField] private DeckConfigSo deckConfig;
        [SerializeField] private HUDView hudView;
        [SerializeField] private HandView handView;
        [SerializeField] private PreRoundView preRoundView;
        [SerializeField] private Transform uiRoot;
        [SerializeField] private VictoryView victoryPrefab;
        [SerializeField] private DefeatView defeatPrefab;
        [SerializeField] private LevelSetSO levelSet;
        [SerializeField] private TextMeshProUGUI phaseLabel;
        [SerializeField] private PlayerProfileService _profileService;
        [SerializeField] private TutorialManager tutorialManager;

        public IRuleEngine RuleEngine => _rule;
        public IGameController Controller => _controller;
        public bool IsReady { get; private set; }
        public event Action OnReady;
        public event Action<EndCause> OnLevelEnd;

        public IHandService Hand => _hand;
        public IDeckService Deck => _deck;
        public LevelConfigSO LevelConfig => levelConfig;
        public IScoreService Score => _score;
        public IHighScoreService Highscores => _highscores;
        public LevelProgressService Progress;
        public PlayerProfileService Profile => _profileService;
        
        private GameStateMachine _fsm;
        private TimeManager _time;
        private ScoreService _score;
        private ComboTracker _combo;
        private DeckService _deck;
        private HandService _hand;
        private SwapService _swap;
        private RuleEngine _rule;
        private IGameController _controller;
        private IHighScoreService _highscores;
        private PreRoundPresenter _preRoundPresenter;
        private PreRoundView _preRoundInstance;

        private void Awake()
        {
            Progress = new LevelProgressService();
            Progress.Load();
            
            _profileService = new PlayerProfileService();
            _profileService.Load();

            var currentIndex = _profileService.Data.currentLevelIndex;
            Debug.Log($"[PROFILE] Carregando level {currentIndex + 1}");
            
            if (levelSet && levelSet.levels.Length > 0)
                levelConfig = levelSet.levels[Mathf.Clamp(currentIndex, 0, levelSet.levels.Length - 1)];
            
            var cfg = Progress.Current(levelSet);
            if (cfg != null)
                levelConfig = cfg;
            
            _fsm   = new GameStateMachine();
            _time  = new TimeManager(levelConfig.initialTimeSeconds);
            _score = new ScoreService(levelConfig);
            _combo = new ComboTracker(levelConfig);
            _deck  = new DeckService();
            _hand  = new HandService(levelConfig.handSize);
            _swap  = new SwapService(_hand, _deck, _time, levelConfig);
            _rule  = new RuleEngine(_hand, _deck, _score, _time, _combo, levelConfig);
            _controller = new New_GameplayCore.Controllers.GameController(
                _fsm, _time, _deck, _hand, _rule, _swap, levelConfig, _score);
            _highscores = new JsonHighScoreService();
            
            _controller.OnEnterPreRound += HandleEnterPreRound;
            _controller.OnLevelEnded += HandleLevelEnded;

            IsReady = true;
            OnReady?.Invoke();
        }

        private void Start()
        {
            if (tutorialManager == null)
            {
                StartGameplay();
                return;
            }
            
            var levelIndexForTutorial = Progress.CurrentIndex + 1;
            
            bool shown = tutorialManager.TryShowTutorial(levelIndexForTutorial);

            if (!shown)
            {
                StartGameplay();
            }
            else
            {
                tutorialManager.OnTutorialFinished += HandleTutorialFinished;
            }
        }

        private void StartGameplay()
        {
            if (phaseLabel)
                phaseLabel.text = $"Fase {Progress.CurrentIndex + 1}";

            _controller.StartLevel(levelConfig, deckConfig);
            hudView.Initialize(_time, _score, _swap);
            handView.Initialize(_hand, _controller);

            _score.OnScoreChanged += (total, delta) =>
            {
                _highscores.TryReportScore(GetLevelId(), total);
            };
        }

        private void HandleTutorialFinished()
        {
            tutorialManager.OnTutorialFinished -= HandleTutorialFinished;
            StartGameplay();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.OnEnterPreRound -= HandleEnterPreRound;
                _controller.OnLevelEnded -= HandleLevelEnded;
            }

            if (tutorialManager != null)
            {
                tutorialManager.OnTutorialFinished -= HandleTutorialFinished;
            }
        }
        
        private void HandleLevelEnded(EndCause cause)
        {
            var totalScore = _score.Total;
            var levelId = levelConfig.levelId;
            if (string.IsNullOrEmpty(levelId))
                levelId = levelConfig.name;

            _profileService.RegisterBestScore(levelId, totalScore);

            switch (cause)
            {
                case EndCause.TargetReached:
                    ShowVictory();
                    break;

                case EndCause.TimeUp:
                    ShowDefeat();
                    break;
            }
        }

        private void ShowVictory()
        {
            var presenter = new VictoryPresenter(
                levelConfig, _score, _time, _highscores, _profileService, Progress, levelSet);

            VictoryModel model = default;
            presenter.OnModelReady += m => model = m;
            presenter.Build();
            
            var currentLevel = Progress.CurrentIndex;
            var totalScore   = _score.Total;
            
            var completed = Progress.CanAdvance(levelConfig, totalScore, 0.75f);
            _profileService.SetLevel(currentLevel, completed);
            
            var rewardGold = model.starsEarned * 10;
            _profileService.AddGold(rewardGold);

            var levelId = levelConfig.levelId;
            if (string.IsNullOrEmpty(levelId))
                levelId = levelConfig.name;
            _profileService.RegisterBestScore(levelId, totalScore);
            
            Progress.MarkNextUnlockedIfEligible(levelConfig, _score.Total, 0.75f);

            var view = Instantiate(victoryPrefab, uiRoot);
            view.Bind(presenter, model);

            presenter.OnReplay += () =>
            {
                Progress.Replay();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            };
            presenter.OnNext += () =>
            {
                Progress.Advance(levelSet);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            };
        }

        private void ShowDefeat()
        {
            var presenter = new DefeatPresenter(levelConfig, _score, _time, _highscores, _profileService);

            DefeatModel model = default;
            presenter.OnModelReady += m => model = m;
            presenter.Build();
            
            Progress.MarkNextUnlockedIfEligible(levelConfig, _score.Total, 0.75f);

            var view = Instantiate(defeatPrefab, uiRoot);
            view.Bind(presenter, model);

            presenter.OnReplay += () =>
            {
                Progress.Replay();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            };
            presenter.OnMenu += () =>
            {
                SceneManager.LoadScene("MainMenu");
            };
        }

        private void HandleEnterPreRound()
        {
            _preRoundPresenter = new PreRoundPresenter(
                _controller as Controllers.GameController,
                levelConfig,
                _deck,
                _highscores);

            var model = _preRoundPresenter.BuildModel(levelConfig, _deck, _highscores);

            _preRoundInstance = Instantiate(preRoundView, uiRoot);
            _preRoundInstance.Bind(_preRoundPresenter, model);
        }

        private void Update()
        {
            _controller.UpdateTick(Time.deltaTime);
        }
    
        private string GetLevelId()
            => string.IsNullOrEmpty(levelConfig.levelId) ? levelConfig.name : levelConfig.levelId;
    }
}
