using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class DailyMissionItemView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private RectTransform coinSpawnPoint;

    [Header("Claim UI")]
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject claimStamp;
    
    [SerializeField] private bool hideProgressWhenCompleted = false;

    private DailyMissionsController _ctrl;
    private DailyMissionState _state;
    private DailyMissionSo _def;
    
    public bool Matches(string missionId) => _state != null && _state.missionId == missionId;

    public void Setup(DailyMissionsController ctrl, DailyMissionState state, DailyMissionSo def)
    {
        _ctrl = ctrl;
        _state = state;
        _def = def;
        Wire();
    }
    
    public void Setup(DailyMissionsController ctrl, DailyMissionState state)
    {
        _ctrl = ctrl;
        _state = state;
        _def = null;
        Wire();
    }

    private void Wire()
    {
        SetDescriptionLocalized();
        SetRewardLocalized();
        Refresh();

        if (claimButton)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClickClaim);
        }

        if (Localizer.Instance != null)
        {
            Localizer.Instance.OnLanguageChanged -= OnLanguageChanged;
            Localizer.Instance.OnLanguageChanged += OnLanguageChanged;
        }
    }

    private void OnDestroy()
    {
        if (Localizer.Instance != null)
            Localizer.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        SetDescriptionLocalized();
        SetRewardLocalized();
    }
    

    public void Refresh()
    {
        var max = Mathf.Max(1, _state.target);
        var cur = Mathf.Clamp(_state.progress, 0, max);

        if (progressBar)
        {
            progressBar.maxValue = max;
            progressBar.value = cur;

            var hide = hideProgressWhenCompleted && _state.completed;
            progressBar.gameObject.SetActive(!hide);
        }

        if (progressText)
        {
            var hide = hideProgressWhenCompleted && _state.completed;
            progressText.gameObject.SetActive(!hide);
            if (!hide) progressText.text = $"{cur}/{max}";
        }

        var canClaim = _state.completed && !_state.claimed;

        if (claimButton)
        {
            claimButton.gameObject.SetActive(true);
            claimButton.interactable = canClaim;
        }

        if (claimStamp) claimStamp.SetActive(_state.claimed);
    }
    

    private void SetDescriptionLocalized()
    {
        if (!descriptionText) return;

        string key = (!string.IsNullOrEmpty(_def?.descriptionKey))
                       ? _def.descriptionKey
                       : (!string.IsNullOrEmpty(_state.missionId) ? $"mission.{_state.missionId}" : null);

        string fallback = !string.IsNullOrEmpty(_def?.descriptionTemplate)
                            ? _def.descriptionTemplate
                            : (!string.IsNullOrEmpty(_state.description) ? _state.description : _state.missionId);

        int level = (_def != null && _def.levelParam > 0) ? _def.levelParam : ExtractTrailingNumber(_state.missionId);
        object[] args = (level > 0) ? new object[] { level, _state.target } : new object[] { _state.target };

        if (Localizer.Instance != null && !string.IsNullOrEmpty(key))
        {
            string txt = Localizer.Instance.TrFormat(key, fallback, args);
            if (txt == fallback && key.Contains("."))
                txt = Localizer.Instance.TrFormat(key.Replace('.', '_'), fallback, args);
            descriptionText.text = txt;
        }
        else
        {
            string txt = fallback;
            txt = txt.Replace("{level}", level.ToString());
            txt = txt.Replace("{target}", _state.target.ToString());
            try { txt = string.Format(txt, args); } catch { }
            descriptionText.text = txt;
        }
    }

    private int ExtractTrailingNumber(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        var m = Regex.Match(s, @"(\d+)$");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    private void SetRewardLocalized()
    {
        var reward = (_def != null && _def.rewardGold > 0) ? _def.rewardGold : _state.rewardGold;
        if (rewardText) rewardText.text = $"+{reward}";
    }
    
    private void OnClickClaim()
    {
        if (_ctrl == null) return;

        if (_ctrl.TryClaimMission(_state.missionId))
        {
            var spawn = coinSpawnPoint ? coinSpawnPoint : (RectTransform)transform;
            CoinCollectFx.Instance?.PlayFromUI(spawn, _state.rewardGold);

            _state.claimed = true;
            Refresh();
        }
    }
}
