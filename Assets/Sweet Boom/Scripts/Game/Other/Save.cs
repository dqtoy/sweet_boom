using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using TMPro;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
/// <summary>
/// <para>[EN] Class with save functions and some survice functions.</para>
/// <para>[RU] Класс предоставляющий функции сохранения, загрузки всех игровых данных, а также некоторые служебные функции.</para>
/// </summary>
public static class Save {
    
    public static GameSave saveData { get; set; } // player progress
    public static GameData gameData { get; private set; } // levels data
    public static ConfigurationSettings configuration { get; set; }
    static bool d = true;
    public static SoundController sound;
    private static string gameDataPath = "/gamedata.txt", saveFileName = "/savedata.txt";
    private static string fullGameInfoFilePath = $"{Application.streamingAssetsPath}{gameDataPath}", userDataSaveDirectory = $"{Application.streamingAssetsPath}{saveFileName}";

    /// <summary>
    /// <para>[EN] Main initialization of the game </para>
    /// <para>[RU] Главная инициализация игры </para>
    /// </summary>
    public static void Init()
    {
        Application.quitting += SaveAllUserData;

        gameData = new GameData();
        InitGameData();

        saveData = new GameSave();
        InitSavedData();
    }
    /// <summary>
    /// [EN] Initialization of game data (levels, ads configs etc.)
    /// [RU] Инициализация игровых данных (уровни, настройки рекламы и т.д.)
    /// </summary>
    /// <returns></returns>
    public static GameData InitGameData()
    {
        try
        {
            if (!File.Exists($"{fullGameInfoFilePath}"))
            {
                Debug.Log("Open Dudle/Level Editor " +
                "to start making levels.");
                gameData = new GameData();
                return gameData;
            }
            else
            {
                string fileData = "";
                string[] fileDataAr = File.ReadAllLines(fullGameInfoFilePath);
                foreach (var line in fileDataAr) fileData += line;
                gameData = new GameData();
                gameData = JsonConvert.DeserializeObject<GameData>(fileData);
               
                configuration = gameData.settings;

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
                return gameData;
            }
        }
        catch (Exception e)
        {
            Debug.Log("[Sweet Boom Editor] Game data is damaged. Error type: " + e.ToString());
            gameData = new GameData();
            return gameData;
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
            for (int i = 0; i < ((to - from) + 1); i++) if (result > i * dist && result <= (i + 1) * dist) return from + i;
        }
    }
    [Serializable]
    public class GameSave
    {
        public List<SaveSlot> levels;
        public event Action uiUpdate;
        public int coins_;

        public int energy;
        public int[] boostersCount;
        public int bombBooster, searchBooster, multiCandyBooster;
        public GameSave()
        {
            var lvl = new SaveSlot(1, LevelStatus.enabled);
            levels = new List<SaveSlot>();
            boostersCount = new int[3];
            energy = 5;
        }
        /// <summary>
        /// <para>[EN] Set completed level to 'complete' and open next level</para>
        /// <para>[RU] Помечает уровень как завершенный и открывает следующий</para>
        /// </summary>
        /// <param name="num">[EN] number of completed level</param>
        public void UnlockNewLevel(int num, int score, int stars)
        {
            Debug.Log("unl");
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
                        Debug.Log($"Completed: {num}, {saveData.levels.Count}");
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

        }
        [JsonIgnore]
        public int Coins
        {
            get { return coins_; }
            set
            {
                coins_ = value;
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
        for(int i = 0; i < gameData.levelCount; i++)
        {
            saveData.levels.Add(new SaveSlot(gameData.levels[i].levelNum, LevelStatus.locked));
            saveData.levels[i].levelNum = gameData.levels[i].levelNum;
            saveData.levels[i].status = LevelStatus.locked;
        }
        saveData.levels[0].status = LevelStatus.enabled;
        SaveAllUserData();
    }
    public static void InitSavedData()
    {
        //Debug.Log("[Sweet Boom Editor] Initialization...");
        if (!File.Exists($"{Application.streamingAssetsPath}{saveFileName}"))
        {
            File.Create($"{Application.streamingAssetsPath}{saveFileName}").Dispose();
            saveData = new GameSave();
            RestorePlayerData();
        }
        else
        {
            try
            {
                string saveDataJson = "";
                string savePath = $"{Application.streamingAssetsPath}{saveFileName}";
                string[] saveDataArray = File.ReadAllLines(savePath);
                foreach (var line in saveDataArray) saveDataJson += line;
                if(saveDataJson != null && saveDataJson != "") saveData = JsonConvert.DeserializeObject<GameSave>(saveDataJson);
                else 
                {
                    Debug.Log("[Sweet Boom Editor] No save data founded.");
                    saveData = new GameSave();
                    RestorePlayerData();
                }
                if(gameData.levels.Count != saveData.levels.Count)
                {
                    foreach (var item in gameData.levels)
                    {
                        bool ex = false;
                        foreach (var savedlvls in saveData.levels) if(savedlvls.levelNum == item.levelNum) { ex = true; break; }
                        if(!ex) saveData.levels.Add(new SaveSlot(item.levelNum, LevelStatus.locked));
                    }
                }
            }
            catch(Exception e)
            {
                Debug.Log($"[Sweet Boom Editor] {e.ToString()}");
                saveData = new GameSave();
                SaveAllUserData();
                //RestorePlayerData();
            }
            finally
            {
                //Debug.Log("[Sweet Boom Editor] Data files initialized.");
            }
        }
    }
    
    public static void SaveAllUserData()
    {
        /* 
        if (!File.Exists($"{Application.streamingAssetsPath}{saveFileName}")) 
        {
            File.Create($"{Application.streamingAssetsPath}{saveFileName}").Dispose();
        }
        */
        string saveDataJson = JsonConvert.SerializeObject(saveData);
        string[] saveJsonArray = new string[saveDataJson.Length / 200 + 1];
        int count = 0, j = 0;
        for (int i = 0; i < saveDataJson.Length; ++i)
        {
            if(count >= 200)
            {
                j++;
                count = 0;
            }
            saveJsonArray[j] += saveDataJson[i];
            count++;
        }
        try
        {
            File.WriteAllLines($"{Application.streamingAssetsPath}{saveFileName}", saveJsonArray);
        }
        catch(Exception e) { Debug.Log($"[Sweet Boom Editor] Game save data read/write error. {e.ToString()}"); }
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
        red,
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
    }

    [Serializable]
    public class Level // Levels class for save added levels by developer
    {
        public int levelNum, fieldWidth, fieldHeight, goals, maxScore, candyReward, goalReward;
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
        public float distance, size;
        public Advert.AdConfig adConfig;
        public List<Shop.CoinShopItem> shopItems;
    }
}
