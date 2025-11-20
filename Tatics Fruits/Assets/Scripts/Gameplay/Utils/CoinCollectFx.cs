using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinCollectFx : MonoBehaviour
{
    [Header("Canvas e Targets")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform targetHud; 
    [SerializeField] private Image coinPrefab;

    [Header("Visual Settings")]
    [SerializeField] private Vector2 coinSize = new Vector2(64, 64);
    [SerializeField] private float startScale = 0.7f;
    
    [Tooltip("Quantidade de moedas visuais por efeito")]
    [SerializeField] private int coinsToSpawn = 14;

    [Tooltip("Raio do espelhamento inicial ao nascer")]
    [SerializeField] private float spread = 90f;
    
    [Header("Timings")]
    [SerializeField] private float spawnInterval = 0.035f;
    [SerializeField] private float travelTime = 0.65f;
    [SerializeField] private Ease travelEase = Ease.InQuad;

    [Header("Target punch")]
    [SerializeField] private float targetPunchScale = 1.15f;
    [SerializeField] private float targetPunchTime = 0.12f;
    [SerializeField] private float targetPadding = 8f;

    private readonly Queue<Image> _pool = new Queue<Image>();
    private RectTransform _poolParent;
    
    public static CoinCollectFx Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        _poolParent = (RectTransform)transform;
        DOTween.Init();
    }

    public void SetTarget(RectTransform target) => targetHud = target;
    
    //<summary> Dispara o FX a partir de um elemento de UI (ex: botão/card) até o HUD do ouro </summary>

    public UniTask PlayFromUI(RectTransform from, int totalGold, Action onAllComplete = null)
    {
        if (!ValidateCommon(totalGold, onAllComplete)) return UniTask.CompletedTask;

        var start  = WorldToCanvasPos(from);
        // mire no centro real do retângulo do alvo:
        var target = CanvasPointAtRectCenter(targetHud);

        return PlayInternalAsync(totalGold, start, target, onAllComplete);
    }
    
    private bool ValidateCommon(int totalGold, Action onAllComplete)
    {
        if (totalGold <= 0) { onAllComplete?.Invoke(); return false; }
        if (rootCanvas == null || coinPrefab == null || targetHud == null)
        {
            Debug.LogWarning("[CoinCollectFx] Faltando refs (Canvas/CoinPrefab/TargetHud).");
            onAllComplete?.Invoke();
            return false;
        }
        return true;
    }

    private async UniTask PlayInternalAsync(int totalGold, Vector2 start, Vector2 target, Action onAllComplete)
    {
        int coins     = Mathf.Clamp(coinsToSpawn, 1, 50);
        int baseValue = Mathf.Max(1, totalGold / coins);
        int remainder = Mathf.Max(0, totalGold - baseValue * coins);

        for (int i = 0; i < coins; i++)
        {
            int valueThis = baseValue + (i < remainder ? 1 : 0);
            SpawnOneCoin(start, target);
            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval));
        }

        await UniTask.Delay(TimeSpan.FromSeconds(travelTime + 0.05f));

        if (targetHud)
        {
            targetHud.DOKill();
            targetHud.DOPunchScale(Vector3.one * (targetPunchScale - 1f), targetPunchTime, 1, 0.8f);
        }

        onAllComplete?.Invoke();
    }

    private void SpawnOneCoin(Vector2 start, Vector2 targetCenter)
    {
        var img = GetFromPool();
        var rt  = (RectTransform)img.transform;

        PrepareRect(rt, (RectTransform)rootCanvas.transform, start);

        var mid = RandomSpread(start);
        // opcional: destino aleatório dentro do retângulo da moeda (fica mais natural):
        var target = CanvasPointRandomInside(targetHud, targetPadding);

        var seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPos(mid,    travelTime * 0.35f).SetEase(Ease.OutQuad));
        seq.Join(  rt.DOPunchScale(Vector3.one * 0.2f, 0.15f, 1, 0.9f));
        seq.Append(rt.DOAnchorPos(target, travelTime * 0.65f).SetEase(travelEase));
        // garanta que “encaixa” no final:
        seq.OnComplete(() =>
        {
            rt.anchoredPosition = target; // snap final
            ReturnToPool(img);
        });
    }

    private Image GetFromPool()
    {
        if (_pool.Count > 0)
        {
            var it = _pool.Dequeue();
            it.gameObject.SetActive(true);
            return it;
        }
        return Instantiate(coinPrefab, _poolParent);
    }

    private void ReturnToPool(Image it)
    {
        var rt = (RectTransform)it.transform;
        rt.DOKill(true);
        rt.localScale       = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
        rt.rotation         = Quaternion.identity;

        it.gameObject.SetActive(false);
        rt.SetParent(_poolParent, false);
        _pool.Enqueue(it);
    }

    private void PrepareRect(RectTransform rt, RectTransform parent, Vector2 anchoredPos)
    {
        rt.DOKill(true);
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = coinSize;
        rt.localScale = Vector3.one * startScale;
        rt.rotation   = Quaternion.identity;
        rt.anchoredPosition = anchoredPos;
        rt.SetAsLastSibling();
    }

    private Vector2 RandomSpread(Vector2 center)
    {
        var dir = UnityEngine.Random.insideUnitCircle.normalized;
        var dst = UnityEngine.Random.Range(spread * 0.4f, spread);
        return center + dir * dst;
    }

    private Vector2 WorldToCanvasPos(RectTransform rectUI)
    {
        var cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        RectTransform canvasRT = (RectTransform)rootCanvas.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            RectTransformUtility.WorldToScreenPoint(cam, rectUI.position),
            cam,
            out var localPoint);

        return localPoint;
    }

    private Vector2 WorldToCanvasPosFromWorld(Vector3 world, Camera worldCam)
    {
        var cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        RectTransform canvasRT = (RectTransform)rootCanvas.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            RectTransformUtility.WorldToScreenPoint(worldCam, world),
            cam,
            out var localPoint);

        return localPoint;
    }
    
    private Vector2 CanvasPointAtRectCenter(RectTransform rt)
    {
        var cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        var canvasRT = (RectTransform)rootCanvas.transform;

        // centro VISUAL do rect (independente do pivot)
        Vector3 worldCenter = rt.TransformPoint(rt.rect.center);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            RectTransformUtility.WorldToScreenPoint(cam, worldCenter),
            cam,
            out var localPoint);

        return localPoint;
    }

    private Vector2 CanvasPointRandomInside(RectTransform rt, float padding)
    {
        var cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        var canvasRT = (RectTransform)rootCanvas.transform;

        // escolhe um ponto aleatório DENTRO do retângulo da moeda
        float x = UnityEngine.Random.Range(rt.rect.xMin + padding, rt.rect.xMax - padding);
        float y = UnityEngine.Random.Range(rt.rect.yMin + padding, rt.rect.yMax - padding);
        Vector3 world = rt.TransformPoint(new Vector3(x, y, 0f));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            RectTransformUtility.WorldToScreenPoint(cam, world),
            cam,
            out var localPoint);

        return localPoint;
    }
}