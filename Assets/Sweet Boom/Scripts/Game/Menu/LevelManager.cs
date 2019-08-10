using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;


public class LevelManager : MonoBehaviour {

    public static string saveFolderName = "/gamedata.txt";
    [Header("Do not touch this fields")]
    private static GameObject[] levels;
    public static bool canSelectLevel;
    public GameObject[] star;
    public GameObject levelInfo;
    public TextMeshProUGUI levelNumTxt;
    [SerializeField] private GameObject levelInfoAnim;
    [SerializeField] private TextMeshProUGUI coinBalance;
    [SerializeField] private GameObject buyCoinsPanel;
    [SerializeField] private GameObject buyCoinsAnim;
    [SerializeField] private GameObject shopPanel, energyPanel, energyPanelAnim;
    [SerializeField] private GameObject shopPanelAnim;
    [SerializeField] private GameObject alertBox, shopManager;
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private GameObject levelFieldGameobject;
    [SerializeField] public TextMeshProUGUI debugGUI;
    [SerializeField] public TextMeshProUGUI energyCountTxt;
    [SerializeField] public Sprite disabledAddButtonSprite, enabledAddButtonSprite;
    [SerializeField] public Button addEnergyButton;
    public static Save.ConfigurationSettings configuration;
    static bool isInit;
    static LevelManager lvlManagerScript;

    private Menu.FaderMethods function;
    private static int curLevel;
    private static bool btnAlertPressed = false;
    private static int timeToRenewEnergy;

    public int EnergyTxt
    {
        set
        {
            if (Save.EnergyCount > 0)
            {
                energyCountTxt.text = value.ToString();
                addEnergyButton.gameObject.GetComponent<Image>().sprite = disabledAddButtonSprite;
                addEnergyButton.interactable = false;
            }
            else
            {
                energyCountTxt.text = "";
                addEnergyButton.gameObject.GetComponent<Image>().sprite = enabledAddButtonSprite;
                addEnergyButton.interactable = true;
            }
        }
    }

    #region SingletonPattern
    public static LevelManager Instance { get; private set; }
    private void SingletonPatternInit()
    {
        if (Instance == null)
            Instance = this;
    }
    #endregion

    #region Clouds
    private CloudMoves[] clouds;
    [SerializeField] private GameObject cloudsParentObject;
    private float levelsFieldSize;
    #endregion

    private void Awake()
    {
        SingletonPatternInit();
        Save.Init();
        UpdateLevels();
        Save.saveData.LevelsCollisionRemove();
        canSelectLevel = true;
        Save.saveData.uiUpdate += () => { coinBalance.text = Save.saveData.Coins.ToString(); };
        Save.EnergyRenewal.Tick += TimerTick;
        CloudsSetup();
    }

    private void Start()
    {
        Advert.ShowAdvertisementInterstitial(Advert.AdConfig.ShowPlace.afterMenuLoad);
        UpdateUI();
    }

    private void OnDisable()
    {
        Save.EnergyRenewal.Tick -= TimerTick;
    }

    private void Update()
    {
        for(int i = 0; i < clouds.Length; i++)
        {
            if (clouds[i].left && Mathf.Abs(clouds[i].rect.anchoredPosition.x) < 600)
            {
                clouds[i].rect.anchoredPosition = new Vector2(clouds[i].rect.anchoredPosition.x + clouds[i].speed * Time.deltaTime, clouds[i].rect.anchoredPosition.y);
            }
            else if (!clouds[i].left && Mathf.Abs(clouds[i].rect.anchoredPosition.x) < 600)
            {
                clouds[i].rect.anchoredPosition = new Vector2(clouds[i].rect.anchoredPosition.x - clouds[i].speed * Time.deltaTime, clouds[i].rect.anchoredPosition.y);
            }
            if (Mathf.Abs(clouds[i].rect.anchoredPosition.x) >= 600)
            {
                clouds[i].left = !clouds[i].left;
                clouds[i].rect.anchoredPosition = clouds[i].left == true ? new Vector2(-599, clouds[i].rect.anchoredPosition.y) : new Vector2(599, clouds[i].rect.anchoredPosition.y);
            }
        }
    }

    public void TimerTick(int time)
    {
        if (Save.EnergyCount < 1)
        {
            energyCountTxt.text = time % 60 > 9 ? $"{time / 60}:{time % 60}" : $"{time / 60}:0{time % 60}";
        }
    }

    private void CloudsSetup()
    {
        const int cloudYInterval = 400;
        levelsFieldSize = levelFieldGameobject.GetComponent<RectTransform>().offsetMax.y + 1000;
        cloudsParentObject.GetComponent<RectTransform>().offsetMax = new Vector2(cloudsParentObject.GetComponent<RectTransform>().offsetMax.x, -(levelsFieldSize - 1000));
        int cloudsCount = (int)Math.Ceiling(levelsFieldSize / cloudYInterval), cloudYPos = 0;
        bool left = true;
        clouds = new CloudMoves[cloudsCount];
        for(int i = 0; i < cloudsCount; i++)
        {
            left = !left;
            clouds[i] = new CloudMoves(Instantiate(cloudPrefab, new Vector3(), Quaternion.identity), cloudsParentObject, left, cloudYPos, UnityEngine.Random.Range(30, 70));
            cloudYPos += (int)cloudYInterval;
        }
    }

    public void UpdateUI()
    {
        Debug.Log("Update UI");
        coinBalance.text = Save.saveData.Coins.ToString();
        EnergyTxt = Save.EnergyCount;
        UpdateLevels();
    }
    public static void UpdateLevels()
    {
        try
        {
            levels = GameObject.FindGameObjectsWithTag("Level");
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i].transform.Find("#LevelNumber").GetComponent<TextMeshProUGUI>().text =
                    Save.gameData.levels[i].levelNum.ToString();
                switch(Save.saveData.levels[i].status)
                {
                    case Save.LevelStatus.enabled:
                        levels[i].transform.Find("#LevelComplete").gameObject.SetActive(false);
                        levels[i].transform.Find("#LevelOpen").gameObject.SetActive(true);
                        levels[i].GetComponent<LevelController>().SetStars(Save.saveData.levels[i].stars);
                        break;
                    case Save.LevelStatus.locked:
                        levels[i].transform.Find("#LevelComplete").gameObject.SetActive(false);
                        levels[i].transform.Find("#LevelOpen").gameObject.SetActive(false);
                        levels[i].GetComponent<LevelController>().SetStars(0);
                        levels[i].GetComponent<BoxCollider2D>().enabled = false;
                        break;
                    case Save.LevelStatus.completed:
                        levels[i].transform.Find("#LevelComplete").gameObject.SetActive(true);
                        levels[i].transform.Find("#LevelOpen").gameObject.SetActive(true);
                        levels[i].GetComponent<LevelController>().SetStars(Save.saveData.levels[i].stars);
                        break;
                }
                levels[i].GetComponent<LevelController>().levelID = Save.gameData.levels[i].levelNum;
            }
        }
        catch(Exception e)
        {
            Debug.Log($"[Sweet Boom Editor] Error: {e.ToString()}");
        }
        // Set star progress ...
    }

    public void LevelSelected(int levelNum, int starsCount)
    {
        SoundController.PlaySound(SoundController.SoundType.clickOpen);
        curLevel = levelNum;
        levelInfo.gameObject.SetActive(true);
        levelNumTxt.text = $"Level {levelNum}";
        for (int i = 0; i < 3; i++)
        {
            if(i < starsCount) star[i].gameObject.SetActive(true);
            else star[i].gameObject.SetActive(false);
        }
        levelInfoAnim.GetComponent<Animator>().SetTrigger("open");
    }
    public void ButtonClick()
    {
        switch (EventSystem.current.currentSelectedGameObject.name)
        {
            case "#exit":
                levelInfo.gameObject.SetActive(false);
                buyCoinsPanel.gameObject.SetActive(false);
                shopPanel.gameObject.SetActive(false);
                alertBox.gameObject.SetActive(false);
                energyPanel.gameObject.SetActive(false);
                canSelectLevel = true;
                SoundController.PlaySound(SoundController.SoundType.clickClose);
                break;
            case "#CoinsBack":
                OpenCoinMenu();
                canSelectLevel = false;
                SoundController.PlaySound(SoundController.SoundType.clickOpen);
                break;
            case "#shop_btn":
                shopPanel.gameObject.SetActive(true);
                shopPanelAnim.GetComponent<Animator>().SetTrigger("open");
                canSelectLevel = false;
                SoundController.PlaySound(SoundController.SoundType.clickOpen);
                break;
            case "#alert_click":
                btnAlertPressed = true;
                SoundController.PlaySound(SoundController.SoundType.clickOpen);
                break;
            case "#EnergyBack":
                energyPanel.gameObject.SetActive(true);
                energyPanelAnim.GetComponent<Animator>().SetTrigger("open");
                SoundController.PlaySound(SoundController.SoundType.clickOpen);
                canSelectLevel = false;
                break;
            case "#rewarded_video":
                
                break;
        }
    }
    public void OpenCoinMenu()
    {
        levelInfo.gameObject.SetActive(false);
        buyCoinsPanel.gameObject.SetActive(false);
        shopPanel.gameObject.SetActive(false);
        alertBox.gameObject.SetActive(false);
        canSelectLevel = false;
        buyCoinsPanel.gameObject.SetActive(true);
        buyCoinsAnim.GetComponent<Animator>().SetTrigger("open");
    }
    public IEnumerator MessageBox(string message, string buttonMessage, Action act)
    {
        alertBox.gameObject.SetActive(true);
        alertBox.transform.Find("#Popup").GetComponent<Animator>().SetTrigger("open");
        alertBox.transform.Find("#Popup").transform.Find("#Message").transform.Find("#txt").GetComponent<TextMeshProUGUI>().text = $"{message}";
        alertBox.transform.Find("#Popup").transform.Find("#alert_click").transform.Find("#but_txt").GetComponent<TextMeshProUGUI>().text = $"{buttonMessage}";
        yield return new WaitUntil(() => btnAlertPressed);
        SoundController.PlaySound(SoundController.SoundType.clickOpen);
        btnAlertPressed = false;
        alertBox.gameObject.SetActive(true);
        act?.Invoke();
    }
    public void Play()
    {
        if(Save.EnergyCount > 0)
        {
            Save.EnergyCount--;
            SoundController.PlaySound(SoundController.SoundType.clickOpen);
            function = () =>
            {
                SceneConnect.OpenScene(curLevel);
                SceneManager.LoadScene(1);
            };
            StartCoroutine(Menu.Fader(function, Menu.FaderFunctions.toBlack));
        }
        else
        {
            energyPanel.gameObject.SetActive(true);
            energyPanelAnim.GetComponent<Animator>().SetTrigger("open");
            SoundController.PlaySound(SoundController.SoundType.clickOpen);
        }
    }
}

public struct CloudMoves
{
    private GameObject cloud_;
    public GameObject cloud
    {
        get
        {
            return cloud_;
        }
        set
        {
            cloud = value;
            rect = value.GetComponent<RectTransform>();
        }
    }
    public RectTransform rect { get; set; }
    public bool left { get; set; }
    public float speed { get; set; }

    public CloudMoves(GameObject cloud, GameObject parent, bool left, float yPos, float speed)
    {
        this.cloud_ = cloud;
        this.left = left;
        cloud.transform.parent = parent.transform;
        rect = cloud.GetComponent<RectTransform>();
        rect.anchoredPosition = left == true ? new Vector2(UnityEngine.Random.Range(-600, -150), yPos) : new Vector2(UnityEngine.Random.Range(150, 600), yPos);
        rect.localScale = new Vector2(1, 1);
        this.speed = speed;
    }
}
