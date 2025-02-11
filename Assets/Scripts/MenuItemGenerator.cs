
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
    public int menuBarSpace = 16;
    [SerializeField]
    public int menuBarSpacing = 48;
    [SerializeField]
    public float menuBarFontScale = 1.3f;

    public void Awake()
    {
        menuBarSpacing = (menuBarSpace + menuBarHeight);
    }
    public void GenerateMenuItem(ScreenDefinition screen, Enums.MenuElement type, Vector2 pos, int align, uint flag, string text, Enums.MenuAction action, int param, Transform parent = null)
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
        menuItem.SetText(text);
        menuItem.AddListener(action, param);
        menuItem.transform.SetSiblingIndex(1);
        Debug.Log("Generated menu item");
    }
    public void CreateStandings(ScreenDefinition screen, Vector2 pos, int teamId, bool isSelf, string nameText, int matchesPlayed, int leaguePoints, int goalDifference)
    {
        Transform child = screen.MenuItems.transform.GetChild(0);
        GameObject newObj = Instantiate(prefabs[12], child, false);
        TeamStandings standings = newObj.GetComponent<TeamStandings>();
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        newObj.name = nameText;
        standings.pos = new Vector2(pos.x, -pos.y);
        standings.FillTeamValues(teamId, nameText, matchesPlayed, goalDifference, leaguePoints, isSelf);
        Debug.Log("Created standings for " + standings.teamName);
        standings.transform.SetSiblingIndex(1);
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

    public void CreatePlayerTrainings(ScreenDefinition screen, Vector2 pos, int stars, int training, int flags, int textIndex, string nameString, Color color, string statusString)
    {
        Transform child = screen.MenuItems.transform;
        GameObject newObj = Instantiate(prefabs[14], child, false);
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        PlayerTraining playerTraining = newObj.GetComponent<PlayerTraining>();
        rectTransform.anchoredPosition = pos;
        playerTraining.pos = new Vector2(pos.x, -pos.y);
        playerTraining.FillPlayerValues(training,flags,stars,textIndex,nameString,color,statusString);
        playerTraining.transform.SetSiblingIndex(1);
    }
}
