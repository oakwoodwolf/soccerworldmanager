using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTraining : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TMP_Text statusText;
    [SerializeField]
    private Image starsSprite;
    public Sprite[] starsArray;
    [SerializeField]
    private Image statusSprite;
    public Sprite[] statusArray;
    [SerializeField]
    private Image piesSprite;
    public Sprite[] piesArray;
    [SerializeField]
    private Image flagsSprite;
    public Sprite[] flagsArray;
    public RectTransform rectTransform;
    
    public int stars;
    public int textIndex;
    public int flags;
    public int training;
    public string nameStr;
    public string teamLikesPositionStr;
    public Color nameColor;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void FillPlayerValues(int training,int flags,int stars, int textIndex, string nameStr, Color color, string teamLikesPositionStr)
    {
        this.stars = stars;
        this.textIndex = textIndex;
        this.flags = flags;
        this.training = training;
        this.nameStr = nameStr;
        nameColor = color;
        this.teamLikesPositionStr = teamLikesPositionStr;
        UpdateText();
    }
    
    public void UpdateText()
    {
        
        flagsSprite.sprite = flagsArray[flags];
        starsSprite.sprite = starsArray[stars];
        piesSprite.sprite = piesArray[textIndex];
        statusSprite.sprite = statusArray[training];
        nameText.text = nameStr;
        nameText.color = nameColor;
        statusText.text = teamLikesPositionStr;
    }
}
