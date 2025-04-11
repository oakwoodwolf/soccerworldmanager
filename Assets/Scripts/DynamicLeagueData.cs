using UnityEngine;

[System.Serializable]
public class DynamicLeagueData : ScriptableObject
{
    public int leagueID;
    public int teamId;
    public int matchesPlayed;
    public int goalsFor;
    public int goalsAgainst;
    public int goalDifference;
    public int leaguePoints;
}
