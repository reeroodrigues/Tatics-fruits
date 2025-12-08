using System.Collections;
using System.Collections.Generic;
using Core.ScriptableObjects;
using UnityEngine;

namespace Core
{
    public class SoundManager : MonoBehaviour
    {
        private const int AUDIO_SOURCE_POOL_SIZE = 10;

        private static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SoundManager]");
                    _instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Sound Database")]
        [SerializeField] private List<SoundClipData> soundDatabase = new();

        [Header("Audio Sources")]
        private AudioSource _musicSource;
        private AudioSource _ambientSource;
        private List<AudioSource> _sfxSourcePool;
        private Dictionary<string, SoundClipData> _soundCache;

        private float _masterVolume = 1f;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        private bool _isMusicEnabled = true;
        private bool _isSFXEnabled = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            BuildSoundCache();
            LoadSettings();
        }

        private void InitializeAudioSources()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;

            _sfxSourcePool = new List<AudioSource>();
            for (int i = 0; i < AUDIO_SOURCE_POOL_SIZE; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxSourcePool.Add(source);
            }
        }

        private void BuildSoundCache()
        {
            _soundCache = new Dictionary<string, SoundClipData>();
            foreach (var soundData in soundDatabase)
            {
                if (soundData != null && !string.IsNullOrEmpty(soundData.soundName))
                {
                    _soundCache[soundData.soundName] = soundData;
                }
            }
        }

        private void LoadSettings()
        {
            var settings = SettingsRepository.Get();
            _isMusicEnabled = settings.musicOn;
            _isSFXEnabled = settings.sfxOn;
            _musicVolume = settings.musicVolume;
            _sfxVolume = settings.sfxVolume;
            _masterVolume = settings.masterVolume;

            UpdateMusicVolume();
            UpdateSFXVolume();
        }

        public void PlayMusic(string soundName, bool fadeIn = false, float fadeDuration = 1f)
        {
            if (!_isMusicEnabled) return;

            if (!_soundCache.TryGetValue(soundName, out var soundData))
            {
                Debug.LogWarning($"[SoundManager] Sound '{soundName}' not found in database");
                return;
            }

            if (soundData.category != SoundCategory.Music)
            {
                Debug.LogWarning($"[SoundManager] Sound '{soundName}' is not a Music category");
                return;
            }

            if (_musicSource.clip == soundData.clip && _musicSource.isPlaying)
            {
                return;
            }

            _musicSource.clip = soundData.clip;
            _musicSource.volume = fadeIn ? 0f : soundData.volume * _musicVolume * _masterVolume;
            _musicSource.pitch = soundData.GetRandomPitch();
            _musicSource.loop = soundData.loop;
            _musicSource.Play();

            if (fadeIn)
            {
                StartCoroutine(FadeAudioSource(_musicSource, soundData.volume * _musicVolume * _masterVolume, fadeDuration));
            }
        }

        public void StopMusic(bool fadeOut = false, float fadeDuration = 1f)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutAndStop(_musicSource, fadeDuration));
            }
            else
            {
                _musicSource.Stop();
            }
        }

        public void PlaySFX(string soundName)
        {
            if (!_isSFXEnabled) return;

            if (!_soundCache.TryGetValue(soundName, out var soundData))
            {
                Debug.LogWarning($"[SoundManager] Sound '{soundName}' not found in database");
                return;
            }

            AudioSource source = GetAvailableSFXSource();
            if (source == null)
            {
                Debug.LogWarning("[SoundManager] No available audio source in pool");
                return;
            }

            source.clip = soundData.clip;
            source.volume = soundData.volume * _sfxVolume * _masterVolume;
            source.pitch = soundData.GetRandomPitch();
            source.loop = soundData.loop;
            source.Play();
        }

        public void PlaySFXAtPoint(string soundName, Vector3 position)
        {
            if (!_isSFXEnabled) return;

            if (!_soundCache.TryGetValue(soundName, out var soundData))
            {
                Debug.LogWarning($"[SoundManager] Sound '{soundName}' not found in database");
                return;
            }

            float volume = soundData.volume * _sfxVolume * _masterVolume;
            AudioSource.PlayClipAtPoint(soundData.clip, position, volume);
        }

        public void StopAllSFX()
        {
            foreach (var source in _sfxSourcePool)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
            }
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
            UpdateSFXVolume();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            UpdateSFXVolume();
        }

        public void SetMusicEnabled(bool enabled)
        {
            _isMusicEnabled = enabled;
            if (!enabled)
            {
                _musicSource.Pause();
            }
            else if (_musicSource.clip != null)
            {
                _musicSource.UnPause();
            }
        }

        public void SetSFXEnabled(bool enabled)
        {
            _isSFXEnabled = enabled;
            if (!enabled)
            {
                StopAllSFX();
            }
        }

        private void UpdateMusicVolume()
        {
            if (_musicSource != null && _musicSource.clip != null)
            {
                string clipName = _musicSource.clip.name;
                if (_soundCache.TryGetValue(clipName, out var soundData))
                {
                    _musicSource.volume = soundData.volume * _musicVolume * _masterVolume;
                }
                else
                {
                    _musicSource.volume = _musicVolume * _masterVolume;
                }
            }
        }

        private void UpdateSFXVolume()
        {
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in _sfxSourcePool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            return _sfxSourcePool[0];
        }

        private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;
        }

        private IEnumerator FadeOutAndStop(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            source.Stop();
            source.volume = startVolume;
        }

        public void RefreshFromSettings()
        {
            LoadSettings();
        }

        public bool IsMusicEnabled => _isMusicEnabled;
        public bool IsSFXEnabled => _isSFXEnabled;
        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SFXVolume => _sfxVolume;
    }
}
