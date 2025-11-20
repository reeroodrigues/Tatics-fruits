using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.SceneManagement;

[Obsolete("This class is deprecated. Use New_GameplayCore.Controllers.GameController and GameControllerInitializer instead.")]
public class GameController_DEPRECATED : MonoBehaviour
{
    [SerializeField] private GameObject preRoundPrefab;
    [SerializeField] private Transform uiContainer;
    [SerializeField] private Sprite starsSprite;

    private GameObject _preRoundInstance;

    [Obsolete("Obsolete")]
    private void Start()
    {
        var phase = GameSession_DEPRECATED._phaseNumber > 0 ? GameSession_DEPRECATED._phaseNumber : 1;
        var points = GameSession_DEPRECATED._targetScore > 0 ? GameSession_DEPRECATED._targetScore : 100;
        var time = GameSession_DEPRECATED._totalTime > 0 ? GameSession_DEPRECATED._totalTime : 60;
        var objectiveDescription = !string.IsNullOrEmpty(GameSession_DEPRECATED._objectiveDescription)
            ? GameSession_DEPRECATED._objectiveDescription
            : $"Score {points} points in {time} seconds.";

        GameSession_DEPRECATED._phaseNumber = phase;
        GameSession_DEPRECATED._targetScore = points;
        GameSession_DEPRECATED._totalTime = time;
        GameSession_DEPRECATED._objectiveDescription = objectiveDescription;

        ShowPreRoundPanel(phase);
    }


    private (int points, int time) GetRandomObjective()
    {
        var objectives = new (int points, int time)[]
        {
            (100, 60),
            (50, 30),
            (150, 90),
            (200, 120)
        };

        return objectives[UnityEngine.Random.Range(0, objectives.Length)];
    }

    [Obsolete("Obsolete")]
    public void StartNewPhase()
    {
        GameSession_DEPRECATED._phaseNumber++;
        ShowPreRoundPanel(GameSession_DEPRECATED._phaseNumber);
        var (points, time) = GetRandomObjective();
        GameSession_DEPRECATED._phaseNumber = 2;
        GameSession_DEPRECATED._targetScore = points;
        GameSession_DEPRECATED._totalTime = time;
        GameSession_DEPRECATED._objectiveDescription = $"Score {points} points in {time} seconds.";
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [Obsolete("Obsolete")]
    private void ShowPreRoundPanel(int phaseNumber)
    {
        var points = GameSession_DEPRECATED._targetScore > 0 ? GameSession_DEPRECATED._targetScore : 100;
        var time = GameSession_DEPRECATED._totalTime > 0 ? GameSession_DEPRECATED._totalTime : 60;
        var objectiveDescription = !string.IsNullOrEmpty(GameSession_DEPRECATED._objectiveDescription)
            ? GameSession_DEPRECATED._objectiveDescription
            : $"Score {points} points in {time} seconds.";

        if (_preRoundInstance == null)
        {
            _preRoundInstance = Instantiate(preRoundPrefab, uiContainer);
        }
        else
        {
            _preRoundInstance.SetActive(true);
        }

        _preRoundInstance.GetComponent<PreRoundPanelController_DEPRECATED>().SetupPreRound(
            phaseNumber,
            objectiveDescription,
            starsSprite,
            points,
            time
        );
    }

}