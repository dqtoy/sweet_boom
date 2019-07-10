﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Create item (sweets etc.)", fileName = "New block", order = 51)]
public class Blocks : ScriptableObject {

    [HideInInspector]
    public GameObject item;
    public string nameOfItem;
    public int priceInStore;
    public Sprite itemSprite;
}
