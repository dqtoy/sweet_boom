using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using CompleteProject;

public class Shop : MonoBehaviour
{
    [Header("LevelManager Script")] [SerializeField] private GameObject levelManager;
    [Header("Refill energy price")] [SerializeField] private int refillPrice;
    [Header("Rewarded video button sprites")] [SerializeField] private Sprite[] rewButtonSprites;
    [Header("RewVideo button")] [SerializeField] private Button rewVideoButton;
    [SerializeField] private TextMeshProUGUI coinBalanceTxt;
    [SerializeField] private Blocks[] boosters;
    [SerializeField] private GameObject coinSpawnParent, coinStoreItemPrefab, boosterParent, boosterStoreItemPrefab, coinStoreImageScaler, refillTxt;
    [HideInInspector] private TextMeshProUGUI[] boostersCountText;
    [SerializeField] private IAPManager purchaseManager;

    public static bool rewardedVideoActivity { get; private set; }

    [HideInInspector] public int CoinBalanceUI 
    { 
        get
        {
            try { return Convert.ToInt32(coinBalanceTxt.text); }
            catch { Debug.LogError("[Sweet Boom Runtime] Can't convert coin balance to int."); return 0; }
        }
        set { coinBalanceTxt.text = value.ToString(); }
    }
    private static bool isShopInited = false;

    private void Start() 
    {
        Advert.InitAdvertisement();
        InitShop();
        InitRewardedVideoButton();
    }

    /// <summary>
    /// [EN] Loop coroutine which calls function with certain delay
    /// [RU] Корутина вызывающая переданный метод с определенной задержкой
    /// </summary>
    /// <param name="func">Function to call</param>
    /// <param name="delay">Delay (seconds)</param>
    /// <returns></returns>
    public IEnumerator Loop(Action func, int delay)
    {
        while (this.enabled)
        {
            if (func == null)
                break;
            func?.Invoke();
            yield return new WaitForSeconds(delay);
        }
    }

    /// <summary>
    /// [EN] Called when user successfully complete rewarded video
    /// [RU] Вызывается при успешном завершении просмотра вознаграждаемой рекламы пользователем
    /// </summary>
    public static void RewardedVideoSucceeded()
    {
        AddCoins(Save.gameData.settings.adConfig.rewardedVideoOpt.rewardedAdvertisementReward);
    }

    /// <summary>
    /// [EN] Method for adding coins
    /// [RU] Метод для добавления внутриигровой валюты
    /// </summary>
    /// <param name="amount">Amount of coins</param>
    public static void AddCoins(int amount)
    {
        Save.saveData.Coins += amount;
        SoundController.PlaySound(SoundController.SoundType.coins);
    }

    public void RewardedVideoLoaded()
    {
        Debug.Log($"[Sweet Boom Editor] Ad loaded");
        if (Advert.isRewardedVideoEnabled)
        {
            rewVideoButton.gameObject.GetComponent<Image>().sprite = rewButtonSprites[0];
            rewVideoButton.gameObject.GetComponent<Button>().interactable = true;
        }
    }

    public void DisableRewardedVideo()
    {
        Debug.Log($"[Sweet Boom Editor] Button disabled");
        if (!Advert.isRewardedVideoEnabled)
        {
            rewVideoButton.gameObject.GetComponent<Image>().sprite = rewButtonSprites[1];
            rewVideoButton.gameObject.GetComponent<Button>().interactable = false;
        }
    }

    private void InitRewardedVideoButton()
    {
        rewVideoButton.onClick.AddListener(new UnityEngine.Events.UnityAction(OnRewardedVideoButtonClick));
        if (Save.gameData.settings.adConfig.rewardedVideoOpt.enabled)
        {
            rewVideoButton.gameObject.GetComponent<Image>().sprite = rewButtonSprites[0];
            rewVideoButton.gameObject.GetComponent<Button>().interactable = true;
            rewardedVideoActivity = true;
            Advert.onUnityAdsRewardedLoaded += RewardedVideoLoaded;
            Advert.onAdMobRewardedLoaded += RewardedVideoLoaded;
            Advert.onVideoStackEmpty += DisableRewardedVideo;
            /*
            StartCoroutine(Loop(() => {
                Debug.Log($"[Sweet Boom Editor] Check advertisement");
                if (Advert.isRewardedVideoEnabled)
                {
                    rewVideoButton.gameObject.GetComponent<Image>().sprite = rewButtonSprites[1];
                }
                else
                {
                    rewVideoButton.gameObject.GetComponent<Image>().sprite = rewButtonSprites[0];
                }
            }, 5));
            */
        }
        else
        {
            rewardedVideoActivity = false;
            rewVideoButton.gameObject.SetActive(false);
        }
    }

    public void InitShop()
    {
        IAPManager.onPurchaseSuccess += onPurchase;
        IAPManager.onPurchaseFailed += onPurchaseFailed;
        refillTxt.GetComponent<TextMeshProUGUI>().text = refillPrice.ToString();
        try
        {
            foreach (RectTransform child in boosterParent.GetComponent<RectTransform>()) Destroy(child.gameObject);
            foreach (RectTransform child in coinSpawnParent.GetComponent<RectTransform>()) Destroy(child.gameObject);
        }
        catch
        {
            Debug.Log("error");
        }

        int posY = -100, count = 0;
        boostersCountText = new TextMeshProUGUI[boosters.Length];
        for (int i = 0; i < boosters.Length; i++)
        {
            GameObject instItem = Instantiate(boosterStoreItemPrefab, boosterParent.transform);
            instItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(instItem.GetComponent<RectTransform>().anchoredPosition.x, posY);
            instItem.transform.Find("#description").GetComponent<TextMeshProUGUI>().text = boosters[i].nameOfItem.ToString();
            instItem.transform.Find("#pic").GetComponent<Image>().sprite = boosters[i].itemSprite;
            instItem.transform.Find("#button").transform.Find("#price").GetComponent<TextMeshProUGUI>().text = boosters[i].priceInStore.ToString();
            boostersCountText[i] = instItem.transform.Find("#pic").gameObject.transform.Find("#count").GetComponent<TextMeshProUGUI>();
            boostersCountText[i].text = Save.saveData.boostersCount[i].ToString();
            instItem.GetComponent<ShopItem>().itemIndex = i;
            instItem.GetComponent<ShopItem>().itemPrice = boosters[i].priceInStore;
            instItem.transform.Find("#button").gameObject.GetComponent<Button>().onClick.
                AddListener(new UnityEngine.Events.UnityAction(() => { BuyBooster(instItem.GetComponent<ShopItem>().itemIndex); }));

            posY -= 150; count++;
            if (count > 4) coinSpawnParent.GetComponent<RectTransform>().offsetMin =
                     new Vector2(0, coinSpawnParent.GetComponent<RectTransform>().offsetMin.y - instItem.GetComponent<RectTransform>().sizeDelta.y * 1.2f);
        }
        
        CoinBalanceUI = Save.saveData.Coins;
        
        GameObject[] old = GameObject.FindGameObjectsWithTag("CoinShopItem") as GameObject[];
        foreach (var p in old) Destroy(p.gameObject);
        posY = -100;
        count = 0;
        float count2 = 0;
        for (int i = 0; i < Save.gameData?.settings?.shopItems?.Count; i++) 
        {
            GameObject obj = Instantiate(coinStoreItemPrefab, coinSpawnParent.transform) as GameObject;
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(obj.GetComponent<RectTransform>().anchoredPosition.x, posY);
            obj.transform.Find("#description").gameObject.GetComponent<TextMeshProUGUI>().text = 
                $"+ {Save.gameData.settings.shopItems[i].coinRew.ToString()}";
            obj.transform.Find("#button").Find("#price").gameObject.GetComponent<TextMeshProUGUI>().text = 
                $"{Save.gameData.settings.shopItems[i].price}$";
            //obj.GetComponent<ShopItem>().itemPrice = (float)Convert.ToDouble(Save.gameData.settings.shopItems[i].price);
            obj.GetComponent<ShopItem>().itemReward = Save.gameData.settings.shopItems[i].coinRew;
            obj.GetComponent<ShopItem>().itemIndex = i;
            obj.transform.Find("#button").gameObject.GetComponent<Button>().onClick.
                AddListener(new UnityEngine.Events.UnityAction(() => { BuyCoins(obj.GetComponent<ShopItem>().itemIndex); }));
            posY -= 150; count++;
            if (count > 4)
            {
                count2 -= obj.GetComponent<RectTransform>().sizeDelta.y * 1.15f;
                coinSpawnParent.GetComponent<RectTransform>().offsetMin = new Vector2(0, count2);
            }
        }
    }

    public void BuyBooster(int id)
    {
        SoundController.PlaySound(SoundController.SoundType.clickOpen);
        if (Save.saveData.Coins >= boosters[id].priceInStore)
        {
            SoundController.PlaySound(SoundController.SoundType.coins);
            Save.saveData.boostersCount[id]++;
            Save.saveData.Coins -= boosters[id].priceInStore;

            for (int i = 0; i < Save.saveData.boostersCount.Length; i++)
                boostersCountText[i].text = Save.saveData.boostersCount[i].ToString();
        }
        else
        {
            StartCoroutine(levelManager.GetComponent<LevelManager>().
                MessageBox("Not enough money!", "Coin Store", () => { levelManager.GetComponent<LevelManager>().OpenCoinMenu(); }));
        }
    }

    public void BuyCoins(int id)
    {
        foreach (var cell in IAPManager.coins)
        {
            if (cell.unityIAPId == Save.configuration.shopItems[id].unityIAPId && 
                cell.appStoreId == Save.configuration.shopItems[id].appStoreId &&
                cell.googlePlayId == Save.configuration.shopItems[id].googlePlayId &&
                cell.coinReward == Save.configuration.shopItems[id].coinRew)
            {
                Debug.Log($"Shop -> BuyCoins id: {id}");
                purchaseManager.BuyConsumable(cell.unityIAPId);
            }
        }
    }

    public void RefillEnergy()
    {
        if (Save.saveData.energy < 1)
        {
            SoundController.PlaySound(SoundController.SoundType.clickOpen);
            if (Save.saveData.Coins >= refillPrice)
            {
                Save.saveData.energy = 5;
                SoundController.PlaySound(SoundController.SoundType.coins);
                Save.saveData.Coins -= refillPrice;
                levelManager.GetComponent<LevelManager>().UpdateUI();
            }
            else StartCoroutine(levelManager.GetComponent<LevelManager>().
                    MessageBox("Not enough money!", "Coin Store", () => { levelManager.GetComponent<LevelManager>().OpenCoinMenu(); }));
        }
    }

    public static void onPurchase(string id)
    {
        Debug.Log($"Purchase successful. ID: {id}");
        foreach (var coinShopSlot in Save.configuration.shopItems)
        {
            if (coinShopSlot.unityIAPId == id)
            {
                Save.saveData.Coins += coinShopSlot.coinRew;
                SoundController.PlaySound(SoundController.SoundType.coins);
            }
        }
    }

    private void OnRewardedVideoButtonClick()
    {
        if (!rewardedVideoActivity)
            return;
        Advert.ShowRewardedVideo();
        // TODO: сделать вознаграждаемую рекламу!!
    }

    public static void onPurchaseFailed()
    {
        Debug.LogError($"[Sweet Boom Editor] Purchase failed!");
    }

    [Serializable]
    public class ShopItemList
    {
        public GameObject itemInScene;
        [HideInInspector] public Sprite image;
        [HideInInspector] public TextMeshProUGUI description, price;
    }
    [Serializable]
    public class CoinShopItem
    {
        public int coinRew;
        public string unityIAPId, googlePlayId, appStoreId, price;
    }
}
