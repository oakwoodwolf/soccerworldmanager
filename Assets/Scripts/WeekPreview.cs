using TMPro;
using UnityEngine;

public class WeekPreview : MenuItem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private TMP_Text teamText;
    public RectTransform rectTransform;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void UpdateText(string newText)
    {
        teamText.text = newText;

    }
}
