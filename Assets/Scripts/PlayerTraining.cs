using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerTraining : MenuItem
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
    public DynamicPlayerData playerData;
    
    public int stars;
    public int textIndex;
    public new int playerFlags;
    public int training;
    public string nameStr;
    public string teamLikesPositionStr;
    public Color nameColor;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public override void Update()
    {
        base.Update();
        UpdateFlags();
        textIndex = (int)(playerData.condition * 10.0f);
        if (textIndex < 0) textIndex = 0; if (textIndex > 9) textIndex = 9;
        statusSprite.sprite = statusArray[training];
        training = playerData.trainingTransfer & GameManager.trainingMask;
        statusSprite.sprite = statusArray[training];
    }

    private void UpdateFlags()
    {
        if ((playerData.weeksBannedOrInjured &  GameManager.injuryMask) != 0)
        {
            playerFlags = 0;
        }
        else if ((playerData.weeksBannedOrInjured &  GameManager.bannedMask) != 0)
        {
            playerFlags = 1;
        }
        else if ((playerData.flags & GameManager.YellowCardMask) != 0)
        {
            playerFlags = 2;
        }
        if (playerData.condition <  GameManager.ShowInjuredRatio)
        {
            playerFlags = 0;
        }
        flagsSprite.sprite = flagsArray[playerFlags];
    }

    public void FillPlayerValues(int stars, string nameStr, Color color, string teamLikesPositionStr, DynamicPlayerData playerData)
    {
        this.stars = stars;
        this.playerData = playerData;
        this.nameStr = nameStr;
        nameColor = color;
        this.teamLikesPositionStr = teamLikesPositionStr;
        UpdateText();
    }
    
    public void UpdateText()
    {
        
        flagsSprite.sprite = flagsArray[playerFlags];
        starsSprite.sprite = starsArray[stars];
        piesSprite.sprite = piesArray[textIndex];
        statusSprite.sprite = statusArray[training];
        nameText.text = nameStr;
        nameText.color = nameColor;
        statusText.text = teamLikesPositionStr;
    }
}
