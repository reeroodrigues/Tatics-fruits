using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SaveManager]");
                    _instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private readonly Dictionary<Type, object> _cache = new();
        private readonly Dictionary<string, DateTime> _lastSaveTimes = new();
        
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 60f;
        [SerializeField] private bool useCompression = false;
        [SerializeField] private bool enableBackup = true;
        [SerializeField] private int maxBackups = 3;

        private float _autoSaveTimer;
        private readonly HashSet<Type> _dirtyData = new();

        public event Action<Type> OnSaved;
        public event Action<Type> OnLoaded;
        public event Action<Type, Exception> OnSaveError;
        public event Action<Type, Exception> OnLoadError;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!autoSaveEnabled) return;

            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= autoSaveInterval)
            {
                _autoSaveTimer = 0f;
                SaveDirty();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveAll();
            }
        }

        private void OnApplicationQuit()
        {
            SaveAll();
        }

        public void Save<T>(T data, bool markClean = true) where T : class, ISaveData
        {
            if (data == null)
            {
                Debug.LogWarning("[SaveManager] Attempted to save null data");
                return;
            }

            try
            {
                data.OnBeforeSave();

                string fileName = data.GetFileName();
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                string filePath = GetFilePath(fileName);

                if (enableBackup && File.Exists(filePath))
                {
                    CreateBackup(filePath);
                }

                File.WriteAllText(filePath, json);
                
                _cache[typeof(T)] = data;
                _lastSaveTimes[fileName] = DateTime.Now;

                if (markClean)
                {
                    _dirtyData.Remove(typeof(T));
                }

                OnSaved?.Invoke(typeof(T));

#if UNITY_EDITOR
                Debug.Log($"[SaveManager] Saved {typeof(T).Name} to {filePath}");
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save {typeof(T).Name}: {e.Message}");
                OnSaveError?.Invoke(typeof(T), e);
            }
        }

        public T Load<T>() where T : class, ISaveData, new()
        {
            if (_cache.TryGetValue(typeof(T), out var cached))
            {
                return cached as T;
            }

            T data = new T();
            string fileName = data.GetFileName();
            string filePath = GetFilePath(fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    data = JsonUtility.FromJson<T>(json) ?? new T();
                    data.OnAfterLoad();

                    _cache[typeof(T)] = data;
                    OnLoaded?.Invoke(typeof(T));

#if UNITY_EDITOR
                    Debug.Log($"[SaveManager] Loaded {typeof(T).Name} from {filePath}");
#endif
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to load {typeof(T).Name}: {e.Message}");
                    OnLoadError?.Invoke(typeof(T), e);
                    
                    if (TryRestoreFromBackup<T>(out var restored))
                    {
                        Debug.Log($"[SaveManager] Restored {typeof(T).Name} from backup");
                        return restored;
                    }
                }
            }
            else
            {
                data.OnAfterLoad();
                _cache[typeof(T)] = data;
                Save(data);
            }

            return data;
        }

        public T GetCached<T>() where T : class, ISaveData
        {
            return _cache.TryGetValue(typeof(T), out var cached) ? cached as T : null;
        }

        public void MarkDirty<T>() where T : class, ISaveData
        {
            _dirtyData.Add(typeof(T));
        }

        public void SaveDirty()
        {
            var types = new List<Type>(_dirtyData);
            foreach (var type in types)
            {
                if (_cache.TryGetValue(type, out var data) && data is ISaveData saveData)
                {
                    var method = typeof(SaveManager).GetMethod(nameof(Save))?.MakeGenericMethod(type);
                    method?.Invoke(this, new object[] { data, true });
                }
            }
        }

        public void SaveAll()
        {
            foreach (var kvp in _cache)
            {
                if (kvp.Value is ISaveData data)
                {
                    var method = typeof(SaveManager).GetMethod(nameof(Save))?.MakeGenericMethod(kvp.Key);
                    method?.Invoke(this, new object[] { data, true });
                }
            }
        }

        public void Delete<T>() where T : class, ISaveData, new()
        {
            T temp = new T();
            string fileName = temp.GetFileName();
            string filePath = GetFilePath(fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[SaveManager] Deleted {filePath}");
            }

            _cache.Remove(typeof(T));
            _dirtyData.Remove(typeof(T));
            _lastSaveTimes.Remove(fileName);
        }

        public void DeleteAll()
        {
            var directory = Application.persistentDataPath;
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, "*.json");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                
                Debug.Log($"[SaveManager] Deleted all save files from {directory}");
            }

            _cache.Clear();
            _dirtyData.Clear();
            _lastSaveTimes.Clear();
        }

        public bool Exists<T>() where T : class, ISaveData, new()
        {
            T temp = new T();
            string fileName = temp.GetFileName();
            return File.Exists(GetFilePath(fileName));
        }

        public DateTime? GetLastSaveTime<T>() where T : class, ISaveData, new()
        {
            T temp = new T();
            string fileName = temp.GetFileName();
            
            if (_lastSaveTimes.TryGetValue(fileName, out var time))
            {
                return time;
            }

            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                return File.GetLastWriteTime(filePath);
            }

            return null;
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        private void CreateBackup(string filePath)
        {
            try
            {
                string backupDir = Path.Combine(Application.persistentDataPath, "Backups");
                Directory.CreateDirectory(backupDir);

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = Path.Combine(backupDir, $"{fileName}_{timestamp}.backup");

                File.Copy(filePath, backupPath, true);

                CleanupOldBackups(backupDir, fileName);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to create backup: {e.Message}");
            }
        }

        private void CleanupOldBackups(string backupDir, string fileName)
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupDir, $"{fileName}_*.backup");
                if (backupFiles.Length > maxBackups)
                {
                    Array.Sort(backupFiles);
                    for (int i = 0; i < backupFiles.Length - maxBackups; i++)
                    {
                        File.Delete(backupFiles[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to cleanup old backups: {e.Message}");
            }
        }

        private bool TryRestoreFromBackup<T>(out T data) where T : class, ISaveData, new()
        {
            data = null;

            try
            {
                string backupDir = Path.Combine(Application.persistentDataPath, "Backups");
                T temp = new T();
                string fileName = Path.GetFileNameWithoutExtension(temp.GetFileName());
                
                var backupFiles = Directory.GetFiles(backupDir, $"{fileName}_*.backup");
                if (backupFiles.Length == 0)
                {
                    return false;
                }

                Array.Sort(backupFiles);
                string latestBackup = backupFiles[backupFiles.Length - 1];

                string json = File.ReadAllText(latestBackup);
                data = JsonUtility.FromJson<T>(json);
                data.OnAfterLoad();

                _cache[typeof(T)] = data;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to restore from backup: {e.Message}");
                return false;
            }
        }

        public string GetSavePath()
        {
            return Application.persistentDataPath;
        }

        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}
