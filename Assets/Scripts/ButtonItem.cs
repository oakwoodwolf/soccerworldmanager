using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class ButtonItem : MenuItem
    {
        private RectTransform _rectTransform;
        [SerializeField]
        private Sprite[] images;
        [SerializeField]
        private Image image;
        [SerializeField]
        public MenuButton button;


        public override void OnValidate()
        {
            SetText(this.text);
            AdjustPosition();
        }
        public override void SetText(string newText)
        {
            _rectTransform = GetComponent<RectTransform>();
            if (button != null)
            {
                image.sprite = button.texture;
                _rectTransform.sizeDelta = button.size;
                if (button.textureSelected != null && button.flags == 1)
                {
                    image.sprite = button.textureSelected;
                    
                }
                mText.text = button.text;
            }
            else
            {
                text = newText;
                switch (newText)
                {
                    case "pageButton":
                        mText.text = "Page 1/3";
                        image.sprite = images[0];
                        _rectTransform.sizeDelta = new Vector2(96, 32);
                        break;
                    case "pageButtonPrev":
                        mText.text = "";
                        image.sprite = images[3];
                        _rectTransform.sizeDelta = new Vector2(32, 32);
                        break;
                    case "pageButtonNext":
                        mText.text = "";
                        image.sprite = images[4];
                        _rectTransform.sizeDelta = new Vector2(32, 32);
                        break;
                    case "nextButton":
                        mText.text = "Next>";
                        image.sprite = images[1];
                        _rectTransform.sizeDelta = new Vector2(64, 32);
                        break;
                    case "backButton":
                        mText.text = "<Back";
                        image.sprite = images[1];
                        _rectTransform.sizeDelta = new Vector2(64, 32);
                        break;
                    case "yesButton":
                        mText.text = "Yes";
                        image.sprite = images[1];
                        _rectTransform.sizeDelta = new Vector2(64, 32);
                        break;
                    case "noButton":
                        mText.text = "No";
                        image.sprite = images[1];
                        _rectTransform.sizeDelta = new Vector2(64, 32);
                        break;
                    case "quitButton":
                        mText.text = "Quit Game";
                        image.sprite = images[0];
                        _rectTransform.sizeDelta = new Vector2(96, 32);
                        break;
                    case "optionOffButton":
                        mText.text = "Off";
                        if (flags == Enums.MenuElementFlag.HideItem)
                        {
                            image.sprite = images[2];
                        }
                        else
                        {
                            image.sprite = images[0];
                        }
                        _rectTransform.sizeDelta = new Vector2(96, 32);
                        break;
                    case "optionOnButton":
                        mText.text = "On";
                        if (flags == Enums.MenuElementFlag.HideItem)
                        {
                            image.sprite = images[2];
                        }
                        else
                        {
                            image.sprite = images[0];
                        }
                        
                        _rectTransform.sizeDelta = new Vector2(96, 32);
                        break;
                    case "useMatchbreakerButton":
                        mText.text = "";
                        image.sprite = images[5];
                        _rectTransform.sizeDelta = new Vector2(48, 48);
                        break;
                    case "chgMatchHomeformationButton":
                        mText.text = "";
                        image.sprite = images[6];
                        _rectTransform.sizeDelta = new Vector2(48, 48);
                        break;
                    default:
                        mText.text = newText;
                        image.sprite = images[0];
                        _rectTransform.sizeDelta = new Vector2(96, 32);
                        break;
                }
            }

            if ((flags & Enums.MenuElementFlag.ButtonSmallerFont) != (Enums.MenuElementFlag)0)
            {
                mText.fontSize = 18 * 0.8f;
            }
        }
        
    }