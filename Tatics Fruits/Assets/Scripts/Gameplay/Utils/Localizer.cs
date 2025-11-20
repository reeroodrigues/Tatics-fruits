using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-100)]
public class Localizer : MonoBehaviour
{
    private static Localizer _instance;
    public static Localizer Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Localizer>();
                
                if (_instance == null)
                {
                    var go = new GameObject("Localizer");
                    _instance = go.AddComponent<Localizer>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    public static bool IsReady => _instance != null && _instance._isInitialized;
    
    private readonly Dictionary<string, string> _table = new();
    private bool _isInitialized = false;

    public string CurrentLanguage { get; private set; } = "pt-BR";
    
    public delegate void LanguageChanged();
    public event LanguageChanged OnLanguageChanged;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var _ = Instance;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    private void Initialize()
    {
        var cfg = SettingsRepository.Get();
        SetLanguage(cfg.language, save: false);
        _isInitialized = true;
    }

    public void SetLanguage(string language, bool save = true)
    {
        CurrentLanguage = language;
        LoadTable(language);
        if (save)
        {
            var s = SettingsRepository.Get();
            s.language = language;
            SettingsRepository.Save(s);
        }
        
        OnLanguageChanged?.Invoke();
    }

    public string TrFormat(string key, string fallback, params object[] args)
    {
        var fmt = Tr(key, fallback);
        try
        {
            return string.Format(fmt, args);
        }
        catch (Exception e)
        {
            return fmt;
        }
    }

    private void LoadTable(string language)
    {
        _table.Clear();
        
        string path = Path.Combine(Application.streamingAssetsPath, "i18n", $"{language}.json");
        
#if UNITY_ANDROID && !UNITY_EDITOR
        var www = new UnityEngine.WWW(path);
        while (!www.isDone) { }
        if(!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
            return;
        }
        var json = www.text;
#else
        if (!File.Exists(path))
        {
            Debug.LogError($"Missing locale + {path}");
            return;
        }
        var json = File.ReadAllText(path); 
#endif
        var wrapper = JsonUtility.FromJson<LocalizationWrapper>(Wrap(json));
        if (wrapper != null && wrapper.entries != null)
        {
            foreach (var e in wrapper.entries)
            {
                _table[e.key] = e.value;
            }
        }
    }
    
    [System.Serializable] private class LocalizationEntry { public string key; public string value; }
    [System.Serializable] private class LocalizationWrapper { public LocalizationEntry[] entries; }

    private string Wrap(string raw)
    {
        var dict = MiniJson.Deserialize(raw) as Dictionary<string, object>;
        var list = new List<LocalizationEntry>();
        foreach (var kv in dict) list.Add(new LocalizationEntry { key = kv.Key, value = kv.Value.ToString() });
        return JsonUtility.ToJson(new LocalizationWrapper { entries = list.ToArray() }, false);
    }

    public string Tr(string key, string fallback = "")
        => _table.TryGetValue(key, out var v) ? v : (string.IsNullOrEmpty(fallback) ? key : fallback);
}