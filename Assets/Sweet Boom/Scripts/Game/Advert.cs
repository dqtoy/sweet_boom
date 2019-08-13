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
    private static bool testMode = true;

    private static string unityAdsID;
    private static AdConfig adConfig;
    private static bool isInit = false;
    private static BannerView banner;
    private static InterstitialAd interstitialAd;
    private static RewardBasedVideoAd rewaredAd;
    private static AdRequest request;
    private static bool isRewardedVideoEnabled_;

    public static int loadedVideoCount { get; private set; } = 0;

    public static event Action onUnityAdsRewardedLoaded;
    public static event Action onAdMobRewardedLoaded;
    public static event Action onVideoStackEmpty;

    private static UnityAdsHelper unityAdsManager;
    public static CurrentPlatform platform { get; set; }
    public static event Action onRewardedVideoComplete, onRewardedVideoSkipped, onRewardedVideoFailed;

    private static int[] intervals = new int[3] { 1, 1, 1 };

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

    /// <summary>
    /// [EN] Advertisement initialization
    /// [RU] Инициализация рекламы
    /// </summary>
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
            //Save.TimerDelegeate(() => { InitAdvertisement(); }, 15);
            return;
        }
        if (!isInit)
        {
            onRewardedVideoComplete += Shop.RewardedVideoSucceeded;
            onRewardedVideoComplete += VideoViewed;
            onUnityAdsRewardedLoaded += VideoLoaded;
            onAdMobRewardedLoaded += VideoLoaded;
            adConfig = Save.configuration?.adConfig ?? AdConfig.SetDefaultConfig();
            if (adConfig.unityAdsEnable)
            {
                if (Advertisement.isSupported && adConfig.unityAdsEnable && adConfig.unityAdsID != "")
                    Advertisement.Initialize(adConfig.unityAdsID, testMode);
                if (adConfig.rewardedVideoOpt.enabled && (adConfig.rewardedVideoOpt.serv == AdServices.unityAds || adConfig.rewardedVideoOpt.serv == AdServices.both))
                {
                    unityAdsManager = new UnityAdsHelper();
                    unityAdsManager.UnityAdsInit(adConfig.unityAdsID, adConfig, testMode);
                }
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
                    Debug.Log("Banner loaded");
                }
                if (adConfig.rewardedVideoOpt.enabled)
                {
                    rewaredAd = RewardBasedVideoAd.Instance;
                    AdRequest request = new AdRequest.Builder().Build();
                    RefreshAdv(AdServices.adMob);
                    rewaredAd.OnAdRewarded += AdMobRewardedVideoComplete;
                    rewaredAd.OnAdLoaded += RewaredAd_OnAdLoaded;
                }
            }
            isInit = true;
        }
    }

    private static void RewaredAd_OnAdLoaded(object sender, EventArgs e)
    {
        onUnityAdsRewardedLoaded();
    }

    /// <summary>
    /// [EN] Loading the necessary data for advertising
    /// [RU] Загрузка необходимых данных для рекламы 
    /// </summary>
    /// <param name="serv"></param>
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
                if (platform == CurrentPlatform.android)
                    rewaredAd.LoadAd(request, Save.gameData.settings.adConfig.adMobAndroidRewardedID);
                else if (platform == CurrentPlatform.ios)
                    rewaredAd.LoadAd(request, Save.gameData.settings.adConfig.adMobIOSRewardedID);
                else
                    Debug.Log("[Sweet Boom Editor] Undefined platform (AdMob)");
            }
        }
        else if (adConfig.unityAdsEnable && (serv == AdServices.unityAds || serv == AdServices.both))
        {
            // TODO: Unity ADs refresh...
        }
    }

    /// <summary>
    /// [EN] Show interstitial ad
    /// [RU] Показать межстраничное объявление
    /// </summary>
    /// <param name="place">Where we should show this ad</param>
    public static void ShowAdvertisementInterstitial(AdConfig.ShowPlace place)
    {
        if (isInit)
        {
            for (int i = 0; i < adConfig.interstitial.Length; ++i)
            {
                if (adConfig.interstitial[i].showPlace == place)
                {
                    switch (adConfig.interstitial[i].serv)
                    {
                        case AdServices.unityAds:
                            if (intervals[i] + 1 % adConfig.interstitial[i].interval == 0)
                                unityAdsManager.ShowInterstitial();
                            intervals[i]++;
                            break;
                        case AdServices.adMob:
                            if (intervals[i] + 1 % adConfig.interstitial[i].interval == 0)
                            {
                                if (interstitialAd.IsLoaded())
                                    interstitialAd.Show();
                                RefreshAdv(AdServices.adMob);
                            }
                            intervals[i]++;
                            break;
                        case AdServices.both:
                            if (intervals[i] + 1 % adConfig.interstitial[i].interval == 0)
                            {
                                if (UnityEngine.Random.Range(1, 2) == 1)
                                {
                                    if (unityAdsManager.ShowInterstitial())
                                        break;
                                    else
                                    {
                                        if (interstitialAd.IsLoaded())
                                            interstitialAd.Show();
                                        RefreshAdv(AdServices.adMob);
                                    }
                                }
                                else
                                {
                                    if (interstitialAd.IsLoaded())
                                    {
                                        interstitialAd.Show();
                                        RefreshAdv(AdServices.adMob);
                                    }
                                    else
                                        unityAdsManager.ShowInterstitial();
                                }
                            }
                            intervals[i]++;
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
        if (!isInit && !adConfig.rewardedVideoOpt.enabled)
            return;
        switch (Save.gameData.settings.adConfig.rewardedVideoOpt.serv)
        {
            case AdServices.unityAds:
                unityAdsManager.ShowRewardedVideo();
                break;
            case AdServices.adMob:
                if (rewaredAd.IsLoaded())
                {
                    rewaredAd.Show();
                    --loadedVideoCount;
                    RefreshAdv(AdServices.adMob);
                }
                break;
            case AdServices.both:
                if (UnityEngine.Random.Range(1, 2) == 1)
                {
                    if (unityAdsManager.ShowRewardedVideo())
                        break;
                    else if (rewaredAd.IsLoaded())
                    {
                        rewaredAd.Show();
                        --loadedVideoCount;
                        RefreshAdv(AdServices.adMob);
                    }
                }
                else
                {
                    if (rewaredAd.IsLoaded())
                    {
                        rewaredAd.Show();
                        --loadedVideoCount;
                        RefreshAdv(AdServices.adMob);
                    }
                    else if (unityAdsManager.ShowRewardedVideo())
                        break;
                }
                break;
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

    public static void VideoLoaded()
    {
        ++loadedVideoCount;
    }

    public static void VideoViewed()
    {
        --loadedVideoCount;
        if (loadedVideoCount < 1)
            onVideoStackEmpty();
    }

    public class UnityAdsHelper : IUnityAdsListener
    {
        private static string placement { get; set; } = "rewardedVideo";
        public string gameID { get; private set; }
        private AdConfig config { get; set; }

        public void UnityAdsInit(string gameID, AdConfig config, bool test)
        {
            this.gameID = gameID;
            this.config = config;
            if (config.unityAdsEnable)
            {
                Advertisement.Initialize(gameID, test);
                Advertisement.AddListener(this);
            }
        }

        public bool ShowRewardedVideo()
        {
            if (Advertisement.IsReady(placement))
            {
                --loadedVideoCount;
                Advertisement.Show(placement);
                return true;
            }
            else
                return false;
        }

        public void OnUnityAdsDidError(string message)
        {

        }

        public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        {
            if (showResult == ShowResult.Finished && placementId == placement)
            {
                Debug.Log("[Sweet Boom Editor] Rewarded video complete! (Unity Ads)");
                onRewardedVideoComplete();
            }
            else if (showResult == ShowResult.Failed && placementId == placement)
            {
                Debug.Log("[Sweet Boom Editor] Rewarded video failed! (Unity Ads)");
                onRewardedVideoFailed();
            }
            else if (showResult == ShowResult.Skipped && placementId == placement)
            {
                Debug.Log("[Sweet Boom Editor] Rewarded video skipped! (Unity Ads)");
                onRewardedVideoSkipped();
            }
        }

        public bool ShowInterstitial()
        {
            if (Advertisement.IsReady())
            {
                Advertisement.Show();
                return true;
            }
            else
                return false;
        }

        public void OnUnityAdsDidStart(string placementId)
        {

        }

        public void OnUnityAdsReady(string placementId)
        {
            onUnityAdsRewardedLoaded();
        }
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
