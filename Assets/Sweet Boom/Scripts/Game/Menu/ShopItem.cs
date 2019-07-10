using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public int itemIndex { get; set; }
    public TextMeshProUGUI reward, price;
    public float itemPrice { get; set; }
    public int itemReward { get; set; }
}
