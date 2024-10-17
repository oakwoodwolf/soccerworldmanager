using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Player;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int MaxLeagues = 8;
    public const int MaxManagers = 128;
    public const int MaxTeams = 128;
    public const int MaxTeamsInLeague = 32;
    public const int MaxPlayers = 3096;

    [HideInInspector]
    public bool BoolLoadGameSuccessful;
    public int FormationCycle;
    public Vector2 FormationSelectionScrollPos;
    public int CurrentPage;
    public int CurrentNumberOfPage;
    public int ProcessMatchDataStartFrameCount;

    /// <summary>
    /// Stuff to keep track of for end of turn screen
    /// </summary>
    /// 
    [Header("Stats")]
    [Tooltip("Keep previous league pos. for end of turn stats")]
    public int StatsPrevLeaguePos;
    [Tooltip("Income this turn.")]
    public int StatsTurnIncome;
    [Tooltip("Income this turn from ticket sales.")]
    public int StatsTurnIncomeTicketSales;
    [Tooltip("Income this turn from TV sponsors.")]
    public int StatsTurnIncomeSponsorsTV;
    [Tooltip("Expenditure this turn.")]
    public int StatsTurnExpend;
    public int StatsTurnExpendSalary;
    [Tooltip("Actual attendance at last player's match.")]
    public int StatsAttendance;
    [Tooltip("Number of seats at last match.")]
    public int StatsStadiumSeats;

    [Header("League End")]
    [Tooltip("Keep track of the league the user is in.")]
    public int LeagueEndSaveUsersLeague;
    [Tooltip("Number of entries for League End Users Final Standings.")]
    public int LeagueEndNumTeamsInFinalStandings;
    [Tooltip("Entries for League End Users Final Standings.")]
    public int[] LeagueEndUsersFinalStandings= new int[MaxTeamsInLeague];

    [Header("Transfer Screen Data")]
    public int TransferOfferInterestedPlayerId;
    public int TransferOfferInterestedTeamIndex;
    public int TransferOfferOfferValue;

    [Tooltip("Count number of offers per-turn, to limit to 3.")]
    [Range(0,3)]
    public int TransferOfferOffersMadeThisTurn;
    public int CurrentBuyPlayerId;
    public int CurrentBuyPlayerOffer;
    public int NumPlayersOnTransferList;
    public int[] TransferList = new int[MaxPlayers];

    [Header("Match Engine Data")]
    public MatchEngine matchEngine;

    [Header("Static Soccer Data")]
    public int NumberOfLeaguesInArrays;
    public StaticLeagueData[] staticLeaguesData = new StaticLeagueData[MaxLeagues];
    public int NumberOfManagersInArrays;
    public StaticManagerData[] staticManagersDatas = new StaticManagerData[MaxManagers];
    public int NumberOfTeamsInArrays;
    public StaticTeamData[] staticTeamsData = new StaticTeamData[MaxTeams];
    public int NumberOfPlayersInArrays;
    public StaticPlayerData[] staticPlayersData = new StaticPlayerData[MaxPlayers];

    private void Awake()
    {
        matchEngine = GetComponent<MatchEngine>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadGameData()
    {
        int checkSaveDataType = -1;
        List<int> scores = new();
        Dictionary<int, int> dictionary = new();
        if (scores.Count > 0)
        {
            //dictionary = scores[];
        }
    }
    private void SaveGameData() { }
    private void LoadTeams() { }
    private void LoadAndPrepareGame() { }
    private void PrepareChooseTeamMenu(int scenarioId)
    {

    }
    private void CreateGameUsingTeam(int teamId, int scenarioId) { }
    private int getArrayIndexForTeam(int teamId) 
    {
        return 0;
    }
}
