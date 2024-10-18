using UnityEngine;
[CreateAssetMenu]
public class ScenarioInfo : ScriptableObject
{
    public Enums.LeagueID leagueID;
    public int startPointsAdjust;
    public int startMoneyAdjust;
    public int yearsTillRetire;
    public Enums.LeagueID leagueIDPromotion;
    public Enums.LeagueID leagueIDRelegation;
}
