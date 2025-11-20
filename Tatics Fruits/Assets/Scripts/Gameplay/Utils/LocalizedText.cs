using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Graphic))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] public string key;
    [TextArea] public string fallback;

    private TextMeshProUGUI _tmp;
    private TextMeshProUGUI _ugui;
    private bool _subscribed;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        _ugui = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        StartCoroutine(EnsureSubscribedThenRefresh());
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private IEnumerator EnsureSubscribedThenRefresh()
    {
        yield return new WaitUntil(() => Localizer.IsReady);

        TrySubscribe();
        Refresh();
    }

    private void TrySubscribe()
    {
        if (_subscribed || !Localizer.IsReady) return;
        Localizer.Instance.OnLanguageChanged += Refresh;
        _subscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!_subscribed || !Localizer.IsReady) return;
        Localizer.Instance.OnLanguageChanged -= Refresh;
        _subscribed = false;
    }

    public void Refresh()
    {
        if (!Localizer.IsReady) return;
        var txt = Localizer.Instance.Tr(key, fallback);
        if (_tmp) _tmp.text = txt;
        else if (_ugui) _ugui.text = txt;
    }
}