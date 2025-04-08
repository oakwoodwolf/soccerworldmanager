using UnityEngine;
using UnityEngine.Serialization;

public class StaticTeamData : ScriptableObject
{
    public int teamId;
    public int stadiumSeats;
    [FormerlySerializedAs("homeTeam1stColour")]
    public Color homeTeam1StColour;
    [FormerlySerializedAs("homeTeam2ndColour")]
    public Color homeTeam2NdColour;
    [FormerlySerializedAs("awayTeam1stColour")]
    public Color awayTeam1StColour;
    [FormerlySerializedAs("awayTeam2ndColour")]
    public Color awayTeam2NdColour;
    public string teamName;
    public string teamName2;

    public void LoadStaticTeamData(string[] txtData)
    {
        int offset = 0;
        if (txtData.Length > 9) // some teams have an extra parameter for team name 2 in code. i.e. st. morrin
        { 
            teamName2 = FixUpStringName(txtData[2]);
            offset = 1; 
        }
        teamId = int.Parse(txtData[0]);
        //Importing colours. TryParseHtmlString is the only way to import hex.
        ColorUtility.TryParseHtmlString("#" + txtData[5+offset], out homeTeam1StColour);
        ColorUtility.TryParseHtmlString("#" + txtData[6+offset], out homeTeam2NdColour);
        ColorUtility.TryParseHtmlString("#" + txtData[7+offset], out awayTeam1StColour);
        ColorUtility.TryParseHtmlString("#" + txtData[8+offset], out awayTeam2NdColour);

        stadiumSeats = int.Parse(txtData[4+offset]);
        teamName = FixUpStringName(txtData[1]) + teamName2;
        name = teamName;
    }
    public string FixUpStringName(string name)
    {
        return name.Replace('_', ' ');
    }
}
