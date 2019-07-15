using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEditor.SceneManagement;
using System.IO;
using System.Net.Mail;

public class EditLevels : EditorWindow {

    public static EditLevels editLevelsWindow;
    public static Texture2D mainTexture, mainTextureRight, logo, playField, leftTexture, warning, headerTexture, logo_menu, logo_set, consoleTexture;
    public static Texture2D candy_circle_green, candy_orange, candy_oval_blue, candy_star_red, candy_triangle_yellow, ice, rock, empty, eraser, candies, fill, fill_gr;
    static Texture2D selBrush, activeFill;
    static Rect mainTextureRect, mainTextureRightRect, playFieldRect, logoRect, leftTextureRect, headerRect, logoMenuRect, logoSetRect, selectColorRect, consoleRect;
    static Vector2 scrollLevelEditor, scrollIAP;
    public static int width, height;
    public static int updWidth, updHeight, greenAm, blueAm, orangeAm, redAm, yellowAm, purpleAm, currentLvlNum, maxGoals, maxScore, candyReward, goalReward;
    public static int[] starTargetScore = new int[3];
    private bool isDataEx;
    public static Texture2D[,] field;
    public static bool cn_gr = false, cn_rd = false, cn_ye = false, cn_bl = false, cn_or = false, cn_prpl, fill_en = false;
    public static string description, editorConsole;
    public static int[,] block;
    public static int selBlockID, selectedLevelInPopup;
    public static int curTab, flag;
    public static float loadingValue;
    Transform LevelSection;
    static string menuConsole;
    GameObject[] sceneObj;
    public static Color blockColor1, blockColor2;
    #region
    static GameObject menuObj, levelObj;
    public string levelPrefabPath;
    static Rect menuTextureRect;
    static bool randomize, sortingLevels;
    public static float sliderValueDistance, sliderValueSize;
    public static ConfigurationSettings conf;
    public static GameObject[] levels;
    #endregion
    #region
    public static UnityEngine.Object prefab;
    public static bool enableFpsCounter;
    #endregion
    #region Advertisement
    public static Advert.AdConfig adConfig = new Advert.AdConfig();
    #endregion
    #region IAPSettings
    //public static List<Shop.CoinShopItem> shopItemsIAP { get; set; } = new List<Shop.CoinShopItem>();
    #endregion
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

    [MenuItem("Dudle/Scenes/Open 'Menu' scene")]
    private static void SelectSceneInMenu()
    {
        try
        {
            Scene curScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(curScene);
            EditorSceneManager.OpenScene("Assets/Sweet Boom/Scenes/Menu.unity");
        }
        catch { Debug.Log("Can't open scene"); }
    }
    [MenuItem("Dudle/Scenes/Open 'Game' scene")]
    private static void SelectSceneInMenu2()
    {
        try
        {
            Scene curScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(curScene);
            EditorSceneManager.OpenScene("Assets/Sweet Boom/Scenes/Game.unity");
        }
        catch { Debug.Log("Can't open scene"); }
    }

    [MenuItem("Dudle/Sweet Boom Editor")]
    public static void Window()
    {
        editLevelsWindow = GetWindow<EditLevels>("Sweet Boom");
        editLevelsWindow.maxSize = new Vector2(600, 800);
        editLevelsWindow.minSize = new Vector2(600, 800);
    }
    #region Initialization
    private void OnEnable()
    {
        sliderValueDistance = 100;
        sliderValueSize = 0.8f;
        loadingValue = 0;
        InitTextures();
        width = 5;
        height = 5;
        curTab = 0;
        blockColor1 = new Color(0.41f, 0.43f, 1, 0.7f);
        blockColor2 = new Color(0.53f, 0.56f, 1, 0.7f);

        try
        {
            conf = LevelDatabase.data.settings;
            if (conf.adConfig != null)
            {
                adConfig = conf.adConfig ?? Advert.AdConfig.SetDefaultConfig();
            }
            else adConfig = Advert.AdConfig.SetDefaultConfig();
        }
        catch
        {
            adConfig = Advert.AdConfig.SetDefaultConfig();
        }
        foreach (GameObject obj in (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (obj.name == "MENU" && obj.tag == "GameUI") menuObj = obj;
            else if (obj.name == "LEVELS" && obj.tag == "GameUI") levelObj = obj;
        }
    }
    private void OnDisable()
    {
        try
        {
            levelObj.SetActive(false);
            menuObj.SetActive(true);
        }
        catch { }
    }
    void InitTextures()
    {
        editorConsole = "...";
        currentLvlNum = 0;
        mainTexture = new Texture2D(1, 1);
        mainTexture.SetPixel(0, 0, new Color(1, 1, 1, 0));
        mainTextureRight = new Texture2D(1, 1);
        mainTextureRight.SetPixel(0, 0, new Color(1, 1, 1, 0));
        consoleTexture = new Texture2D(1, 1);
        consoleTexture.SetPixel(0, 0, new Color(0.7f, 0.7f, 0.7f, 1f));

        headerTexture = new Texture2D(1, 1);
        
        leftTexture = new Texture2D(1, 1);
        leftTexture.SetPixel(0, 0, new Color(1, 1, 1, 0));

        playField = Resources.Load<Texture2D>("gradient");
        candy_circle_green = Resources.Load<Texture2D>("candy_circle_green");
        candy_orange = Resources.Load<Texture2D>("candy_orange");
        candy_oval_blue = Resources.Load<Texture2D>("candy_oval_blue");
        candy_star_red = Resources.Load<Texture2D>("candy_star_red");
        candy_triangle_yellow = Resources.Load<Texture2D>("candy_triangle_yellow");
        ice = Resources.Load<Texture2D>("ice");
        rock = Resources.Load<Texture2D>("rock");
        empty = Resources.Load<Texture2D>("empty");
        eraser = Resources.Load<Texture2D>("eraser");
        candies = Resources.Load<Texture2D>("candy_switch");
        fill = Resources.Load<Texture2D>("filling");
        fill_gr = Resources.Load<Texture2D>("filling_green");
        warning = Resources.Load<Texture2D>("warning_yellow");
        logo_menu = Resources.Load<Texture2D>("level_editor_logo_menu");
        logo_set = Resources.Load<Texture2D>("level_editor_logo_settings");
        logo = Resources.Load<Texture2D>("editor_logo");

        updHeight = height;
        updWidth = width;
        field = new Texture2D[updHeight, updWidth];
        block = new int[updHeight, updWidth];
        for(int i = 0; i < updHeight; i++)
        {
            for(int y = 0; y < updWidth; y++) block[i, y] = 0;
        }
        LevelDatabase.Initialization();
        int[] trg = new int[5];
    }
    void OnGUI()
    {
        Draw();
        FirstBlock();
    }
    static void Draw() // DRAW EVERYTHING
    {
        headerRect.x = 305;
        headerRect.y = 10;
        headerRect.width = Screen.width - 310;
        headerRect.height = 20;
        //headerTexture.Apply();
        //GUI.DrawTexture(headerRect, headerTexture);

        if (curTab == 0)
        {
            mainTextureRect.x = 305;
            mainTextureRect.y = 30;
            mainTextureRect.width = Screen.width - 310;
            mainTextureRect.height = 175;
            mainTexture.Apply();          

            var btnwidth = (Screen.width - mainTextureRect.x * 3) / 2;
            mainTextureRightRect.x = Screen.width / 2 + 5;
            mainTextureRightRect.y = 150;
            //mainTextureRightRect.width = (Screen.width - mainTextureRect.x) / 2 - 20;
            mainTextureRightRect.width = ((Screen.width - playFieldRect.x * 3) / 2) / 2 - 5;
            mainTextureRightRect.height = 230;
            mainTextureRight.Apply();

            leftTextureRect.x = 5;
            leftTextureRect.y = 150;
            leftTextureRect.width = Screen.width / 2 - 10;
            leftTextureRect.height = 260;
            leftTexture.Apply();

            logoRect.x = 0;
            logoRect.y = 0;
            logoRect.width = 295;
            logoRect.height = 140;

            selectColorRect.x = Screen.width / 2 + 5 + mainTextureRightRect.width + 4;
            selectColorRect.y = 150;
            selectColorRect.width = ((Screen.width - playFieldRect.x * 3) / 2) / 2 - 2;
            selectColorRect.height = 270;
            consoleTexture.Apply();

            playFieldRect.x = 10;
            playFieldRect.y = 400;
            playFieldRect.width = Screen.width - playFieldRect.x * 2;
            playFieldRect.height = 400;

            consoleRect.x = Screen.width / 2 + 5;
            consoleRect.y = 340;
            consoleRect.width = (((Screen.width - playFieldRect.x * 3) / 2) / 2 - 2) * 2 + 2;
            consoleRect.height = 45;

            GUI.DrawTexture(consoleRect, consoleTexture);
            GUI.DrawTexture(mainTextureRect, mainTexture);
            GUI.DrawTexture(mainTextureRightRect, mainTextureRight);
            GUI.DrawTexture(playFieldRect, playField);
            GUI.DrawTexture(logoRect, logo);
            GUI.DrawTexture(leftTextureRect, leftTexture);
        }
        else if(curTab == 1)
        {
            menuTextureRect.x = 305;
            menuTextureRect.y = 30;
            menuTextureRect.width = Screen.width - 310;
            menuTextureRect.height = 400;

            logoMenuRect.x = 0;
            logoMenuRect.y = 0;
            logoMenuRect.width = 295;
            logoMenuRect.height = 140;
            GUI.DrawTexture(logoMenuRect, logo_menu);
        }
        else if(curTab == 2)
        {
            leftTextureRect.x = 5;
            leftTextureRect.y = 150;
            leftTextureRect.width = Screen.width / 2 - 10;
            leftTextureRect.height = 400;
            leftTexture.Apply();

            menuTextureRect.x = 305;
            menuTextureRect.y = 30;
            menuTextureRect.width = Screen.width - 310;
            menuTextureRect.height = 600;

            logoSetRect.x = 0;
            logoSetRect.y = 0;
            logoSetRect.width = 295;
            logoSetRect.height = 140;
            GUI.DrawTexture(logoSetRect, logo_set);
        }
    }
    #endregion
    public enum Candies
    {
        blue_candy,
        orange_candy,
        yellow_candy,
        red_candy,
        green_candy
    }
    public static void FirstBlock()
    {
        GUILayout.BeginArea(headerRect); // LEFT ZONE
        GUILayout.BeginHorizontal();
        curTab = GUILayout.Toolbar(curTab, new string[] { "Level editor", "Menu editor", "Settings & Ad" });
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        #region Level editor
        if (curTab == 0)
        {
            editLevelsWindow.maxSize = new Vector2(600, 800);
            editLevelsWindow.minSize = new Vector2(600, 800);
            if (flag != 0)
            {
                flag = 0;
                try
                {
                    menuObj.SetActive(true);
                    levelObj.SetActive(false);
                }
                catch { }
            }
            
            if (selBrush != null)
            {
                switch (selBrush.name)
                {
                    case "candy_switch": selBlockID = (int)BlockID.candy; break;
                    case "ice": selBlockID = (int)BlockID.ice; break;
                    case "rock": selBlockID = (int)BlockID.block; break;
                    case "empty": selBlockID = (int)BlockID.empty; break;
                }
            }
            else selBlockID = (int)BlockID.nil;

            GUILayout.BeginArea(mainTextureRect); // LEFT ZONE
            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("To start making level choose width and height, select brush and click on the player field.", MessageType.Warning);
            if (GUILayout.Button("Help", GUILayout.Height(40)))
            {
                NewLevelWin.OpenWindowNew(NewLevelWin.WindowType.help);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("New Level", GUILayout.Height(35)))
            {
                NewLevelWin.OpenWindowNew(NewLevelWin.WindowType.newLevel);
            }

            if (GUILayout.Button("Open Level", GUILayout.Height(35)))
            {
                NewLevelWin.OpenWindowNew(NewLevelWin.WindowType.open);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Delete Level", GUILayout.Height(35))) NewLevelWin.OpenWindowNew(NewLevelWin.WindowType.delete);

            if (GUILayout.Button("Save / Save As", GUILayout.Height(35))) NewLevelWin.OpenWindowNew(NewLevelWin.WindowType.saveLevelAs);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.BeginArea(mainTextureRightRect);

            //GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Target:");

            GUILayout.BeginHorizontal();
            cn_rd = GUILayout.Toggle(cn_rd, "red candies");
            if (!cn_rd) redAm = 0;
            redAm = EditorGUILayout.IntField(redAm);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            cn_gr = GUILayout.Toggle(cn_gr, "green candies");
            if (!cn_gr) greenAm = 0;
            greenAm = EditorGUILayout.IntField(greenAm);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            cn_bl = GUILayout.Toggle(cn_bl, "blue candies");
            if (!cn_bl) blueAm = 0;
            blueAm = EditorGUILayout.IntField(blueAm);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            cn_ye = GUILayout.Toggle(cn_ye, "yellow candies");
            if (!cn_ye) yellowAm = 0;
            yellowAm = EditorGUILayout.IntField(yellowAm);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            cn_or = GUILayout.Toggle(cn_or, "orange candies");
            if (!cn_or) orangeAm = 0;
            orangeAm = EditorGUILayout.IntField(orangeAm);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            cn_prpl = GUILayout.Toggle(cn_prpl, "purple candies");
            if (!cn_prpl) purpleAm = 0;
            purpleAm = EditorGUILayout.IntField(purpleAm);
            GUILayout.EndHorizontal();

            //GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.BeginArea(selectColorRect); // SELECT COLOR SECTION

            GUILayout.Label("Background cubes");
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical();
            GUILayout.Label("Color#1");
            blockColor1 = EditorGUILayout.ColorField(blockColor1);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Color#2");
            blockColor2 = EditorGUILayout.ColorField(blockColor2);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Default colors");

            EditorGUILayout.LabelField("Reward for each candy");
            candyReward = EditorGUILayout.IntField(candyReward);

            EditorGUILayout.LabelField("Reward for each goal");
            goalReward = EditorGUILayout.IntField(goalReward);

            if (GUILayout.Button("Set default settings"))
            {
                candyReward = 10;
                goalReward = 15;
                blockColor1 = new Color(0.41f, 0.43f, 1, 0.7f);
                blockColor2 = new Color(0.53f, 0.56f, 1, 0.7f);
            }
            GUILayout.EndArea();
            GUILayout.EndHorizontal();
            GUILayout.BeginArea(consoleRect);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Level Editor Console:");
            GUILayout.Label(editorConsole);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            /*
            if (GUILayout.Button("Save Changes", GUILayout.Height(40)))
            {
                LevelDatabase.SaveData();
                ConsoleLog("Changes saved!");
            }
            */
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndArea();

            GUILayout.BeginArea(playFieldRect); // PLAY FIELD
            GUILayout.BeginVertical();
            scrollLevelEditor = EditorGUILayout.BeginScrollView(scrollLevelEditor, GUILayout.Height(playFieldRect.height - 2));
            if (fill_en) activeFill = fill_gr; else activeFill = fill;
            GUILayout.Label("Play Field");
            GUILayout.BeginVertical();

            float butScale = (Screen.width - playFieldRect.x * 2) / updWidth - (0.5f * (updWidth + 1));
            if (butScale > 70) butScale = 70;
            else if (butScale < 30) butScale = 30;
            for (int i = 0; i < updHeight; i++)
            {
                GUILayout.BeginHorizontal();
                for (int y = 0; y < updWidth; y++)
                {
                    if (field[i, y] == null)
                    {
                        if (GUILayout.Button("", GUILayout.Height(butScale), GUILayout.Width(butScale)))
                        {
                            if (fill_en && selBrush != eraser)
                            {
                                fill_en = false;
                                for (int p = 0; p < updHeight; p++)
                                {
                                    for (int q = 0; q < updWidth; q++)
                                    {
                                        if (field[p, q] == null)
                                        {
                                            field[p, q] = selBrush;
                                            block[p, q] = selBlockID;
                                        }
                                    }
                                }
                            }
                            else if (selBrush != eraser)
                            {
                                field[i, y] = selBrush;
                                block[i, y] = selBlockID;
                            }
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(field[i, y], GUILayout.Height(butScale), GUILayout.Width(butScale)))
                        {
                            if (selBrush == eraser)
                            {
                                field[i, y] = null;
                                block[i, y] = (int)BlockID.nil;
                            }
                            else if (fill_en && selBrush != eraser)
                            {
                                fill_en = false;
                                Texture2D fil = field[i, y];
                                for (int p = 0; p < updHeight; p++)
                                {
                                    for (int q = 0; q < updWidth; q++)
                                    {
                                        if (field[p, q] == fil)
                                        {
                                            field[p, q] = selBrush;
                                            block[p, q] = selBlockID;
                                        }
                                    }
                                }
                            }
                            else if(fill_en && selBrush == eraser)
                            {
                                fill_en = false;
                                Texture2D fil = field[i, y];
                                for (int p = 0; p < updHeight; p++)
                                {
                                    for (int q = 0; q < updWidth; q++)
                                    {
                                        if (field[p, q] == fil)
                                        {
                                            field[p, q] = null;
                                            block[p, q] = (int)BlockID.nil;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                field[i, y] = selBrush;
                                block[i, y] = selBlockID;
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
            
            GUILayout.EndArea();
            
            GUILayout.BeginArea(leftTextureRect); // LEFT ZONE
            if (currentLvlNum == 0) GUILayout.Label("Level: none");
            else GUILayout.Label("Level: " + currentLvlNum);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Description: ");
            description = EditorGUILayout.TextArea(description, GUILayout.Height(30), GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Width");
            width = EditorGUILayout.IntField(width);
            if (width < 5) width = 5;
            else if (width > 12) width = 12;

            GUILayout.Label("Height");
            height = EditorGUILayout.IntField(height);
            if (height < 5) height = 5;
            else if (height > 12) height = 12;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate play field", GUILayout.Height(20)))
            {
                updHeight = height;
                updWidth = width;
                field = new Texture2D[updHeight, updWidth];
                block = new int[updHeight, updWidth];
            }

            if (GUILayout.Button("Reset field", GUILayout.Height(20)))
            {
                for (int i = 0; i < updHeight; i++)
                {
                    for (int y = 0; y < updWidth; y++)
                    {
                        field[i, y] = null;
                        block[i, y] = (int)BlockID.nil;
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max moves: ");
            maxGoals = EditorGUILayout.IntField(maxGoals);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max score: ");
            maxScore = EditorGUILayout.IntField(maxScore);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Score to reach 1 star: ");
            starTargetScore[0] = EditorGUILayout.IntField(starTargetScore[0]);
            if (starTargetScore[0] >= maxScore) starTargetScore[0] = 0;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Score to reach 2 stars: ");
            starTargetScore[1] = EditorGUILayout.IntField(starTargetScore[1]);
            if (starTargetScore[1] >= maxScore) starTargetScore[1] = 0;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Score to reach 3 stars: ");
            starTargetScore[2] = EditorGUILayout.IntField(starTargetScore[2]);
            if (starTargetScore[2] >= maxScore) starTargetScore[2] = 0;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            
            //GUILayout.Label("Brushes:"); // Select brush
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button(candies, GUILayout.Width(40), GUILayout.Height(40))) selBrush = candies;
            if (GUILayout.Button(ice, GUILayout.Width(40), GUILayout.Height(40))) selBrush = ice;
            if (GUILayout.Button(rock, GUILayout.Width(40), GUILayout.Height(40))) selBrush = rock;
            if (GUILayout.Button(empty, GUILayout.Width(40), GUILayout.Height(40))) selBrush = empty;
            GUILayout.Label(" ");
            if (GUILayout.Button(eraser, GUILayout.Width(40), GUILayout.Height(40))) selBrush = eraser;
            if (GUILayout.Button(activeFill, GUILayout.Width(40), GUILayout.Height(40))) fill_en = !fill_en;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        #endregion
        #region Menu editor
        else if (curTab == 1)
        {
            editLevelsWindow.maxSize = new Vector2(600, 300);
            editLevelsWindow.minSize = new Vector2(600, 300);
            try
            {
                if(flag != 1)
                {
                    flag = 1;
                    menuObj.SetActive(false);
                    levelObj.SetActive(true);
                    try
                    {
                        sliderValueDistance = conf.distance;
                        sliderValueSize = conf.size;
                        randomize = conf.randomizePositionOfIcons;
                        sortingLevels = conf.sortingLevelsInMenu;
                    }
                    catch
                    {
                        sliderValueDistance = 130;
                        sliderValueSize = 0.82f;
                        randomize = false;
                        sortingLevels = true;
                    }
                }
            }
            catch { }
            GUILayout.BeginArea(menuTextureRect);
            if(menuObj != null && levelObj != null) EditorGUILayout.HelpBox($"Here you can spawn level icons. You have {LevelDatabase.data.levels.Count} levels.", MessageType.Info);
            else EditorGUILayout.HelpBox("Can't find required components or the wrong scene is open.", MessageType.Error);
            
            GUILayout.Label("Distance between icons:");
            GUILayout.BeginHorizontal();
            sliderValueDistance = (float)Math.Round(GUILayout.HorizontalSlider(sliderValueDistance, 70, 200));
            sliderValueDistance = (int)EditorGUILayout.FloatField(sliderValueDistance, GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.Label("Icons size:");
            GUILayout.BeginHorizontal();
            sliderValueSize = GUILayout.HorizontalSlider(sliderValueSize, 0.3f, 1);
            sliderValueSize = (float)Math.Round(EditorGUILayout.FloatField(sliderValueSize, GUILayout.Width(50)), 2);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            randomize = EditorGUILayout.Toggle(randomize);
            GUILayout.Label("Randomize x position of icons.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            sortingLevels = EditorGUILayout.Toggle(sortingLevels);
            GUILayout.Label("Sorting levels when game start.");
            GUILayout.Label(" ");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(levels != null)
            {
                //for (int i = 0; i < levels.Length-1; i++) Handles.DrawPolyLine(levels[i].transform.position, levels[i + 1].transform.position);
                //Handles.DrawLine(levels[0].transform.position, levels[0].transform.position);
            }
            if (GUILayout.Button("Create level icons", GUILayout.Height(25)))
            {
                try
                {
                    /*
                    if(flag == true)
                    {
                        menuObj.SetActive(false);
                        levelObj.SetActive(true);
                        editLevelsWindow.maxSize = new Vector2(600, 400);
                        editLevelsWindow.minSize = new Vector2(600, 400);
                        flag = false;
                    }
                    */
                    levels = new GameObject[LevelDatabase.data.levels.Count];
                    GameObject parent = GameObject.Find("#LevelIconsParent");
                    GameObject[] oldLevels = GameObject.FindGameObjectsWithTag("Level");
                    foreach(GameObject obj in oldLevels) DestroyImmediate(obj);
                    for(int i = 0; i < LevelDatabase.data.levels.Count; i++)
                    {
                        levels[i] = Instantiate((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Sweet Boom/Prefabs/Level.prefab", typeof(GameObject)), Vector3.zero, Quaternion.identity);
                        levels[i].transform.parent = parent.transform;
                        levels[i].GetComponent<RectTransform>().localScale = new Vector3(sliderValueSize, sliderValueSize, sliderValueSize);
                        if(randomize)
                        {
                            if (i == 0) levels[i].GetComponent<RectTransform>().localPosition = new Vector2(UnityEngine.Random.Range(-200, 200), 200);
                            else levels[i].GetComponent<RectTransform>().localPosition = new Vector2(UnityEngine.Random.Range(-200, 200), 
                                levels[i - 1].GetComponent<RectTransform>().localPosition.y + sliderValueDistance);
                        }
                        else
                        {
                            if (i == 0) levels[i].GetComponent<RectTransform>().localPosition = new Vector2(0, 200);
                            else levels[i].GetComponent<RectTransform>().localPosition = 
                                    new Vector2(0, levels[i - 1].GetComponent<RectTransform>().localPosition.y + sliderValueDistance);
                        }
                        levels[i].gameObject.name = $"#level:{i+1}";
                        levels[i].transform.Find("#Stars").gameObject.SetActive(true);
                        levels[i].transform.Find("#LevelNumber").GetComponent<TextMeshProUGUI>().text = $"{i + 1}";
                        Selection.activeGameObject = levels[i].gameObject;
                        EditorGUIUtility.PingObject(levels[i]);
                    }
                    menuConsole = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}] Levels spawned.";
                }
                catch
                {
                    NewLevelWin.OpenWindowNew(NewLevelWin.WindowType.notification);
                }
            }
            if(GUILayout.Button("Delete level icons", GUILayout.Height(25)))
            {
                GameObject[] oldLevels = GameObject.FindGameObjectsWithTag("Level");
                foreach (GameObject obj in oldLevels) DestroyImmediate(obj);
                menuConsole = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}] Level icons deleted!";
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Save changes", GUILayout.Height(40)))
            {
                ConfigurationSettings conf = new ConfigurationSettings(sortingLevels, randomize, sliderValueDistance, sliderValueSize, enableFpsCounter, adConfig, LevelDatabase.shopItems);
                LevelDatabase.data.settings = conf;
                LevelDatabase.SaveData();
                menuConsole = $"[{DateTime.UtcNow.Hour}:{DateTime.UtcNow.Minute}] Changes saved!";
            }
            
            GUILayout.Label("Level editor console:");
            if (menuConsole == "" || menuConsole == null) menuConsole = "...";
            GUILayout.Label(menuConsole);
            GUILayout.EndArea();
        }
        #endregion
        #region Advertisement && shop settings
        else if(curTab == 2)
        {
            editLevelsWindow.maxSize = new Vector2(600, 600);
            editLevelsWindow.minSize = new Vector2(600, 600);
            GUILayout.BeginArea(menuTextureRect);
            enableFpsCounter = EditorGUILayout.ToggleLeft("Enable fps counter in game", enableFpsCounter);
            EditorGUILayout.LabelField("Customization", EditorStyles.boldLabel);
            prefab = EditorGUILayout.ObjectField(prefab, typeof(Sprite), false);
            Blocks asset = (Blocks)AssetDatabase.LoadAssetAtPath("Assets/Candy World/Prefabs/Blocks/Green Candy.asset", typeof(Blocks));
            
            EditorGUILayout.LabelField("Advertisement settings", EditorStyles.boldLabel);
            adConfig.unityAdsEnable = EditorGUILayout.ToggleLeft("Use Unity Ads", adConfig.unityAdsEnable);
            adConfig.adMobEnable = EditorGUILayout.ToggleLeft("Use AdMob", adConfig.adMobEnable);
            EditorGUI.BeginDisabledGroup(!adConfig.unityAdsEnable);
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label("Unity Ads ID");
            adConfig.unityAdsID = EditorGUILayout.TextField(adConfig.unityAdsID);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!adConfig.adMobEnable);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("AdMob ID");
            adConfig.adMobID = EditorGUILayout.TextField(adConfig.adMobID);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Banner ID");
            adConfig.adMobBannerID = EditorGUILayout.TextField(adConfig.adMobBannerID);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Interstitial ID");
            adConfig.adMobPictureID = EditorGUILayout.TextField(adConfig.adMobPictureID);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Rewarded video ID");
            adConfig.adMobRewardedID = EditorGUILayout.TextField(adConfig.adMobRewardedID);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!adConfig.adMobEnable && !adConfig.unityAdsEnable);
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Interstitial Ad");
            EditorGUILayout.BeginVertical("box");
            adConfig.interstitial[0].enabled = EditorGUILayout.ToggleLeft("Shows after lose", adConfig.interstitial[0].enabled);
            EditorGUI.BeginDisabledGroup(!adConfig.interstitial[0].enabled);
            adConfig.interstitial[0].showPlace = Advert.AdConfig.ShowPlace.afterLose;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Service: ");
            adConfig.interstitial[0].serv = (Advert.AdServices)EditorGUILayout.EnumPopup(adConfig.interstitial[0].serv);
            GUILayout.Label("Interval: ");
            adConfig.interstitial[0].interval = EditorGUILayout.IntField(adConfig.interstitial[0].interval);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            adConfig.interstitial[1].enabled = EditorGUILayout.ToggleLeft("Shows after win", adConfig.interstitial[1].enabled);
            EditorGUI.BeginDisabledGroup(!adConfig.interstitial[1].enabled);
            adConfig.interstitial[1].showPlace = Advert.AdConfig.ShowPlace.afterWin;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Service: ");
            adConfig.interstitial[1].serv = (Advert.AdServices)EditorGUILayout.EnumPopup(adConfig.interstitial[1].serv);
            GUILayout.Label("Interval: ");
            adConfig.interstitial[1].interval = EditorGUILayout.IntField(adConfig.interstitial[1].interval);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            adConfig.interstitial[2].enabled = EditorGUILayout.ToggleLeft("Shows in menu", adConfig.interstitial[2].enabled);
            EditorGUI.BeginDisabledGroup(!adConfig.interstitial[2].enabled);
            adConfig.interstitial[2].showPlace = Advert.AdConfig.ShowPlace.afterMenuLoad;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Service: ");
            adConfig.interstitial[2].serv = (Advert.AdServices)EditorGUILayout.EnumPopup(adConfig.interstitial[2].serv);
            GUILayout.Label("Interval: ");
            adConfig.interstitial[2].interval = EditorGUILayout.IntField(adConfig.interstitial[2].interval);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            adConfig.rewardedVideoOpt.enabled = EditorGUILayout.ToggleLeft("Enable rewarded video", adConfig.rewardedVideoOpt.enabled);
            EditorGUI.BeginDisabledGroup(!adConfig.rewardedVideoOpt.enabled);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Service: ");
            adConfig.rewardedVideoOpt.serv = (Advert.AdServices)EditorGUILayout.EnumPopup(adConfig.rewardedVideoOpt.serv);
            GUILayout.Label("Reward per video: ");
            adConfig.rewardedVideoOpt.rewardedAdvertisementReward = EditorGUILayout.IntField(adConfig.rewardedVideoOpt.rewardedAdvertisementReward);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Banner settings");
            adConfig.banner.enabled = EditorGUILayout.ToggleLeft("Enable banner", adConfig.banner.enabled);
            EditorGUI.BeginDisabledGroup(!adConfig.banner.enabled);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Service: AdMob");
            //adConfig.banner.serv = (Advert.AdConfig.AdServices)EditorGUILayout.EnumPopup(adConfig.banner.serv);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Save changes", GUILayout.Height(40)))
            {
                try
                {
                    ConfigurationSettings conf = new ConfigurationSettings(sortingLevels, randomize, sliderValueDistance, sliderValueSize, enableFpsCounter, adConfig, LevelDatabase.shopItems);
                    Debug.Log($"test: {conf.adConfig.rewardedVideoOpt.enabled}");
                    LevelDatabase.data.settings = conf;
                    LevelDatabase.SaveData();
                }
                catch (Exception ex)
                {
                    if (EditorUtility.DisplayDialog("Sweet Boom Editor", "Exception while saving", "Ok")) { }
                    /*
                    switch (EditorUtility.DisplayDialogComplex("Sweet Boom Editor", $"Exception while saving: {ex.ToString()}", "Ok", "Report a bug", "Cancel"))
                    {
                        case 0:
                            Debug.Log("0");
                            break;
                        case 1:
                            Debug.Log("1");
                            break;
                        case 2:
                            Debug.Log("2");
                            break;
                    }
                    */
                }
            }
            if (GUILayout.Button("DELETE ALL GAME DATA", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Delete game data?", "Are you sure that you want to delete ALL game data? This action is permanent.", "Yes, delete it."))
                {
                    if (File.Exists($"{Application.streamingAssetsPath}{LevelDatabase.saveFolderName}"))
                    {
                        File.Delete($"{Application.streamingAssetsPath}{LevelDatabase.saveFolderName}");
                        NewLevelWin.selectLevelPopupInfo = new string[1] { "" };
                        Debug.Log("[Sweet Boom Editor] Game data deleted successfully");
                    }
                    else Debug.Log("[Sweet Boom Editor] You already delete main game data file");
                }
            }
            try
            {
                if (flag != 2)
                {
                    flag = 2;
                    menuObj.SetActive(true);
                    levelObj.SetActive(false);
                    try
                    {
                        enableFpsCounter = conf.fps;
                    }
                    catch { }
                }
            }
            catch {  }
            GUILayout.EndArea();
            // Left 
            GUILayout.BeginArea(leftTextureRect);

            EditorGUILayout.LabelField("IAP Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create codes for each coin item in shop.", MessageType.Info);
            scrollIAP = EditorGUILayout.BeginScrollView(scrollIAP, GUILayout.Width(leftTextureRect.width - 10), GUILayout.Height(leftTextureRect.height - 85));

            if (GUILayout.Button("Create item"))
            {
                LevelDatabase.shopItems.Add(new Shop.CoinShopItem() { coinRew = 0, price = "0" });
            }

            for (int i = 0; i < LevelDatabase.shopItems.Count; i++)
            {
                EditorGUILayout.BeginVertical("box", GUILayout.Width(leftTextureRect.width - 150));
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Reward:");
                LevelDatabase.shopItems[i].coinRew = EditorGUILayout.IntField(LevelDatabase.shopItems[i].coinRew);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Price in $:");
                LevelDatabase.shopItems[i].price = EditorGUILayout.TextField(LevelDatabase.shopItems[i].price);
                EditorGUILayout.EndHorizontal();
                LevelDatabase.shopItems[i].unityIAPId = EditorGUILayout.TextField("Unity IAP ID: ", LevelDatabase.shopItems[i].unityIAPId);
                LevelDatabase.shopItems[i].googlePlayId = EditorGUILayout.TextField("Google Play ID: ", LevelDatabase.shopItems[i].googlePlayId);
                LevelDatabase.shopItems[i].appStoreId = EditorGUILayout.TextField("AppStore IAP ID: ", LevelDatabase.shopItems[i].appStoreId);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove"))
                {
                    LevelDatabase.shopItems.RemoveAt(i);
                }
                if (GUILayout.Button("Move up"))
                {
                    if (i > 0)
                    {
                        Shop.CoinShopItem item = LevelDatabase.shopItems[i];
                        LevelDatabase.shopItems[i] = LevelDatabase.shopItems[i - 1];
                        LevelDatabase.shopItems[i - 1] = item;
                    }
                }
                if (GUILayout.Button("Move down"))
                {
                    if (i < LevelDatabase.shopItems.Count - 1)
                    {
                        Shop.CoinShopItem item = LevelDatabase.shopItems[i];
                        LevelDatabase.shopItems[i] = LevelDatabase.shopItems[i + 1];
                        LevelDatabase.shopItems[i + 1] = item;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        #endregion
    }
    public static async void ProgressBarUpdate()
    {
        float prog = 0;
        while(prog < 1)
        {
            prog += 0.2f;
            loadingValue = prog;
            await Task.Delay(500);
        }
        loadingValue = 0;
    }
    
    public static void UpdateBlocks(int[,] data)
    {
        for(int i = 0; i < updHeight; i++)
        {
            for(int y = 0; y < updWidth; y++)
            {
                switch(block[i, y])
                {
                    case (int)BlockID.nil: field[i, y] = null; break;
                    case (int)BlockID.ice: field[i, y] = ice; break;
                    case (int)BlockID.empty: field[i, y] = empty; break;
                    case (int)BlockID.candy: field[i, y] = candies; break;
                    case (int)BlockID.block: field[i, y] = rock; break;
                }
            }
        }
    }
    public static void ConsoleLog(string message)
    {
        if(DateTime.Now.Hour < 10) editorConsole = $"[0{DateTime.Now.Hour}:{DateTime.Now.Minute}] {message}";
        else editorConsole = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}] {message}";
    }
}
#region Small window
public class NewLevelWin : EditorWindow
{
    EditLevels mainClass;  
    public static NewLevelWin levelNewWin;
    Texture2D back, warningPic, helpText;
    Rect backRect, warningRec, helpRect;

    private int newLevelNum, saveAsNum, selectedLevelInt = 0;
    private string commentFornewLevel, errorField = "";
    private static WindowType curType;
    public static string[] selectLevelPopupInfo = { "" };
    public static int[] popupLevelID;
    public enum WindowType
    {
        newLevel,
        saveLevel,
        saveLevelAs,
        help,
        delete,
        open,
        notification
    }
    private void OnEnable() 
    {
        InitTextures();
    }
    private void InitTextures()// Initialization
    {
        helpText = new Texture2D(1, 1);
        helpText.SetPixel(0, 0, new Color(1, 1, 1, 0));


        back = new Texture2D(1, 1);
        back.SetPixel(0, 0, new Color(1, 1, 1, 0));
        warningPic = Resources.Load<Texture2D>("warning_yellow");
    }

    public static void OpenWindowNew(WindowType type)
    {
        curType = type;
        if(curType == WindowType.newLevel)
        {
            levelNewWin = (NewLevelWin)GetWindow(typeof(NewLevelWin));
            levelNewWin.titleContent = new GUIContent("New level");
            levelNewWin.maxSize = new Vector2(200, 200);
            levelNewWin.minSize = new Vector2(200, 200);
            levelNewWin.Show();
        }
        else if(curType == WindowType.notification)
        {
            levelNewWin = (NewLevelWin)GetWindow(typeof(NewLevelWin));
            levelNewWin.titleContent = new GUIContent(":(");
            levelNewWin.maxSize = new Vector2(300, 100);
            levelNewWin.minSize = new Vector2(300, 100);
            levelNewWin.Show();
        }
        else if(curType == WindowType.saveLevelAs)
        {
            levelNewWin = (NewLevelWin)GetWindow(typeof(NewLevelWin));
            levelNewWin.titleContent = new GUIContent("Save As");
            levelNewWin.maxSize = new Vector2(200, 120);
            levelNewWin.minSize = new Vector2(200, 120);
            levelNewWin.Show();
        }
        else if (curType == WindowType.help)
        {
            levelNewWin = (NewLevelWin)GetWindow(typeof(NewLevelWin));
            levelNewWin.titleContent = new GUIContent("Help");
            levelNewWin.maxSize = new Vector2(400, 350);
            levelNewWin.minSize = new Vector2(400, 350);
            levelNewWin.Show();
        }
        else if (curType == WindowType.delete)
        {
            levelNewWin = (NewLevelWin)GetWindow(typeof(NewLevelWin));
            levelNewWin.titleContent = new GUIContent("Delete level");
            levelNewWin.maxSize = new Vector2(200, 100);
            levelNewWin.minSize = new Vector2(200, 100);
            levelNewWin.Show();
        }
        else if (curType == WindowType.open)
        {
            levelNewWin = (NewLevelWin)GetWindow(typeof(NewLevelWin));
            levelNewWin.titleContent = new GUIContent("Open level");
            levelNewWin.maxSize = new Vector2(200, 120);
            levelNewWin.minSize = new Vector2(200, 120);
            levelNewWin.Show();
        }

    }
    private void OnGUI()
    {
        Draw();
    }
    private void Draw()
    {
        switch (curType)
        {
            case WindowType.delete:
                warningRec.x = 10;
                warningRec.y = 20;
                warningRec.width = 50;
                warningRec.height = 50;
                warningPic.Apply();
                GUI.DrawTexture(warningRec, warningPic);

                backRect.x = 70;
                backRect.y = 10;
                backRect.width = Screen.width - 80;
                backRect.height = Screen.height - backRect.y * 2;
                back.Apply();
                GUI.DrawTexture(backRect, back);
                break;

            case WindowType.notification:
                warningRec.x = 10;
                warningRec.y = 20;
                warningRec.width = 50;
                warningRec.height = 50;
                warningPic.Apply();
                GUI.DrawTexture(warningRec, warningPic);

                backRect.x = 70;
                backRect.y = 10;
                backRect.width = Screen.width - 80;
                backRect.height = Screen.height - backRect.y * 2;
                back.Apply();
                GUI.DrawTexture(backRect, back);
                break;

            case WindowType.saveLevelAs:
                warningRec.x = 10;
                warningRec.y = 20;
                warningRec.width = 50;
                warningRec.height = 50;
                warningPic.Apply();
                GUI.DrawTexture(warningRec, warningPic);

                backRect.x = 70;
                backRect.y = 10;
                backRect.width = Screen.width - 80;
                backRect.height = Screen.height - backRect.y * 2;
                back.Apply();
                GUI.DrawTexture(backRect, back);
                break;
            default:
                backRect.x = 10;
                backRect.y = 10;
                backRect.width = Screen.width - backRect.x * 2;
                backRect.height = Screen.height - backRect.y * 2;
                back.Apply();
                GUI.DrawTexture(backRect, back);
                break;
        }

        GUILayout.BeginArea(backRect);

        if(curType == WindowType.newLevel)
        {
            EditLevels.blockColor1 = new Color(0.41f, 0.43f, 1, 0.7f);
            EditLevels.blockColor2 = new Color(0.53f, 0.56f, 1, 0.7f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("New level number: ");
            newLevelNum = EditorGUILayout.IntField(newLevelNum, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.Label("Comment (optional): ");
            commentFornewLevel = EditorGUILayout.TextArea(commentFornewLevel, GUILayout.Height(50));
            if (GUILayout.Button("Create!", GUILayout.Height(50))) // Create new level
            {
                if (commentFornewLevel == null) commentFornewLevel = "";
                int[,] bl = new int[5, 5];
                for(int i = 0; i < 5; i++)
                {
                    for (int u = 0; u < 5; u++) bl[i, u] = (int)EditLevels.BlockID.nil;
                }
                int[] trg = new int[5];
                EditLevels.currentLvlNum = newLevelNum;
                
                EditLevels.description = commentFornewLevel;
                levelNewWin.Close();
            }
        }
        else if(curType == WindowType.saveLevelAs) // ------------------------------------------------------------------------------------------------------------------------- SAVE LEVEL
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Save as level:");
            saveAsNum = EditorGUILayout.IntField(saveAsNum, GUILayout.Width(50));
            if (saveAsNum < 1) saveAsNum = 1;
            GUILayout.EndHorizontal();
            GUILayout.Label("If such level exists" + System.Environment.NewLine + "it will be replaced.");
            if (GUILayout.Button("Save", GUILayout.Height(50))) // Create new level
            {
                if (saveAsNum < 1) errorField = "All levels nums should begin from 1";
                else if(saveAsNum > 0)
                {
                    int[] target = new int[6];
                    target[0] = EditLevels.redAm;
                    target[1] = EditLevels.greenAm;
                    target[2] = EditLevels.blueAm;
                    target[3] = EditLevels.yellowAm;
                    target[4] = EditLevels.orangeAm;
                    target[5] = EditLevels.purpleAm;
                    Color[] colorToSave = new Color[2];
                    colorToSave[0] = EditLevels.blockColor1;
                    colorToSave[1] = EditLevels.blockColor2;
                    Level saveThis = new Level(saveAsNum, EditLevels.updWidth, EditLevels.updHeight, EditLevels.description, EditLevels.block, target, colorToSave, EditLevels.starTargetScore, 
                        EditLevels.maxGoals, EditLevels.maxScore, EditLevels.candyReward, EditLevels.goalReward);
                    LevelDatabase.SaveLevel(saveAsNum, saveThis);
                    levelNewWin.Close();
                }
                EditLevels.ProgressBarUpdate();
            }
            GUILayout.Label(errorField);
            
        }
        else if(curType == WindowType.notification)
        {
            GUILayout.Label($"The current scene does not contain {System.Environment.NewLine}the required components or {System.Environment.NewLine}the wrong scene is open.");
            if (GUILayout.Button("OK", GUILayout.Height(30)))
            {
                levelNewWin.Close();
            }
        }
        else if (curType == WindowType.delete)
        {
            GUILayout.Label("Delete level " + EditLevels.currentLvlNum + "?");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", GUILayout.Height(50))) // Create new level
            {
                levelNewWin.Close();
            }
            if (GUILayout.Button("Delete", GUILayout.Height(50))) // Delete current level
            {
                LevelDatabase.DeleteLevel(EditLevels.currentLvlNum);
                EditLevels.currentLvlNum = 0;
                levelNewWin.Close();
            }
            GUILayout.EndHorizontal();
        }
        else if (curType == WindowType.open)
        {
            GUILayout.Label("Choose level to open");
            if(selectLevelPopupInfo[0] == "") GUILayout.Label("You have no created levels yet");
            GUILayout.BeginHorizontal();
            selectedLevelInt = EditorGUILayout.Popup(selectedLevelInt, selectLevelPopupInfo);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open", GUILayout.Height(50))) // Create new level
            {
                if(selectLevelPopupInfo[0] != "")
                {
                    OpenLevel(selectedLevelInt);
                    levelNewWin.Close();
                }
            }
            GUILayout.EndHorizontal();
        }
        else if (curType == WindowType.help)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditLevels.candies, GUILayout.Width(50), GUILayout.Height(50))) ;
            GUILayout.Label("Add candy block on play field.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditLevels.ice, GUILayout.Width(50), GUILayout.Height(50))) ;
            GUILayout.Label("Add ice block on play field. ");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditLevels.rock, GUILayout.Width(50), GUILayout.Height(50))) ;
            GUILayout.Label("Add rock block on play field.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditLevels.empty, GUILayout.Width(50), GUILayout.Height(50))) ;
            GUILayout.Label("If you don't want the block to appear " + System.Environment.NewLine + "in this place choose this.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditLevels.eraser, GUILayout.Width(50), GUILayout.Height(50))) ;
            GUILayout.Label("Just eraser, use him to delete blocks.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditLevels.fill, GUILayout.Width(50), GUILayout.Height(50))) ;
            GUILayout.Label("Press this and you see green indicator." + System.Environment.NewLine
                + "Then choose brush to fill and press on play field.");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }
    public void OpenLevel(int num)
    {
        for (int i = 0; i < LevelDatabase.data.levelCount; i++)
        {
            if (LevelDatabase.data.levels[i].levelNum == popupLevelID[num])
            {
                EditLevels.updWidth = LevelDatabase.data.levels[i].fieldWidth;
                EditLevels.updHeight = LevelDatabase.data.levels[i].fieldHeight;
                EditLevels.width = LevelDatabase.data.levels[i].fieldWidth;
                EditLevels.height = LevelDatabase.data.levels[i].fieldHeight;

                EditLevels.redAm = LevelDatabase.data.levels[i].target[0];
                EditLevels.greenAm = LevelDatabase.data.levels[i].target[1];
                EditLevels.blueAm = LevelDatabase.data.levels[i].target[2];
                EditLevels.yellowAm = LevelDatabase.data.levels[i].target[3];
                EditLevels.orangeAm = LevelDatabase.data.levels[i].target[4];
                EditLevels.purpleAm = LevelDatabase.data.levels[i].target[5];

                if (EditLevels.greenAm > 0) EditLevels.cn_gr = true;
                if (EditLevels.blueAm > 0) EditLevels.cn_bl = true;
                if (EditLevels.orangeAm > 0) EditLevels.cn_or = true;
                if (EditLevels.redAm > 0) EditLevels.cn_rd = true;
                if (EditLevels.yellowAm > 0) EditLevels.cn_ye = true;
                if (EditLevels.purpleAm > 0) EditLevels.cn_prpl = true;

                EditLevels.maxGoals = LevelDatabase.data.levels[i].goals;
                EditLevels.starTargetScore = LevelDatabase.data.levels[i].starScore;
                EditLevels.maxScore = LevelDatabase.data.levels[i].maxScore;

                EditLevels.blockColor1 = LevelDatabase.data.levels[i].levelColors[0]; 
                EditLevels.blockColor2 = LevelDatabase.data.levels[i].levelColors[1];

                EditLevels.field = new Texture2D[EditLevels.updHeight, EditLevels.updWidth];
                EditLevels.block = new int[EditLevels.updHeight, EditLevels.updWidth];

                for(int p = 0; p < EditLevels.updHeight; p++)
                {
                    for (int q = 0; q < EditLevels.updWidth; q++)
                    {
                        for(int o = 0; o < EditLevels.updWidth * EditLevels.updHeight; o++)
                        {
                            if (LevelDatabase.data.levels[i].savedBlock[o].x == p && LevelDatabase.data.levels[i].savedBlock[o].y == q)
                                EditLevels.block[p, q] = LevelDatabase.data.levels[i].savedBlock[o].z;
                        }
                    }
                }
                EditLevels.description = LevelDatabase.data.levels[i].comment;
                EditLevels.currentLvlNum = LevelDatabase.data.levels[i].levelNum;
                EditLevels.UpdateBlocks(EditLevels.block);
            }
        }
    }
}
#endregion
