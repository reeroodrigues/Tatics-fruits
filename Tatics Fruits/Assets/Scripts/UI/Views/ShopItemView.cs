using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ShopItemView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image icon;
    
    [SerializeField] private TextMeshProUGUI title;
    
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [SerializeField] private TextMeshProUGUI priceText;

    [SerializeField] private TextMeshProUGUI rarityText;

    [SerializeField] private Button buyButton;
    [SerializeField] private Image  buyButtonGraphic;
    [SerializeField] private TextMeshProUGUI buyLabel;

    [SerializeField] private GameObject ownedOverlay;
    [SerializeField] private GameObject ribbonNew;

    [Header("Behavior")]
    [SerializeField] private bool disableButtonWhenCantAfford = false;
    [SerializeField] private bool showPriceInsideButton = true;
    [SerializeField] private bool showOnlyPriceInButton = true;
    [SerializeField] private bool showGoldWordInButton = false;

    [Header("Localization Keys")]
    [SerializeField] private string buyTextKey   = "shop.buy";
    [SerializeField] private string ownedTextKey = "shop.owned";
    [SerializeField] private string goldTextKey  = "currency.gold";

    public static Func<string, string> Translate = key => key;

    [Header("FX")]
    [SerializeField] private float cantAffordShakeDuration = 0.35f;
    [SerializeField] private float cantAffordShakeStrength = 0.25f;
    [SerializeField] private int   cantAffordShakeVibrato  = 18;
    [SerializeField] private Color cantAffordFlashColor    = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private float cantAffordFlashDuration = 0.20f;
    [SerializeField] private float purchasePunchDuration   = 0.15f;
    [SerializeField] private float purchasePunchScale      = 0.12f;

    private string _cardId;
    private int _price;
    private PlayerProfileController _profile;
    //private PowerUpCardSO _so;

    private Color _btnOriginalColor = Color.white;
    private Tween _activeTween;

    private string T(string key) => (Translate != null ? Translate(key) : key) ?? key;

    private void Awake()
    {
        if (!buyLabel && buyButton)
            buyLabel = buyButton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (!buyButtonGraphic && buyButton)
            buyButtonGraphic = buyButton.targetGraphic as Image;

        if (buyButtonGraphic)
            _btnOriginalColor = buyButtonGraphic.color;

        if (buyButton && buyButtonGraphic && buyButton.targetGraphic != buyButtonGraphic)
            buyButton.targetGraphic = buyButtonGraphic;
    }

    private void RelocalizeFromSO()
    {
        // if (_so == null) return;
        // if (title) title.text = _so.GetLocalizedName();
        // if (descriptionText) descriptionText.text = _so.GetLocalizedDescription();
        
        RefreshState();
    }
    
    public void Setup(string cardId, string displayName, Sprite sprite, int priceGold,
                      PlayerProfileController profile, bool isNew = false, string rarity = null)
    {
        _cardId  = cardId;
        _price   = priceGold;
        _profile = profile;
        //_so      = null;

        if (icon)  icon.sprite = sprite;
        if (title) title.text  = displayName;

        if (priceText) priceText.text = $"{priceGold} {T(goldTextKey)}";
        if (ribbonNew) ribbonNew.SetActive(isNew);
        if (rarityText) rarityText.text = string.IsNullOrEmpty(rarity) ? "" : rarity;

        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClick);
        }

        if (Localizer.Instance != null)
        {
            Localizer.Instance.OnLanguageChanged -= RelocalizeFromSO;
            Localizer.Instance.OnLanguageChanged += RelocalizeFromSO;
        }

        _profile.OnGoldChanged -= HandleGoldChanged;
        _profile.OnGoldChanged += HandleGoldChanged;

        RefreshState();
    }

    private void OnEnable() => RefreshState();

    private void OnDisable()
    {
        if (Localizer.Instance != null)
            Localizer.Instance.OnLanguageChanged -= RelocalizeFromSO;

        if (buyButton)        buyButton.transform.DOKill();
        if (buyButtonGraphic) buyButtonGraphic.DOKill();
        _activeTween?.Kill();
        _activeTween = null;
    }

    private void OnDestroy()
    {
        if (_profile != null)
            _profile.OnGoldChanged -= HandleGoldChanged;
    }

    private void HandleGoldChanged(int _) => RefreshState();

    private void RefreshState()
    {
        if (_profile == null) return;

        bool owned = _profile.HasCard(_cardId);
        if (ownedOverlay) ownedOverlay.SetActive(owned);

        bool hasMoney = _profile.CanAfford(_price);
        bool interactable = !owned && (!disableButtonWhenCantAfford || hasMoney);
        if (buyButton) buyButton.interactable = interactable;

        if (buyLabel)
        {
            buyLabel.gameObject.SetActive(true);
            buyLabel.enabled = true;
            buyLabel.alpha   = 1f;

            string buyWord   = T(buyTextKey);
            string ownedWord = T(ownedTextKey);
            string goldWord  = T(goldTextKey);

            if (owned)
            {
                buyLabel.text = ownedWord;
            }
            else if (showPriceInsideButton)
            {
                buyLabel.text = showOnlyPriceInButton
                    ? (showGoldWordInButton ? $"{_price} {goldWord}" : $"{_price}")
                    : $"{buyWord}\n{_price} {goldWord}";
            }
            else
            {
                buyLabel.text = buyWord;
            }
        }

        if (priceText && priceText != buyLabel)
            priceText.gameObject.SetActive(!showPriceInsideButton);

        if (title) title.alpha = owned ? 0.6f : 1f;
        if (buyButtonGraphic) buyButtonGraphic.color = _btnOriginalColor;
        if (buyButton)        buyButton.transform.localScale = Vector3.one;
    }

    private void OnBuyClick()
    {
        if (_profile.HasCard(_cardId)) return;

        if (_profile.TryPurchaseCard(_cardId, _price))
        {
            if (buyButton)
            {
                buyButton.transform.DOKill();
                buyButton.transform.DOPunchScale(Vector3.one * purchasePunchScale, purchasePunchDuration, 8, 0.9f);
            }
            RefreshState();
        }
        else
        {
            FeedbackCantAfford();
        }
    }

    private void FeedbackCantAfford()
    {
        if (buyButton)
        {
            buyButton.transform.DOKill();
            buyButton.transform.localScale = Vector3.one;
            buyButton.transform.DOShakeScale(
                duration:  cantAffordShakeDuration,
                strength:  cantAffordShakeStrength,
                vibrato:   cantAffordShakeVibrato,
                randomness: 90f
            );
        }

        if (buyButtonGraphic)
        {
            buyButtonGraphic.DOKill();
            buyButtonGraphic.color = _btnOriginalColor;
            var seq = DOTween.Sequence();
            seq.Append(buyButtonGraphic.DOColor(cantAffordFlashColor, cantAffordFlashDuration * 0.5f));
            seq.Append(buyButtonGraphic.DOColor(_btnOriginalColor, cantAffordFlashDuration * 0.5f));
            _activeTween = seq;
        }
    }
}
