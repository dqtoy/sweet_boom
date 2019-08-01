using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject fader;
    [SerializeField] private RectTransform alertPanelRect, losePanelRect, winPanelRect, pausePanelRect;
    [SerializeField] private TextMeshProUGUI scoreTxt, movesTxt, levelTxt;
    [SerializeField] private ParticleSystem winParticles;

    [SerializeField] private int starEdge;

    [SerializeField] private GameObject[] stars;
    [SerializeField] private Slider soundSlider, musicSlider;

    [SerializeField] private Image _headerProgressSlider;
    [SerializeField] private GameObject[] headerStars;
    private GameObject[] headerStars_enabled = new GameObject[3];
    [Header("Name of enabled star gameobject (change with object name).")] [SerializeField] private string starGameobjectName;
    private bool blockHidingOfMessage = false;
    private float curTargetPer = 0;

    // --------------------------------------------------------------------------------------------------------------
    public float headerProgressSlider
    {
        get { return _headerProgressSlider.fillAmount; }
        set { _headerProgressSlider.fillAmount = value; }
    }
    public void goalSet(int value)
    {
        try { movesTxt.text = value.ToString(); }
        catch (Exception ex) { throw new Exception($"Can't convert value: {ex.ToString()}"); }
    }
    private Func<object> func; // if returns 0 - end, 1 - not...
    private int defaultMaxScore { get; set; }

    private void Awake()
    {
        
    }
    private void Start()
    {

    }
    private void Update()
    {
        
    }
    public void ButtonClick()
    {
        SoundController.PlaySound(SoundController.SoundType.clickOpen);
        switch (EventSystem.current.currentSelectedGameObject.name)
        {
            case "#QuitFromLose":
                // TODO
                //StartCoroutine(ShowMessage(new string[1] { "" }, false, ShowUI.gameLose));
                StartCoroutine(GameController.GameSceneFader(() => { SceneManager.LoadScene(0); }, GameController.FaderFunctions.toBlack));
                break;
            case "#QuitFromWin":
                // TODO
                //StartCoroutine(ShowMessage(new string[1] { "" }, false, ShowUI.gameWin));
                StartCoroutine(GameController.GameSceneFader(() => { SceneManager.LoadScene(0); }, GameController.FaderFunctions.toBlack));
                break;
            case "#ResumeButton":
                StartCoroutine(ShowMessage(new string[1] { "" }, false, ShowUI.pause));
                break;
            case "#PauseButton":
                UpdateSliders();
                StartCoroutine(ShowMessage(new string[1] { "" }, true, ShowUI.pause));
                break;
            case "#RetryButton":
                SoundController.PlaySound(SoundController.SoundType.clickOpen);
                StartCoroutine(Menu.Fader(() =>
                {
                    SceneManager.LoadScene(1);
                }, Menu.FaderFunctions.toBlack));
                break;
                
        }
    }
    private void UpdateSliders()
    {
        if (SoundController.musicActive) musicSlider.value = musicSlider.maxValue;
        else musicSlider.value = musicSlider.minValue;

        if (SoundController.soundActive) soundSlider.value = soundSlider.maxValue;
        else soundSlider.value = soundSlider.minValue;
    }
    public void OnSliderValueChanged()
    {
        var slider = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();
        switch (EventSystem.current.currentSelectedGameObject.name)
        {
            case "#SoundSlider":
                if (slider.value == slider.maxValue)
                    SoundController.UpdateSoundSettings(SoundController.SoundSettings.enabled, SoundController.MusicSettings.current);
                else if (slider.value == slider.minValue)
                    SoundController.UpdateSoundSettings(SoundController.SoundSettings.disabled, SoundController.MusicSettings.current);
                break;
            case "#MusicSlider":
                if (slider.value == slider.maxValue)
                    SoundController.UpdateSoundSettings(SoundController.SoundSettings.current, SoundController.MusicSettings.enabled);
                else if (slider.value == slider.minValue)
                    SoundController.UpdateSoundSettings(SoundController.SoundSettings.current, SoundController.MusicSettings.disabled);
                break;
        }

    }
    public enum ShowUI
    {
        defaultAlert,
        gameWin,
        gameLose,
        pause
    }
    /// <summary>
    /// <para>[EN] (Coroutine) Shows a popup UI with dimming background.</para>
    /// <para>[RU] (Корутина) Показывает всплывающее окно с затемнением фона. </para>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="duration"></param>
    /// <param name="ui"></param>
    /// <returns></returns>
    public IEnumerator ShowMessage(string message, int duration, ShowUI ui)
    {
        RectTransform uiToShow = CheckUI(ui);
        uiToShow.Find("#AlertTxt").GetComponent<TextMeshProUGUI>().text = message;
        float yPos = uiToShow.GetComponent<RectTransform>().localPosition.y, target = 0, scalePercent = 0.85f;
        StartCoroutine(GameController.GameSceneFader(null, GameController.FaderFunctions.imperfectBlack));
        int res = 1;
        while (res != 0)
        {
            if (yPos > target) yPos = Mathf.Lerp(yPos, target, scalePercent * Time.deltaTime * 10);
            else if (yPos == target || yPos < target) yPos = Mathf.Lerp(yPos, target - 1503, scalePercent * Time.deltaTime * 10);
            uiToShow.localPosition = new Vector3(uiToShow.localPosition.x, yPos, 0);
            if (yPos < target + 1 && yPos > target)
            {
                uiToShow.localPosition = new Vector3(uiToShow.localPosition.x, target, 0);
                yPos = 0;
                yield return new WaitForSeconds(duration);
            }
            else if (yPos < target - 1500)
            {
                uiToShow.localPosition = new Vector3(uiToShow.localPosition.x, target + 1500, 0);
                StartCoroutine(GameController.GameSceneFader(null, GameController.FaderFunctions.fromImperfectBlack));
                res = 0;
            }
            yield return new WaitForEndOfFrame();
        }
    }
    /// <summary>
    /// <para>[EN] (Coroutine) Shows a popup UI with dimming background.</para>
    /// <para>[RU] (Корутина) Показывает всплывающее окно с затемнением фона. </para>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="show"></param>
    /// <param name="ui"></param>
    /// <param name="p">Stars count for 'gameWin' UI</param>
    /// <returns></returns>
    public IEnumerator ShowMessage(string[] message, bool show, ShowUI ui, params object[] p)
    {
        RectTransform uiToShow = CheckUI(ui);
        
        if (blockHidingOfMessage) yield return new WaitWhile(() => blockHidingOfMessage);
        if((show && !fader.activeSelf) || (!show && fader.activeSelf))
        {
            float yPos = uiToShow.GetComponent<RectTransform>().localPosition.y, target = 0, scalePercent = 0.85f;
            if (show)
            {
                for (int i = 1; i < message.Length + 1; i++)
                {
                    try
                    {
                        uiToShow.Find($"#AlertTxt{i}").GetComponent<TextMeshProUGUI>().text = message[i - 1];
                    }
                    catch (Exception ex) { }
                }
                StartCoroutine(GameController.GameSceneFader(null, GameController.FaderFunctions.imperfectBlack));
                target = 0;
                StartCoroutine(BlockVariable(1));
            }
            else target -= 1500;
            int res = 1;
            while (res != 0)
            {

                yPos = Mathf.Lerp(yPos, target, scalePercent * Time.deltaTime * 10);
                uiToShow.localPosition = new Vector3(uiToShow.localPosition.x, yPos, 0);
                if (yPos < target + 1.5f && yPos > target - 1.5f)
                {
                    uiToShow.localPosition = new Vector3(uiToShow.localPosition.x, target, 0);
                    if(!show) StartCoroutine(GameController.GameSceneFader(null, GameController.FaderFunctions.fromImperfectBlack));
                    if (show && ui == ShowUI.gameWin)
                    {
                        StartCoroutine(StarsShow((int)p[0]));
                        winParticles.Play();
                    }
                    res = 0;
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }
    public IEnumerator StarsShow(int starCount)
    {
        for (int i = 0; i < starCount; i++)
        {
            if (i == 0 || GameController.gameInfo.hasStar[i - 1])
            {
                stars[i].gameObject.SetActive(true);
                SoundController.PlaySound(SoundController.SoundType.clickOpen);
                yield return new WaitForSeconds(0.5f);
            }
            else break;
        }
    }
    private RectTransform CheckUI(ShowUI ui)
    {
        switch(ui)
        {
            case ShowUI.defaultAlert: return alertPanelRect; 
            case ShowUI.gameWin: return winPanelRect;
            case ShowUI.gameLose: return losePanelRect;
            case ShowUI.pause: return pausePanelRect;
        }
        throw new Exception("[Sweet Boom Editor] Undefined ui");
    }
    private IEnumerator BlockVariable(int duration)
    {
        blockHidingOfMessage = true;
        yield return new WaitForSeconds(duration);
        blockHidingOfMessage = false;
    }
    private IEnumerator MoveSlider()
    {
        if(curTargetPer!= headerProgressSlider)
        {
            while (Mathf.Abs(headerProgressSlider - curTargetPer) > 0.001f)
            {
                headerProgressSlider = Mathf.Lerp(headerProgressSlider, curTargetPer, .1f);
                yield return new WaitForEndOfFrame();
            }
            headerProgressSlider = curTargetPer;
        }
    }
    public IEnumerator Loop(Func<bool> arg, float timing)
    {
        bool isOK = false;
        while(!isOK)
        {
            if (arg()) isOK = true;
            yield return new WaitForSeconds(timing);
        }
    }
    public void SetScore(int score) // Set user current score
    {
        curTargetPer = Mathf.InverseLerp(0, defaultMaxScore, score);
        StartCoroutine(MoveSlider());

        for(int i = 0; i < 2; i++)
        {
            if (GameController.gameInfo.hasStar[i] != true && score > GameController.levelData.levelInfo.starScore[i])
            {
                GameController.gameInfo.hasStar[i] = true;
                headerStars_enabled[i].SetActive(true);
            }
        }
        UpdateUI();
    }
    
    public void InitUI()
    {
        defaultMaxScore = GameController.levelData.levelInfo.maxScore;
        for (int i = 0; i < headerStars.Length; i++) headerStars_enabled[i] = headerStars[i].transform.Find(starGameobjectName).gameObject;

        headerProgressSlider = 0;
        for (int i = 0; i < 2; i++)
        {
            headerStars_enabled[i].gameObject.SetActive(false);
            float p = Mathf.InverseLerp(0, GameController.levelData.levelInfo.maxScore, GameController.levelData.levelInfo.starScore[i]);
            headerStars[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(0, starEdge, p), headerStars[i].GetComponent<RectTransform>().anchoredPosition.y);
        }
        levelTxt.text = GameController.levelData.levelNum.ToString();
        scoreTxt.text = "0";
        movesTxt.text = $"{GameController.gameInfo.maxMoves}";
    }
    public void UpdateUI()
    {
        if(GameController.gameInfo.currentScore.ToString() != scoreTxt.text)
        {
            StartCoroutine(Loop(() => {
                if (Convert.ToInt32(scoreTxt.text) == GameController.gameInfo.currentScore) return true;
                else
                {
                    scoreTxt.text = (Convert.ToInt32(scoreTxt.text) + 1).ToString();
                    return false;
                }
            }, .01f));
        }
        movesTxt.text = GameController.gameInfo.currentMoves.ToString();
    }
}
