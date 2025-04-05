
using UnityEngine;

public class MenuItemGenerator : MonoBehaviour
{

    [SerializeField]
    private GameObject[] prefabs;
    [SerializeField]
    public int playerTrainingYOffset = 96;
    [SerializeField]
    public int menuBarHeight = 32;
    [SerializeField]
    public float standingsTextFontScale = 0.85f;
    [SerializeField]
    public int menuBarSpace = 16;
    [SerializeField]
    public int menuBarSpacing = 48;
    [SerializeField]
    public float menuBarFontScale = 1.3f;

    public void Awake()
    {
        menuBarSpacing = (menuBarSpace + menuBarHeight);
    }
    public MenuItem GenerateMenuItem(ScreenDefinition screen, Enums.MenuElement type, Vector2 pos, int align, uint flag, string text, Enums.MenuAction action, int param, Transform parent = null,float fontScale=0)
    {
        Transform newParent = screen.MenuItems.transform;
        if (parent != null)
        {
             newParent = parent;
        }
        GameObject newObj = Instantiate(prefabs[(int)type], newParent, false);
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        newObj.name = text;
        MenuItem menuItem = newObj.GetComponent<MenuItem>();
        menuItem.type = type;
        menuItem.pos = new Vector2(pos.x, -pos.y);
        menuItem.alignment = align;
        if (fontScale > 0)
        {
            menuItem.mText.fontSize = fontScale;
        }
        menuItem.SetText(text);
        menuItem.AddListener(action, param);
        menuItem.transform.SetSiblingIndex(1);
        Debug.Log("Generated menu item");
        return menuItem;
    }
    public void CreateStandings(ScreenDefinition screen, Vector2 pos, int teamId, bool isSelf, string nameText, int matchesPlayed, int leaguePoints, int goalDifference)
    {
        Transform child = screen.MenuItems.transform;
        GameObject newObj = Instantiate(prefabs[12], child, false);
        TeamStandings standings = newObj.GetComponent<TeamStandings>();
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        newObj.name = nameText;
        standings.pos = new Vector2(pos.x, -pos.y);
        standings.FillTeamValues(teamId, nameText, matchesPlayed, goalDifference, leaguePoints, isSelf);
        Debug.Log("Created standings for " + standings.teamName);
    }
    public void CreateWeekPreview(ScreenDefinition screen, Vector2 pos, string nameText)
    {
        GameObject newObj = Instantiate(prefabs[13], screen.MenuItems.transform, false);
        WeekPreview weekPreview = newObj.GetComponent<WeekPreview>();
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        weekPreview.pos = new Vector2(pos.x, -pos.y);
        newObj.name = nameText;
        weekPreview.UpdateText(nameText);
    }

    public void CreatePlayerTrainings(ScreenDefinition screen, Vector2 pos, int stars, string nameString, Color color, string statusString, DynamicPlayerData playerData)
    {
        Transform child = screen.MenuItems.transform;
        GameObject newObj = Instantiate(prefabs[14], child, false);
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        PlayerTraining playerTraining = newObj.GetComponent<PlayerTraining>();
        rectTransform.anchoredPosition = pos;
        playerTraining.pos = new Vector2(pos.x, -pos.y);
        playerTraining.FillPlayerValues(stars,nameString,color,statusString, playerData);
        playerTraining.transform.SetSiblingIndex(1);
    }

    public void GenerateFormationMarker(ScreenDefinition screen, Vector2 pos, int param, string name = "marker")
    {
        Transform child = screen.transform.GetChild(1);
        GameObject newObj = Instantiate(prefabs[15], child, false);
        newObj.name = name;
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        PitchMarker marker = newObj.GetComponent<PitchMarker>();
        marker.param = param;
    }
    /**
     * Generates shirts, which depicts players assigned at a formation in AssignPlayers
     */
    public void GenerateShirt(ScreenDefinition screen, Vector2 pos, int stars, string nameStr, Color color1, Color color2, int teamLikesPosition, DynamicPlayerData playerData)
    {
        Transform child = screen.transform.GetChild(1);
        GameObject newObj = Instantiate(prefabs[16], child, false);
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        Shirt shirt = newObj.GetComponent<Shirt>();
        shirt.FillPlayerValues(stars, nameStr, color1, color2, teamLikesPosition, playerData);
    }
}
