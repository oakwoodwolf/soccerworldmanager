using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using static Enums;
using Random = UnityEngine.Random;


    public class MenuScrollBar : MenuItem
    {
        public const float ScrollBarWidth = 10.0f;
        public const float ScrollBarHeight = 320.0f;
        public RectTransform bar;
        public RectTransform barHandle;   
        public Vector2 minMaxRange;

        public override void Awake()
        {
            base.Awake();
            gameManager.activeMenuScrollBar = this;
        }

        public override void Update()
        {
            base.Update();
            float range = 480.0f + Math.Abs(minMaxRange.y) + Math.Abs(minMaxRange.x);
					
            float scrollBarLen = (480.0f / range) * ScrollBarHeight;
            float scrollStep = (ScrollBarHeight - scrollBarLen)/(range - 480.0f); //SCROLL_BAR_SIZE / range;
            float scrollYoffset = (range - 480.0f) * scrollStep;
					
            float x = (ScrollBarWidth/2);
            float y = ((480 - ScrollBarWidth)/2);
            bar.sizeDelta = new Vector2(x, y);
            
            float w = ScrollBarWidth * 0.75f;
            barHandle.anchoredPosition = new Vector2(x+((ScrollBarWidth-w)/2), scrollYoffset-(scrollStep*gameManager.menuScrollY));
            barHandle.sizeDelta = new Vector2(w, scrollBarLen);

            if (gameManager.menuScrollY < minMaxRange.y)
                gameManager.menuScrollY = minMaxRange.y;
            if (gameManager.menuScrollY > minMaxRange.x)
                gameManager.menuScrollY = minMaxRange.x;
        }
    }