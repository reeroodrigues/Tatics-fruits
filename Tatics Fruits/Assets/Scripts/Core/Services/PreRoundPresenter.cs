using UnityEngine;

namespace New_GameplayCore.Services
{
    public class PreRoundPresenter : IPreRoundPresenter
    {
        private readonly Controllers.GameController _controller;
        private readonly LevelConfigSO _cfg;
        private readonly IDeckService _deck;
        private readonly IHighScoreService  _highscores;
        private PreRoundModel _model;

        public System.Action<PreRoundModel> OnModelReady;
        public System.Action OnRequestClose;

        public PreRoundPresenter(Controllers.GameController controller, LevelConfigSO cfg, IDeckService deck,
            IHighScoreService highscores)
        {
            _controller = controller;
            _cfg = cfg;
            _deck = deck;
            _highscores = highscores;
        }

        public PreRoundModel BuildModel(LevelConfigSO cfg, IDeckService deck, IHighScoreService hs)
        {
            var total = 0;
            var list = new System.Collections.Generic.List<DeckEntrySummary>();
            foreach (var e in cfg.deck.entries)
            {
                list.Add(new DeckEntrySummary { type = e.type, quantity = e.quantity });
                total += e.quantity;
            }

            var s1 = Mathf.CeilToInt(cfg.targetScore * cfg.star1Threshold);
            var s2 = Mathf.CeilToInt(cfg.targetScore * cfg.star2Threshold);
            var s3 = cfg.targetScore;
            
            var levelId = string.IsNullOrEmpty(cfg.levelId) ? cfg.name : cfg.levelId;
            var best = hs.GetBest(levelId);

            _model = new PreRoundModel()
            {
                levelId = levelId,
                displayName = string.IsNullOrEmpty(cfg.displayName) ? cfg.name : cfg.displayName,
                description = cfg.description,

                targetScore = cfg.targetScore,
                star1Score = s1,
                star2Score = s2,
                star3Score = s3,

                initialTimeSec = cfg.initialTimeSeconds,
                handSize = cfg.handSize,
                timeBonusOnPair = cfg.timeBonusOnPair,
                swapAllPenalty = cfg.swapAllTimePenalty,
                swapRandomPenalty = cfg.swapRandomTimePenalty,
                allowRefillFromDiscard = cfg.allowEmptyDeckRefill,

                deckTotalCount = total,
                composition = list.ToArray(),
                
                bestScore = best,
                
                useFixedSeed = cfg.useFixedSeed,
                effectiveSeed = cfg.useFixedSeed ? cfg.fixedSeed : UnityEngine.Random.Range(int.MinValue, int.MaxValue),
            };
            
            OnModelReady?.Invoke(_model);
            return _model;
        }

        public void OnStartClicked()
        {
            _controller.BeginPlayFromPreRound(_model);
            OnRequestClose?.Invoke();
        }

        public void OnBackClicked()
        {
            _controller.BackToLevelSelect();
            OnRequestClose?.Invoke();
        }

        public void OnToggleUseFixedSeed(bool value)
        {
            _model.useFixedSeed = value;
            _model.effectiveSeed = value ? _cfg.fixedSeed : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            OnModelReady?.Invoke(_model);
        }
    }
}