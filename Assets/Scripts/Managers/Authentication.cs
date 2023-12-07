using UnityEngine;
using Assets.Scripts;
using System.Threading.Tasks;
using System;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#elif UNITY_IOS
using Apple.GameKit;
#endif

using Unity.Services.Authentication;
using Unity.Services.Core;
using Assets.Scripts.Events;

public class Authentication : BaseBehaviour
{
    private string authCode;
    string Signature;
    string TeamPlayerID;
    string Salt;
    string PublicKeyUrl;
    ulong Timestamp;

#if UNITY_ANDROID
    private async Task LoginGooglePlayGamesAsync()
    {
        PlayGamesPlatform.Activate();
        var tcs = new TaskCompletionSource<bool>();

        PlayGamesPlatform.Instance.Authenticate(success =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Authorization code: " + code);
                    authCode = code;
                    tcs.SetResult(true);
                });
            }
            else
            {
                Debug.Log("Failed to retrieve Google play games authorization code");
                Debug.Log("Login Unsuccessful");
                tcs.SetResult(false);
            }
        });

        // Wait for the authentication process to complete
        bool success = await tcs.Task;
        if (!success)
        {
            // Handle the authentication failure
            Debug.Log("Google Play Games authentication failed.");
        }
    }
#elif UNITY_IOS
    public async Task LoginGameCenterAsync()
    {
        if (!GKLocalPlayer.Local.IsAuthenticated)
        {
            // Perform the authentication.
            var player = await GKLocalPlayer.Authenticate();
            Debug.Log($"GameKit Authentication: player {player}");

            // Grab the display name.
            var localPlayer = GKLocalPlayer.Local;
            Debug.Log($"Local Player: {localPlayer.DisplayName}");

            // Fetch the items.
            var fetchItemsResponse = await GKLocalPlayer.Local.FetchItems();

            Signature = Convert.ToBase64String(fetchItemsResponse.Signature);
            TeamPlayerID = localPlayer.TeamPlayerId;
            Debug.Log($"Team Player ID: {TeamPlayerID}");

            Salt = Convert.ToBase64String(fetchItemsResponse.Salt);
            PublicKeyUrl = fetchItemsResponse.PublicKeyUrl;
            Timestamp = fetchItemsResponse.Timestamp;

            Debug.Log($"GameKit Authentication: signature => {Signature}");
            Debug.Log($"GameKit Authentication: publickeyurl => {PublicKeyUrl}");
            Debug.Log($"GameKit Authentication: salt => {Salt}");
            Debug.Log($"GameKit Authentication: Timestamp => {Timestamp}");
        }
        else
        {
            Debug.Log("AppleGameCenter player already logged in.");
        }
    }
#endif

    private async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    private async Task LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Link is successful.");
            await EventsService.UserSignUpAsync(UserSignUp.Google);
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message
            Debug.Log("This user is already linked with another account. Signing in instead.");
        }

        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    async Task SignInWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL, string salt, ulong timestamp)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(signature, teamPlayerId, publicKeyURL, salt, timestamp);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    async Task LinkWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL, string salt, ulong timestamp)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithAppleGameCenterAsync(signature, teamPlayerId, publicKeyURL, salt, timestamp);
            Debug.Log("Link is successful.");
            await EventsService.UserSignUpAsync(UserSignUp.Apple);
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async Task InitializeAsync()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        await LoginGooglePlayGamesAsync();
#elif UNITY_IOS && !UNITY_EDITOR
        await LoginGameCenterAsync();
#endif
        await SignInPlayerAsync();
    }

    public async Task SignInPlayerAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            return;

        bool hasToken = false;
        ;
#if UNITY_ANDROID
        hasToken = !string.IsNullOrEmpty(authCode);
#elif UNITY_IOS
        hasToken = !string.IsNullOrEmpty(Signature);
#endif

        if (hasToken)
        {
#if UNITY_ANDROID
            await SignInWithGooglePlayGamesAsync(authCode);
#elif UNITY_IOS
            await SignInWithAppleGameCenterAsync(Signature, TeamPlayerID, PublicKeyUrl, Salt, Timestamp);
#endif
        }
        else
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log($"Player id: {AuthenticationService.Instance.PlayerId}");
        await EventsService.UserUidAsync(AuthenticationService.Instance.PlayerId);
    }

    public async Task LinkPlayerAsync()
    {
#if UNITY_ANDROID
        await LinkWithGooglePlayGamesAsync(authCode);
#elif UNITY_IOS
        await LinkWithAppleGameCenterAsync(Signature, TeamPlayerID, PublicKeyUrl, Salt, Timestamp);
#endif
    }
}
