
    using System;
    using UnityEngine;

    public class ShadedBox : MenuItem
    {
        private RectTransform _rectTransform;
        public override void SetText(string textValue)
        {
            int wh = Int32.Parse(textValue);
            int w = (wh>>16)&65535;
            int h = wh&65535;
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.sizeDelta = new Vector2(w, h);
            
        }

        public override void OnValidate()
        {
            //SetText(text);
            AdjustPosition();
        }
    }
