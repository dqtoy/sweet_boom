using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using GoogleMobileAds.Api;
using System.IO;
using System;


#pragma warning disable 168

public static class Advert
{
    private static string unityAdsID;
    private static AdConfig adConfig;
    private static bool isInit = false;
    private static BannerView banner;
    private static InterstitialAd interstitialAd;
    private static RewardBasedVideoAd rewaredAd;
    private static AdRequest request;
    private static bool isRewardedVideoEnabled_;
    public static CurrentPlatform platform { get; set; }
    public static event Action onRewardedVideoComplete, onRewardedVideoSkipped, onRewardedVideoFailed;

    public static bool isRewardedVideoEnabled
    {
        get
        {
            // TODO: create rewarded video interval
            if (adConfig.rewardedVideoOpt.serv == AdServices.unityAds)
                return Advertisement.IsReady("rewardedVideo");
            else if (adConfig.rewardedVideoOpt.serv == AdServices.adMob)
                return rewaredAd.IsLoaded();
            else
                return rewaredAd.IsLoaded() || Advertisement.IsReady("rewardedVideo");
        }
        set
        {
            isRewardedVideoEnabled_ = true;
        }
    }

    public static void InitAdvertisement()
    {
#if UNITY_ANDROID
        platform = CurrentPlatform.android;
#elif UNITY_IOS
        platform = CurrentPlatform.ios;
#else
        platform = CurrentPlatform.undefined;
#endif
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Save.TimerDelegeate(() => { InitAdvertisement(); }, 15);
            return;
        }
        if (!isInit)
        {
            onRewardedVideoComplete += Shop.RewardedVideoSucceeded;
            adConfig = Save.configuration?.adConfig ?? AdConfig.SetDefaultConfig();
            if (adConfig.unityAdsEnable)
            {
                if (Advertisement.isSupported) Advertisement.Initialize(adConfig.unityAdsID);
            }
            if (adConfig.adMobEnable)
            {
                MobileAds.Initialize(Save.gameData.settings.adConfig.adMobID);
                if (adConfig.interstitial[0].serv == AdServices.adMob || adConfig.interstitial[1].serv == AdServices.adMob 
                    || adConfig.interstitial[2].serv == AdServices.adMob)
                {
                    if (platform == CurrentPlatform.android)
                        interstitialAd = new InterstitialAd(adConfig.adMobAndroidPictureID);
                    else if (platform == CurrentPlatform.ios)
                        interstitialAd = new InterstitialAd(adConfig.adMobIOSPictureID);
                }
                if (adConfig.banner.enabled)
                {
                    if (platform == CurrentPlatform.android)
                        banner = new BannerView(adConfig.adMobAndroidBannerID, AdSize.Banner, AdPosition.Bottom);
                    else if (platform == CurrentPlatform.ios)
                        banner = new BannerView(adConfig.adMobIOSBannerID, AdSize.Banner, AdPosition.Bottom);
                    AdRequest request = new AdRequest.Builder().Build();
                    banner.LoadAd(request);
                }
                if (adConfig.rewardedVideoOpt.enabled)
                {
                    rewaredAd = RewardBasedVideoAd.Instance;
                    AdRequest request = new AdRequest.Builder().Build();
                    rewaredAd.OnAdRewarded += AdMobRewardedVideoComplete;
                }
            }
            isInit = true;
        }
    }
    public static void RefreshAdv(AdServices serv)
    {
        if (adConfig.adMobEnable && (serv == AdServices.adMob || serv == AdServices.both))
        {
            /*
            if (adConfig.banner.enabled)
            {
                AdRequest req = new AdRequest.Builder().Build();
                banner.LoadAd(req);
            }
            */
            if ((adConfig.interstitial[0].enabled || adConfig.interstitial[1].enabled || adConfig.interstitial[2].enabled) &&
                (adConfig.interstitial[0].serv == AdServices.adMob || adConfig.interstitial[1].serv == AdServices.adMob ||
                        adConfig.interstitial[2].serv == AdServices.adMob))
            {
                AdRequest req = new AdRequest.Builder().Build();
                interstitialAd.LoadAd(req);
            }
            if (adConfig.rewardedVideoOpt.enabled)
            {
                if (adConfig.rewardedVideoOpt.serv == AdServices.adMob)
                {
                    if (platform == CurrentPlatform.android)
                        rewaredAd.LoadAd(request, Save.gameData.settings.adConfig.adMobAndroidRewardedID);
                    else if (platform == CurrentPlatform.ios)
                        rewaredAd.LoadAd(request, Save.gameData.settings.adConfig.adMobIOSRewardedID);
                    else
                        Debug.Log("[Sweet Boom Editor] Undefined platform (AdMob)");
                }
                else if (adConfig.rewardedVideoOpt.serv == AdServices.unityAds)
                {

                }
            }
        }
        else if (adConfig.unityAdsEnable && (serv == AdServices.unityAds || serv == AdServices.both))
        {
            // TODO: Unity ADs refresh...
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="place"></param>
    public static void ShowAdvertisementInterstitial(AdConfig.ShowPlace place)
    {
        if (isInit)
        {
            Debug.Log("[Sweet Boom Editor] Advertisement shows");
            foreach (var item in adConfig.interstitial)
            {
                if (item.showPlace == place)
                {
                    switch(item.serv)
                    {
                        case AdServices.unityAds:
                            if (Advertisement.isSupported)
                                Advertisement.Show();
                            break;
                        case AdServices.adMob:
                            if (interstitialAd.IsLoaded())
                                interstitialAd.Show();
                            RefreshAdv(AdServices.adMob);
                            break;
                    }
                }
            }
        }
        else
        {
            Debug.Log("[Sweet Boom Editor] Advertisement not initialized");
        }
    }
    
    public static void ShowRewardedVideo()
    {
        if (!isInit)
            return;
        switch (Save.gameData.settings.adConfig.rewardedVideoOpt.serv)
        {
            case AdServices.unityAds:
                if (Advertisement.IsReady("rewardedVideo"))
                    Advertisement.Show("rewardedVideo", new ShowOptions() { resultCallback = RewardedVideoResult });
                break;
            case AdServices.adMob:
                if (platform == CurrentPlatform.android)
                    rewaredAd.LoadAd(request, Save.gameData.settings.adConfig.adMobAndroidRewardedID);
                else if (platform == CurrentPlatform.ios)
                    rewaredAd.LoadAd(request, Save.gameData.settings.adConfig.adMobIOSRewardedID);
                else
                    Debug.Log("[Sweet Boom Editor] Undefined platform (AdMob)");
                break;
            case AdServices.both:
                if (UnityEngine.Random.Range(1, 2) == 1)
                {
                    if (Advertisement.IsReady())
                        Advertisement.Show("rewardedVideo", new ShowOptions() { resultCallback = RewardedVideoResult });
                    else if (rewaredAd.IsLoaded())
                        rewaredAd.Show();
                }
                else
                {
                    if (rewaredAd.IsLoaded())
                        rewaredAd.Show();
                    else if (Advertisement.IsReady())
                        Advertisement.Show("rewardedVideo", new ShowOptions() { resultCallback = RewardedVideoResult });
                }
                break;
        }
    }

    public static void RewardedVideoResult(ShowResult result)
    {
        if (result == ShowResult.Finished)
        {
            Debug.Log("[Sweet Boom Editor] Rewarded video complete! (Unity Ads)");
            onRewardedVideoComplete();
        }
    }

    public static void AdMobRewardedVideoComplete(object sender, EventArgs args)
    {
        Debug.Log("[Sweet Boom Editor] Rewarded video complete (AdMob)");
        onRewardedVideoComplete();
        RefreshAdv(AdServices.adMob);
    }

    public static void CleanMemory()
    {
        interstitialAd?.Destroy();
    }
    [Serializable]
    public class AdConfig
    {
        public bool unityAdsEnable, adMobEnable;
        public string unityAdsID, adMobID, adMobAndroidBannerID, adMobAndroidPictureID, adMobAndroidRewardedID;
        public string adMobIOSBannerID, adMobIOSPictureID, adMobIOSRewardedID;
        public ShowOpt[] interstitial = new ShowOpt[3];
        public BannerOpt banner;
        public RewardedVideoOptions rewardedVideoOpt;
        [Serializable]
        public class ShowOpt
        {
            public bool enabled;
            public ShowPlace showPlace;
            public AdServices serv;
            public int interval;
        }
        [Serializable]
        public class BannerOpt
        {
            public bool enabled;
            public AdServices serv;
        }
        [Serializable]
        public class RewardedVideoOptions
        {
            public bool enabled;
            public AdServices serv;
            public int rewardedAdvertisementReward;
        }
        public enum ShowPlace
        {
            afterLose,
            afterWin,
            afterMenuLoad
        }
        public static AdConfig SetDefaultConfig()
        {
            Debug.Log("[Sweet Boom Editor] Default config setted.");
            AdConfig conf = new AdConfig()
            {
                unityAdsEnable = false,
                adMobEnable = false,
                unityAdsID = "",
                adMobID = "",
                adMobAndroidBannerID = "",
                adMobAndroidPictureID = "",
                adMobAndroidRewardedID = "",
                adMobIOSBannerID = "",
                adMobIOSPictureID = "",
                adMobIOSRewardedID = "",
                banner = new BannerOpt()
                {
                    enabled = false,
                    serv = AdServices.both
                },
                interstitial = new ShowOpt[3]
                {
                    new ShowOpt()
                    {
                        enabled = false,
                        showPlace = ShowPlace.afterLose,
                        serv = AdServices.both,
                        interval = 3
                    },
                    new ShowOpt()
                    {
                        enabled = false,
                        showPlace = ShowPlace.afterWin,
                        serv = AdServices.both,
                        interval = 3
                    },
                    new ShowOpt()
                    {
                        enabled = false,
                        showPlace = ShowPlace.afterMenuLoad,
                        serv = AdServices.both,
                        interval = 3
                    }
                },
                rewardedVideoOpt = new RewardedVideoOptions()
                {
                    enabled = false,
                    serv = AdServices.both
                },
            };
            return conf;
        }
        
    }
    public enum AdServices
    {
        unityAds,
        adMob,
        both
    }

    public enum CurrentPlatform
    {
        android,
        ios,
        undefined
    }
}
