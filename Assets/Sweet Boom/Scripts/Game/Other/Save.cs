﻿#define DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using TMPro;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;

/// <summary>
/// <para>[EN] Class with save functions and some survice functions.</para>
/// <para>[RU] Класс предоставляющий функции сохранения, загрузки всех игровых данных, а также некоторые служебные функции.</para>
/// </summary>
public static class Save {
    
    public static GameSave saveData { get; set; } // player progress
    public static GameData gameData { get; private set; } // levels data
    public static LevelManager lvlManager { get; set; }
    public static Timer EnergyRenewal { get; set; }
    public static ConfigurationSettings configuration
    {
        get
        {
            return gameData.settings;
        }
        set
        {
            gameData.settings = value;
        }
    }
    static bool d = true;
    public static SoundController sound;
    private static string gameDataPath = "data.sb", saveFileName = "save.sb";
    public static Advert.CurrentPlatform curPlatform { get; set; }
    private static string currentSavePath = "";

    private static bool isCalled = false;
    public static int EnergyCount
    {
        get
        {
            return saveData.energy;
        }
        set
        {
            if (value < 5 && !EnergyRenewal.IsRunning)
            {
                Debug.Log($"Start timer first time. Delay: {gameData.settings.delayEnergy}");
                EnergyRenewal.StartTimer(gameData.settings.delayEnergy * 60);
            }
            if (value > 0 && value < 6)
            {
                saveData.energy = value;
            }
            else
            {
                saveData.energy = 0;
            }
        }
    }

    /// <summary>
    /// <para>[EN] Main initialization of the game </para>
    /// <para>[RU] Главная инициализация игры </para>
    /// </summary>
    public static void Init()
    {
#if UNITY_EDITOR
        curPlatform = Advert.CurrentPlatform.undefined;
#elif UNITY_ANDROID && !DEBUG
        curPlatform = Advert.CurrentPlatform.android;
#elif UNITY_IOS
        curPlatform = Advert.CurrentPlatform.ios;
#elif DEBUG || (!UNITY_ANDROID && !UNITY_IOS)
        curPlatform = Advert.CurrentPlatform.undefined;
#endif
        Application.quitting += SaveAllUserData;

        gameData = new GameData();
        InitGameData();

        saveData = new GameSave();
        InitSavedData();
        InitEnergy();
        //EnergyRenewal = new Timer(Save.gameData.settings.delayEnergy * 60);
    }

    /// <summary>
    /// [EN] Calls when user quit or pause application
    /// [RU] Вызывается при закрытии приложения
    /// </summary>
    public static void GameQuitOrPause()
    {
        saveData.closeTime = DateTime.Now;
        if (EnergyCount < 5)
            saveData.timeToRenewEnergy = Save.EnergyRenewal.Interval;
        else
            saveData.timeToRenewEnergy = -1;
        SaveAllUserData();
    }

    /// <summary>
    /// [EN] Energy section initialization
    /// [RU] Инициализация сектора энергии
    /// </summary>
    private static void InitEnergy()
    {
        if (isCalled)
            return;
        isCalled = true;
        EnergyRenewal = new Timer();
        EnergyRenewal.TimerEnd += TimeIsUp;

        if (saveData.timeToRenewEnergy >= 0)
        {
            int offTime = (int)(DateTime.Now - Save.saveData.closeTime).TotalSeconds;
            if (offTime >= 0)
            {
                if (offTime >= saveData.timeToRenewEnergy)
                {
                    offTime -= saveData.timeToRenewEnergy;
                    ++saveData.energy;
                    if (offTime > 0)
                    {
                        saveData.energy += offTime / (gameData.settings.delayEnergy * 60);
                        if (saveData.energy < 5)
                        {
                            EnergyRenewal.StartTimer(gameData.settings.delayEnergy * 60 - offTime % (gameData.settings.delayEnergy * 60));
                        }
                    }
                }
                else
                {
                    EnergyRenewal.StartTimer(saveData.timeToRenewEnergy - offTime);   
                }
            }
        }
        else if (saveData.energy < 5)
        {
            EnergyRenewal.StartTimer(Save.gameData.settings.delayEnergy * 60);
        }
        EnergyCount = saveData.energy;
    }

    private static void TimeIsUp()
    {
        if (Save.EnergyCount < 5)
        {
            // TODO: sDKfja;sldkfjm
            ++Save.EnergyCount;
            if (LevelManager.Instance != null && LevelManager.Instance.isActiveAndEnabled)
                LevelManager.Instance?.UpdateUI();
            EnergyRenewal.StartTimer(Save.gameData.settings.delayEnergy * 60);
        }
    }

    /// <summary>
    /// [EN] Initialization of game data (levels, ads configs etc.)
    /// [RU] Инициализация игровых данных (уровни, настройки рекламы и т.д.)
    /// </summary>
    /// <returns></returns>
    public static void InitGameData()
    {
        string fileData = "";
        Debug.Log($"{Application.streamingAssetsPath}");
        Debug.Log($"{Application.dataPath}");
        if (curPlatform == Advert.CurrentPlatform.undefined || curPlatform == Advert.CurrentPlatform.ios)
        {
            Debug.Log($"[Sweet Boom Editor] platform: {curPlatform}");
            string path = $"{Application.streamingAssetsPath}/{gameDataPath}";
            if (!File.Exists(path))
            {
                Debug.Log("Open Dudle/Level Editor " +
                "to start making levels.");
                gameData = GameData.SetDefualt();
            }
            else
            {
                fileData = File.ReadAllText(path);
                gameData = new GameData();
                gameData = JsonConvert.DeserializeObject<GameData>(fileData);
                gameData = gameData ?? GameData.SetDefualt();
                configuration = gameData.settings ?? ConfigurationSettings.SetDefaultConfig();
                if (configuration.sortingLevelsInMenu) // Level sorting
                {
                    for (int i = 0; i < gameData.levels.Count - 1; i++)
                    {
                        int min = int.MaxValue;
                        Level minLvl = new Level();
                        for (int j = i; j < gameData.levels.Count; j++)
                        {
                            if (gameData.levels[j].levelNum < min)
                            {
                                min = gameData.levels[j].levelNum;
                                Level l = gameData.levels[i];
                                gameData.levels[i] = gameData.levels[j];
                                gameData.levels[j] = l;
                            }
                        }
                    }
                }
            }
        }
        else if (curPlatform == Advert.CurrentPlatform.android)
        {
            CoroutineManager.CoroutineStart(InitGameDataAndroid());
        }
    }

    /// <summary>
    /// [EN] Loading data from 'streaming assets' folder
    /// [RU] Загрузка игровых данных из папки 'streaming assets'
    /// </summary>
    /// <returns></returns>
    public static IEnumerator InitGameDataAndroid()
    {
        string filePathAndroid = $"{Application.streamingAssetsPath}/{gameDataPath}";
        string readedData = "";
        if (filePathAndroid.Contains("jar:file://"))
        {
            UnityWebRequest www = UnityWebRequest.Get(filePathAndroid);
            yield return www.SendWebRequest();
            readedData = www.downloadHandler.text;
        }
        else // We are in Editor
        {
            if (File.Exists(filePathAndroid))
                readedData = File.ReadAllText(filePathAndroid);
        }
        gameData = new GameData();
        gameData = JsonConvert.DeserializeObject<GameData>(readedData);
        gameData = gameData ?? GameData.SetDefualt();
        configuration = gameData.settings ?? ConfigurationSettings.SetDefaultConfig();
        if (configuration.sortingLevelsInMenu) // Level sorting
        {
            for (int i = 0; i < gameData.levels.Count - 1; i++)
            {
                int min = int.MaxValue;
                Level minLvl = new Level();
                for (int j = i; j < gameData.levels.Count; j++)
                {
                    if (gameData.levels[j].levelNum < min)
                    {
                        min = gameData.levels[j].levelNum;
                        Level l = gameData.levels[i];
                        gameData.levels[i] = gameData.levels[j];
                        gameData.levels[j] = l;
                    }
                }
            }
        }
    }

    /// <summary>
    /// <para>[EN] Return a random int number between 'from' [inclusive] and 'to' [inclusive]</para>
    /// <para>[RU] Возвращает случаное значение типа int в заданном промежутке (включая передаваемые параметры)</para>
    /// </summary>
    /// <param name="from">beginning of range</param>
    /// <param name="to">end of range</param>
    /// <returns></returns>
    public static int Randomizer(int from, int to) // Randomizer ('from' and 'to' - inclusive)
    {
        float dist = (float)100 / (float)((to - from) + 1);
        while(true)
        {
            int result = UnityEngine.Random.Range(1, 101);
            for (int i = 0; i < ((to - from) + 1); i++)
                if (result > i * dist && result <= (i + 1) * dist)
                    return from + i;
        }
    }

    /// <summary>
    /// [EN] Invokes the delegate after the specified time has elapsed
    /// [RU] Вызывает переданный делегат по истечению указанного времени
    /// </summary>
    /// <param name="action">Invoked delegate</param>
    /// <param name="time">Time (in seconds)</param>
    /// <returns></returns>
    public static IEnumerator TimerDelegeate(Action action, int time)
    {
        yield return new WaitForSeconds(time * 1000);
        action?.Invoke();
    }

    /// <summary>
    /// [EN] 
    /// </summary>
    [Serializable]
    public class GameSave
    {
        public List<SaveSlot> levels;
        public event Action uiUpdate;
        public int coins_;

        public int energy, timeToRenewEnergy;
        public DateTime closeTime;
        public int[] boostersCount;
        public int bombBooster, searchBooster, multiCandyBooster;
        public GameSave()
        {
            var lvl = new SaveSlot(1, LevelStatus.enabled);
            levels = new List<SaveSlot>();
            boostersCount = new int[3];
            energy = 5;
            timeToRenewEnergy = -1;
            closeTime = DateTime.Now;
        }

        /// <summary>
        /// <para>[EN] Set completed level to 'complete' and open next level</para>
        /// <para>[RU] Помечает уровень как завершенный и открывает следующий</para>
        /// </summary>
        /// <param name="num">Number of completed level</param>
        /// <param name="score">Score of completed level</param>
        /// <param name="stars">Earned stars</param>
        public void UnlockNewLevel(int num, int score, int stars)
        {
            for(int i = 0; i < saveData.levels.Count; i++)
            {
                if (saveData.levels[i].levelNum == num)
                {
                    if (saveData.levels[i].status == LevelStatus.completed)
                    {
                        if (score > saveData.levels[i].score) saveData.levels[i].score = score;
                        if (stars > saveData.levels[i].stars) saveData.levels[i].stars = stars;
                    }
                    else if (saveData.levels[i].status == LevelStatus.enabled)
                    {
                        saveData.levels[i].status = LevelStatus.completed;
                        saveData.levels[i].score = score;
                        saveData.levels[i].stars = stars;
                        if (i + 1 < saveData.levels.Count && saveData.levels[i + 1].status == LevelStatus.locked)
                        {
                            saveData.levels[i + 1].status = LevelStatus.enabled;
                        }
                        else if (i + 2 > saveData.levels.Count)
                        {
                            Debug.Log("[Sweet Boom Editor] This is last level.");
                        }
                    }
                    else if (saveData.levels[i].status == LevelStatus.locked) Debug.LogError("[Sweet Boom Editor] Unknown error");
                }
            }
            SaveAllUserData();
        }

        public void LevelsCollisionRemove()
        {
            if (saveData.levels.Count > 0)
            {
                for (int i = 0; i < saveData.levels.Count; ++i)
                {
                    if (saveData.levels[i].levelNum == 1 && saveData.levels[i].status == LevelStatus.locked)
                    {
                        saveData.levels[i].status = LevelStatus.enabled;
                        SaveAllUserData();
                        return;
                    }
                }
            }
            for(int i = 0; i < saveData.levels.Count; i++)
            {
                if(saveData.levels[i].status == LevelStatus.locked && i - 1 >= 0 && saveData.levels[i - 1].status == LevelStatus.completed)
                {
                    saveData.levels[i].status = LevelStatus.enabled;
                }
            }
        }

        /// <summary>
        /// <para>[EN] Reset all game progress</para>
        /// <para>[RU] Удаляет весь пользовательский прогресс</para>
        /// </summary>
        public void ResetProgress()
        {
            saveData = new GameSave();
            RestorePlayerData();
        }

        [JsonIgnore]
        public int Coins
        {
            get { return coins_; }
            set
            {
                coins_ = Mathf.Clamp(value, 0, int.MaxValue);
                uiUpdate();
            }
        }
    }

    /// <summary>
    /// [EN] Save slot wich presents each level
    /// [RU] Класс представляющий каждый уровень в сохранении 
    /// </summary>
    [Serializable]
    public class SaveSlot
    {
        public int levelNum;
        public LevelStatus status;
        public int stars;
        public int score;

        public SaveSlot(int num, LevelStatus status) // Initialize new level
        {
            levelNum = num;
            this.status = status;
        }
        [JsonConstructor]
        public SaveSlot(int num, LevelStatus status, int score, int stars) : this(num, status) // Savecompleted level
        {
            this.score = score;
            this.stars = stars;
        }
    }

    public enum LevelStatus
    {
        locked,
        enabled,
        completed
    }

    /// <summary>
    /// <para>[EN] returns level by number </para>
    /// <para>[RU] Возвращает уровень с соответствующим номером </para>
    /// </summary>
    /// <param name="number">[EN] level number </param>
    public static SaveSlot FindByNumber(int number)
    {
        foreach (var item in saveData.levels) if(item.levelNum == number) return item;
        return null;
    }

    public static void RestorePlayerData()
    {
        saveData.levels = new List<SaveSlot>();
        saveData.energy = 5;
        saveData.coins_ = 1000;
        for(int i = 0; i < gameData.levels.Count; i++)
        {
            saveData.levels.Add(new SaveSlot(gameData.levels[i].levelNum, LevelStatus.locked));
            saveData.levels[i].levelNum = gameData.levels[i].levelNum;
            saveData.levels[i].status = LevelStatus.locked;
        }
        if (saveData.levels.Count > 0)
            saveData.levels[0].status = LevelStatus.enabled;
        SaveAllUserData();
    }

    public static void InitSavedData()
    {
        string path;
        if (curPlatform == Advert.CurrentPlatform.android)
            path = $"{Application.persistentDataPath}/{saveFileName}";
        else
            path = $"{Application.dataPath}/{saveFileName}";
        currentSavePath = path;
        Debug.Log($"Save path: {currentSavePath}");
        if (!File.Exists(path))
        {
            File.Create(path).Dispose();
            saveData = new GameSave();
            RestorePlayerData();
        }
        else
        {
            string saveDataJson = "";
            if (!File.Exists(path))
            {
                saveData = new GameSave();
                SaveAllUserData();
            }
            else
            {
                saveDataJson = File.ReadAllText(path);
            }
            if (saveDataJson != null && saveDataJson != "") saveData = JsonConvert.DeserializeObject<GameSave>(saveDataJson);
            else
            {
                Debug.Log("[Sweet Boom Editor] No save data founded.");
                saveData = new GameSave();
                RestorePlayerData();
            }
            if (gameData.levels.Count != saveData.levels.Count)
            {
                foreach (var item in gameData.levels)
                {
                    bool ex = false;
                    foreach (var savedlvls in saveData.levels) if (savedlvls.levelNum == item.levelNum) { ex = true; break; }
                    if (!ex) saveData.levels.Add(new SaveSlot(item.levelNum, LevelStatus.locked));
                }
            }
        }
    }

    /// <summary>
    /// [EN] Call this method if you want to save player's progress
    /// [RU] Статический метод для сохранения прогресса игрока в 
    /// </summary>
    public static void SaveAllUserData()
    {
        string saveDataJson = JsonConvert.SerializeObject(saveData);
        if (!File.Exists(currentSavePath))
            File.Create(currentSavePath).Dispose();
        File.WriteAllText(currentSavePath, saveDataJson);
    }

    public static void DebugMessage(string message)
    {
        lvlManager.debugGUI.text += System.Environment.NewLine + message;
    }
    public enum GameDataBlockID
    {
        nil = 0,
        empty,
        candy,
        ice,
        block,
        red,
        green,
        blue,
        yellow,
        orange,
        purple
    }
    public enum BlockID
    {
        nil = 0,
        empty = 1,
        candy = 2,
        ice = 3,
        block = 4
    }
    public enum CandyType
    {
        red = 5,
        green,
        blue,
        yellow,
        orange, 
        purple,
        multi,
        nil = -1
    }
    [Serializable]
    public class GameData // All game data from SweetBoom editor
    {
        public List<Level> levels;
        public int levelCount;
        public ConfigurationSettings settings;

        [JsonConstructor]
        public GameData(List<Level> levels, int levelCount, ConfigurationSettings settings)
        {
            this.levels = levels;
            this.levelCount = levelCount;
            this.settings = settings;
        }

        public GameData()
        {
            this.levels = null;
            this.levelCount = 0;
            this.settings = null;
        }

        public static GameData SetDefualt()
        {
            return new GameData()
            {
                levelCount = 0,
                levels = new List<Level>(),
                settings = ConfigurationSettings.SetDefaultConfig()
            };
        }
    }

    [Serializable]
    public class Level // Levels class for save added levels by developer
    {
        public int levelNum, fieldWidth, fieldHeight, goals, maxScore, candyReward, goalReward,
            lvlComplete, lvlCompleteAgain, lvlSuperComplete;
        public string comment;
        public Vector3Int[] savedBlock;
        public int[] target;
        public Color[] levelColors;
        public int[] starScore = new int[3];
    }
    [System.Serializable]
    public class ConfigurationSettings
    {
        public bool sortingLevelsInMenu, randomizePositionOfIcons, fps;
        public int delayEnergy;
        public string rateUsLink;
        public float distance, size;
        public Advert.AdConfig adConfig;
        public List<Shop.CoinShopItem> shopItems;

        public static ConfigurationSettings SetDefaultConfig()
        {
            return new ConfigurationSettings()
            {
                sortingLevelsInMenu = true,
                randomizePositionOfIcons = false,
                distance = 130,
                size = 0.82f,
                fps = false,
                delayEnergy = 15,
                adConfig = Advert.AdConfig.SetDefaultConfig(),
                shopItems = new List<Shop.CoinShopItem>()
                {
                    new Shop.CoinShopItem()
                    {
                        coinRew = 200,
                        price = "0.50",
                        unityIAPId = "test1",
                        appStoreId = "appStoreTest1",
                        googlePlayId = "googlePlayTest1"
                    },
                    new Shop.CoinShopItem()
                    {
                        coinRew = 500,
                        price = "0.99",
                        unityIAPId = "test2",
                        appStoreId = "appStoreTest2",
                        googlePlayId = "googlePlayTest2"
                    },
                    new Shop.CoinShopItem()
                    {
                        coinRew = 900,
                        price = "1.50",
                        unityIAPId = "test3",
                        appStoreId = "appStoreTest3",
                        googlePlayId = "googlePlayTest3"
                    }
                }
            };
        }
    }
}
