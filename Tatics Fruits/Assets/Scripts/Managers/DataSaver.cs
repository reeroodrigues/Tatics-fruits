using System;
using System.Collections;
using System.IO;
using Firebase.Database;
using UnityEngine;

namespace Managers
{
    [Serializable]
    public class DataToSave
    {
        public string userName;
        public int totalCoins;
        public int crrLevel;
        public int highScore;
        public long lastUpdatedTicks;
    }

    public class DataSaver : MonoBehaviour
    {
        [Header("Player Data")]
        [SerializeField] private string userId; // defina isso pelo Firebase Auth ou outro sistema de ID
        [SerializeField] public DataToSave dataToSave = new DataToSave();

        private DatabaseReference _databaseReference;
        private string _localFilePath;

        public event Action<DataToSave> OnDataLoaded;
        public event Action OnDataNotFound;
        public event Action<Exception> OnLoadFailed;

        private void Awake()
        {
            _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            
            _localFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
            Debug.Log($"[DataSaver] Local file path: {_localFilePath}");
        }

        /// <summary>
        /// Salva dados no local (JSON) e tenta salvar no Firebase.
        /// </summary>
        public void SaveData()
        {
            if (!ValidateUserId()) return;

            if (dataToSave == null)
                dataToSave = new DataToSave();
            
            dataToSave.lastUpdatedTicks = DateTime.UtcNow.Ticks;
            
            SaveLocal();
            
            var json = JsonUtility.ToJson(dataToSave);
            _databaseReference
                .Child("users")
                .Child(userId)
                .SetRawJsonValueAsync(json)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError($"[DataSaver] Erro ao salvar dados no Firebase: {task.Exception}");
                    }
                    else if (task.IsCompletedSuccessfully)
                    {
                        Debug.Log("[DataSaver] Dados salvos com sucesso no Firebase.");
                    }
                });
        }

        public void SetUserId(string uid)
        {
            userId = uid;
            Debug.Log($"[Data saver] UserId set: {userId}");
        }

        /// <summary>
        /// Carrega dados do Firebase + Local, resolve conflito e sincroniza.
        /// </summary>
        public void LoadData()
        {
            if (!ValidateUserId()) return;
            StartCoroutine(LoadDataCoroutine());
        }

        private IEnumerator LoadDataCoroutine()
        {
            var serverDataTask = _databaseReference.Child("users").Child(userId).GetValueAsync();
            
            var localData = LoadLocal();
            
            yield return new WaitUntil(() => serverDataTask.IsCompleted);

            DataToSave cloudData = null;

            if (serverDataTask.IsFaulted)
            {
                Debug.LogError($"[DataSaver] Erro ao carregar dados do Firebase: {serverDataTask.Exception}");
                OnLoadFailed?.Invoke(serverDataTask.Exception);
            }
            else
            {
                var snapshot = serverDataTask.Result;

                if (snapshot != null && snapshot.Exists)
                {
                    var jsonData = snapshot.GetRawJsonValue();

                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        Debug.Log("[DataSaver] Server data found!");
                        cloudData = JsonUtility.FromJson<DataToSave>(jsonData);
                    }
                    else
                    {
                        Debug.Log("[DataSaver] Snapshot existe mas não tem JSON.");
                    }
                }
                else
                {
                    Debug.Log("[DataSaver] Nenhum dado encontrado no Firebase pra esse usuário.");
                }
            }
            
            dataToSave = ResolveDataConflict(localData, cloudData);

            if (dataToSave != null)
            {
                Debug.Log("[DataSaver] Dados finais resolvidos. Sincronizando local + Firebase.");
                
                if (dataToSave.lastUpdatedTicks == 0)
                    dataToSave.lastUpdatedTicks = DateTime.UtcNow.Ticks;
                
                SaveLocal();
                var json = JsonUtility.ToJson(dataToSave);
                _databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json);

                OnDataLoaded?.Invoke(dataToSave);
            }
            else
            {
                Debug.Log("[DataSaver] Nenhum dado local nem remoto. Criando novo perfil padrão.");
                dataToSave = CreateNewDefaultData();
                SaveLocal();
                var json = JsonUtility.ToJson(dataToSave);
                _databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json);

                OnDataNotFound?.Invoke();
            }
        }

        #region Local JSON

        private void SaveLocal()
        {
            try
            {
                var json = JsonUtility.ToJson(dataToSave);
                File.WriteAllText(_localFilePath, json);
                Debug.Log("[DataSaver] Dados salvos localmente.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataSaver] Erro ao salvar localmente: {e}");
            }
        }

        private DataToSave LoadLocal()
        {
            try
            {
                if (!File.Exists(_localFilePath))
                {
                    Debug.Log("[DataSaver] Nenhum arquivo local encontrado.");
                    return null;
                }

                var json = File.ReadAllText(_localFilePath);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log("[DataSaver] Arquivo local está vazio.");
                    return null;
                }

                var data = JsonUtility.FromJson<DataToSave>(json);
                Debug.Log("[DataSaver] Dados locais carregados com sucesso.");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataSaver] Erro ao carregar dados locais: {e}");
                return null;
            }
        }

        #endregion

        #region Helpers

        private DataToSave ResolveDataConflict(DataToSave local, DataToSave cloud)
        {
            if (local == null && cloud == null)
                return null;
            
            if (local != null && cloud == null)
            {
                Debug.Log("[DataSaver] Usando apenas dados locais.");
                return local;
            }

            if (local == null && cloud != null)
            {
                Debug.Log("[DataSaver] Usando apenas dados da nuvem.");
                return cloud;
            }
            
            if (cloud.lastUpdatedTicks > local.lastUpdatedTicks)
            {
                Debug.Log("[DataSaver] Dados da nuvem são mais recentes. Usando cloud.");
                return cloud;
            }
            else
            {
                Debug.Log("[DataSaver] Dados locais são mais recentes (ou empatados). Usando local.");
                return local;
            }
        }

        private DataToSave CreateNewDefaultData()
        {
            return new DataToSave
            {
                userName = "Guest",
                totalCoins = 0,
                crrLevel = 1,
                highScore = 0,
                lastUpdatedTicks = DateTime.UtcNow.Ticks
            };
        }

        private bool ValidateUserId()
        {
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("[DataSaver] userId está vazio! Configure o ID do jogador antes de salvar/carregar.");
                return false;
            }

            return true;
        }

        // Se você usar Firebase Auth, pode chamar isso após logar:
        // public void SetUserId(string uid) => userId = uid;

        #endregion
    }
}
