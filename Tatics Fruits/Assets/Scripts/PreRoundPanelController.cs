using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PreRoundPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _objectiveText;
    [SerializeField] private Image _starsImage;
    [SerializeField] private Button _backToMenuButton;
    [SerializeField] private Button _startPhaseButton;
    [SerializeField] private Button _changeObjectiveButton;
    [SerializeField] private ObjectiveProvider _objectiveProvider;
    [SerializeField] private Timer _timer;
    
    private int _currentTargetScore;
    private int _currentTotalTime;
    
    private void Start()
    {
        _backToMenuButton.onClick.AddListener(BackToMenu);
        _startPhaseButton.onClick.AddListener(StartPhase);
        _changeObjectiveButton.onClick.AddListener(ChangeObjective);
    }

    public void SetupPreRound(int phaseNumber, string objectiveDescription, Sprite starsSprite, int targetScore, int totalTime)
    {
        _objectiveText.text = objectiveDescription;
        _starsImage.sprite = starsSprite;
        _currentTargetScore = targetScore;
        _currentTotalTime = totalTime;

        if (_timer == null)
        {
            _timer = FindObjectOfType<Timer>();
        }

        if (_timer != null)
        {
            _timer.SetTotalTime(totalTime);
        }

        var scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.SetTargetScore(targetScore);
        }
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void StartPhase()
    {
        gameObject.SetActive(false);
    
        if (_timer == null)
        {
            _timer = FindObjectOfType<Timer>();
        }

        if (_timer != null)
        {
            _timer.StartTimer();
        }
    }
    
    private void ChangeObjective()
    {
        if (_objectiveProvider == null)
        {
            return;
        }

        var newObjective = _objectiveProvider.GetRandomObjectives();
        _currentTargetScore = newObjective._points;
        _currentTotalTime = newObjective._time;

        _objectiveText.text = $"Score {_currentTargetScore} points in {_currentTotalTime} seconds.";

        if (_timer == null)
            _timer = FindObjectOfType<Timer>();

        if (_timer != null)
            _timer.SetTotalTime(_currentTotalTime);
        
        var scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
            scoreManager.SetTargetScore(_currentTargetScore);
    }
}