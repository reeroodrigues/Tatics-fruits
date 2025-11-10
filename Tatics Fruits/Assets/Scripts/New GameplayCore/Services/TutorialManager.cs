using System;
using New_GameplayCore;
using New_GameplayCore.Views;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialPanel tutorialPopup;
    [SerializeField] private int targetLevel = 1;
    [SerializeField] private bool showOnFirstLogin = true;

    private const string TutorialFile = "tutorial_data.json";
    private TutorialData _data;

    public event Action OnTutorialFinished;

    private void Awake()
    {
        _data = JsonDataService.Load<TutorialData>(TutorialFile);
        if (_data == null)
            _data = new TutorialData();

        if (tutorialPopup != null)
        {
            tutorialPopup.OnTutorialFinished += HandlePopupFinished;
        }
        else
        {
            Debug.LogWarning("[TutorialManager] TutorialPopup não atribuído no Inspector.");
        }
    }

    public bool TryShowTutorial(int currentLevel)
    {
        if (tutorialPopup == null)
            return false;

        if (HasCompletedTutorial())
            return false;

        bool isFirstLogin = IsFirstLogin();
        bool shouldShow =
            (showOnFirstLogin && isFirstLogin) ||
            (currentLevel == targetLevel);

        if (!shouldShow)
            return false;

        Debug.Log("[TutorialManager] Exibindo tutorial.");
        tutorialPopup.Show();
        MarkFirstLoginDone();
        return true;
    }

    private void HandlePopupFinished()
    {
        _data.tutorialCompleted = true;
        JsonDataService.Save(TutorialFile, _data);
        Debug.Log("[TutorialManager] Tutorial finalizado, flag salva.");
        OnTutorialFinished?.Invoke();
    }

    private bool HasCompletedTutorial()
        => _data != null && _data.tutorialCompleted;

    private bool IsFirstLogin()
        => _data == null || !_data.firstLoginDone;

    private void MarkFirstLoginDone()
    {
        if (_data == null)
            _data = new TutorialData();

        if (!_data.firstLoginDone)
        {
            _data.firstLoginDone = true;
            JsonDataService.Save(TutorialFile, _data);
        }
    }
}
