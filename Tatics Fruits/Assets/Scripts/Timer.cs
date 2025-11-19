using System;
using System.Collections;
using DefaultNamespace;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Image timerImage;
    public TextMeshProUGUI timerText;
    public GameObject addedTimeTextPrefab;
    public Transform addedTimeTextParent;
    public float totalTime;
    public float remainingTime;
    public event Action OnRoundEnd;

    private bool _isPulsing = false;
    private bool _isRunning = false;
    private bool _isPaused;

    public Timer(bool isPaused)
    {
        _isPaused = isPaused;
    }

    public void SetTotalTime(float time)
    {
        totalTime = time;
        remainingTime = totalTime;
        UpdateTimerText();
        
        {
            Debug.Log("Aguardando clique do botão Start...");
        };
    }

    public void StartTimer()
    {
        _isRunning = true;
        Debug.Log("Timer iniciado!");
    }

    private void Update()
    {
        if (!_isRunning || _isPaused) return;

        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            timerImage.fillAmount = remainingTime / totalTime;
            UpdateTimerText();

            if (remainingTime <= 10 && !_isPulsing)
            {
                StartPulsingEffect();
            }
        }
        else
        {
            StopTimer();
            EndRound();
        }
    }

    public void StopTimer()
    {
        _isRunning = false;
        StopPulsingEffect();
    }

    public void PauseTimer()
    {
        _isPaused = true;
    }

    public void ResumeTimer()
    {
        _isPaused = false;
    }

    public void AddTime(float timeToAdd)
    {
        remainingTime += timeToAdd;
        remainingTime = Mathf.Clamp(remainingTime, 0, totalTime);
        AnimateTimerText();
        ShowAddedTimeEffect(timeToAdd);
    }

    private void ShowAddedTimeEffect(float timeToAdd)
    {
        if (addedTimeTextPrefab == null || addedTimeTextParent == null) return;
    
        var addedTimeText = Instantiate(addedTimeTextPrefab, addedTimeTextParent);
        var textComponent = addedTimeText.GetComponent<TextMeshProUGUI>();

        if (textComponent != null)
        {
            var isReduction = timeToAdd < 0;
            textComponent.text = isReduction ? $"{timeToAdd:F1}s" : $"+{timeToAdd:F1}s";
            textComponent.color = isReduction ? Color.red : Color.green;
        }
    
        addedTimeText.transform.DOLocalMoveY(50f, 1f).SetRelative().SetEase(Ease.OutQuad);
        textComponent.DOFade(0, 1f).OnComplete(() => Destroy(addedTimeText));
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            var seconds = Mathf.CeilToInt(remainingTime);
            timerText.text = seconds.ToString();
        }
    }

    private void AnimateTimerText()
    {
        timerText?.transform.DOScale(1.3f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    private void StartPulsingEffect()
    {
        if (timerText == null) return;

        _isPulsing = true;
        timerText.color = Color.red;
        timerText.transform.DOScale(1.3f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
    }

    private void StopPulsingEffect()
    {
        if (timerText == null) return;

        _isPulsing = false;
        timerText.color = Color.white;
        timerText.transform.localScale = Vector3.one;
        timerText.transform.DOKill();
    }

    public float GetTimeRemaining()
    {
        return remainingTime;
    }

    public void FreezeForSeconds(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        _isPaused = true;
        yield return new WaitForSecondsRealtime(duration);
        _isPaused = false;
    }
    
    private void EndRound()
    {
        Debug.Log("Round Ended!");
        OnRoundEnd?.Invoke();
    }
}
