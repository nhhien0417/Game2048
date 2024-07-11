using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppLovinMax;
using System;

public class AppLovinManager : Singleton<AppLovinManager>
{
    private string _sdkKey = "uKIZcZMfN-PVr0eB-9d0cGgTRz6udPAWlT5L1R_PQPvFPXQOh3OIJ4rl1xdfKpB4GGPHiOg7pZMC6obos-4A2w";
    private string _bannerAdUnitID = "99c9d89acaae5537";
    private string _interstitialAdUnitID = "e5d4799c480289c4";
    private string _rewardedAdUnitId = "4a7e24ed5ab3fe70";

    private int _retryAttempt;

    public void Initialize()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            InitializeInterstitialAd();
            InitializeBannerAd();
            InitializeRewardedAd();
        };

        MaxSdk.SetSdkKey(_sdkKey);
        MaxSdk.InitializeSdk();
    }

    #region BannerAd
    public void ShowBannerAd()
    {
        MaxSdk.ShowBanner(_bannerAdUnitID);
    }
    private void InitializeBannerAd()
    {
        MaxSdk.CreateBanner(_bannerAdUnitID, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerBackgroundColor(_bannerAdUnitID, Color.black);
    }
    #endregion

    #region InterstitialAd
    public void InitializeInterstitialAd()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

        LoadInterstitialAd();
    }

    private void LoadInterstitialAd()
    {
        MaxSdk.LoadInterstitial(_interstitialAdUnitID);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        _retryAttempt = 0;
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        _retryAttempt++;
        var retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));

        Invoke("LoadInterstitial", (float)retryDelay);
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        LoadInterstitialAd();
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadInterstitialAd();
    }

    public void ShowInterstitialAd()
    {
        if (MaxSdk.IsInterstitialReady(_interstitialAdUnitID))
        {
            MaxSdk.ShowInterstitial(_interstitialAdUnitID);
        }
    }
    #endregion

    #region RewardedAd
    public void InitializeRewardedAd()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(_rewardedAdUnitId);
    }

    private void OnRewardedLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        _retryAttempt = 0;
    }

    private void OnRewardedLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        _retryAttempt++;
        var retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));

        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        LoadRewardedAd();
    }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        GameManager.Instance.RemainingUndos = GameManager.Instance.RewardedUndos;
        TileBoard.Instance.UndoRemainingText.text = GameManager.Instance.RemainingUndos.ToString();
    }

    public void ShowRewardedAd()
    {
        if (MaxSdk.IsRewardedAdReady(_rewardedAdUnitId))
        {
            MaxSdk.ShowRewardedAd(_rewardedAdUnitId);
        }
    }
    #endregion
}
