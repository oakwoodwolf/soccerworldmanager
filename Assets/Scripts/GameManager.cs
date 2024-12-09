using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
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
    public ScreenDefinition[] screens = new ScreenDefinition[(int)Enums.Screen.Max];
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
    [SerializeField] private int playerRatingStartValue = 128;
    public SponsorID playersSponsor;
    public int playersWeeksWithSponsor;
    public int playersMatchBreaker;
    public MatchStrategy playersMatchStrategy;

    public int[] playersBalance = new int[MaxWeeks];
    public int week;
    public Formation formationType;
    public FormationData[] formations = new FormationData[(int)Formation.KFormationMax];
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

    [Tooltip("The player value relative to the league index.")]
    public int[] leagueToPlayerValue =
    {
        1000, 800, 0, 0, 0, 0, 0, 0, 0, 0,
        800, // kLeagueId_Scotland = 10
        0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        500 //kLeagueId_USA,
    };

    [Tooltip(
        "these values are multiplied by a players stars value (at the start of a season) to determine a weekly salary")]
    public float[] leagueToPlayerSalaryRatio =
    {
        0.0045f, //kLeagueId_Premium = 0,
        0.00055f, //kLeagueId_Chumpionship,
        0, 0, 0, 0, 0, 0, 0, 0,
        0.00055f, //kLeagueId_Scotland = 10
        0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0.00065f, //kLeagueId_USA,
    };

   
    
    
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
                screens[i].gameObject.SetActive(false); 
            }

        }
        
        screens[0].gameObject.SetActive(true);
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
        RectTransform items = screens[(int)Enums.Screen.ChooseTeam].MenuItems.GetComponent<RectTransform>();
        items.sizeDelta = new Vector2(320f,menuItemGenerator.menuBarSpacing*numTeamsInScenarioLeague+72);
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            int dataIndex = teamIndexsForScenarioLeague[i];
            menuItemGenerator.GenerateMenuItem(screens[(int)Enums.Screen.ChooseTeam],Enums.MenuElement.TextBar, new Vector2(0,-72-menuItemGenerator.menuBarSpacing*i),0,0,staticTeamsData[dataIndex].teamName, Enums.MenuAction.SelectTeamAndCreateGame, staticTeamsData[dataIndex].teamId);
        }
        menuItemGenerator.GenerateMenuItem(screens[(int)Enums.Screen.ChooseTeam],Enums.MenuElement.Button, new Vector2(8,-72-menuItemGenerator.menuBarSpacing*numTeamsInScenarioLeague),0,0,"backButton", Enums.MenuAction.GotoMenu, (int)Enums.Screen.ChooseLeague);

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
        GameObject screenToActivate = screens[newScreen].gameObject;
        GameObject screenToDeactivate = screens[oldScreen].gameObject;
        
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
        GameObject screenToActivate = screens[(int)newScreen].gameObject;
        GameObject screenToDeactivate = screens[(int)oldScreen].gameObject;
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

        playerRating = playerRatingStartValue;
        playersSponsor = SponsorID.None;
        playersWeeksWithSponsor = 0;
        playersMatchBreaker = -1;
        matchEngine.state = Enums.MatchEngineState.MatchOver;
        playersMatchStrategy = Enums.MatchStrategy.Balanced;

        ResetLeaguePoints();
        
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            int playerId = staticPlayersData[i].playerId;
            int currentTeamId = dynamicPlayersData[i].teamId;
            if (currentTeamId != -1)
            {
                int leagueId = GetTeamsLeagueID(currentTeamId);
                int playerValue = DetermineValueOfPlayerID(playerId, leagueId);
                dynamicPlayersData[i].weeklySalary = (short)(playerValue * leagueToPlayerSalaryRatio[leagueId]);
                if (dynamicPlayersData[i].weeklySalary <= 0) dynamicPlayersData[i].weeklySalary = 1;
            }
            else
            {
                dynamicPlayersData[i].weeklySalary = 0;
            }
        }

        playersYearsToRetire = scenarioData[playersScenario].yearsTillRetire;
        int teamIndex = GetTeamDataIndexForTeamID(playersTeam);
        dynamicTeamsData[teamIndex].cashBalance += scenarioData[playersScenario].startMoneyAdjust;

        int teamLeagueIndex = -1;
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            if (premiumLeagueData[i].teamId == playersTeam)
            {
                teamLeagueIndex = i;
                break;
            }
        }

        if (teamLeagueIndex != -1)
        {
            premiumLeagueData[teamLeagueIndex].leaguePoints = scenarioData[scenarioId].startMoneyAdjust; // Initial scenario points
        }
        
        numPlayersInPlayersTeam = FillTeamPlayerArray(playersTeamPlayerIds, playersTeam);  
        AutofillFormationFromPlayerIDs(playersInFormation, playersTeamPlayerIds, numPlayersInPlayersTeam, Formation.KFormation442, playersTeam);
        
        SaveGameData();
        GoToMenu(Enums.Screen.PreTurn);
        lastSponsorUpdateTurn = -1;
        lastMatchbreakerUpdateTurn = -1;
    }

    private void AutofillFormationFromPlayerIDs(int[] formation, int[] playerIds, int numPlayers, Formation formationType, int teamId)
    {
        for (int i = 0; i < MaxPlayersInSquad; i++)
        {
            formation[i] = -1;
        }
        var formationData = formations[(int)formationType];
        for (int i = 0; i < MaxPlayersInSquad; i++)
        {
            int playerId = -1;
           int formPosType = (int)formationData.formations[i].formation;
           if ((formPosType & (int)PlayerFormation.Substitute) !=0)
           {
               int subnum = i - 11;
               int style = (int)GetEnumManagerStyleForTeamId(teamId);
               if (style > 0)
               {
                   subnum++;
               }

               subnum &= 3;
               formPosType = 1 << subnum;
           }
           if ((formPosType & (int)PlayerFormation.Goalkeeper) !=0)
           {
               playerId = AssignSquad(formation, playerIds, numPlayers, formPosType);
           }
        }
    }

    private int AssignSquad(int[] formation, int[] playerIds, int numPlayers, int formPosType)
    {
        switch ((PlayerFormation)formPosType)
        {
            case PlayerFormation.Goalkeeper:
                return ChooseGoalKeeperForFormation(formation, playerIds, numPlayers);
                break;
        }

        return -1;
    }
/// <summary>
/// Determine the goalkeeper candidate for the formation
/// </summary>
/// <param name="formation"></param>
/// <param name="playerIds"></param>
/// <param name="numPlayers"></param>
/// <returns>the index for the player to assign goalkeeper.</returns>
    private int ChooseGoalKeeperForFormation(int[] formation, int[] playerIds, int numPlayers)
    {
        int resultIndex = -1;
        float resultRating = 0.0f;

        for (int i = 0; i < numPlayers; i++)
        {
            int playerId = playerIds[i];
            for (int j = 0; j < MaxPlayersInSquad; j++)
            {
                if (formation[j] == playerId) playerId = -1; // player was already in the squad.
            }
            
            //Check if player is already assigned
            if (playerId != -1)
            {
                int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                float starsRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
                
                float positionScale = 1.0f;
                float outOfPositionScale = 1.0f;
                if ((int)CheckPlayerIndexIsHappyInPosition(playerId, PlayerFormation.Goalkeeper) < 1)
                {
                    outOfPositionScale = 0.5f;
                }
                
                if (dynamicPlayersData[dataIndex].weeksBannedOrInjured > 0)
                {
                    starsRating = 0.0f;
                }

                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Goalkeeper) > 0)
                {
                    positionScale = 2.0f;
                }
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Defender) > 0)
                {
                    positionScale = 1.5f;
                }
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.MidFielder) > 0)
                {
                    positionScale = 0.75f;
                }
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Attacker) > 0)
                {
                    positionScale = 0.5f;
                }
                starsRating += starsRating * dynamicPlayersData[dataIndex].condition*outOfPositionScale*positionScale;
                if (starsRating > resultRating)
                {
                    resultRating = starsRating;
                    resultIndex = dataIndex;
                }
            }
        }
        return resultIndex;
    }

    private PlayerFormation CheckPlayerIndexIsHappyInPosition(int playerId, PlayerFormation position)
    {
        int playerIndex = GetPlayerDataIndexForPlayerID(playerId);
        if (position == PlayerFormation.Substitute) return PlayerFormation.Substitute;
        return staticPlayersData[playerIndex].playerPositionFlags & position;
    }   

    private ManagementStyle GetEnumManagerStyleForTeamId(int teamId)
    {
        
        int managerIndex = GetIndexToManagerForTeamID(teamId);
        float style = staticManagersData[managerIndex].styleOffset;
        if (style is <= 4.9f and >= -4.9f)
        {
            return ManagementStyle.Balanced;
        }
        else
        {
            if (style < -9.9f)
            {
                return ManagementStyle.VeryDefensive;
            }
            else if (style < -4.9f)
            {
                return ManagementStyle.Defensive;
            }
            else if (style > 9.9f)
            {
                return ManagementStyle.StrongAttacking;
            }
            else if (style > 4.9f)
            {
                return ManagementStyle.Attacking;
            }
        }
        return ManagementStyle.Balanced;
    }
    
    /// <param name="teamId">The team to retrive the manager of</param>
    /// <returns>the index to the manager for a given teamId, note this is the index of the manager, NOT the ID!!! </returns>
    private int GetIndexToManagerForTeamID(int teamId)
    {
        int result = -1;
        Debug.Assert(numberOfManagersInArrays > 0);
        for (int i = 0; i < numberOfManagersInArrays; i++)
        {
            if (dynamicManagersData[i].teamId == teamId)
            {
                result = i;
                break;
            }
        }
        return result;
    }

    private int FillTeamPlayerArray(int[] playerArray, int teamId)
    {
        int numPlayersFound = 0;
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            if (dynamicPlayersData[i].teamId == teamId)
            {
                if (numPlayersFound < MaxPlayersInATeam)
                {
                    playerArray[numPlayersFound] = staticPlayersData[i].playerId;
                    numPlayersFound++;
                }
                else
                {
                    Debug.LogError("Team ID " + teamId + " is out of range!");
                }
            }
        }
        return numPlayersFound;
    }

    private int DetermineValueOfPlayerID(int playerId, int leagueId)
    {
        
        int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
        float starsRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
        
        int result = 0;
        result = (int)(starsRating * leagueToPlayerValue[leagueId]);
        result *= (int)dynamicPlayersData[dataIndex].condition;
        return result;
    }

    private int GetPlayerDataIndexForPlayerID(int playerId)
    {
        int playerIndex = -1;
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            if (staticPlayersData[i].playerId == playerId)
            {
                playerIndex = i;
                break;
            }
        }
        Debug.Assert(playerIndex != -1);
        return playerIndex;
    }
/// <summary>
/// determine the stars rating for a player, adjusted for their team and league
/// </summary>
/// <param name="dataIndex">the playerdata's index</param>
/// <returns>A player's star rating</returns>
    private float GetTeamLeagueAdjustedStarsRatingForPlayerIndex(int dataIndex)
    {
        float stars = dynamicPlayersData[dataIndex].starsRating;
        int leagueIndex = GetIndexToLeagueData((int)playersLeague);
        int teamId = dynamicPlayersData[dataIndex].teamId;
        if (teamId != -1)
        {
            int leagueId = GetTeamsLeagueID(teamId);
            leagueIndex = GetIndexToLeagueData(leagueId);
        }

        stars -= staticLeaguesData[leagueIndex].minStarRating;
        if (stars < 0.0f)
        {
            stars = 0.0f;
        }
        else if (stars >= 5.9f)
        {
            stars = 5.9f;
        }
        return stars;
    }

    private int GetIndexToLeagueData(int leagueId)
    {
        int result = -1;
        for (int i = 0; i < numberOfLeaguesInArrays; i++)
        {
            if ((int)staticLeaguesData[i].leagueId == leagueId)
            {
                result = i;
                break;
            }
        }
        return result;
    }

    private int GetTeamsLeagueID(int currentTeamId)
    {
        int dataIndex = GetTeamDataIndexForTeamID(currentTeamId);
        if (dataIndex != -1)
        {
            return (int)dynamicTeamsData[dataIndex].leagueID;
        }
        else
        {
            return -1;
        }
        
    }

    private int GetTeamDataIndexForTeamID(int teamId)
    {
        int teamIndex = -1;
        for (int i = 0; i < numberOfTeamsInArrays; i++)
        {
            if (staticTeamsData[i].teamId == teamId)
            {
                teamIndex = i;
                break;
            }
        }

        Debug.Assert(teamIndex != -1);
        return teamIndex;
    }

    private void ResetLeaguePoints()
    {
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            int dataIndex = teamIndexsForScenarioLeague[i];
            premiumLeagueData[dataIndex].teamId = staticTeamsData[dataIndex].teamId;
            premiumLeagueData[dataIndex].matchesPlayed = 0;
            premiumLeagueData[dataIndex].goalsFor = 0;
            premiumLeagueData[dataIndex].goalsAgainst = 0;
            premiumLeagueData[dataIndex].goalDifference = 0;

        }

        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            premiumLeagueYellowCards[i] = 5 & 0x00ff;
            //_PremiumLeagueYellowCards[i] = ( kPremiumLeague_YellowsUntilBan & kYellowCardsUntilBanMask );		// low=cards until ban, high= total card recieved (hence 0 at season start)
        }
    }
}
