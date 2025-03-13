using UnityEngine;

public class StaticLeagueData : ScriptableObject
{
    public Enums.LeagueID leagueId;
    public float ticketValue;

    public int minSponsorIncomePerTurn;
    public int maxSponsorIncomePerTurn;
    public int tvIncomePerTurn;

    public int minStarRating;
    public int maxStarRating;
    public string leagueName;

    public void LoadStaticLeagueData(string[] txtData)
    {
        this.leagueId = (Enums.LeagueID)int.Parse(txtData[0]);
        this.ticketValue = float.Parse(txtData[2]);
        this.minSponsorIncomePerTurn = int.Parse(txtData[3]);
        this.maxSponsorIncomePerTurn = int.Parse(txtData[4]);
        this.tvIncomePerTurn = int.Parse(txtData[5]);
        this.minStarRating = int.Parse(txtData[6]);
        this.maxStarRating = int.Parse(txtData[7]);
        this.leagueName = txtData[1];
        this.name = leagueName;
    }
    public override string ToString()
    {
        string str = this.leagueName + ":\t" + this.leagueId + "\n\t TV Income Per-Turn:\t" + this.tvIncomePerTurn;
        return str;
    }
}
