using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.Serialization;

public class MenuItemGenerator : MonoBehaviour
{

    [SerializeField]
    private GameObject[] prefabs = new GameObject[(int)Enums.MenuElement.Max];
    [SerializeField]
    public int menuBarHeight = 32;
    [SerializeField]
    public int menuBarSpace = 16;
    [SerializeField]
    public int menuBarSpacing = 48;
    [SerializeField]
    public float menuBarFontScale = 1.3f;

    public void Awake()
    {
        menuBarSpacing = (menuBarSpace + menuBarHeight);
    }

    public void GenerateMenuItem(ScreenDefinition screen, Enums.MenuElement type, Vector2 pos, int align, uint flag, string text, Enums.MenuAction action, int param)
    {
        GameObject newObj = Instantiate(prefabs[(int)type], screen.MenuItems.transform, false);
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        newObj.name = text;
        MenuItem menuItem = newObj.GetComponent<MenuItem>();
        menuItem.SetText(text);
        menuItem.AddListener(action, param);
    }
}
