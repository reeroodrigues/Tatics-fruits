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
        
            if (_currentUser == null)
            {
                SignInAsGuest();
            }
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
                Debug.LogError($"[LoginManager] User is null in OnFirebaseSignedIn!");
                return;
            }
            
            var userId = user.UserId;
            var displayName = user.DisplayName ?? "Guest";
            
            Debug.Log($"[LoginManager] User signed in - ID: {userId}, Name: {displayName}");

            if (dataSaver != null)
            {
                dataSaver.SetUserId(userId);
                dataSaver.LoadData();

                var profileController = FindObjectOfType<PlayerProfileController>();
                if (profileController != null && profileController.Data != null)
                {
                    dataSaver.dataToSave.userName = string.IsNullOrEmpty(displayName) || displayName == "Guest" ? profileController.Data.playerName : displayName;
                    dataSaver.dataToSave.crrLevel = profileController.Data.currentLevelIndex;
                    dataSaver.dataToSave.highScore = profileController.Data.highestLevelUnlocked;
                    dataSaver.dataToSave.ownedCards =  new List<string>(profileController.Data.ownedCards);
                    dataSaver.dataToSave.equippedDeck = new List<string>(profileController.Data.equippedDeck);
                    dataSaver.dataToSave.unlockedAvatar = new List<int>(profileController.Data.unlockedAvatars);
                    dataSaver.dataToSave.purchasedAvatar = new List<int>(profileController.Data.purchasedAvatars);
                    dataSaver.dataToSave.bestScores = new Dictionary<string, int>(profileController.Data.BestScores);
                    dataSaver.dataToSave.musicOn = profileController.Data.musicOn;
                    dataSaver.dataToSave.sfxOn = profileController.Data.sfxOn;
                    dataSaver.dataToSave.vfxOn = profileController.Data.vfxOn;
                    dataSaver.dataToSave.language = profileController.Data.language;
                    dataSaver.dataToSave.dailyDayKey = profileController.Data.daily.dayKey;
                    dataSaver.dataToSave.lastLoginDayKey = profileController.Data.daily.login.lastClaimDayKey;
                    
                    dataSaver.SaveData();
                    Debug.Log($"[LoginManager] ✅ Full player profile synced to Firebase - Coins: {profileController.Data.gold}, Cards: {profileController.Data.ownedCards.Count}, Avatars: {profileController.Data.unlockedAvatars.Count}");
                }
            }
            else
            {
                Debug.LogError("[LoginManager] DataSaver is null!");
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
