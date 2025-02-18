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
    private TMP_Text smallText;
    [SerializeField]
    private TMP_Text bigText;
    [SerializeField]
    private Image image;
    [SerializeField]
    public float iconBarBigFontScale = 1.0f;
    [SerializeField]
    public float iconBarSmallFontScale = 0.8f;
    [SerializeField]
    public Sprite[] icons;
    public MenuIconBar menuIconBar;
    public override void SetText(string newText)
    {
        mText.text = newText;
        if (menuIconBar == null)
        {
            switch (newText)
            {
                case "English Premium League":
                    smallText.text = "Take your favourite team to the top of the Premium League!";
                    image.sprite = icons[0];
                    param = 0;
                    break;
                case "E.P.L. Problems":
                    smallText.text = "Start with -15 points. Avoid relegation from the League!";
                    image.sprite = icons[1];
                    param = 1;
                    break;
                case "League USA":
                    smallText.text = "A coast to coast battle for the \\ntop of the American League!";
                    image.sprite = icons[2];
                    param = 5;
                    break;
                case "Chumpionship League":
                    smallText.text = "Can you earn a promotion to \\nthe Premium League?";
                    image.sprite = icons[0];
                    param = 2;
                    break;
                case "Chumpionship Chaos":
                    smallText.text = "Start with -15 points. Avoid relegation from the League!";
                    image.sprite = icons[1];
                    param = 3;
                    break;
                case "League of Scotland":
                    smallText.text = "Take your favourite Scottish team to the top of the League!";
                    image.sprite = icons[0];
                    param = 4;
                    break;
            
                case "Dog It":
                    smallText.text = "A cunningly timed dog on the pitch.";
                    image.sprite = icons[3];
                    param = 0;
                    break;
                case "Spectacles":
                    smallText.text = "Did you see that?";
                    image.sprite = icons[3];
                    param = 1;
                    break; 
                case "Foot Loose":
                    smallText.text = "Getting away with anything?";
                    image.sprite = icons[3];
                    param = 2;
                    break;
            }

        }
        else
        {
            smallText.text = menuIconBar.textSmall;
            bigText.text = menuIconBar.textBig;
            image.sprite = menuIconBar.texture;
        }
    }
    public override void OnValidate()
    {
        SetText(this.text);
        AdjustPosition();
    }

    public void Populate(Sprite icon, string bigText, string smallText)
    {
        image.sprite = icon;
        this.bigText.text = bigText;
        this.smallText.text = smallText;
    }
}
