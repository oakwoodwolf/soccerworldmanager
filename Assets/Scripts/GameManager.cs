using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using static Enums;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    //Constants
    public const int MaxSponsors = 3;
    public const int MaxNumOfSponsors = 20;
    public const int MaxLeagues = 8;
    public const int MaxManagers = 128;
    public const int MaxTeams = 128;
    public const int MaxTeamsInLeague = 32;
    public const int MaxWeeks = MaxTeamsInLeague * 2;
    public const int MaxMatchbreakers = 3;
    public const int MaxNumOfMatchbreakers = 10;
    public const int MaxPlayers = 3096;
    public const int MaxPlayersInList = 32;
    public const int MaxPlayersInATeam = 64;
    public const int MaxPlayersInFormation = 11;
    public const int MaxNumberOfSubsOnBench = 3;
    public const int MaxPlayersInSquad = MaxPlayersInFormation + MaxNumberOfSubsOnBench;
    public const int MaxSheets = 3;
    public const int PremiumLeagueYellowsUntilBan = 5;
    public const int YellowCardsUntilBanMask = 0x00ff;
    public const int YellowCardsInTournamentMask = 0xff00;
    public const int YellowCardsInTournamentBitShift = 8;
    public const int trainingMask = 0xff;
    public const int transferMask = 0xff00;
    public const int transferBitShift = 8;
    public const int injuryMask = 0x00ff;
    public const int bannedMask = 0xff00;
    public const int bannedBitShift = 8;
    
    public const int YellowCardMask= 3 << 0;
    public const int RedCardMask= 1 << 2;
    public const int WarningMask= 1 << 3;
    public const int BeenOnPitchMask = 1 << 4;



    
    public bool SFXEnabled = true;
    public bool VibrationEnabled = true;

    public Enums.Screen currentScreen;
    [FormerlySerializedAs("Screens")]
    public ScreenDefinition[] screens = new ScreenDefinition[(int)Enums.Screen.Max];
    public MenuItemGenerator menuItemGenerator;
    public MenuItem[] currentMenuItems;
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
    [Tooltip("Number of seats at last match.")]
    public string statsPreturn;

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
    public SponsorID[] sponsorIDs = new SponsorID[MaxSponsors];
    public SponsorInfo[] sponsorInfo = new SponsorInfo[1];
    public int[] availableSponsors = new int[MaxSponsors];
    
    [Header("Matchbreaker Data")]
    public int lastMatchbreakerUpdateTurn;
    public int[] availableMatchbreakers = new int[MaxMatchbreakers];
    public MatchBreakerInfo[] matchbreakerInfo;

    [Header("Training info")] 
    public const float MaxPlayerCondition = 1.0f;
    public const float ShowInjuredRatio = 0.33f;
    public const float IntensiveTrainingStarsRating = 0.05f;
    public const float IntensiveTrainingCondition = -0.2f;
    public const float LightTrainingStarsRating = -0.002f;
    public const float LightTrainingCondition = 0.02f;
    public const float NoTrainingStarsRating = -0.05f;
    public const float NoTrainingCondition = 0.2f;
    public const float ConditionAdjustPerTurn = -0.016f;
    public const float ConditionAdjustRecoverOverWeek = 0.2055f;

    
    [Header("Opposition Team Data")]
    [Tooltip("Attempt to prevent opposition players/formation from being regenerated due to menu re-navigation")]
    public int lastOppositionTeamAssignmentId;
    public int oppositionTeamId;
    public Enums.Formation oppositionTeamFormationType;
    public int numPlayersInOppositionTeam;
    public int[] oppositionTeamPlayerIds = new int[MaxPlayersInATeam];
    public Formation[] playersInOppositionFormation = new Formation[MaxPlayersInSquad];

    [Header("Static Soccer Data")]
    public LeagueInfo[] leagueDetails;
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
    public int playersSponsor;
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


    [Header("SaveData")] 
    public string usernameDefaultKey = "userName";
    public string highScoresDefaultKey = "highScores";
    public string highScoresMarathonKey = "highScoresMarathon";
    public string soccerSaveDataKey = "soccerSaveData";
    public string soccerSaveLeagueDataKey = "soccerLeagueSaveData";
    public string soccerLeagueYellowCardDataKey = "soccerLeagueYellowCardData";
    public string soccerPlayerFormationDataKey = "soccerPlayerFormationData";
    public string soccerBalanceHistoryDataKey = "soccerBalanceHistoryData";
    public string soccerDynamicTeamDataKey = "soccerDynamicTeamData";
    public string soccerDynamicPlayerDataKey = "soccerDynamicPlayerData";
    public string soccerDynamicManagerDataKey = "soccerDynamicManagerData";
    [Header("Audio")]
    public AudioSource aSource;
    public AudioClip[] aClip;

    
    private void Awake()
    {
        matchEngine = GetComponent<MatchEngine>();
        aSource = GetComponent<AudioSource>();
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
        
        GoToMenu(Enums.Screen.Title);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        
    }
    public bool CheckGameData()
    {
        int checkSaveDataType = -1;
        string jsonforData = PlayerPrefs.GetString(soccerSaveDataKey);
        List<Dictionary<string, object>> scores = JsonUtility.FromJson<List<Dictionary<string, object>>>(jsonforData);

        if (scores.Count > 0)
        {
            Debug.Log(scores[0]["score"].ToString());
            return true;
        }
        else 
        {
            Debug.Log("No scores found in saved data.");
            return false;
        }
    }
    public bool LoadGameData()
    {
        int checkSaveDataType = -1;
        string jsonforData = PlayerPrefs.GetString(soccerSaveDataKey);
        List<Dictionary<string, object>> scores = JsonUtility.FromJson<List<Dictionary<string, object>>>(jsonforData);

        if (scores.Count > 0)
        {
            Dictionary<string, object> mainData = scores[0];

            if (mainData.ContainsKey("SaveDataType"))
            {
                checkSaveDataType = (int)(mainData["SaveDataType"]);
                if (checkSaveDataType != 5 && checkSaveDataType != 4 && checkSaveDataType != 3)
                {
                    Debug.Log("Unsupported save data type.");
                
                    return false;
                }
            }
            else
            {
                Debug.Log("SaveDataType key is missing.");
                return false;
            }
            playersTeam = (int)mainData["PlayersTeam"];
            playersLeague = (LeagueID)mainData["PlayersLeague"];
            playersYearsToRetire = (int)mainData["PlayersYearsToRetire"];
            playersScenario = (int)mainData["PlayersScenario"];
            week = (int)mainData["Week"];
            formationType = (Formation)mainData["Formation"];
            numTeamsInScenarioLeague = (int)mainData["TeamsInScenarioLeague"];
            playerRating  = (int)mainData["PlayerRating"];
            playersSponsor  = (int)mainData["PlayerSponsor"];
            playersWeeksWithSponsor  = (int)mainData["PlayersWeeksWithSponsor"];
            playersMatchBreaker  = (int)mainData["PlayersMatchBreaker"];
            playersMatchStrategy  = (MatchStrategy)mainData["PlayersMatchStratergy"];
            
            LoadLeagueData();
            LoadTeams();
            LoadPlayers();
            LoadManagers();
            
            string  balanceHistoryJson = PlayerPrefs.GetString(soccerBalanceHistoryDataKey);
            List<Dictionary<string, object>> balanceHistoryData = JsonUtility.FromJson<List<Dictionary<string, object>>>(balanceHistoryJson);
           
            if (balanceHistoryData.Count > 0)
            {
                if (balanceHistoryData.Count != week)
                {
                    return false;
                }

                for (int i = 0; i < balanceHistoryData.Count; i++)
                {
                    Dictionary<string, object> dictionary = balanceHistoryData[i];
                    playersBalance[i] = (int)dictionary["PlayersBalance"];
                }
            }
            
            //Load Dynamic Team Data
            string  dynamicTeamJson = PlayerPrefs.GetString(soccerDynamicTeamDataKey);
            List<Dictionary<string, object>> dynamicTeamData = JsonUtility.FromJson<List<Dictionary<string, object>>>(dynamicTeamJson);
           
            if (dynamicTeamData.Count > 0)
            {
                if (dynamicTeamData.Count != numberOfTeamsInArrays)
                {
                    return false;
                }

                for (int i = 0; i < dynamicTeamData.Count; i++)
                {
                    Dictionary<string, object> dictionary = dynamicTeamData[i];
                    int teamId = (int)dictionary["teamId"];
                    int dataIndex = GetArrayIndexForTeam(teamId);
                    if (dataIndex == -1)
                    {
                        return false;
                    }
                    
                    dynamicTeamsData[dataIndex].leagueID = (LeagueID)dictionary["leagueId"];
                    dynamicTeamsData[dataIndex].cashBalance = (int)dictionary["cashBalance"];
                    dynamicTeamsData[dataIndex].fanMorale = (float)dictionary["fanMorale"];
                }
            }
            //Load Dynamic player Data
            string  dynamicPlayerJson = PlayerPrefs.GetString(soccerDynamicPlayerDataKey);
            List<Dictionary<string, object>> dynamicPlayerData = JsonUtility.FromJson<List<Dictionary<string, object>>>(dynamicPlayerJson);
           
            if (dynamicPlayerData.Count > 0)
            {
                if (dynamicPlayerData.Count != numberOfPlayersInArrays)
                {
                    return false;
                }

                for (int i = 0; i < dynamicPlayerData.Count; i++)
                {
                    Dictionary<string, object> dictionary = dynamicPlayerData[i];
                    int playerId = (int)dictionary["playerId"];
                    int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                    if (dataIndex == -1)
                    {
                        return false;
                    }
                    
                    dynamicPlayersData[dataIndex].starsRating = (float)dictionary["starsRating"];
                    dynamicPlayersData[dataIndex].condition = (float)dictionary["condition"];
                    dynamicPlayersData[dataIndex].trainingTransfer = (short)dictionary["trainingtransfer"];
                    dynamicPlayersData[dataIndex].teamId = (short)dictionary["teamId"];
                    dynamicPlayersData[dataIndex].weeklySalary = (short)dictionary["weeklySalary"];
                    dynamicPlayersData[dataIndex].morale = (short)dictionary["morale"];
                    dynamicPlayersData[dataIndex].weeksBannedOrInjured = (short)dictionary["weeksBannedOrInjured"];
                    dynamicPlayersData[dataIndex].flags = (ushort)dictionary["flags"];
                }
            }
            //Load Dynamic Manager Data
            string  dynamicManagerJson = PlayerPrefs.GetString(soccerDynamicTeamDataKey);
            List<Dictionary<string, object>> dynamicManagerData = JsonUtility.FromJson<List<Dictionary<string, object>>>(dynamicManagerJson);
           
            if (dynamicManagerData.Count > 0)
            {
                if (dynamicManagerData.Count != numberOfTeamsInArrays)
                {
                    return false;
                }

                for (int i = 0; i < dynamicManagerData.Count; i++)
                {
                    Dictionary<string, object> dictionary = dynamicManagerData[i];
                    int managerId = (int)dictionary["managerId"];
                    int dataIndex = GetIndexToManagerForTeamID(managerId);
                    if (dataIndex == -1)
                    {
                        return false;
                    }
                    
                    dynamicManagersData[dataIndex].teamId = (int)dictionary["teamId"];
                }
            }

            int managerIndex = GetIndexToManagerForTeamID(playersTeam);
            if (managerIndex != -1)
            {
                dynamicManagersData[managerIndex].teamId = -1;
            }
            FillTeamsInLeagueArray(teamIndexsForScenarioLeague, playersLeague);
            numPlayersInPlayersTeam = FillTeamPlayerArray(playersTeamPlayerIds, playersTeam);
            AutofillFormationFromPlayerIDs(playersInFormation,playersTeamPlayerIds,numPlayersInPlayersTeam,formationType,playersTeam);
        }
        else 
        {
            Debug.Log("No scores found in saved data.");
            return false;
        }

        
        return true;
    }
    private void SaveGameData()
    {
        string name = PlayerPrefs.GetString(usernameDefaultKey, "Player");
        DateTime date = DateTime.Now;
        
        List<Dictionary<string, object>> scores = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "name", name },
                { "date", date.ToString(CultureInfo.InvariantCulture) },
                { "SaveDataType", 5 },
                { "PlayersTeam", playersTeam },
                { "PlayersScenario", playersScenario },
                { "PlayersLeague", playersLeague },
                { "PlayersWeeksWithSponsor", playersWeeksWithSponsor },
                { "PlayersSponsor", playersSponsor },
                { "PlayersMatchStrategy", playersMatchStrategy },
                { "PlayersMatchBreaker", playersMatchBreaker },
                { "PlayersYearsToRetire", playersYearsToRetire },
                { "Week", week },
                { "Formation", formationType },
                { "TeamsInScenarioLeague", numTeamsInScenarioLeague },
                { "PlayerRating", playerRating }
            }
        };
        PlayerPrefs.SetString(soccerSaveDataKey, JsonUtility.ToJson(scores));
        //Balance history
        List<Dictionary<string, object>> balanceHistory = new List<Dictionary<string, object>>();
        for (int i = 0; i < week; i++)
        {
            balanceHistory.Add(new Dictionary<string, object>
            {
                { "name", name },
                { "PlayersBalance", playersBalance[i] }
            });
        }
        PlayerPrefs.SetString(soccerBalanceHistoryDataKey, JsonUtility.ToJson(balanceHistory));
        //Player Formation
        List<Dictionary<string, object>> playerFormationData = new List<Dictionary<string, object>>();
        for (int i = 0; i < MaxPlayersInSquad; i++)
        {
            playerFormationData.Add(new Dictionary<string, object>
            {
                { "name", name },
                { "playerId", playersInFormation[i] }
            });
        }
        PlayerPrefs.SetString(soccerPlayerFormationDataKey, JsonUtility.ToJson(playerFormationData));
        //League data
        List<Dictionary<string, object>> leagueData = new List<Dictionary<string, object>>();
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            leagueData.Add(new Dictionary<string, object>
            {
                { "name", name },
                { "teamId", premiumLeagueData[i].teamId },
                { "matchesPlayed", premiumLeagueData[i].matchesPlayed },
                { "goalsFor", premiumLeagueData[i].goalsFor },
                { "goalsAgainst", premiumLeagueData[i].goalsAgainst },
                { "goalDifference", premiumLeagueData[i].goalDifference },
                { "leaguePoints", premiumLeagueData[i].leaguePoints }
            });
        }
        PlayerPrefs.SetString(soccerSaveLeagueDataKey, JsonUtility.ToJson(leagueData));
        //Dynamic team data
        List<Dictionary<string, object>> dynamicTeamData = new List<Dictionary<string, object>>();
        for (int i = 0; i < numberOfTeamsInArrays; i++)
        {
            dynamicTeamData.Add(new Dictionary<string, object>
            {
                { "name", name },
                { "teamId", staticTeamsData[i].teamId },
                { "leagueId", dynamicTeamsData[i].leagueID },
                { "cashBalance", dynamicTeamsData[i].cashBalance },
                { "fanMorale", dynamicTeamsData[i].fanMorale },
            });
        }
        PlayerPrefs.SetString(soccerDynamicTeamDataKey, JsonUtility.ToJson(dynamicTeamData));
        //Dynamic player data
        List<Dictionary<string, object>> dynamicPlayerData = new List<Dictionary<string, object>>();
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            dynamicPlayerData.Add(new Dictionary<string, object>
            {
                { "name", name },
                { "playerId", staticPlayersData[i].playerId },
                { "starsRating", dynamicPlayersData[i].starsRating },
                { "condition", dynamicPlayersData[i].condition },
                { "trainingTransfer", dynamicPlayersData[i].trainingTransfer },
                { "weeklySalary", dynamicPlayersData[i].weeklySalary },
                { "teamID", dynamicPlayersData[i].teamId },
                { "morale", dynamicPlayersData[i].morale },
                { "weeksBannedOrInjured", dynamicPlayersData[i].weeksBannedOrInjured },
                { "flags", dynamicPlayersData[i].flags },
            });
        }
        PlayerPrefs.SetString(soccerDynamicPlayerDataKey, JsonUtility.ToJson(dynamicPlayerData));
        //Dynamic manager data
        List<Dictionary<string, object>> dynamicManagerData = new List<Dictionary<string, object>>();
        for (int i = 0; i < numberOfManagersInArrays; i++)
        {
            dynamicManagerData.Add(new Dictionary<string, object>
            {
                { "name", name },
                { "managerId", staticManagersData[i].managerId },
                { "teamId", dynamicManagersData[i].teamId },
            });
        }
        PlayerPrefs.SetString(soccerDynamicManagerDataKey, JsonUtility.ToJson(dynamicManagerData));
        //Dynamic manager data
        List<Dictionary<string, object>> yellowCardData = new List<Dictionary<string, object>>();
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            yellowCardData.Add(new Dictionary<string, object>
            {
                { "name", name },
                { "playerId", staticPlayersData[i].playerId },
                { "yellowCards", premiumLeagueYellowCards[i] },
            });
        }
        PlayerPrefs.SetString(soccerLeagueYellowCardDataKey, JsonUtility.ToJson(yellowCardData));
        PlayerPrefs.Save();
        //Reset some match parameters
        lastOppositionTeamAssignmentId = -1;
        matchEngine.maxAwayTeamPlayersOnPitch = 11;
        matchEngine.maxHomeTeamPlayersOnPitch = 11;
    }
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
        Debug.Log("Rounds: " + rounds.Length + " " + totalRounds + " " + matchesPerRound);
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
                rounds[round, match] = new MatchInfo
                {
                    homeTeam = home,
                    awayTeam = away
                };
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

    public int FillTeamsInLeagueArray(int[] teamIndexes, Enums.LeagueID leagueID)
    {
        int count = 0;
        for (int i = 0; i < numberOfTeamsInArrays; i++)
        {
            Debug.Log(leagueID + " " + count);
            if (dynamicTeamsData[i].leagueID == leagueID) 
            {
                teamIndexes[count] = i;
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
        int result = -1;
        for (int i = 0; i < numberOfTeamsInArrays; i++)
        {
            if (staticTeamsData[i].teamId == teamId)
            {
                result = i;
                break;
            }
        }
        return result;
        
    }
    public void GoToMenu(int newScreen=0)
    {
        int oldScreen = (int)currentScreen;
        currentScreen = (Enums.Screen)newScreen;
        ScreenDefinition screenToActivate = screens[(int)newScreen];
        GameObject screenToDeactivate = screens[oldScreen].gameObject;

        screenToActivate.gameObject.SetActive(true);
        currentMenuItems = screenToActivate.MenuItems.GetComponentsInChildren<MenuItem>();
        if (screenToDeactivate != null)
        {
            screenToDeactivate.SetActive(false);
        }
        HandleCurrentScreen(currentScreen,currentMenuItems);
    }
    public void GoToMenu(Enums.Screen newScreen)
    {
        Enums.Screen oldScreen = currentScreen;
        currentScreen = newScreen;
        ScreenDefinition screenToActivate = screens[(int)newScreen];
        GameObject screenToDeactivate = screens[(int)oldScreen].gameObject;
        screenToDeactivate.SetActive(false);
        screenToActivate.gameObject.SetActive(true);
        currentMenuItems = screenToActivate.MenuItems.GetComponentsInChildren<MenuItem>();
        
        HandleCurrentScreen(currentScreen,currentMenuItems);
    }

    /// <summary>
    /// This handles preprocessing on screen transition.
    /// </summary>
    /// <param name="newScreen">The now-current screen</param>
    /// <param name="menuItems">An array of menuitems to be altered.</param>
    public void HandleCurrentScreen(Enums.Screen newScreen, MenuItem[] menuItems)
    {
        switch (newScreen)
        {
            case Enums.Screen.Title:
                if (CheckGameData())
                {
                    menuItems[0].gameObject.SetActive(true);
                }
                else
                {
                    menuItems[0].gameObject.SetActive(false);
                }
                
                break;
            case Enums.Screen.PreTurn:
                LeagueInfo info = GetLeagueInfoForId(playersLeague); 
                statsPreturn = info.leagueName + "\nWeek " + (week+1) + " of " + ((numTeamsInScenarioLeague-1)*2);
                menuItems[2].SetText(statsPreturn);
                string playerCashBalance = "Cash Balance: \n" + GetTeamCashBalance(playersTeam) + "k";
                menuItems[6].SetText(playerCashBalance);
                string[] managerRatingStrings =
                {
                    "Looking Grim",
                    "Not Going Well",
                    "Not Too Bad",
                    "Somewhat Dull",
                    "Better Than Dull",
                    "Progressing Well",
                    "Impressive",
                    // overflow strings
                    "Very Impressive",
                    "Godlike!",
                };
                string currentPlayerRating = "Manager Rating:\n" + managerRatingStrings[playerRating/(256/7)];
                menuItems[4].SetText(currentPlayerRating);
                menuItems[0].SetText(staticTeamsData[GetArrayIndexForTeam(playersTeam)].teamName);
                playersBalance[week] = GetTeamCashBalance(playersTeam);
                //reset turn values
                statsTurnIncome = 0;
                statsTurnIncomeTicketSales = 0;
                statsTurnIncomeSponsorsTV = 0;
                statsTurnExpend = 0;
                statsTurnExpendSalary = 0;

                statsPrevLeaguePos = GetPositionInLeagueTableForTeamId(playersTeam);

                if (week >= ((numTeamsInScenarioLeague - 1) * 2))
                {
                    GoToMenu(Enums.Screen.LeagueFinished);
                }
                break;
            case Enums.Screen.WeekPreview:
                menuItems[2].SetText(statsPreturn);
                float yOffset = -72f;
                for (int home = 0; home < numTeamsInScenarioLeague; home++)
                {
                    for (int away = 0; away < numTeamsInScenarioLeague; away++)
                    {
                        if (PremiumLeagueMatchesPlayed[home, away] == week)
                        {
                            int homeTeamIndex = teamIndexsForScenarioLeague[home];
                            int awayTeamIndex = teamIndexsForScenarioLeague[away];
                            int homeTeamId = staticTeamsData[homeTeamIndex].teamId;
                            int awayTeamId = staticTeamsData[awayTeamIndex].teamId;
                            string previewString = staticTeamsData[homeTeamIndex].teamName + " vs " + staticTeamsData[awayTeamIndex].teamName;
                            menuItemGenerator.CreateWeekPreview(screens[(int)currentScreen], new Vector2(0.0f, yOffset), previewString);
                            yOffset -= 32;
                        }
                    }
                }
                break;
           case Enums.Screen.Standings:
                menuItems[5].SetText(statsPreturn);
                float yOff = -72f;
                for (int i = 0; i < numTeamsInScenarioLeague; i++)
                {
                    int teamIndex = GetTeamDataIndexForTeamID(premiumLeagueData[i].teamId);
                    int teamNo = i + 1;
                    bool isSelf = premiumLeagueData[i].teamId == playersTeam;
                    menuItemGenerator.CreateStandings(screens[(int)currentScreen], new Vector2(0.0f, yOff), teamNo, isSelf, staticTeamsData[teamIndex].teamName, premiumLeagueData[i].matchesPlayed, premiumLeagueData[i].leaguePoints, premiumLeagueData[i].goalDifference);
                    yOff -= 32;
                }
                RectTransform items = screens[(int)currentScreen].MenuItems.transform.GetChild(0).GetComponent<RectTransform>();
                items.sizeDelta = new Vector2(320f,-yOff+240);
                break;
           case Enums.Screen.OtherBusiness:
                if (playersSponsor == -1)
                {
                    GoToMenu(Enums.Screen.AssignSponsor);
                }
               break;
           case Enums.Screen.TrainPlayers:
                RectTransform itemsForTraining = screens[(int)Enums.Screen.ChooseTeam].MenuItems.transform.GetChild(0).GetComponent<RectTransform>();
                
               
               float yOffTrain = 0.0f;
                int maxItems = numPlayersInPlayersTeam - (currentPage * MaxPlayersInList);
                if (maxItems > MaxPlayersInList) maxItems = MaxPlayersInList;
                itemsForTraining.sizeDelta = new Vector2(320f,menuItemGenerator.playerTrainingYOffset+120+(22*maxItems));
                for (int j = 0; j < MaxPlayersInATeam; j++)
                {
                    menuItemGenerator.GenerateMenuItem(screens[(int)Enums.Screen.TrainPlayers],MenuElement.TextBarHalf, new Vector2(0,-1*(395-menuItemGenerator.playerTrainingYOffset-(22*j))),0,0," "+(j+1) + ")", Enums.MenuAction.CyclePlayerTraining, j,itemsForTraining);
                } 
                for (int i = 0; i < maxItems; i++)
                {
                    int playerId = playersTeamPlayerIds[i + currentPage * MaxPlayersInList];
                    int playerDataIndex = GetPlayerDataIndexForPlayerID(playerId);

                    int textIndex = (int)(dynamicPlayersData[playerDataIndex].condition * 10.0f);
                    if (textIndex < 0) textIndex = 0; if (textIndex > 9) textIndex = 9;
                    int stars = (int)GetTeamLeagueAdjustedStarsRatingForPlayerIndex(playerDataIndex);
                    if (stars < 0) stars = 0; if (stars > 5) stars = 5;
                    int training = dynamicPlayersData[playerDataIndex].trainingTransfer & trainingMask;
                    int flag = 0;
                    if ((dynamicPlayersData[playerDataIndex].weeksBannedOrInjured & injuryMask) != 0)
                    {
                        flag = 0;
                    }
                    else if ((dynamicPlayersData[playerDataIndex].weeksBannedOrInjured & bannedMask) != 0)
                    {
                        flag = 1;
                    }
                    else if ((dynamicPlayersData[playerDataIndex].flags & YellowCardMask) != 0)
                    {
                        flag = 2;
                    }
                    if (dynamicPlayersData[playerDataIndex].condition < ShowInjuredRatio)
                    {
                        flag = 0;
                    }
                    menuItemGenerator.CreatePlayerTrainings(screens[(int)currentScreen], new Vector2(0.0f, yOffTrain), stars, training, flag, textIndex);
                    yOffTrain -= 22f;
                }
               break;
            case Enums.Screen.AssignSponsor:
            
                if (lastSponsorUpdateTurn != week)
                {
                    FillAvailableSponsorsList();
                    lastSponsorUpdateTurn = week;
                }
               
                int sponsorId = availableSponsors[0];
                IconBar sbar = menuItems[3].GetComponent<IconBar>();
                sbar.Populate(sponsorInfo[sponsorId].texture,sponsorInfo[sponsorId].textBig,sponsorInfo[sponsorId].textSmall);
                
                sponsorId = availableSponsors[1];
                sbar = menuItems[4].GetComponent<IconBar>();
                sbar.Populate(sponsorInfo[sponsorId].texture,sponsorInfo[sponsorId].textBig,sponsorInfo[sponsorId].textSmall);
                 
                sponsorId = availableSponsors[2];
                sbar = menuItems[5].GetComponent<IconBar>();
                sbar.Populate(sponsorInfo[sponsorId].texture,sponsorInfo[sponsorId].textBig,sponsorInfo[sponsorId].textSmall);
               break;
           case Enums.Screen.BuyMatchBreaker:
                if (lastMatchbreakerUpdateTurn != week)
                {
                    FillAvailableMatchbreakersList();
                    lastMatchbreakerUpdateTurn = week;
                }
               
                int mbId = availableMatchbreakers[0];
                IconBar bar = menuItems[3].GetComponent<IconBar>();
                MenuItem price = menuItems[6].GetComponent<MenuItem>();
                price.SetText("$"+matchbreakerInfo[mbId].cost+"k");
                bar.Populate(matchbreakerInfo[mbId].texture,matchbreakerInfo[mbId].textBig,matchbreakerInfo[mbId].textSmall);
                
                mbId = availableMatchbreakers[1];
                bar = menuItems[4].GetComponent<IconBar>();
                bar.Populate(matchbreakerInfo[mbId].texture,matchbreakerInfo[mbId].textBig,matchbreakerInfo[mbId].textSmall);
                price = menuItems[7].GetComponent<MenuItem>();
                price.SetText("$"+matchbreakerInfo[mbId].cost+"k");
                
                mbId = availableMatchbreakers[2];
                bar = menuItems[5].GetComponent<IconBar>();
                bar.Populate(matchbreakerInfo[mbId].texture,matchbreakerInfo[mbId].textBig,matchbreakerInfo[mbId].textSmall);
                price = menuItems[7].GetComponent<MenuItem>();
                price.SetText("$"+matchbreakerInfo[mbId].cost+"k");
                string matchbreakerCashTxt = "Cash: " + GetTeamCashBalance(playersTeam) + "k";
                menuItems[9].SetText(matchbreakerCashTxt);
                break;
        }
    }
    
    private void FillAvailableSponsorsList()
    {
        for (int i = 0; i < MaxSponsors; i++)
        {
            availableSponsors[i] = -1;
            int ind;
            bool bFree = true;

            do
            {
                bFree = true;
                ind = (int)(Random.value * MaxNumOfSponsors);
                for (int k = 0; k < MaxSponsors; k++)
                {
                    if (availableSponsors[k] == ind)
                    {
                        bFree = false;
                    }
                }
            } while (!bFree);
            availableSponsors[i] = ind;
        }
    }

    private void FillAvailableMatchbreakersList()
    {
        for (int i = 0; i < MaxMatchbreakers; i++)
        {
            availableMatchbreakers[i] = -1;
            int ind;
            bool bFree = true;

            do
            {
                bFree = true;
                ind = (int)(Random.value * MaxNumOfMatchbreakers);
                for (int k = 0; k < MaxMatchbreakers; k++)
                {
                    if (availableMatchbreakers[k] == ind)
                    {
                        bFree = false;
                    }
                }
            } while (!bFree);
            availableMatchbreakers[i] = ind;
        }
    }

    int GetPositionInLeagueTableForTeamId(int teamId)
    {
        int result = -1;

        for (int team = 0; team < numTeamsInScenarioLeague; team++)
        {
            if (premiumLeagueData[team].teamId == teamId)
            {
                result = team + 1;
                break;
            }
        }
        return result;
    }
    LeagueInfo GetLeagueInfoForId(LeagueID leagueId)
    {
        foreach (LeagueInfo info in leagueDetails)
        {
            if (info.leagueId == leagueId)
            {
                return info;
            }
        }
        return null;
    }
    int GetTeamCashBalance(int teamId)
    {
        int dataIndex = GetTeamDataIndexForTeamID(teamId);
        if (dataIndex != -1)
        {
            return dynamicTeamsData[dataIndex].cashBalance;    
        }
        else
        {
            return -1;
        }
    }
    int AddTeamCashBalance(int teamId, int value)
    {
        int dataIndex = GetTeamDataIndexForTeamID(teamId);
        if (dataIndex != -1)
        {
            dynamicTeamsData[dataIndex].cashBalance += value;   
            return dynamicTeamsData[dataIndex].cashBalance; 
        }
        else
        {
            return 0;
        }
    }
    /// <summary>
    /// This is a translation of the old test function, used when starting a new game.
    /// </summary>
    public void StartGame()
    {
        string jsonforData = PlayerPrefs.GetString(soccerSaveDataKey);
        Debug.Log(jsonforData);
        if (!string.IsNullOrEmpty(jsonforData) && !jsonforData.Equals("{}")) //savedata is present
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
        Debug.Log(staticTeamsData[GetArrayIndexForTeam(teamId)].teamName);
        int managerIndex = 0;
        if (managerIndex != -1)
        {
            dynamicManagersData[managerIndex].teamId = -1; // set unemployed
        }
        week = 0;

        playerRating = playerRatingStartValue;
        playersSponsor = -1;
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
        BuildMatchSchedule(numTeamsInScenarioLeague);
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
            Debug.Log(premiumLeagueData.Length + " " + staticTeamsData.Length + " " + dataIndex);
            premiumLeagueData[i] = ScriptableObject.CreateInstance<DynamicLeagueData>();
            premiumLeagueData[i].teamId = staticTeamsData[dataIndex].teamId;
            premiumLeagueData[i].matchesPlayed = 0;
            premiumLeagueData[i].goalsFor = 0;
            premiumLeagueData[i].goalsAgainst = 0;
            premiumLeagueData[i].goalDifference = 0;

        }
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            premiumLeagueYellowCards[i] = PremiumLeagueYellowsUntilBan & YellowCardsUntilBanMask;
        }
    }

    public void SwapLeagueTableEntry(int team1, int team2)
    {
        int teamId = premiumLeagueData[team1].teamId;
        int matchesPlayed = premiumLeagueData[team1].matchesPlayed;
        int goalsFor = premiumLeagueData[team1].goalsFor;
        int goalsAgainst = premiumLeagueData[team1].goalsAgainst;
        int goalDifference = premiumLeagueData[team1].goalDifference;
        int leaguePoints = premiumLeagueData[team1].leaguePoints;
        
        premiumLeagueData[team1].teamId =  premiumLeagueData[team2].teamId;
        premiumLeagueData[team1].matchesPlayed = premiumLeagueData[team2].matchesPlayed;
        premiumLeagueData[team1].goalsFor = premiumLeagueData[team2].goalsFor;
        premiumLeagueData[team1].goalsAgainst = premiumLeagueData[team2].goalsAgainst;
        premiumLeagueData[team1].goalDifference = premiumLeagueData[team2].goalDifference;
        premiumLeagueData[team1].leaguePoints = premiumLeagueData[team2].leaguePoints;
        
        premiumLeagueData[team2].teamId = teamId;
        premiumLeagueData[team2].matchesPlayed = matchesPlayed;
        premiumLeagueData[team2].goalsFor = goalsFor;
        premiumLeagueData[team2].goalsAgainst = goalsAgainst;
        premiumLeagueData[team2].goalDifference = goalDifference;
        premiumLeagueData[team2].leaguePoints = leaguePoints;
    }

    public void BuyMatchbreaker(int index)
    {
        int mbIndex = availableMatchbreakers[index];
        int cash = GetTeamCashBalance(playersTeam);
        int cost = matchbreakerInfo[mbIndex].cost;
        if (cash > cost)
        {
            AddTeamCashBalance(playersTeam, -cost);
            statsTurnExpend += cost;
            playersMatchBreaker = mbIndex;
            SaveGameData();
            GoToMenu(Enums.Screen.OtherBusiness);
        }
        else
        {
            SoundEngine_StartEffect(Sounds.BadInput);
        }
       
    }
    
    public void BuySponsor(int index)
    {
        int oldSponsor = playersSponsor;
        playersSponsor = availableSponsors[index];
        if (oldSponsor != playersSponsor) playersWeeksWithSponsor = 0;
        SaveGameData();
        GoToMenu(Enums.Screen.OtherBusiness);
       
    }

    public void SoundEngine_StartEffect(Sounds sound)
    {
        if (SFXEnabled)
        {
            aSource.PlayOneShot(aClip[(int)sound]);
        }
    }
}
