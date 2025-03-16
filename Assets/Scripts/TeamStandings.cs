using TMPro;
using UnityEngine;

public class TeamStandings : MenuItem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private TMP_Text teamText;
    [SerializeField]
    private TMP_Text teamNameText;
    [SerializeField]
    private TMP_Text matchesPlayedText;
    [SerializeField]
    private TMP_Text goalDifferenceText;
    [SerializeField]
    private TMP_Text leaguePointsText;
    public RectTransform rectTransform;
    
    public int teamNumber;
    public string teamName;
    public int matchesPlayed;
    public int goalDifference;
    public int leaguePoints;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    

    public void FillTeamValues(int teamNumber, string teamName, int matchesPlayed, int goalDifference, int leaguePoints, bool isSelf)
    {
        this.teamNumber = teamNumber;
        this.teamName = teamName;
        this.matchesPlayed = matchesPlayed;
        this.goalDifference = goalDifference;
        this.leaguePoints = leaguePoints;
        UpdateText(isSelf);
    }
    
    public void UpdateText(bool isSelf)
    {
        teamText.text = teamNumber.ToString();
        teamNameText.text = teamName;
        matchesPlayedText.text = matchesPlayed.ToString();
        goalDifferenceText.text = goalDifference.ToString();
        leaguePointsText.text = leaguePoints.ToString();
        Color mainColor; Color goalColor;
        if (isSelf)
        {
            mainColor = new Color(255, 255, 255*0.8f, 255);
            goalColor = new Color(255*0.9f, 255*0.9f, 255*0.7f, 255);
        }
        else
        {
            mainColor = new Color(255*0.7f, 255*0.8f, 255*0.9f, 255);
            goalColor = new Color(255*0.6f, 255*0.7f, 255*0.8f, 255);
        }
        teamText.color = mainColor;
        teamNameText.color = mainColor;
        matchesPlayedText.color = mainColor;
        goalDifferenceText.color = goalColor;
        leaguePointsText.color = mainColor;
    }
}
