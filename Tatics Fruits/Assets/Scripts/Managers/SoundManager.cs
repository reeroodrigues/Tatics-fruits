using System;
using System.Collections.Generic;
using Core.ScriptableObjects;
using UnityEngine;

namespace Managers
{
    public class SoundManager : MonoBehaviour
    {
        private const int AudioSourcePoolSize = 10;

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
        
        [Header("Audio Source")]
        private AudioSource _musicSource;
        private AudioSource _ambientSource;
        private List<AudioSource> _sfxSourcePool;
        private Dictionary<string, SoundClipData> _soundCache;

        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 1f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private bool isMusicEnabled = true;
        [SerializeField] private bool isSfxEnabled = true;

        private void Awake()
        {
            if(_instance != null && _instance != this)
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
            for (int i = 0; i < AudioSourcePoolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
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
                    _soundCache[soundData.soundName] = soundData;
            }
        }

        private void LoadSettings()
        {
            var settings = SettingsRepository.Get();
            isMusicEnabled = settings.musicOn;
            isSfxEnabled = settings.sfxOn;

            UpdateMusicVolume();
            UpdateSFXVolume();
        }

        public void PlayMusic(string soundName, bool fadeIn = false, float fadeDuration = 1f)
        {
            if(!isMusicEnabled)
                return;

            if (!_soundCache.TryGetValue(soundName, out var soundData))
            {
                Debug.Log($"$[Sound Manager] Sound '{soundName}' is not found in database");
                return;
            }

            if (soundData.category != SoundCategory.Music)
            {
                Debug.Log($"$[Sound Manager] Sound '{soundName}' is not a Music category");
                return;
            }
            
            if(_musicSource.clip == soundData.clip && _musicSource.isPlaying)
                return;
            
            _musicSource.clip = soundData.clip;
            _musicSource.volume = fadeIn ? 0f : soundData.volume * musicVolume * masterVolume;
            _musicSource.pitch = soundData.GetRandomPitch();
            _musicSource.loop = soundData.loop;
            _musicSource.Play();

            if (fadeIn)
            {
                StartCoroutine(FadeAudioSource(_musicSource, soundData.volume * musicVolume * masterVolume, fadeDuration));
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
            if(!isSfxEnabled)
                return;

            if (!_soundCache.TryGetValue(soundName, out var soundData))
            {
                Debug.Log($"$[Sound Manager] Sound '{soundName}' is not found in database");
                return;
            }

            var source = GetAvailableSFXSource();
            if (source == null)
            {
                Debug.Log($"[Sound Manager] No available audio soure in pool");
                return;
            }
            
            source.clip = soundData.clip;
            source.volume = soundData.volume * sfxVolume * masterVolume;
            source.pitch = soundData.GetRandomPitch();
            source.loop = soundData.loop;
            source.Play();
        }

        public void PlaySFXAtPoint(string soundName, Vector3 position)
        {
            if(!isSfxEnabled)
                return;

            if (!_soundCache.TryGetValue(soundName, out var soundData))
            {
                Debug.Log($"$[Sound Manager] Sound '{soundName}' is not found in database");
                return;
            }
            
            var volume = soundData.volume * sfxVolume * masterVolume;
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
            masterVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
            UpdateSFXVolume();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateSFXVolume();
        }
        
        public void SetMusicEnabled(bool enabled)
        {
            isMusicEnabled = enabled;
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
            isSfxEnabled = enabled;
            if (!enabled)
            {
                StopAllSFX();
            }
        }
        
        private void UpdateMusicVolume()
        {
            if (_musicSource.clip != null && _soundCache.ContainsKey(_musicSource.clip.name))
            {
                var soundData = _soundCache[_musicSource.clip.name];
                _musicSource.volume = soundData.volume * musicVolume * masterVolume;
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
        
        private System.Collections.IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
        {
            var startVolume = source.volume;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;
        }
        
        private System.Collections.IEnumerator FadeOutAndStop(AudioSource source, float duration)
        {
            var startVolume = source.volume;
            var elapsed = 0f;

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

        public bool IsMusicEnabled => isMusicEnabled;
        public bool IsSFXEnabled => isSfxEnabled;
        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;
    }
}