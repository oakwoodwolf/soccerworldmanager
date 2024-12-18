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


        public override void OnValidate()
        {
            SetText(this.text);
            
        }
        public override void SetText(string newText)
        {
            _rectTransform = GetComponent<RectTransform>();
            this.text = newText;
            switch (newText)
            {
                case "nextButton":
                    this.mText.text = "Next>";
                    image.sprite = images[1];
                    _rectTransform.sizeDelta = new Vector2(64, 32);
                    break;
                case "backButton":
                    this.mText.text = "<Back";
                    image.sprite = images[1];
                    _rectTransform.sizeDelta = new Vector2(64, 32);
                    break;
                case "yesButton":
                    this.mText.text = "Yes";
                    image.sprite = images[1];
                    _rectTransform.sizeDelta = new Vector2(64, 32);
                    break;
                case "noButton":
                    this.mText.text = "No";
                    image.sprite = images[1];
                    _rectTransform.sizeDelta = new Vector2(64, 32);
                    break;
                case "quitButton":
                    this.mText.text = "Quit Game";
                    image.sprite = images[0];
                    _rectTransform.sizeDelta = new Vector2(96, 32);
                    break;
                default:
                    this.mText.text = newText;
                    image.sprite = images[0];
                    _rectTransform.sizeDelta = new Vector2(96, 32);
                    break;
            }
        }
        
    }