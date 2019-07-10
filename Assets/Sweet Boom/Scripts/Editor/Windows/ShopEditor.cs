using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(Shop))]
public class ShopEditor : Editor
{
    /*
    private List<Shop.CoinShopItem> itemList = new List<Shop.CoinShopItem>();
    private void OnEnable() {
        LevelDatabase.Initialization();
        itemList = LevelDatabase.shopItems;
    }
    public override void OnInspectorGUI()
    {
        if(itemList.Count > 0)
        {
            for(int i = 0; i < itemList.Count; ++i)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Shop item {i + 1}", EditorStyles.boldLabel);
                itemList[i].price = EditorGUILayout.FloatField($"Price in $", itemList[i].price);
                itemList[i].coinRew = EditorGUILayout.IntField($"Coin reward", itemList[i].coinRew);
                if(GUILayout.Button("Remove item", GUILayout.MaxWidth(200))) itemList.Remove(itemList[i]);
                EditorGUILayout.EndVertical();
            }
        }
        if(GUILayout.Button("Add item"))
        {
            itemList.Add(new Shop.CoinShopItem());
        }
        if(GUILayout.Button("Save changes"))
        {
            LevelDatabase.shopItems = itemList;
            EditorUtility.DisplayDialog("Sweet Boom Editor", "Your shop items saved successfully", "Yay!");
        }
        base.OnInspectorGUI();
    }
    */
}
