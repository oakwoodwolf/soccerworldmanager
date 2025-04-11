using UnityEngine;


public class DynamicTeamData : ScriptableObject
{
    public float fanMorale;
    public int cashBalance;
    public Enums.LeagueID leagueID;
    public void LoadDynamicTeamData(string[] txtData, int offset = 0)
    {
        leagueID = (Enums.LeagueID)int.Parse(txtData[2 + offset]);
        cashBalance = int.Parse(txtData[3 + offset]);
    }
}
