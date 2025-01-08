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
    private GameObject[] prefabs;
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
        menuItem.type = type;
        menuItem.pos = pos;
        menuItem.alignment = align;
        menuItem.SetText(text);
        menuItem.AddListener(action, param);
        Debug.Log("Generated menu item");
    }
    public void CreateStandings(ScreenDefinition screen, Vector2 pos, int teamId, bool isSelf, string nameText, int matchesPlayed, int leaguePoints, int goalDifference)
    {
        GameObject newObj = Instantiate(prefabs[12], screen.MenuItems.transform, false);
        TeamStandings standings = newObj.GetComponent<TeamStandings>();
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        newObj.name = nameText;
        standings.FillTeamValues(teamId, nameText, matchesPlayed, goalDifference, leaguePoints, isSelf);
        Debug.Log("Created standings for " + standings.teamName);
    }
}
