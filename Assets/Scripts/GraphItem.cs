


using System;
using UnityEngine;

public class GraphItem : MenuItem
    {
       public MenuGraph graph;
       [SerializeField]
       private RectTransform backDrop;
       [SerializeField]
       private RectTransform zeroHorizontal;
       [SerializeField]
       private GameObject interval;
       [SerializeField]
       private GameObject value;
       public int[] data;
       int minValue;
       int maxValue;

       public void Start()
       {
           data = new int[graph.numInArray];
       }

       public override void OnValidate()
        {
           AdjustPosition();
        }
        public void SetGraph()
        {
            RectTransform.sizeDelta = graph.size;
            backDrop.sizeDelta = new Vector2(graph.size.x+8, graph.size.y+32);
            data = graph.array;
            minValue = (int)graph.defaultMinMax.x;
            maxValue = (int)graph.defaultMinMax.y;
            int x = (int)RectTransform.anchoredPosition.x;
            int y = (int)RectTransform.anchoredPosition.y;
            int w = (int)RectTransform.sizeDelta.x;
            int h = (int)RectTransform.sizeDelta.y;
            mText.text = graph.title;
            for (int lp = 0; lp < (graph.numInArray); lp++)
            {
                if (data[lp] < minValue)
                    minValue = data[lp];
                if (data[lp] > maxValue)
                    maxValue = data[lp];
            }

            int range = (Math.Abs(maxValue) + Math.Abs(minValue));
            float yRatio = h / (float)range;
            float zeroLine = y -((0 - minValue)*yRatio);
            zeroHorizontal.anchoredPosition = new Vector2( zeroHorizontal.anchoredPosition.x, zeroLine);
            
            // line intervals on the zero-line
            int stepx = w;
            if (graph.numInArray > 1)
                stepx = w/ (graph.numInArray - 1);
            for (int lp = 0; lp < graph.numInArray; lp++)
            {
                RectTransform newInterval = Instantiate(interval, transform, false).GetComponent<RectTransform>();
                newInterval.name = "Interval " + lp;
                newInterval.anchoredPosition = new Vector2(lp*stepx, zeroLine+2);
            }
            
            // Thicker Data Lines
            // TODO make this a line
            for (int lp = 0; lp < graph.numInArray-1; lp++)
            {
                float startValue = data[lp];
                float endValue;
                if (graph.numInArray > 1)
                    endValue = data[lp + 1];
                else
                    endValue = startValue;
                startValue /= range;
                endValue /= range;
                RectTransform dataValue = Instantiate(value, transform, false).GetComponent<RectTransform>();
                dataValue.anchoredPosition = new Vector2(lp*stepx, zeroLine+(startValue*h));
                dataValue.sizeDelta = new Vector2( ((lp + 1)*stepx) - lp*stepx, Math.Abs((zeroLine + (endValue * h))-(zeroLine + (startValue * h))));
                dataValue.name = "Data " + startValue;

            }

        }
        
    }