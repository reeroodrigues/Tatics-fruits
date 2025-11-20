using System;
using System.Threading.Tasks;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;
using Unity.Services.Core;


public class LoginManager : MonoBehaviour
{
    private string m_GooglePlayGamesToken;

    private void Awake()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();
    }

    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("Google Play Games access successful");
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => 
                {
                    Debug.Log($"Authorization code" + code);
                    m_GooglePlayGamesToken = code;
                });
            }
            else
            {
                Debug.Log("Google Play Games access failed, status: {status}");
            }
        });
    }

    public void StartSignInWithGooglePlayGames()
    {
        if(!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            LoginGooglePlayGames();
            return;
        }

        SingInOrLinkWithGooglePlayGames();
    }

    private async void SingInOrLinkWithGooglePlayGames()
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
            return;

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await SingInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
        else
        {
            await LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
    }
    
    private async Task SingInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
        }
        catch (AuthenticationException ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async Task LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
        }
        catch (AuthenticationException ex) when(ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            Console.WriteLine(ex);
            throw;
        }
        catch (AuthenticationException ex)
        {
            Console.WriteLine(ex);
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine(ex);
        }
    }

    
}