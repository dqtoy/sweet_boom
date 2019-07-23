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
    private static bool isRewardedVideoEnabled_;
    public static CurrentPlatform platform { get; set; }
    public static event Action onRewardedVideoComplete, onRewardedVideoSkipped, onRewardedVideoFailed;

    public static bool isRewardedVideoEnabled
    {
        get
        {
            // TODO: create rewarded video interval
            return Advertisement.IsReady("rewardedVideo");
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
                if (adConfig.interstitial[0].enabled || adConfig.interstitial[1].enabled || adConfig.interstitial[2].enabled)
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
                }
                if (adConfig.rewardedVideoOpt.enabled)
                {
                    rewaredAd = RewardBasedVideoAd.Instance;
                    AdRequest request = new AdRequest.Builder().Build();
                    if (platform == CurrentPlatform.android)
                    {
                        // TODO: Вознаграждаемая реклама AdMob
                        rewaredAd.LoadAd(request, Save.gameData.settings.adConfig.adMobAndroidRewardedID);
                    }
                        
                }
            }
            isInit = true;
        }
    }
    public static void RefreshAdv()
    {
        try
        {
            if(adConfig.adMobEnable)
            {
                if(adConfig.banner.enabled)
                {
                    AdRequest req = new AdRequest.Builder().Build();
                    banner.LoadAd(req);
                }
                if(adConfig.interstitial[0].enabled || adConfig.interstitial[1].enabled || adConfig.interstitial[2].enabled)
                {
                    AdRequest req = new AdRequest.Builder().Build();
                    interstitialAd.LoadAd(req);
                }
            }
        }
        catch
        {
            Debug.Log("[Sweet Boom Ads] Error while refreshing.");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="place"></param>
    public static IEnumerable ShowAdvertisement(AdConfig.ShowPlace place)
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
                            try
                            {
                                if (Advertisement.isSupported) Advertisement.Show();
                            }
                            catch (Exception ex)
                            {
                                Debug.Log($"[Sweet Boom Ads] Error while trying to display unity ads ({ex.ToString()})");
                            }
                            break;
                        case AdServices.adMob:
                            // Admob code...
                            if (interstitialAd.IsLoaded()) interstitialAd.Show();
                            break;
                    }
                }
            }
        }
        else
        {
            Debug.Log("[Sweet Boom Editor] Advertisement not initialized");
            yield return null;
        }
    }
    
    public static void ShowRewardedVideo()
    {
        switch (Save.gameData.settings.adConfig.rewardedVideoOpt.serv)
        {
            case AdServices.unityAds:
                if (Advertisement.IsReady("rewardedVideo"))
                    Advertisement.Show("rewardedVideo", new ShowOptions() { resultCallback = RewardedVideoResult });
                break;
            case AdServices.adMob:

                break;
            case AdServices.both:
                break;
        }
        
    }

    public static void RewardedVideoResult(ShowResult result)
    {
        if (result == ShowResult.Finished)
        {
            Debug.Log("[Sweet Boom Editor] Rewarded video complete!");
            onRewardedVideoComplete();
        }
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
