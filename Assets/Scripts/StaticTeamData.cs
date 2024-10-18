using UnityEngine;

public class StaticTeamData : ScriptableObject
{
    public int teamId;
    public int stadiumSeats;
    public Color homeTeam1stColour;
    public Color homeTeam2ndColour;
    public Color awayTeam1stColour;
    public Color awayTeam2ndColour;
    public string teamName;
    public string teamName2;

    public void LoadStaticTeamData(string[] txtData)
    {
        int offset = 0;
        if (txtData.Length > 9) // some teams have an extra parameter for team name 2.
        { 
            teamName2 = FixUpStringName(txtData[2]);
            offset = 1; 
        }
        teamId = int.Parse(txtData[0]);
        //Importing colours. TryParseHtmlString is the only way to import hex.
        ColorUtility.TryParseHtmlString("#" + txtData[5+offset], out homeTeam1stColour);
        ColorUtility.TryParseHtmlString("#" + txtData[6+offset], out homeTeam2ndColour);
        ColorUtility.TryParseHtmlString("#" + txtData[7+offset], out awayTeam1stColour);
        ColorUtility.TryParseHtmlString("#" + txtData[8+offset], out awayTeam2ndColour);

        stadiumSeats = int.Parse(txtData[4+offset]);
        teamName = FixUpStringName(txtData[1]);
    }
    public string FixUpStringName(string name)
    {
        return name.Replace('_', ' ');
    }
}
