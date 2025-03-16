
    using System;
    using UnityEngine;

    public class ShadedBox : MenuItem
    {
        private RectTransform _rectTransform;
        public int w;
        public int h;
        /// <summary>
        /// Parses the string to width and height, the <i>left number</i> bit-shifted is the width, and the last number is the height. 
        /// </summary>
        /// <param name="textValue"></param>
        public override void SetText(string textValue)
        {
            
            
            string[] parts = textValue.Replace("(", "").Replace(")", "").Split(new[] { "<<", "|" }, StringSplitOptions.RemoveEmptyEntries);
        
            if (parts.Length == 3 && int.TryParse(parts[0], out int width) && int.TryParse(parts[2], out int height))
            {
                w = width;
                h = height;
                _rectTransform = GetComponent<RectTransform>();
                _rectTransform.sizeDelta = new Vector2(w, h);
            }
           
            
        }

        public override void OnValidate()
        {
            SetText(text);
            AdjustPosition();
        }
    }
