using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
    public Sprite[] starsArray;
    [SerializeField]
    private Image shirt;
    [SerializeField]
    private Image shirt2;
    [SerializeField]
    private Image shirtOutline;
    [SerializeField]
    private Image piesSprite;
    public Sprite[] piesArray;
    [SerializeField]
    private Image conditionSprite;
    public Sprite[] conditionsArray;
    public RectTransform rectTransform;
    public DynamicPlayerData playerData;
    public FormationInfo formationInfo;
    public int stars;
    public int textIndex;
    public new int playerFlags;
    public int training;
    public string nameStr;
    public string teamLikesPositionStr;
    public Color primaryColor;
    public Color secondaryColor;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public override void Update()
    {
        base.Update();
        textIndex = (int)(playerData.condition * 10.0f);
    }
    
    public void FillPlayerValues(int stars, string nameStr, Color color, string teamLikesPositionStr, DynamicPlayerData playerData)
    {
        this.stars = stars;
        this.playerData = playerData;
        this.nameStr = nameStr;
        this.teamLikesPositionStr = teamLikesPositionStr;
        UpdateText();
    }
    
    public void UpdateText()
    {
        
        starsSprite.sprite = starsArray[stars];
        piesSprite.sprite = piesArray[textIndex];
        shirt.color = primaryColor;
        shirt2.color = secondaryColor;
        statusText.text = teamLikesPositionStr;
    }
}
