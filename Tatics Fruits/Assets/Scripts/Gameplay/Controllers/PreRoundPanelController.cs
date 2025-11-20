using System;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Obsolete("This class is deprecated. Use New_GameplayCore.Services.PreRoundPresenter and PreRoundView instead.")]
public class PreRoundPanelController_DEPRECATED : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private Image starsImage;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button startPhaseButton;
    [SerializeField] private Button changeObjectiveButton;
    [SerializeField] private ObjectiveProvider objectiveProvider;
    [SerializeField] private Timer_DEPRECATED timer;
    
    private int _currentTargetScore;
    private int _currentTotalTime;
    
    [Obsolete("Obsolete")]
    private void Start()
    {
        backToMenuButton.onClick.AddListener(BackToMenu);
        startPhaseButton.onClick.AddListener(StartPhase);
        changeObjectiveButton.onClick.AddListener(ChangeObjective);
    }

    [Obsolete("Obsolete")]
    public void SetupPreRound(int phaseNumber, string objectiveDescription, Sprite starsSprite, int targetScore, int totalTime)
    {
        objectiveText.text = objectiveDescription;
        starsImage.sprite = starsSprite;
        _currentTargetScore = targetScore;
        _currentTotalTime = totalTime;

        if (timer == null)
        {
            timer = FindObjectOfType<Timer_DEPRECATED>();
        }

        if (timer != null)
        {
            timer.SetTotalTime(totalTime);
        }

        var scoreManager = FindObjectOfType<ScoreManager_DEPRECATED>();
        if (scoreManager != null)
        {
            scoreManager.SetTargetScore(targetScore);
        }
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    [Obsolete("Obsolete")]
    private void StartPhase()
    {
        gameObject.SetActive(false);
    
        if (timer == null)
        {
            timer = FindObjectOfType<Timer_DEPRECATED>();
        }

        if (timer != null)
        {
            timer.StartTimer();
        }
    }
    
    [Obsolete("Obsolete")]
    private void ChangeObjective()
    {
        if (objectiveProvider == null)
        {
            return;
        }

        var newObjective = objectiveProvider.GetRandomObjectives();
        _currentTargetScore = newObjective._points;
        _currentTotalTime = newObjective._time;

        objectiveText.text = $"Score {_currentTargetScore} points in {_currentTotalTime} seconds.";

        if (timer == null)
            timer = FindObjectOfType<Timer_DEPRECATED>();

        if (timer != null)
            timer.SetTotalTime(_currentTotalTime);
        
        var scoreManager = FindObjectOfType<ScoreManager_DEPRECATED>();
        if (scoreManager != null)
            scoreManager.SetTargetScore(_currentTargetScore);
    }
}