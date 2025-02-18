using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleBar : MenuItem
{

    [SerializeField]
    private Image logo;
    public override void OnValidate()
    {
        base.OnValidate();
        if ((flags & Enums.MenuElementFlag.MatchEngineTitleBar) == (Enums.MenuElementFlag)0)
        {
            int width = 96;
            if ((flags & Enums.MenuElementFlag.Size64TitleBarIcon) != (Enums.MenuElementFlag)0)
            {
                width = 64;
            }
            logo.rectTransform.anchoredPosition = new Vector2(0, (480 - width)*-1);
            logo.rectTransform.sizeDelta = new Vector2(width, width);
        }
    }
    public override void Update()
    {
        base.Update();
        if ((flags & Enums.MenuElementFlag.MatchEngineTitleBar) == (Enums.MenuElementFlag)0)
        {
            int width = 96;
            if ((flags & Enums.MenuElementFlag.Size64TitleBarIcon) != (Enums.MenuElementFlag)0)
            {
                width = 64;
            }
            logo.rectTransform.anchoredPosition = new Vector2(0, (480 - width)*-1);
            logo.rectTransform.sizeDelta = new Vector2(width, width);
        }
    }
}
