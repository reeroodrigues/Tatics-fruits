using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

namespace Managers
{
    public class LoginManager : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private DataSaver dataSaver;

        private FirebaseAuth _firebaseAuth;
        private FirebaseUser _currentUser;

        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            await InitializeFirebaseAsync();
        }


        private async Task InitializeFirebaseAsync()
        {
            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _firebaseAuth = FirebaseAuth.DefaultInstance;
                    Debug.Log("[LoginManager] Firebase initialized successfully");
                
                    _currentUser = _firebaseAuth.CurrentUser;
                    if (_currentUser != null)
                    {
                        Debug.Log($"[LoginManager] Existing user found: {_currentUser.UserId}");
                        OnFirebaseSignedIn(_currentUser);
                    }
                    else
                    {
                        Debug.Log("[LoginManager] No existing user found. Creating new guest user...");
                        SignInAsGuest();
                    }
                }
                else
                {
                    Debug.LogError($"[LoginManager] Could not resolve Firebase dependencies: {dependencyStatus}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoginManager] Error initializing Firebase: {e}");
            }
        }

        public void LoginGooglePlayGames()
        {
            PlayGamesPlatform.Instance.Authenticate((status) =>
            {
                if (status == SignInStatus.Success)
                {
                    Debug.Log("[LoginManager] Google Play Games authentication successful");
                    
                    PlayGamesPlatform.Instance.RequestServerSideAccess(false, idToken =>
                    {
                        Debug.Log($"[LoginManager] ID Token received: {idToken?.Substring(0, 20)}...");
                        SignInWithGooglePlayGamesFirebase(idToken);
                    });
                }
                else
                {
                    Debug.LogError($"[LoginManager] Google Play Games authentication failed: {status}");
                }
            });
        }

        private void SignInWithGooglePlayGamesFirebase(string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
            {
                Debug.LogError("[LoginManager] ID Token is null or empty!");
                return;
            }

            Debug.Log("[LoginManager] Signing in to Firebase with Google Play Games...");
        
            Credential credential = PlayGamesAuthProvider.GetCredential(idToken);
        
            _firebaseAuth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("[LoginManager] Firebase sign-in was canceled");
                    return;
                }
            
                if (task.IsFaulted)
                {
                    Debug.LogError($"[LoginManager] Firebase sign-in failed: {task.Exception}");
                    return;
                }

                _currentUser = task.Result;
                Debug.Log($"[LoginManager] Firebase sign-in successful! User ID: {_currentUser.UserId}");
            
                OnFirebaseSignedIn(_currentUser);
            });
        }

        public void SignInAsGuest()
        {
            Debug.Log("[LoginManager] Signing in as guest...");
        
            _firebaseAuth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("[LoginManager] Guest sign-in was canceled");
                    return;
                }
            
                if (task.IsFaulted)
                {
                    Debug.LogError($"[LoginManager] Guest sign-in failed: {task.Exception}");
                    return;
                }

                _currentUser = task.Result.User;
                Debug.Log($"[LoginManager] Guest sign-in successful! User ID: {_currentUser.UserId}");
            
                OnFirebaseSignedIn(_currentUser);
            });
        }

        private void OnFirebaseSignedIn(FirebaseUser user)
        {
            if (user == null)
            {
                Debug.LogError("[LoginManager] User is null in OnFirebaseSignedIn!");
                return;
            }

            var uid = user.UserId;
            var displayName = user.DisplayName ?? "Guest";

            Debug.Log($"[LoginManager] User signed in - ID: {uid}, Name: {displayName}");

            if (dataSaver == null)
            {
                Debug.LogError("[LoginManager] DataSaver is null!");
                return;
            }

            dataSaver.SetUserId(uid);
            
            dataSaver.OnDataLoaded -= HandleDataLoaded;
            dataSaver.OnLoadFailed -= HandleLoadFailed;
            dataSaver.OnDataNotFound -= HandleDataNotFound;

            dataSaver.OnDataLoaded += HandleDataLoaded;
            dataSaver.OnLoadFailed += HandleLoadFailed;
            dataSaver.OnDataNotFound += () =>
            {
                dataSaver.SaveData(force: true);
            };

            dataSaver.LoadData();

            void HandleDataLoaded(DataToSave cloud)
            {
                Debug.Log($"[LoginManager] ✅ Data loaded from DataSaver. Coins(cloud/local resolved): {cloud.totalCoins}");

                var profileController = FindObjectOfType<PlayerProfileController>();
                if (profileController == null || profileController.Data == null)
                {
                    Debug.LogWarning("[LoginManager] PlayerProfileController not found or Data is null.");
                    return;
                }
                
                profileController.Data.playerName = string.IsNullOrEmpty(displayName) || displayName == "Guest"
                    ? cloud.userName
                    : displayName;

                profileController.Data.gold = cloud.totalCoins;
                profileController.Data.currentLevelIndex = cloud.crrLevel;
                profileController.Data.highestLevelUnlocked = cloud.highScore;

                if (cloud.ownedCards != null) profileController.Data.ownedCards = new List<string>(cloud.ownedCards);
                if (cloud.equippedDeck != null) profileController.Data.equippedDeck = new List<string>(cloud.equippedDeck);
                if (cloud.unlockedAvatar != null) profileController.Data.unlockedAvatars = new List<int>(cloud.unlockedAvatar);
                if (cloud.purchasedAvatar != null) profileController.Data.purchasedAvatars = new List<int>(cloud.purchasedAvatar);
                if (cloud.bestScores != null) profileController.Data.BestScores = new Dictionary<string, int>(cloud.bestScores);

                profileController.Data.musicOn = cloud.musicOn;
                profileController.Data.sfxOn = cloud.sfxOn;
                profileController.Data.vfxOn = cloud.vfxOn;
                profileController.Data.language = cloud.language;

                if (profileController.Data.daily != null)
                {
                    profileController.Data.daily.dayKey = cloud.dailyDayKey ?? "";
                    if (profileController.Data.daily.login != null)
                        profileController.Data.daily.login.lastClaimDayKey = cloud.lastLoginDayKey ?? "";
                }

                profileController.SaveProfile();
                
                dataSaver.OnDataLoaded -= HandleDataLoaded;
                dataSaver.OnLoadFailed -= HandleLoadFailed;
                dataSaver.OnDataNotFound -= HandleDataNotFound;

                Debug.Log($"[LoginManager] ✅ Profile applied. Gold(now): {profileController.Data.gold}");
            }

            void HandleDataNotFound()
            {
                Debug.Log("[LoginManager] No data found in cloud/local. Using default created by DataSaver.");
                
                dataSaver.OnDataLoaded -= HandleDataLoaded;
                dataSaver.OnLoadFailed -= HandleLoadFailed;
                dataSaver.OnDataNotFound -= HandleDataNotFound;
            }

            void HandleLoadFailed(Exception e)
            {
                Debug.LogError($"[LoginManager] Load failed: {e}");

                dataSaver.OnDataLoaded -= HandleDataLoaded;
                dataSaver.OnLoadFailed -= HandleLoadFailed;
                dataSaver.OnDataNotFound -= HandleDataNotFound;
            }
        }

        public bool IsSignedIn()
        {
            return _currentUser != null;
        }

        public string GetUserId()
        {
            return _currentUser?.UserId;
        }
    }
}
