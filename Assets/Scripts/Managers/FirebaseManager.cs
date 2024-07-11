using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Extensions;
using Firebase;
using Firebase.Analytics;
using Firebase.RemoteConfig;
using Firebase.Messaging;
using Firebase.Crashlytics;
using System.Threading.Tasks;

public class FirebaseManager : Singleton<FirebaseManager>
{
    public void Initialize()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                FirebaseApp app = FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.

                // When this property is set to true, Crashlytics will report all
                // uncaught exceptions as fatal events. This is the recommended behavior.

                InitializeRemoteConfig();
                InitializeCrashlytics();
                InitializeCloudMessaging();
            }
            else
            {
                Debug.LogError(String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    #region RemoteConfig
    public Task InitializeRemoteConfig()
    {
        Debug.Log("Fetching data...");
        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);

        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }

    private void FetchComplete(Task fetchTask)
    {
        if (!fetchTask.IsCompleted)
        {
            Debug.LogError("Retrieval hasn't finished.");

            return;
        }

        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;

        if (info.LastFetchStatus != LastFetchStatus.Success)
        {
            Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");

            return;
        }

        remoteConfig.ActivateAsync().ContinueWithOnMainThread(
            task =>
            {
                Debug.Log($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");

                GameManager.Instance.RewardedUndos = (int)remoteConfig.GetValue("RVUndos").LongValue;
                GameManager.Instance.InitialUndos = (int)remoteConfig.GetValue("InitialUndos").LongValue;
                SettingsManager.Instance.ThemeFetch = remoteConfig.GetValue("Theme").StringValue;

                GameManager.Instance.IsFetched = true;
            }
        );
    }
    #endregion

    #region Analytics
    public void TapSettingsEvent()
    {
        FirebaseAnalytics.LogEvent("tap_settings");
    }

    public void TapNewGameEvent(int moves)
    {
        FirebaseAnalytics.LogEvent("tap_new_game", "moves", moves);
    }

    public void GameOverEvent(int moves, float time, string theme, string highestTile)
    {
        Parameter[] _gameOver = {   new Parameter("moves", moves),
                                    new Parameter("time", time),
                                    new Parameter("theme", theme),
                                    new Parameter("highest_tile", highestTile)
                                };

        FirebaseAnalytics.LogEvent("game_over", _gameOver);
    }
    #endregion

    #region CloudMessaging
    public void InitializeCloudMessaging()
    {
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }
    #endregion

    #region Crashlytics
    public void InitializeCrashlytics()
    {
        Crashlytics.ReportUncaughtExceptionsAsFatal = true;
    }
    #endregion
}
