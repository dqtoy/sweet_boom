using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class LevelController : MonoBehaviour {

    [HideInInspector]
    private LevelManager manager;
    [HideInInspector]
    public int levelID, starsCount;
    private RectTransform allObjAttachedToStars;
    private GameObject[] stars = new GameObject[3];
    private GameObject allStars;

    public void OnMouseUpAsButton()
    {
        if(LevelManager.canSelectLevel)
        {
            LevelManager.canSelectLevel = false;
            manager = FindObjectOfType<LevelManager>();
            manager.LevelSelected(levelID, starsCount);
        }
    }
    public void SetStars(int activeStars)
    {
        starsCount = activeStars;
        if(activeStars != 0)
        {
            RectTransform[] parent = gameObject.GetComponentsInChildren<RectTransform>();
            foreach (RectTransform obj in parent) for (int i = 0; i < 3; i++) if (obj.gameObject.name == $"#star{i + 1}complete") stars[i] = obj.gameObject;
            for (int i = 0; i < 3; i++)
            {
                stars[i].gameObject.SetActive(false);
                if (i < activeStars) stars[i].gameObject.SetActive(true);
            }
        }
        else transform.Find("#Stars").gameObject.SetActive(false);
    }
}
