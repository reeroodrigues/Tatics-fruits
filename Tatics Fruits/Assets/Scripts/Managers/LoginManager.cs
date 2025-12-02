using System;
using System.Threading.Tasks;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Managers;

public class LoginManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DataSaver dataSaver;

    private string m_GooglePlayGamesToken;

    private async void Awake()
    {
        await InitializeUnityServicesAsync();
        
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        
        LoginGooglePlayGames();
    }

    private async Task InitializeUnityServicesAsync()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized &&
                UnityServices.State != ServicesInitializationState.Initializing)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("[LoginManager] Unity Services inicializado com sucesso.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LoginManager] Erro ao inicializar Unity Services: {e}");
        }
    }

    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[LoginManager] Google Play Games access successful");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log($"[LoginManager] Authorization code: {code}");
                    m_GooglePlayGamesToken = code;
                });
            }
            else
            {
                Debug.LogError($"[LoginManager] Google Play Games access failed, status: {status}");
            }
        });
    }

    /// <summary>
    /// Pode ser chamado por um botão "Continuar" / "Login"
    /// </summary>
    public void StartSignInWithGooglePlayGames()
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.Log("[LoginManager] Não autenticado no GPG ainda, tentando login...");
            LoginGooglePlayGames();
            return;
        }

        SignInOrLinkWithGooglePlayGames();
    }

    private async void SignInOrLinkWithGooglePlayGames()
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.LogWarning("[LoginManager] Google Play Games token está vazio, não é possível logar no UGS ainda.");
            return;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
        else
        {
            await LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
    }

    private async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            Debug.Log("[LoginManager] SignInWithGooglePlayGamesAsync iniciado...");
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("[LoginManager] SignInWithGooglePlayGamesAsync concluído com sucesso.");

            OnSignedInToUGS();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[LoginManager] AuthenticationException (SignIn): {ex}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[LoginManager] RequestFailedException (SignIn): {ex}");
        }
    }

    private async Task LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            Debug.Log("[LoginManager] LinkWithGooglePlayGamesAsync iniciado...");
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("[LoginManager] LinkWithGooglePlayGamesAsync concluído com sucesso.");

            OnSignedInToUGS();
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            Debug.LogWarning($"[LoginManager] Conta já estava vinculada ao GPG: {ex}");
            
            if (AuthenticationService.Instance.IsSignedIn)
            {
                OnSignedInToUGS();
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[LoginManager] AuthenticationException (Link): {ex}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[LoginManager] RequestFailedException (Link): {ex}");
        }
    }

    /// <summary>
    /// Chamado sempre que o login no Unity Authentication for bem-sucedido.
    /// Aqui integramos com o DataSaver.
    /// </summary>
    private void OnSignedInToUGS()
    {
        string playerId = AuthenticationService.Instance.PlayerId;
        string playerName = PlayGamesPlatform.Instance.GetUserDisplayName();

        if (dataSaver != null)
        {
            dataSaver.SetUserId(playerId);
            dataSaver.LoadData();

            if (!string.IsNullOrEmpty(playerName))
            {
                dataSaver.dataToSave.userName = playerName;
                dataSaver.SaveData();
            }
        }
    }
}
