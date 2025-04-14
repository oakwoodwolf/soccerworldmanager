using System;
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
    private TMP_Text priceText;
    [SerializeField]
    private Image starsSprite;
    public Sprite[] starsArray;
    [SerializeField]
    private Image statusSprite;
    public Sprite[] statusArray;
    public Sprite[] transferArray;
    [SerializeField]
    private Image piesSprite;
    public Sprite[] piesArray;
    [SerializeField]
    private Image flagsSprite;
    public Sprite[] flagsArray;
    public RectTransform rectTransform;
    public DynamicPlayerData playerData;
    
    public int stars;
    [FormerlySerializedAs("textIndex")]
    public int pie;
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
        pie = (int)(playerData.condition * 10.0f);
        pie = Math.Clamp(pie,0,9);
        switch (gameManager.currentScreen)
        {
            case Enums.Screen.SellPlayers:
            case Enums.Screen.TransferOffers:
                training = (playerData.trainingTransfer & GameManager.transferMask) >> GameManager.transferBitShift;
                statusSprite.sprite = transferArray[training];
                break;
            case Enums.Screen.TrainPlayers:
                training = playerData.trainingTransfer & GameManager.trainingMask;
                statusSprite.sprite = statusArray[training];
                break;
        }
        
    }

    private void UpdateFlags()
    {
        if ((playerData.weeksBannedOrInjured &  GameManager.injuryMask) != 0)
            playerFlags = 0;
        else if ((playerData.weeksBannedOrInjured &  GameManager.bannedMask) != 0)
            playerFlags = 1;
        else if ((playerData.flags & GameManager.YellowCardMask) != 0)
            playerFlags = 2;
        if (playerData.condition <  GameManager.ShowInjuredRatio)
            playerFlags = 0;
        flagsSprite.sprite = flagsArray[playerFlags];
    }

    public void FillPlayerValues(int stars, string nameStr, Color color, string teamLikesPositionStr, DynamicPlayerData playerData)
    {
        this.stars = stars;
        this.playerData = playerData;
        this.nameStr = nameStr;
        name = nameStr;
        nameColor = color;
        this.teamLikesPositionStr = teamLikesPositionStr;
        UpdateText();
    }
    
    public void UpdateText()
    {
        pie = (int)(playerData.condition * 10.0f);
        pie = Math.Clamp(pie,0,9);
        flagsSprite.sprite = flagsArray[playerFlags];
        piesSprite.sprite = piesArray[pie];
        switch (gameManager.currentScreen)
        {
            case Enums.Screen.BuyPlayers:
                statusSprite.gameObject.SetActive(false);
                starsSprite.gameObject.SetActive(false);
                priceText.text = "$"+stars+"k";
                menuAction = Enums.MenuAction.BuyPlayerReview;
                break;
            case Enums.Screen.TransferOffers:
                training = playerData.trainingTransfer & (GameManager.transferMask>>GameManager.transferBitShift);
                menuAction = Enums.MenuAction.Null;
                break;
            case Enums.Screen.SellPlayers:
                
                training = playerData.trainingTransfer & (GameManager.transferMask>>GameManager.transferBitShift);
                statusSprite.sprite = transferArray[training];
                starsSprite.gameObject.SetActive(false);
                priceText.gameObject.SetActive(false);
                menuAction = Enums.MenuAction.CyclePlayerTransferStatus;
                break;
            case Enums.Screen.TrainPlayers:
                starsSprite.sprite = starsArray[stars];
                statusSprite.sprite = statusArray[training];
                priceText.gameObject.SetActive(false);
                menuAction = Enums.MenuAction.CyclePlayerTraining;
                break;
            case Enums.Screen.AssignPlayers:
                statusSprite.gameObject.SetActive(false);
                starsSprite.gameObject.SetActive(false);
                priceText.gameObject.SetActive(false);
                menuAction = Enums.MenuAction.AssignPlayerToFormation;
                break;
        }
        nameText.text = nameStr;
        priceText.color = nameColor;
        nameText.color = nameColor;
        statusText.text = teamLikesPositionStr;
    }

}
