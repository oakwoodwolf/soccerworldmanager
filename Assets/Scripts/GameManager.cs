using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using static Enums;
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
    [FormerlySerializedAs("Screens")]
    public GameObject[] screens = new GameObject[(int)Enums.Screen.Max];
    public MenuItemGenerator menuItemGenerator;
    public int formationCycle;
    public Vector2 formationSelectionScrollPos;
    public int currentPage;
    public int currentNumberOfPage;
    public int processMatchDataStartFrameCount;
    public ScenarioInfo[] scenarioData;
    /// <summary>
    /// Stuff to keep track of for end of turn screen
    /// </summary>
    /// 
    [Header("Stats")]
    [Tooltip("Keep previous league pos. for end of turn stats")]
    public int statsPrevLeaguePos;
    [Tooltip("Income this turn.")]
    public int statsTurnIncome;
    [Tooltip("Income this turn from ticket sales.")]
    public int statsTurnIncomeTicketSales;
    [Tooltip("Income this turn from TV sponsors.")]
    public int statsTurnIncomeSponsorsTV;
    [Tooltip("Expenditure this turn.")]
    public int statsTurnExpend;
    public int statsTurnExpendSalary;
    [Tooltip("Actual attendance at last player's match.")]
    public int statsAttendance;
    [Tooltip("Number of seats at last match.")]
    public int statsStadiumSeats;

    [Header("League End")]
    [Tooltip("Keep track of the league the user is in.")]
    public int leagueEndSaveUsersLeague;
    [Tooltip("Number of entries for League End Users Final Standings.")]
    public int leagueEndNumTeamsInFinalStandings;
    [Tooltip("Entries for League End Users Final Standings.")]
    public int[] leagueEndUsersFinalStandings= new int[MaxTeamsInLeague];

    [Header("Transfer Screen Data")]
    public int transferOfferInterestedPlayerId;
    public int transferOfferInterestedTeamIndex;
    public int transferOfferOfferValue;

    [Tooltip("Count number of offers per-turn, to limit to 3.")]
    [Range(0,3)]
    public int transferOfferOffersMadeThisTurn;
    public int currentBuyPlayerId;
    public int currentBuyPlayerOffer;
    public int numPlayersOnTransferList;
    public int[] transferList = new int[MaxPlayers];

    [Header("Match Engine Data")]
    public MatchEngine matchEngine;
    public PlayersMatch playersMatch;

    [Header("Sponsor Data")]
    public int lastSponsorUpdateTurn;
    public Enums.SponsorID[] sponsorIDs = new Enums.SponsorID[MaxSponsors];

    [Header("Matchbreaker Data")]
    public int lastMatchbreakerUpdateTurn;
    public int[] availableMatchbreakers = new int[MaxMatchbreakers];

    [Header("Opposition Team Data")]
    [Tooltip("Attempt to prevent opposition players/formation from being regenerated due to menu re-navigation")]
    public int lastOppositionTeamAssignmentId;
    public int oppositionTeamId;
    public Enums.Formation oppositionTeamFormationType;
    public int numPlayersInOppositionTeam;
    public int[] oppositionTeamPlayerIds = new int[MaxPlayersInATeam];
    public Formation[] playersInOppositionFormation = new Formation[MaxPlayersInSquad];

    [Header("Static Soccer Data")]
    public int numberOfLeaguesInArrays;
    public StaticLeagueData[] staticLeaguesData = new StaticLeagueData[MaxLeagues];
    public int numberOfManagersInArrays;
    public StaticManagerData[] staticManagersData = new StaticManagerData[MaxManagers];
    public int numberOfTeamsInArrays;
    public StaticTeamData[] staticTeamsData = new StaticTeamData[MaxTeams];
    public int numberOfPlayersInArrays;
    public StaticPlayerData[] staticPlayersData = new StaticPlayerData[MaxPlayers];


    [Header("Dynamic Soccer Data")]
    public int playersTeam;
    public LeagueID playersLeague;
    public int playersScenario;
    public int playerRating = 128;
    public SponsorID playersSponsor;
    public int playersWeeksWithSponsor;
    public int playersMatchBreaker;
    public MatchStrategy playersMatchStrategy;

    public int[] playersBalance = new int[MaxWeeks];
    public int week;
    public Formation formationType;
    public int playersYearsToRetire;

    public DynamicManagerData[] dynamicManagersData = new DynamicManagerData[MaxManagers];
    public DynamicTeamData[] dynamicTeamsData = new DynamicTeamData[MaxTeams];
    public DynamicPlayerData[] dynamicPlayersData = new DynamicPlayerData[MaxPlayers];
    public DynamicLeagueData[] premiumLeagueData = new DynamicLeagueData[MaxTeamsInLeague];

    public int numPlayersInPlayersTeam;
    public int[] playersTeamPlayerIds = new int[MaxPlayersInATeam];
    public int[] playersInFormation = new int[MaxPlayersInSquad];
    [Tooltip("Number of teams in scenario's league (if it's a league) needed so we can load the league data but also so we know the total number of weeks")]
    public int numTeamsInScenarioLeague;
    [Tooltip("Store indices of teams in scenario league")]
    public int[] teamIndexsForScenarioLeague = new int[MaxTeamsInLeague];
    public ushort[] premiumLeagueYellowCards = new ushort[MaxPlayers];
    public int[,] PremiumLeagueMatchesPlayed = new int[MaxTeamsInLeague, MaxTeamsInLeague];
    
    
    
    
    
    private void Awake()
    {
        matchEngine = GetComponent<MatchEngine>();
        playersMatch = GetComponent<PlayersMatch>();
        menuItemGenerator = GetComponent<MenuItemGenerator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i < screens.Length; i++)
        {
            if (screens[i] != null) 
            {
                screens[i].SetActive(false); 
            }

        }
        
        screens[0].SetActive(true);
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
    private void LoadTeams() 
    {
        numberOfTeamsInArrays = 0;
        string path = "Assets/data/teamdata.txt";
        //Check if data file exists.
        if (File.Exists(path))
        {
            StreamReader sr = new(path);
            int teamCount = int.Parse(sr.ReadLine() ?? throw new InvalidOperationException());
            for (int i = 0; i < teamCount; i++) //Iterate through each line
            {
                string[] line = sr.ReadLine()?.Split();
                if (line != null && line.Length > 8) // ensure its split enough
                {
                    int offset = 0;
                    if (line.Length > 9) 
                    {
                        offset = 1;
                    } // some teams have an extra parameter for team name 2.

                    //Add to the list of leaguesData.
                    staticTeamsData[numberOfTeamsInArrays] = ScriptableObject.CreateInstance<StaticTeamData>();
                    staticTeamsData[numberOfTeamsInArrays].LoadStaticTeamData(line);
                    dynamicTeamsData[numberOfTeamsInArrays] = ScriptableObject.CreateInstance<DynamicTeamData>();
                    dynamicTeamsData[numberOfTeamsInArrays].LoadDynamicTeamData(line,offset);
                    numberOfTeamsInArrays++;
                }
                else // throw error. Invalid data
                {
                    Debug.LogError("Team ID is -1!");
                }

            }
        }
    }
    private void LoadManagers()
    {
        numberOfManagersInArrays = 0;
        string path = "Assets/data/Manager_Data.txt";
        //Check if data file exists.
        if (File.Exists(path))
        {
            StreamReader sr = new(path);
            int managerCount = int.Parse(sr.ReadLine() ?? throw new InvalidOperationException());
            for (int i = 0; i < managerCount; i++) //Iterate through each line
            {
                string[] line = sr.ReadLine()?.Split();
                if (line != null && line.Length == 4) // ensure its split enough
                {
                    //Add to the list of leaguesData.
                    staticManagersData[numberOfManagersInArrays] = ScriptableObject.CreateInstance<StaticManagerData>();
                    staticManagersData[numberOfManagersInArrays].LoadStaticManagerData(line);
                    dynamicManagersData[numberOfManagersInArrays] = ScriptableObject.CreateInstance<DynamicManagerData>();
                    dynamicManagersData[numberOfManagersInArrays].LoadDynamicManagerData(line);
                    numberOfManagersInArrays++;
                }
                else // throw error. Invalid data
                {
                    Debug.LogError("Manager ID is -1!");
                }

            }
        }
    }
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
        playersScenario = scenarioId;
        playersLeague = scenarioData[playersScenario].leagueID;
        LoadLeagueData();
        LoadTeams();
        LoadPlayers();
        LoadManagers();
        // Filter the league players
        numTeamsInScenarioLeague = CountTeamsInLeague(playersLeague);
        FillTeamsInLeagueArray(teamIndexsForScenarioLeague, playersLeague);
        // Dynamic menu work
        
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            int dataIndex = teamIndexsForScenarioLeague[i];
            menuItemGenerator.GenerateMenuItem(screens[(int)Enums.Screen.ChooseTeam].transform,Enums.MenuElement.TextBar, new Vector2(0,-270+(menuItemGenerator.menuBarSpacing*i)),0,0,staticTeamsData[dataIndex].teamName, Enums.MenuAction.SelectTeamAndCreateGame, staticTeamsData[dataIndex].teamId);
        }
        
        GoToMenu(Enums.Screen.ChooseTeam);
    }

    public int FillTeamsInLeagueArray(int[] pArray, Enums.LeagueID leagueID)
    {
        int count = 0;
        for (int i = 0; i < numberOfTeamsInArrays; i++)
        {
            if (dynamicTeamsData[i].leagueID == leagueID) 
            {
                pArray[count] = i;
                count++;
            }
        }
        return count;
    }

    private int CountTeamsInLeague(Enums.LeagueID leagueID)
    {
        int count = 0;
        for (int i = 0; i < numberOfTeamsInArrays; i++)
        {
            if (dynamicTeamsData[i].leagueID == leagueID) { count++; }
        }
        return count;
    }

    /// <summary>
    /// Loads all the leagues from a .txt file. Unlike the old implementation,
    /// the mapping is done on the object's side, this just reads the line and passes it to the object.
    /// </summary>
    private void LoadLeagueData()
    {
        numberOfLeaguesInArrays = 0;
        string path = "Assets/data/LeagueData.txt";
        //Check if data file exists.
        if (File.Exists(path))
        {
            StreamReader sr = new(path);
            int leagueCount = int.Parse(sr.ReadLine());
            for (int i=0; i < leagueCount; i++) //Iterate through each line
            {
                string[] line = sr.ReadLine().Split();
                if (line.Length == 8) // ensure its split enough
                {
                    //Add to the list of leaguesData.
                    staticLeaguesData[numberOfLeaguesInArrays] = ScriptableObject.CreateInstance<StaticLeagueData>();
                    staticLeaguesData[numberOfLeaguesInArrays].LoadStaticLeagueData(line);
                    Debug.Log(staticLeaguesData[numberOfLeaguesInArrays].ToString());
                    numberOfLeaguesInArrays++;
                }
                else // throw error. Invalid data
                {
                    Debug.LogError("League ID is -1!");
                }
                
            }
        }
    }

    private void LoadPlayers()
    {
        numberOfPlayersInArrays = 0;
        string path = "Assets/data/PlayerData.txt";
        //Check if data file exists.
        if (File.Exists(path))
        {
            StreamReader sr = new(path);
            int teamCount = int.Parse(sr.ReadLine());
            for (int i = 0; i < teamCount; i++) //Iterate through each line
            {
                string[] line = sr.ReadLine().Split();
                if (line.Length == 5) // ensure its split enough
                {
                    //Add to the list of playerData.
                    staticPlayersData[numberOfPlayersInArrays] = ScriptableObject.CreateInstance<StaticPlayerData>();
                    staticPlayersData[numberOfPlayersInArrays].LoadStaticPlayerData(line);
                    dynamicPlayersData[numberOfPlayersInArrays] = ScriptableObject.CreateInstance<DynamicPlayerData>();
                    dynamicPlayersData[numberOfPlayersInArrays].LoadDynamicPlayerData(line);
                    numberOfPlayersInArrays++;
                }
                else // throw error. Invalid data
                {
                    Debug.LogError("Invalid data!");
                }

            }
        }
    }

    private int GetArrayIndexForTeam(int teamId) 
    {
        return 0;
    }
    public void GoToMenu(int newScreen=0)
    {
        int oldScreen = (int)currentScreen;
        currentScreen = (Enums.Screen)newScreen;
        GameObject screenToActivate = screens[newScreen];
        GameObject screenToDeactivate = screens[oldScreen];
        
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
        GameObject screenToActivate = screens[(int)newScreen];
        GameObject screenToDeactivate = screens[(int)oldScreen];
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
    
    public void CreateGameUsingTeam(int teamId, int scenarioId) 
    {
        playersTeam = teamId;
        int managerIndex = 0;
        if (managerIndex != -1)
        {
            dynamicManagersData[managerIndex].teamId = -1; // set unemployed
        }
        week = 0;
        playersSponsor = SponsorID.None;
    }
}
