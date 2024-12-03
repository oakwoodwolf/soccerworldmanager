using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IconBar : MenuItem
{
    [SerializeField]
    private TMP_Text mText;
    GameManager _gameManager;
    [SerializeField]
    public float iconBarBigFontScale = 1.0f;
    [SerializeField]
    public float iconBarSmallFontScale = 0.8f;
    public override void SetText(string newText)
    {
        
    }
}
