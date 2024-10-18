using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    //Constants
    public const int MaxSponsors = 3;
    public const int MaxLeagues = 8;
    public const int MaxManagers = 128;
    public const int MaxTeams = 128;
    public const int MaxTeamsInLeague = 32;
    public const int MaxWeeks = MaxTeamsInLeague * 2;
    public const int MaxMatchbreakers = 3;
    public const int MaxPlayers = 3096;
    public const int MaxPlayersInATeam = 64;
    public const int MaxPlayersInFormation = 11;
    public const int MaxNumberOfSubsOnBench = 3;
    public const int MaxPlayersInSquad = MaxPlayersInFormation + MaxNumberOfSubsOnBench;

    public Enums.Screen currentScreen;
    public GameObject[] Screens = new GameObject[(int)Enums.Screen.Max];

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
    public PlayersMatch playersMatch;

    [Header("Sponsor Data")]
    public int LastSponsorUpdateTurn;
    public Enums.SponsorID[] SponsorIDs = new Enums.SponsorID[MaxSponsors];

    [Header("Matchbreaker Data")]
    public int LastMatchbreakerUpdateTurn;
    public int[] AvailableMatchbreakers = new int[MaxMatchbreakers];

    [Header("Opposition Team Data")]
    [Tooltip("Attempt to prevent opposition players/formation from being regenerated due to menu re-navigation")]
    public int LastOppositionTeamAssignmentId;
    public int OppositionTeamId;
    public Enums.Formation OppositionTeamFormationType;
    public int NumPlayersInOppositionTeam;
    public int[] OppositionTeamPlayerIds = new int[MaxPlayersInATeam];
    public Enums.Formation[] PlayersInOppositionFormation = new Enums.Formation[MaxPlayersInSquad];

    [Header("Static Soccer Data")]
    public int NumberOfLeaguesInArrays;
    public StaticLeagueData[] staticLeaguesData = new StaticLeagueData[MaxLeagues];
    public int NumberOfManagersInArrays;
    public StaticManagerData[] staticManagersDatas = new StaticManagerData[MaxManagers];
    public int NumberOfTeamsInArrays;
    public StaticTeamData[] staticTeamsData = new StaticTeamData[MaxTeams];
    public int NumberOfPlayersInArrays;
    public StaticPlayerData[] staticPlayersData = new StaticPlayerData[MaxPlayers];


    [Header("Dynamic Soccer Data")]
    public int PlayersTeam;
    public int PlayersLeague;
    public int PlayersScenario;
    public int PlayerRating;
    public Enums.SponsorID PlayersSponsor;
    public int PlayersWeeksWithSponsor;
    public int PlayersMatchBreaker;
    public Enums.MatchStrategy PlayersMatchStrategy;

    public int[] PlayersBalance = new int[MaxWeeks];
    public int Week;
    public Enums.Formation FormationType;
    public int PlayersYearsToRetire;

    public DynamicManagerData[] dynamicManagersData = new DynamicManagerData[MaxManagers];
    public DynamicTeamData[] dynamicTeamsData = new DynamicTeamData[MaxTeams];
    public DynamicPlayerData[] dynamicPlayersData = new DynamicPlayerData[MaxPlayers];
    public DynamicLeagueData[] PremiumLeagueData = new DynamicLeagueData[MaxTeamsInLeague];

    public int NumPlayersInPlayersTeam;
    public int[] PlayersTeamPlayerIds = new int[MaxPlayersInATeam];
    public int[] PlayersInFormation = new int[MaxPlayersInSquad];
    [Tooltip("Number of teams in scenario's league (if it's a league) needed so we can load the league data but also so we know the total number of weeks")]
    public int NumTeamsInScenarioLeague;
    [Tooltip("Store indices of teams in scenario league")]
    public int[] TeamIndexsForScenarioLeague = new int[MaxTeamsInLeague];
    public ushort[] PremiumLeagueYellowCards = new ushort[MaxPlayers];
    public int[,] PremiumLeagueMatchesPlayed = new int[MaxTeamsInLeague, MaxTeamsInLeague];
    
    
    
    
    
    private void Awake()
    {
        matchEngine = GetComponent<MatchEngine>();
        playersMatch = GetComponent<PlayersMatch>();
    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i < Screens.Length; i++)
        {
            if (Screens[i] != null) 
            {
                Screens[i].SetActive(false); 
            }

        }
        
        Screens[0].SetActive(true);
        currentScreen = Enums.Screen.Title;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        
    }

    public bool LoadGameData()
    {
        int checkSaveDataType = -1;
        List<int> scores = new();
        Dictionary<int, int> dictionary = new();
        if (scores.Count > 0)
        {
            //dictionary = scores[];

        }
        else
        {
            return false;
        }
        return true;
    }
    private void SaveGameData() { }
    private void LoadTeams() { }
    public void LoadAndPrepareGame() 
    {
        bool loadGameSuccessful = LoadGameData();
        if (loadGameSuccessful)
        {
            BuildMatchSchedule(0);
            GoToMenu(Enums.Screen.PreTurn);
        }
        else
        {
            GoToMenu(Enums.Screen.LoadGameError);
        }
    }
    /// <summary>
    /// This builds the schedule for each match, assigning home and away teams for each.
    /// </summary>
    /// <param name="numTeamsInLeague">The amount of teams in the league to assign matches to.</param>
    private void BuildMatchSchedule(int numTeamsInLeague)
    {
        int totalRounds = numTeamsInLeague - 1;
        int matchesPerRound = numTeamsInLeague / 2;
        MatchInfo[,] rounds = new MatchInfo[totalRounds,matchesPerRound];
        for (int round = 0; round < totalRounds; round++)
        {   //Assign home and away team for each match
            for (int match = 0; match < matchesPerRound; match++)
            {
                int home = (round + match) % (numTeamsInLeague - 1);
                int away = (numTeamsInLeague - 1 - match + round) % (numTeamsInLeague - 1);
                if (match == 0)
                {
                    away = numTeamsInLeague - 1;
                }
                rounds[round, match].homeTeam = home;
                rounds[round, match].awayTeam = away;
            }
        }
        // Interleave so that home and away games are fairly evenly dispersed
        MatchInfo[,] interleaved = new MatchInfo[totalRounds, matchesPerRound];
        int even = 0, odd = numTeamsInLeague/2;
        for (int i = 0; i < totalRounds; i++)
        {
            if (i % 2 == 0) //even
            {
                for (int k=0;  k < matchesPerRound; k++)
                {
                    interleaved[i, k] = rounds[even, k];
                }
                even++;
            }
            else //odd
            {
                for (int k = 0; k < matchesPerRound; k++)
                {
                    interleaved[i, k] = rounds[odd, k];
                }
                odd++;
            }
        }
        // map interleaved rounds
        for (int i = 0; i < totalRounds; i++)
        {
            for (int k = 0; k < matchesPerRound; k++)
            {
                rounds[i, k] = interleaved[i, k];
            }
        }

        // Flip alternate home/away matches for the 'last team'
        for (int round = 0; round < totalRounds; round++)
        {
            if (round % 2 == 1) // odd
            {
                int homeId = rounds[round,0].homeTeam;
                int awayId = rounds[round,0].awayTeam;

                rounds[round,0].homeTeam = awayId;
                rounds[round,0].awayTeam = homeId;
            }
        }

        // Ensure teams don't match themselves
        for (int i = 0; i < numTeamsInLeague; i++)
        {
            for (int k = 0; k < numTeamsInLeague; k++)
            {
                if (k == i)
                {
                    PremiumLeagueMatchesPlayed[i, k] = -2; // flag match IMPOSSIBLE to play (ie, against self!)
                }
            }
        }
        // Fill the premium league matches for possible matches
        for (int i = 0; i < totalRounds; i++)
        {
            for (int k = 0; k < matchesPerRound; k++)
            {
                int homeId = rounds[i,k].homeTeam;
                int awayId = rounds[i,k].awayTeam;

                PremiumLeagueMatchesPlayed[homeId,awayId] = i;

                // return visit
                PremiumLeagueMatchesPlayed[awayId,homeId] = i + (numTeamsInLeague - 1);
            }
        }
    }

    public void PrepareChooseTeamMenu(int scenarioId)
    {
        PlayersScenario = scenarioId;
        //todo load league data, including teams, players and managers.
        GoToMenu(Enums.Screen.ChooseTeam);
    }
    private void CreateGameUsingTeam(int teamId, int scenarioId) { }
    private int getArrayIndexForTeam(int teamId) 
    {
        return 0;
    }
    public void GoToMenu(int newScreen=0)
    {
        int oldScreen = (int)currentScreen;
        currentScreen = (Enums.Screen)newScreen;
        GameObject screenToActivate = Screens[newScreen];
        GameObject screenToDeactivate = Screens[oldScreen];
        
        screenToActivate.SetActive(true);
        Debug.Log(screenToActivate.activeSelf);
        if (screenToDeactivate != null)
        {
            screenToDeactivate.SetActive(false);
        }
       
    }
    public void GoToMenu(Enums.Screen newScreen)
    {
        Enums.Screen oldScreen = currentScreen;
        currentScreen = newScreen;
        GameObject screenToActivate = Screens[(int)newScreen];
        GameObject screenToDeactivate = Screens[(int)oldScreen];
        screenToActivate.SetActive(true);
        screenToDeactivate.SetActive(false);
    }
    /// <summary>
    /// This is a translation of the old test function, used when starting a new game.
    /// </summary>
    public void StartGame()
    {
        List<int> savedata = new();
        if (savedata.Count > 0) //savedata is present
        {
            GoToMenu(Enums.Screen.Confirm);
        }
        else
        {
            GoToMenu(Enums.Screen.ChooseLeague);
        }
    }
}
