using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shirt : MenuItem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TMP_Text statusText;
    [SerializeField]
    private Image starsSprite;
    [SerializeField]
    private Image shirt;
    [SerializeField]
    private Image shirt2;
    [SerializeField]
    private Image shirtOutline;
    [SerializeField]
    private Image piesSprite;
    [SerializeField]
    private Image conditionSprite;
    [SerializeField]
    private Image cardImage;
    [SerializeField]
    private Image happyImage;
    public RectTransform rectTransform;
    public DynamicPlayerData playerData;
    [Tooltip("The player's Stars rating")]
    public int stars;
    [Tooltip("The player's condition, depicted as a wheel")]
    public int condition;
    [Tooltip("Whether the player has red or yellow flags.")]
    public Enums.Texture cardTexture;

    public string nameStr;
    [Tooltip("Whether the player likes the position")]
    public bool isHappyInFormation;
    [Tooltip("Primary Shirt Colour for Team")]
    public Color primaryColor;
    [Tooltip("Shirt Accent Colour for Team")]
    public Color secondaryColor;
    
    
    /**
     *
     * private void CreateShirt(float shirtX, float shirtY, Color primaryColor, Color secondaryColor, int dataIndex,
        int nameIndex, int playerId, FormationData formation, int i)
    {
        Image shirt1 = RectMake(shirtX - 32, shirtY-32,64, 64, Enums.Texture.Shirt1Stcolour).GetComponent<Image>();
        shirt1.color = primaryColor;
        Image shirt2 = RectMake(shirtX - 32, shirtY-32,64, 64,Enums.Texture.Shirt2Ndcolour).GetComponent<Image>();
        shirt2.color = secondaryColor;
        Image shirtOutline = RectMake(shirtX - 32, shirtY-32,64, 64,Enums.Texture.ShirtOutline).GetComponent<Image>();
        int cardTexture = -1;
        int textIndex = (int)(dynamicPlayersData[dataIndex].condition * 10.0f);
        if (textIndex < 0) textIndex = 0;
        if (textIndex > 9) textIndex = 9;
        Image pie = RectMake(shirtX-12, shirtY-12,24, 24,(Enums.Texture)(int)(Enums.Texture.Pie10+textIndex)).GetComponent<Image>();
        if (nameIndex != -1)
        {
            int res = CheckPlayerIdIsHappyInFormation(playerId, formation.formations[i]);
            if (res != 0)
            {
                RectMake(shirtX - 11, shirtY - 11, 22, 22,iconHappy);
            }
            else
            {
                RectMake(shirtX - 11, shirtY - 11, 22, 22,iconSad);

            }
            int playerIndex = GetPlayerDataIndexForPlayerID(playerId);
            if (playerIndex != -1)
            {
                int stars = (int)GetTeamLeagueAdjustedStarsRatingForPlayerIndex(playerIndex);
                if (stars < 0) stars = 0;
                if (stars > 5) stars = 5;
                if (stars > 0)
                    RectMake(shirtX - 23, shirtY-46,52, 26, Enums.Texture.Stars1+(stars-1));
            }
        }
    }
     */
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public override void Update()
    {
        base.Update();
        condition = (int)(playerData.condition * 10.0f);
    }
    
    public void FillPlayerValues(int stars, string nameStr, Color color1, Color color2, int teamLikesPosition, DynamicPlayerData playerData)
    {
        this.stars = stars;
        this.playerData = playerData;
        this.name = playerData.name;
        this.nameStr = name;
        primaryColor = color1;
        secondaryColor = color2;
        isHappyInFormation = teamLikesPosition > 0;
        condition = (int)(playerData.condition * 10.0f);
        //Card Logic
        if ((playerData.flags & GameManager.YellowCardMask) == 1)
            cardTexture = Enums.Texture.Cards1Yellow;
        else if ((playerData.flags & GameManager.YellowCardMask) == 2)
            cardTexture = Enums.Texture.Cards2Yellow;
        if ((playerData.flags & GameManager.RedCardMask) != 0)
            cardTexture = Enums.Texture.Cards1Red;
        UpdateText();
    }
    
    public void UpdateText()
    {
        starsSprite.sprite = gameManager.textures[(int)Enums.Texture.Stars1+(stars-1)];
        piesSprite.sprite = gameManager.textures[(int)Enums.Texture.Pie10+ (condition-1)];
        shirt.color = primaryColor;
        shirt2.color = secondaryColor;
        nameText.text = nameStr;
        happyImage.sprite = isHappyInFormation ? gameManager.iconHappy : gameManager.iconSad;
        if ((int)cardTexture>0) cardImage.sprite = gameManager.textures[(int)cardTexture];
    }
}
