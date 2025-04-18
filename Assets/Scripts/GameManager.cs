using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
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
    public const int MaxPlayersOnScreen = 25;
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

    public const int DefaultGoalScoreBonus = 10;
    public const int DefaultIncomeFromSponsors = 200;
    public const float IncomePerTicket = 0.05f;


    
    public bool SFXEnabled = true;
    public bool VibrationEnabled = true;

    public Enums.Screen currentScreen;
    public int currentScreenSubState;
    public ScreenDefinition currentScreenDefinition;
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
    public float menuScrollY;
    public float saveMenuScrollY;
    public MenuScrollBar activeMenuScrollBar;
    public float scrollSpeed = 10f;
    private Vector2 lastTouchPosition;
    private bool isTouching = false;
    public Transform menuTransform;
    public float[] playerNameUVOffsets = new float[MaxPlayersInATeam+1];
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
    public Enums.LeagueID leagueEndSaveUsersLeague;
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
    public int[] playersInOppositionFormation = new int[MaxPlayersInSquad];

    public Formation[] cpuFormationBalance =
    {
        Formation.KFormation541,
        Formation.KFormation514,
        Formation.KFormation532,
        Formation.KFormation442,
        Formation.KFormation433,
        Formation.KFormation424,
        Formation.KFormation352,


    };

    [Header("Pitch Images")] 
    // Used instead of manually defining coordinates in renderScene
    public GameObject spritePrefab;
    public Sprite[] textures;
    public Sprite texturePitch;
    public Sprite iconSad;
    public Sprite iconHappy;
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
    public int[] endOfTurnBalanceArray = new int[MaxWeeks];
    public MenuGraph menuGraph;
    public int week;
    public MenuIconBar[] formationMenuIconBars;
    public Formation formationType;
    public FormationData[] formations = new FormationData[(int)Formation.KFormationMax];

    public string[] formationStrings =
    {
        "4-4-2",		// kFormation_442
        "4-2-4",		// kFormation_424
        "4-3-3",		// kFormation_433
        "5-3-2",		// kFormation_532
        "5-1-4",		// kFormation_514
        "3-5-2",		// kFormation_352
        "5-4-1",		// kFormation_541
    };
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
        GoToMenu(Enums.Screen.Title);
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMoveTap();
        RenderScene(1);
    }

    private void RenderScene(int mode=0)
    {
        
        // Draw specific elements under menuItems
        if (currentScreen is Enums.Screen.MatchEngine)
        {
            matchEngine.Render(Time.deltaTime);
        }
        if (currentScreen is Enums.Screen.ProcessLeagueFinish)
        {
            if ((week & 1) != 0)
                GoToMenu(Enums.Screen.ProcessLeagueFinish);
            ProcessLeagueEndData();
        }
        if (currentScreen is Enums.Screen.AssignPlayers or Enums.Screen.OppositionFormation)
        {
            if (mode == 0 ) ClearScene();
            FormationData formation;
            switch (currentScreen)
            {
                case Enums.Screen.AssignPlayers:
                    formation = formations[(int)formationType];
                    currentScreenDefinition.transform.GetChild(1).name = formationStrings[(int)formationType];
                    break;
                default:
                    formation = formations[(int)oppositionTeamFormationType];
                    currentScreenDefinition.transform.GetChild(1).name = formationStrings[(int)oppositionTeamFormationType];
                    break;
            }

            if (currentScreenSubState == 0)
            {
                formationSelectionScrollPos.x -= (formationSelectionScrollPos.x - 160) * 0.1f;
                formationSelectionScrollPos.y -= (formationSelectionScrollPos.y - 340) * 0.1f;
            }
            else
            {
                float formationScrollTargetX = (320*0.75f) + (-formation.formations[formationCycle].pos.x * 512);
                float formationScrollTargetY = 240 + (256+(-formation.formations[formationCycle].pos.y * 512));				
                formationSelectionScrollPos.x -= (formationSelectionScrollPos.x - formationScrollTargetX) * 0.1f;
                formationSelectionScrollPos.y -= (formationSelectionScrollPos.y - formationScrollTargetY) * 0.1f;
            }
            currentScreenDefinition.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(formationSelectionScrollPos.x, -formationSelectionScrollPos.y);
            currentScreenDefinition.transform.GetChild(1).Rotate(0f, 0f, 0.0f);
            if (mode == 0)
            {
                var pitch = RectMake(-164.5f, -256,330, 512,texturePitch);
                for (int i = 0; i < MaxPlayersInSquad; i++)
                {
                    int playerId;
                    int nameIndex;
                    int dataIndex = -1;

                    if (currentScreen != Enums.Screen.OppositionFormation)
                    {
                        playerId = playersInFormation[i];
                        nameIndex = GetIndexIntoPlayersTeamList(playersTeamPlayerIds, numPlayersInPlayersTeam, playerId);
                    }
                    else
                    {
                        playerId = playersInOppositionFormation[i];
                        nameIndex = GetIndexIntoPlayersTeamList(oppositionTeamPlayerIds, numPlayersInOppositionTeam, playerId);
                    }
                    
                    if (playerId != -1)
                    {
                        dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                        if (dynamicPlayersData[dataIndex].weeksBannedOrInjured != 0)
                        {
                            playerId = -1;
                            dataIndex = -1;
                            nameIndex = -1;
                        }
                    }

                    if (currentScreenSubState == 0) //show formation
                    {
                        if (playerId != -1)
                        {
                            if (dataIndex != -1)
                            {
                                bool home = false;
                                if (currentScreen != Enums.Screen.OppositionFormation)
                                {
                                    if (playersMatch.homeTeam == playersTeam) home = true;
                                }
                                else
                                {
                                    if (playersMatch.homeTeam != playersTeam) home = true;
                                }

                                Color primaryColor;
                                Color secondaryColor;
                                if (home)
                                {
                                    primaryColor = playersMatch.homeTeam1StColour;
                                    secondaryColor = playersMatch.homeTeam2NdColour;
                                }
                                else
                                {
                                    primaryColor = playersMatch.awayTeam1StColour;
                                    secondaryColor = playersMatch.awayTeam2NdColour;
                                }
                                
                                int stars = 1;
                                if (nameIndex != -1)
                                {
                                    int playerIndex = GetPlayerDataIndexForPlayerID(playerId);
                                    if (playerIndex != -1)
                                    {
                                        stars = (int)GetTeamLeagueAdjustedStarsRatingForPlayerIndex(playerIndex);
                                        if (stars < 0) stars = 0;
                                        if (stars > 5) stars = 5;
                                    }
                                }

                                int res = CheckPlayerIdIsHappyInFormation(playerId, formation.formations[i].formation);
                                menuItemGenerator.GenerateShirt(currentScreenDefinition, new Vector2((formation.formations[i].pos.x * 512),
                                -(-256 + (formation.formations[i].pos.y * 512))), stars,
                                    "", primaryColor, secondaryColor, res, dynamicPlayersData[dataIndex]);


                            }
                        }
                    }

                    menuItemGenerator.GenerateFormationMarker(currentScreenDefinition,new Vector2((formation.formations[i].pos.x * 512) - 12,
                        -(-256 + formation.formations[i].pos.y*512)-12), i, formation.formations[i].name);
                }
            }
            }
            
    }
    
    private int CheckPlayerIdIsHappyInFormation(int playerId, PlayerFormation formation)
    {
        int playerIndex = GetPlayerDataIndexForPlayerID(playerId);
        if ((formation & PlayerFormation.Substitute) != (PlayerFormation)0)
            return (int)PlayerFormation.Substitute;
        return (int)(staticPlayersData[playerIndex].playerPositionFlags & formation);
    }
    
    public int CheckPlayerIndexIsHappyInFormation(int playerIndex, FormationInfo formation)
    {
        if (playerIndex == -1) return 0;
        if ((formation.formation & PlayerFormation.Substitute) != (PlayerFormation)0)
            return (int)PlayerFormation.Substitute;
        return (int)(staticPlayersData[playerIndex].playerPositionFlags & formation.formation);
    }

    /// <summary>
/// A function to delete everything in renderScene
/// </summary>
    private void ClearScene()
    {
        Transform renderTarget = currentScreenDefinition.transform.GetChild(1);
        for (int c = 0; c < renderTarget.childCount; c++)
        {
            Destroy(renderTarget.GetChild(c).gameObject);
        }
    }

    /// <summary>
    /// Instantiates an Image prefab and assigns it to the current screen.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="sprite"></param>
    /// <returns></returns>
    private GameObject RectMake(float x, float y, float width, float height, Sprite sprite)
    {
        GameObject newImg = Instantiate(spritePrefab, currentScreenDefinition.transform.GetChild(1));
        Image img = newImg.GetComponent<Image>();
        img.sprite = sprite;
        newImg.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -y);
        newImg.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        return newImg;
    }
    
    private GameObject RectMake(float x, float y, float width, float height, Enums.Texture texture)
    {
        GameObject newImg = Instantiate(spritePrefab, currentScreenDefinition.transform.GetChild(1));
        Image img = newImg.GetComponent<Image>();
        img.sprite = textures[(int)texture];
        newImg.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -y);
        newImg.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        return newImg;
    }

    private int GetIndexIntoPlayersTeamList(int[] teamList, int numOnList, int forPlayerId)
    {
        int result = -1;
        for (int i = 0; i < numOnList; i++)
        {
            if (teamList[i] == forPlayerId)
            {
                result = i;
                break;
            }
        }
        return result;
    }

    private void HandleMoveTap()
    {
        if (Input.touchCount > 0 && activeMenuScrollBar)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && isTouching)
            {
                float deltaY = touch.position.y - lastTouchPosition.y;
                menuScrollY += deltaY * 0.4f; // Adjust sensitivity
                lastTouchPosition = touch.position;
                Debug.Log("menuScrollY: " + menuScrollY);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isTouching = false;
            }
        }
    }

    void FixedUpdate()
    {
        
    }
    public bool CheckGameData()
    {
        int checkSaveDataType = -1;
        string jsonforData = PlayerPrefs.GetString(soccerSaveDataKey);
        SoccerSaveData scores = JsonUtility.FromJson<SoccerSaveData>(jsonforData);

        if (scores != null)
        {
            return true;
        }
        else 
        {
            Debug.Log("No scores found in saved data. " + jsonforData);
            return false;
        }
    }
    public bool LoadGameData()
    {
        int checkSaveDataType = -1;
        string jsonforData = PlayerPrefs.GetString(soccerSaveDataKey);
        SoccerSaveData mainData = JsonUtility.FromJson<SoccerSaveData>(jsonforData);

    

        if (mainData.SaveDataType != 5 && mainData.SaveDataType != 4 && mainData.SaveDataType != 3)
        {
            Debug.LogError("Unsupported save data type.");
            return false;
        }


        playersTeam = mainData.PlayersTeam;
        playersLeague = mainData.PlayersLeague;
        playersYearsToRetire = mainData.PlayersYearsToRetire;
        playersScenario = mainData.PlayersScenario;
        week = mainData.Week;
        formationType = mainData.Formation;
        numTeamsInScenarioLeague = mainData.TeamsInScenarioLeague;
        playerRating  = mainData.PlayerRating;
        playersSponsor  = mainData.PlayersSponsor;
        playersWeeksWithSponsor  = mainData.PlayersWeeksWithSponsor;
        playersMatchBreaker  = mainData.PlayersMatchBreaker;
        playersMatchStrategy  = mainData.PlayersMatchStrategy;
        LoadLeagueData();
        LoadTeams();
        LoadPlayers();
        LoadManagers();
        
        string  balanceHistoryJson = PlayerPrefs.GetString(soccerBalanceHistoryDataKey);
        BalanceHistory balanceHistory = JsonUtility.FromJson<BalanceHistory>(balanceHistoryJson);
       
        if (balanceHistory.data.Count > 0)
        {
            if (balanceHistory.data.Count != week)
            {
                Debug.LogError("Loading Game failed: week count doesn't match balance history.");
                return false;
            }

            for (int i = 0; i < balanceHistory.data.Count; i++)
            {
                playersBalance[i] = balanceHistory.data[i];
            }
        }
        
        //Load Dynamic Team Data
        string  dynamicTeamJson = PlayerPrefs.GetString(soccerDynamicTeamDataKey);
        TeamSave dynamicTeam = JsonUtility.FromJson<TeamSave>(dynamicTeamJson);
       
        if (dynamicTeam.data.Count > 0)
        {
            if (dynamicTeam.data.Count != numberOfTeamsInArrays)
            {
                Debug.LogError("Loading Game failed: team count is different.");
                return false;
            }

            for (int i = 0; i < dynamicTeam.data.Count; i++)
            {
                SaveTeamData team = dynamicTeam.data[i];
                int teamId = team.teamId;
                int dataIndex = GetArrayIndexForTeam(teamId);
                if (dataIndex == -1)
                {
                    Debug.LogError("Loading Game failed: Team not found.");
                    return false;
                }
                
                dynamicTeamsData[dataIndex].leagueID = team.leagueID;
                dynamicTeamsData[dataIndex].cashBalance = team.cashBalance;
                dynamicTeamsData[dataIndex].fanMorale = team.fanMorale;
            }
        }
        //Load Dynamic player Data
        string  dynamicPlayerJson = PlayerPrefs.GetString(soccerDynamicPlayerDataKey);
        PlayerSave dynamicPlayer = JsonUtility.FromJson<PlayerSave>(dynamicPlayerJson);
       
        if (dynamicPlayer.data.Count > 0)
        {
            if (dynamicPlayer.data.Count != numberOfPlayersInArrays)
            {
                Debug.LogError("Loading Game failed: Player count doesnt match.");

                return false;
            }

            for (int i = 0; i < dynamicPlayer.data.Count; i++)
            {
                SavePlayerData player = dynamicPlayer.data[i];
                int playerId = player.playerID;
                int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                if (dataIndex == -1)
                {
                    Debug.LogError("Loading Game failed: Player not found");

                    return false;
                }
                
                dynamicPlayersData[dataIndex].starsRating = player.starsRating;
                dynamicPlayersData[dataIndex].condition = player.condition;
                dynamicPlayersData[dataIndex].trainingTransfer = player.trainingTransfer;
                dynamicPlayersData[dataIndex].teamId = player.teamId;
                dynamicPlayersData[dataIndex].weeklySalary = player.weeklySalary;
                dynamicPlayersData[dataIndex].morale = player.morale;
                dynamicPlayersData[dataIndex].weeksBannedOrInjured = player.weeksBannedOrInjured;
                dynamicPlayersData[dataIndex].flags = player.flags;
            }
        }
        //Load Dynamic Manager Data
        string  dynamicManagerJson = PlayerPrefs.GetString(soccerDynamicManagerDataKey);
        ManagerSave dynamicManager = JsonUtility.FromJson<ManagerSave>(dynamicManagerJson);
       
        if (dynamicManager.data.Count > 0)
        {
            if (dynamicManager.data.Count != numberOfManagersInArrays)
            {
                Debug.LogError("Loading Game failed: Manager counts do not match.");
                return false;
            }

            for (int i = 0; i < dynamicManager.data.Count; i++)
            {
                SaveManagerData manager = dynamicManager.data[i];
                int managerId = manager.managerId;
                int dataIndex = GetIndexToManagerForManagerID(managerId);
                if (dataIndex == -1)
                {
                    Debug.LogError("Loading Game failed: Manager "+i+" not found\n"+dynamicManagerJson);

                    return false;
                }
                
                dynamicManagersData[dataIndex].teamId = manager.teamId;
            }
        }
        // Check that player manages their own team!!!
        int managerIndex = GetIndexToManagerForTeamID(playersTeam);
        if (managerIndex != -1)
        {
            dynamicManagersData[managerIndex].teamId = -1; // fire the manager associated with the players team!!! 
        }
        FillTeamsInLeagueArray(teamIndexsForScenarioLeague, playersLeague);
        numPlayersInPlayersTeam = FillTeamPlayerArray(playersTeamPlayerIds, playersTeam);
        AutofillFormationFromPlayerIDs(playersInFormation,playersTeamPlayerIds,numPlayersInPlayersTeam,formationType,playersTeam);
        
        // Formation data.
        string playerFormationJson = PlayerPrefs.GetString(soccerPlayerFormationDataKey);
        List<int> playerFormationData = JsonUtility.FromJson<List<int>>(playerFormationJson);
        if (playerFormationData.Count > 0)
        {
            int count = playerFormationData.Count;
            if (count != MaxPlayersInSquad)
            {
                Debug.LogError("Loading Game failed: formation count doesnt match squad");
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                playersInFormation[i] = playerFormationData[i];
            }
        }
        
        // League data. The simplest of them
        string leagueDataJson = PlayerPrefs.GetString(soccerSaveLeagueDataKey);
        LeagueSave league = JsonUtility.FromJson<LeagueSave>(leagueDataJson);
        if (league.data.Count > 0)
        {
            int count = league.data.Count;
            Debug.Assert(count == numTeamsInScenarioLeague);
            ResetLeaguePoints();
            for (int i = 0; i < count; i++)
            {
                SaveLeagueData leagueData = league.data[i];
                
                premiumLeagueData[i].teamId = leagueData.teamId;
                premiumLeagueData[i].matchesPlayed = leagueData.matchesPlayed;
                premiumLeagueData[i].leaguePoints = leagueData.leaguePoints;
                premiumLeagueData[i].goalDifference = leagueData.goalDifference;
                premiumLeagueData[i].goalsAgainst = leagueData.goalsAgainst;
                premiumLeagueData[i].goalsFor = leagueData.goalsFor;
            }
        }

        string yellowCardJson = PlayerPrefs.GetString(soccerLeagueYellowCardDataKey);
        CardSave yellowCard = JsonUtility.FromJson<CardSave>(yellowCardJson);
        if (yellowCard.data.Count > 0)
        {
            int count = yellowCard.data.Count;
            if (count != numberOfPlayersInArrays && checkSaveDataType != 3)
            {
                Debug.LogError("Loading Game failed: card count doesnt match player count");
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                int dataIndex = i;
                premiumLeagueYellowCards[dataIndex] = yellowCard.data[i].yellowCard;
            }
        }
        return true;
    }
  
    private void SaveGameData()
    {
        string name = PlayerPrefs.GetString(usernameDefaultKey, "Player");
        DateTime date = DateTime.Now;
        
        SoccerSaveData score = new SoccerSaveData
        {
            name = name,
            date = date.ToString(CultureInfo.InvariantCulture),
            SaveDataType = 5,
            PlayersTeam = playersTeam,
            PlayersScenario = playersScenario,
            PlayersLeague = playersLeague,
            PlayersWeeksWithSponsor = playersWeeksWithSponsor,
            PlayersSponsor = playersSponsor,
            PlayersMatchStrategy = playersMatchStrategy,
            PlayersMatchBreaker = playersMatchBreaker,
            PlayersYearsToRetire = playersYearsToRetire,
            Week = week,
            Formation = formationType,
            TeamsInScenarioLeague = numTeamsInScenarioLeague,
            PlayerRating = playerRating
        };
     
        PlayerPrefs.SetString(soccerSaveDataKey, JsonUtility.ToJson(score));
        //Balance history
        BalanceHistory balanceHistory = new BalanceHistory
        {
            data = new List<int>()
        };
        for (int i = 0; i < week; i++)
        {
            balanceHistory.data.Add(playersBalance[i]);
        }
        PlayerPrefs.SetString(soccerBalanceHistoryDataKey, JsonUtility.ToJson(balanceHistory));
        //Player Formation
        BalanceHistory playerFormation = new BalanceHistory()
        {
            data = new List<int>()
        };
        for (int i = 0; i < MaxPlayersInSquad; i++)
        {
            playerFormation.data.Add(playersInFormation[i]);
        }
        PlayerPrefs.SetString(soccerPlayerFormationDataKey, JsonUtility.ToJson(playerFormation));
        //League data
        LeagueSave leagues = new LeagueSave
        {
            data = new List<SaveLeagueData>()
        };
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            SaveLeagueData league = new SaveLeagueData
            {
                matchesPlayed = premiumLeagueData[i].matchesPlayed,
                teamId =  premiumLeagueData[i].teamId,
                leaguePoints = premiumLeagueData[i].leaguePoints,
                goalDifference = premiumLeagueData[i].goalDifference,
                goalsFor = premiumLeagueData[i].goalsFor,
                goalsAgainst = premiumLeagueData[i].goalsAgainst,
            };
            leagues.data.Add(league);
        }
        PlayerPrefs.SetString(soccerSaveLeagueDataKey, JsonUtility.ToJson(leagues));
        //Dynamic team data
        TeamSave dynamicTeam = new TeamSave
        {
            data = new List<SaveTeamData>()
        };
        for (int i = 0; i < numberOfTeamsInArrays; i++)
        {
            SaveTeamData data = new SaveTeamData()
            {
                teamId = staticTeamsData[i].teamId,
                leagueID = dynamicTeamsData[i].leagueID,
                cashBalance = dynamicTeamsData[i].cashBalance,
                fanMorale =  dynamicTeamsData[i].fanMorale,
            };
            dynamicTeam.data.Add(data);
        }
        PlayerPrefs.SetString(soccerDynamicTeamDataKey, JsonUtility.ToJson(dynamicTeam));
        //Dynamic player data
        PlayerSave dynamicPlayer = new PlayerSave
        {
            data = new List<SavePlayerData>()
        };
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            SavePlayerData data = new SavePlayerData
            {
                playerID = staticPlayersData[i].playerId,
                trainingTransfer =  dynamicPlayersData[i].trainingTransfer,
                starsRating =  dynamicPlayersData[i].starsRating,
                condition =  dynamicPlayersData[i].condition,
                weeklySalary =  dynamicPlayersData[i].weeklySalary,
                morale =  dynamicPlayersData[i].morale,
                weeksBannedOrInjured =  dynamicPlayersData[i].weeksBannedOrInjured,
                flags =  dynamicPlayersData[i].flags,
                teamId = dynamicPlayersData[i].teamId,
            };
            dynamicPlayer.data.Add(data);
        }
        PlayerPrefs.SetString(soccerDynamicPlayerDataKey, JsonUtility.ToJson(dynamicPlayer));
        //Dynamic manager data
        ManagerSave dynamicManager = new ManagerSave
        {
            data = new List<SaveManagerData>()
        };
        for (int i = 0; i < numberOfManagersInArrays; i++)
        {
            SaveManagerData data = new SaveManagerData
            {
                managerId = staticManagersData[i].managerId,
                teamId =  dynamicManagersData[i].teamId
            };
            dynamicManager.data.Add(data);
        }
        PlayerPrefs.SetString(soccerDynamicManagerDataKey, JsonUtility.ToJson(dynamicManager));
        //Dynamic manager data
        CardSave yellowCard = new CardSave
        {
            data = new List<YellowCardData>()
        };
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            yellowCard.data.Add(new YellowCardData
            {
                playerId = staticPlayersData[i].playerId,
                yellowCard = premiumLeagueYellowCards[i],
            });
        }
        PlayerPrefs.SetString(soccerLeagueYellowCardDataKey, JsonUtility.ToJson(yellowCard));
        PlayerPrefs.Save();
        //Reset some match parameters
        lastOppositionTeamAssignmentId = -1;
        matchEngine.maxAwayTeamPlayersOnPitch = 11;
        matchEngine.maxHomeTeamPlayersOnPitch = 11; 
        Debug.Log("Save information\nSoccer Data\t"+PlayerPrefs.GetString(soccerSaveDataKey)+"\nBalance History\t"+PlayerPrefs.GetString(soccerBalanceHistoryDataKey)+"\nLeague Data\t"+PlayerPrefs.GetString(soccerSaveLeagueDataKey)+"\nTeam Data\t"+PlayerPrefs.GetString(soccerDynamicTeamDataKey)+"\nPlayer Data\t"+"\nManager Data\t"+PlayerPrefs.GetString(soccerDynamicManagerDataKey) +"\nYellow Card Data\t"+PlayerPrefs.GetString(soccerLeagueYellowCardDataKey));
    }
    private void LoadTeams() 
    {
        numberOfTeamsInArrays = 0;
        string path = "teamdata";
        TextAsset data = Resources.Load<TextAsset>(path);
        //Check if data file exists.
        if (data != null)
        {
            string[] lines = data.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int teamCount = int.Parse(lines[0]);
            for (int i = 0; i < teamCount; i++) //Iterate through each line
            {
                string[] line = lines[i+1].Split();
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
                    dynamicTeamsData[numberOfTeamsInArrays].name = staticTeamsData[numberOfTeamsInArrays].name;
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
        string path = "Manager_Data";
        //Check if data file exists.
        TextAsset data = Resources.Load<TextAsset>(path);
        if (data != null)
        {
            string[] lines = data.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int managerCount = int.Parse(lines[0]);
            for (int i = 0; i < managerCount; i++) //Iterate through each line
            {
                string[] line = lines[i+1].Split();
                if (line != null && line.Length == 4) // ensure its split enough
                {
                    //Add to the list of leaguesData.
                    staticManagersData[numberOfManagersInArrays] = ScriptableObject.CreateInstance<StaticManagerData>();
                    staticManagersData[numberOfManagersInArrays].LoadStaticManagerData(line);
                    dynamicManagersData[numberOfManagersInArrays] = ScriptableObject.CreateInstance<DynamicManagerData>();
                    dynamicManagersData[numberOfManagersInArrays].LoadDynamicManagerData(line);
                    dynamicManagersData[numberOfManagersInArrays].name = staticManagersData[numberOfManagersInArrays].name;
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
        Debug.Log("Load game succesful" + loadGameSuccessful);
        if (loadGameSuccessful)
        {
            BuildMatchSchedule(numTeamsInScenarioLeague);
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
        GoToMenu(Enums.Screen.ChooseTeam);
        RectTransform items = currentScreenDefinition.MenuItems.GetComponent<RectTransform>();
        items.sizeDelta = new Vector2(320f,menuItemGenerator.menuBarSpacing*numTeamsInScenarioLeague+72);
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            int dataIndex = teamIndexsForScenarioLeague[i];
            menuItemGenerator.GenerateMenuItem(currentScreenDefinition,Enums.MenuElement.TextBar, new Vector2(0,-72-menuItemGenerator.menuBarSpacing*i),0,0,staticTeamsData[dataIndex].teamName, Enums.MenuAction.SelectTeamAndCreateGame, staticTeamsData[dataIndex].teamId);
        }
        menuItemGenerator.GenerateMenuItem(currentScreenDefinition,Enums.MenuElement.Button, new Vector2(8,-72-menuItemGenerator.menuBarSpacing*numTeamsInScenarioLeague),0,0,"backButton", Enums.MenuAction.GotoMenu, (int)Enums.Screen.ChooseLeague);

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
        string path = "LeagueData";
        TextAsset data = Resources.Load<TextAsset>(path);
        //Check if data file exists.
        if (data != null)
        {
            string[] lines = data.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int leagueCount = int.Parse(lines[0]);
            for (int i = 0; i < leagueCount; i++) //Iterate through each line
            {
                string[] line = lines[i+1].Split();
                if (line.Length == 8) // ensure its split enough
                {
                    //Add to the list of leaguesData.
                    staticLeaguesData[numberOfLeaguesInArrays] = ScriptableObject.CreateInstance<StaticLeagueData>();
                    Debug.Log(lines[i]);
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
        string path = "PlayerData";
        TextAsset data = Resources.Load<TextAsset>(path);
        //Check if data file exists.
        if (data != null)
        {
            string[] lines = data.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int teamCount = int.Parse(lines[0]);
            for (int i = 0; i < teamCount; i++) //Iterate through each line
            {
                string[] line = lines[i+1].Split();
                if (line.Length == 5) // ensure its split enough
                {
                    //Add to the list of playerData.
                    staticPlayersData[numberOfPlayersInArrays] = ScriptableObject.CreateInstance<StaticPlayerData>();
                    staticPlayersData[numberOfPlayersInArrays].LoadStaticPlayerData(line);
                    dynamicPlayersData[numberOfPlayersInArrays] = ScriptableObject.CreateInstance<DynamicPlayerData>();
                    dynamicPlayersData[numberOfPlayersInArrays].LoadDynamicPlayerData(line);
                    dynamicPlayersData[numberOfPlayersInArrays].name = staticPlayersData[numberOfPlayersInArrays].playerSurname;
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
        GoToMenu((Enums.Screen)newScreen);
    }
    public void GoToMenu(Enums.Screen newScreen)
    {
        menuScrollY = 0;
        formationCycle = 0;
        formationSelectionScrollPos = Vector2.zero;
        
        currentScreen = newScreen;
        ScreenDefinition screenToActivate = screens[(int)newScreen];
        ScreenDefinition screenToDeactivate = currentScreenDefinition;
        Destroy(screenToDeactivate?.gameObject);
        currentScreenDefinition = Instantiate(screenToActivate, menuTransform);
        currentScreenDefinition.gameObject.SetActive(true);
        currentScreenDefinition.transform.SetSiblingIndex(0);
        currentMenuItems = currentScreenDefinition.MenuItems.GetComponentsInChildren<MenuItem>();
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
            case Enums.Screen.Options:
                SetOptionsRadioButtons(menuItems);
                break;
            case Enums.Screen.PreTurn:
                matchEngine.state = MatchEngineState.MatchOver;
                LeagueInfo info = GetLeagueInfoForId(playersLeague); 
                menuItems[0].SetText(staticTeamsData[GetArrayIndexForTeam(playersTeam)].teamName);
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
                            WeekPreview preview = menuItemGenerator.CreateWeekPreview(currentScreenDefinition, new Vector2(0.0f, yOffset), previewString);
                            if (playersTeam == homeTeamId || playersTeam == awayTeamId)
                                preview.mText.color = new Color(1.0f,1.0f,0.8f);
                            yOffset -= 32;
                        }
                    }
                }
                break;
           case Enums.Screen.Standings:
                menuItems[6].SetText(statsPreturn);
                float yOff = -72f;
                for (int i = 0; i < numTeamsInScenarioLeague; i++)
                {
                    int teamIndex = GetTeamDataIndexForTeamID(premiumLeagueData[i].teamId);
                    int teamNo = i + 1;
                    bool isSelf = premiumLeagueData[i].teamId == playersTeam;
                    TeamStandings standings= menuItemGenerator.CreateStandings(currentScreenDefinition, new Vector2(16.0f, yOff), teamNo, isSelf, staticTeamsData[teamIndex].teamName, premiumLeagueData[i].matchesPlayed, premiumLeagueData[i].leaguePoints, premiumLeagueData[i].goalDifference);
                    yOff -= 32;
                    if (premiumLeagueData[i].teamId == playersTeam)
                        standings.SetColor(new Color(1.0f,1.0f,0.8f), new Color (0.9f,0.9f,0.7f));
                }
                RectTransform items = menuItems[1].transform.GetComponent<RectTransform>();
                items.sizeDelta = new Vector2(288f,-yOff);
                break;
           case Enums.Screen.OtherBusiness:
                if (playersSponsor == -1)
                {
                    GoToMenu(Enums.Screen.AssignSponsor);
                }
               break;
           case Enums.Screen.BuyPlayers:
                int playersOnMarket = CountPlayersOnTransferMarketExcludingTeam(playersTeam);
                int numPlayersInUsersTeam = CountNumberOfPlayersInTeam(playersTeam);
                currentNumberOfPage = (playersOnMarket / MaxPlayersInList) + 1;
                if (currentNumberOfPage > MaxSheets)
                    currentNumberOfPage = MaxSheets;
                if (currentPage >= currentNumberOfPage)
                    currentPage = currentNumberOfPage - 1;
                MenuScrollBar buyBar = menuItems[3].GetComponent<MenuScrollBar>();
                int buttonYBuy = 8;

                if ((playersOnMarket == 0) || (numPlayersInUsersTeam>=MaxPlayersInATeam))
                {
                    string textToUse;
                    if (numPlayersInUsersTeam>=MaxPlayersInATeam)
                        textToUse = "Maximum number of players reached. Please sell a player to buy more.";
                    else
                        textToUse = "No players are available to buy at this time.\\nPlease try again later.";
                    
                    menuItems[1].pos = new Vector2(menuItems[1].pos.x, 180);
                    menuItems[1].AdjustPosition(); menuItems[1].SetText("(256<<16)|104");
                    menuItems[2].pos = new Vector2(menuItems[2].pos.x, 180);
                    menuItems[2].AdjustPosition(); menuItems[2].SetText(textToUse);
                    menuItems[4].pos = new Vector2(menuItems[4].pos.x, 16);
                    menuItems[4].AdjustPosition(); menuItems[4].SetText("backButton");
                    buttonYBuy = -256;
                    menuItems[5].pos = new Vector2(menuItems[5].pos.x, buttonYBuy);
                    menuItems[5].AdjustPosition(); menuItems[5].SetText("Page " + (currentPage+1) + "/" + currentNumberOfPage);
                    menuItems[6].pos = new Vector2(menuItems[6].pos.x, buttonYBuy);
                    menuItems[6].AdjustPosition(); menuItems[6].SetText("pageButtonPrev");
                    menuItems[7].pos = new Vector2(menuItems[7].pos.x, buttonYBuy);
                    menuItems[7].AdjustPosition(); menuItems[7].SetText("pageButtonNext");
                    buyBar.minMaxRange = new Vector2(0, 0);
                    
                }
                else {
                   //Generate List
                   menuItems[1].pos = new Vector2(menuItems[1].pos.x, 310);
                   menuItems[1].AdjustPosition(); menuItems[1].SetText("(256<<16)|104");
                   menuItems[2].pos = new Vector2(menuItems[2].pos.x, 310);
                   menuItems[2].AdjustPosition(); menuItems[2].SetText("Tap a player's name to make an offer\nCash: $"+GetTeamCashBalance(playersTeam)+"k");
                   
                    int maxItemsBuying = playersOnMarket;
                    if (maxItemsBuying > MaxPlayersOnScreen) maxItemsBuying = MaxPlayersOnScreen;
                   
                   buyBar.minMaxRange = new Vector2(0, -((menuItemGenerator.playerBuySpacing * maxItemsBuying) +
                                                         menuItemGenerator.playerBuyingYOffset + 112));
                   if (buyBar.minMaxRange.y < -480) 
                       buyBar.minMaxRange = new Vector2(0, buyBar.minMaxRange.y+480);
                   else
                       buyBar.minMaxRange = new Vector2(0, 0);
                   
                   float yOffBuy = -22.0f;
                   
                    for (int i = 0; i < maxItemsBuying; i++)
                    {
                        int playerId = transferList[i];
                        int playerDataIndex = GetPlayerDataIndexForPlayerID(playerId);
                       string nameString = String.Empty;
                       string playerLikesPositionString = String.Empty;
                       Color color = Color.white;
                        if (playerDataIndex != -1) // Handle Player name
                        {
                            string positionString = "--";
                           
                            playerLikesPositionString = FillPlayerLikesStringForPlayerIndex(playerDataIndex);
                            int formationIndex = FillPositionStringForPlayerIndexs(playerDataIndex, formations[(int)formationType], ref positionString);
                            if (formationIndex != -1) // Turn yellow
                            {
                                color = new Color(1.0f,1.0f,0.8f,1.0f);
                            }

                            if (dynamicPlayersData[playerDataIndex].weeksBannedOrInjured != 0) // Turn Red
                            {
                                color = new Color(1.0f,0.0f,0.0f,1.0f);
                            }
                            nameString = "("+positionString+") "+staticPlayersData[playerDataIndex].playerSurname;
                        }


                        int playerValue = 0;
                        int teamId = dynamicPlayersData[playerDataIndex].teamId;
                        if (teamId != -1)
                        {
                            int leagueId = GetTeamsLeagueID(teamId);
                            playerValue = DetermineValueOfPlayerID(playerId, leagueId);
                        }
                        menuItemGenerator.CreatePlayerTrainings(currentScreenDefinition, new Vector2(16.0f, yOffBuy), playerValue, nameString,color,playerLikesPositionString,dynamicPlayersData[playerDataIndex]);
                        menuItemGenerator.GenerateMenuItem(currentScreenDefinition,MenuElement.TextBarHalfDouble, new Vector2(0,yOffBuy),0,0," "+(i+1) + ")", Enums.MenuAction.BuyPlayerReview, i).name = nameString;
                        yOffBuy -= menuItemGenerator.playerBuySpacing;
                    }
                    yOffBuy = 0.0f;
                    buttonYBuy = (int)(yOffBuy - menuItemGenerator.playerBuyingYOffset*2);
                   menuItems[5].pos = new Vector2(menuItems[5].pos.x, -buttonYBuy);
                   menuItems[5].AdjustPosition(); menuItems[5].SetText("Page " + (currentPage+1) + "/" + currentNumberOfPage);
                   menuItems[6].pos = new Vector2(menuItems[6].pos.x, -buttonYBuy);
                   menuItems[6].AdjustPosition(); menuItems[6].SetText("pageButtonPrev");
                   menuItems[7].pos = new Vector2(menuItems[7].pos.x, -buttonYBuy);
                   menuItems[7].AdjustPosition(); menuItems[7].SetText("pageButtonNext");
                   }
                break;
            case Enums.Screen.BuyPlayerOffer:
                int offerPlayerId = currentBuyPlayerId;
                int offerPlayerIndex = GetPlayerDataIndexForPlayerID(offerPlayerId);
                int offerPlayerValue = 0;
                int offerTeamIndex = -1;
                int offerTeamId = dynamicPlayersData[offerPlayerIndex].teamId;
                if (offerTeamId != -1)
                {
                    int leagueId = GetTeamsLeagueID(offerTeamId);
                    offerPlayerValue = DetermineValueOfPlayerID(offerPlayerId, leagueId);
                    offerTeamIndex = GetTeamDataIndexForTeamID(offerTeamId);
                }

                int condPercnt = (int)dynamicPlayersData[offerPlayerIndex].condition * 100;

                string playerPositionString = FillPlayerLikesStringForPlayerIndex(offerPlayerIndex);
                
                TransferStatus status = (TransferStatus)((dynamicPlayersData[offerPlayerIndex].trainingTransfer & transferMask) >> transferBitShift);
                string statusString;
                switch (status)
                {
                    case TransferStatus.FreeTransfer: statusString = "Free Transfer"; break;
                    case TransferStatus.OffersAtValue: statusString = "Offers at value"; break;
                    case TransferStatus.AnyOffers: statusString = "Any offer considered"; break;
                    default: statusString = "ERROR"; break;
                }

                string buyPlayerDetails;
                if (offerTeamIndex != -1)
                    buyPlayerDetails = "Name: "+staticPlayersData[offerPlayerIndex].playerSurname+"\nCond: "+condPercnt+"% Pos: "+playerPositionString+"\nTeam: "+staticTeamsData[offerTeamIndex].teamName+"\nValue: $"+offerPlayerValue+"\n("+statusString+")";
                else
                    buyPlayerDetails = "Name: "+staticPlayersData[offerPlayerIndex].playerSurname+"\nCond: "+condPercnt+"% Pos: "+playerPositionString+"\nTeam: Unemployed\nValue: $"+offerPlayerValue+"\n("+statusString+")";
                menuItems[4].SetText(buyPlayerDetails);
                string buyPlayerOfferTxt = "Current Offer:\n$"+currentBuyPlayerOffer+"k";
                menuItems[6].SetText(buyPlayerOfferTxt);
                break;
            case Enums.Screen.BuyPlayerConfirmOffer:
                int currentPlayerId = currentBuyPlayerId;
                int currentPlayerIndex = GetPlayerDataIndexForPlayerID(currentPlayerId);
                int currentPlayerValue = 0;
                int currentTeamIndex = -1;
                int currentTeamId = dynamicPlayersData[currentPlayerIndex].teamId;
                if (currentTeamId != -1)
                {
                    int leagueId = GetTeamsLeagueID(currentTeamId);
                    currentPlayerValue = DetermineValueOfPlayerID(currentPlayerId, leagueId);
                    currentTeamIndex = GetTeamDataIndexForTeamID(currentTeamId);
                }
                bool happyToSell = false;
                TransferStatus currentStatus = (TransferStatus)((dynamicPlayersData[currentPlayerIndex].trainingTransfer & transferMask) >> transferBitShift);
                switch (currentStatus)
                {
                    case TransferStatus.FreeTransfer: happyToSell = true; break;
                    case TransferStatus.OffersAtValue:
                        if (currentBuyPlayerOffer >= currentPlayerValue)
                            happyToSell = true;
                        break;
                    case TransferStatus.AnyOffers:
                        float acceptableRatio = 0.9f;
                        if (currentBuyPlayerOffer >= (currentPlayerValue * acceptableRatio))
                            happyToSell = true;
                        break;
                    default: happyToSell = false; break;
                }
                
                if (!happyToSell)
                    menuItems[2].SetText(staticTeamsData[currentTeamIndex].teamName+" have declined your offer of $"+currentBuyPlayerOffer+"k for their player "+staticPlayersData[currentPlayerIndex].playerSurname+".\n");
                else
                {
                    if (currentTeamId != 1)
                        menuItems[2].SetText(staticTeamsData[currentTeamIndex].teamName+" have ACCEPTED your offer of $"+currentBuyPlayerOffer+"k for their player "+staticPlayersData[currentPlayerIndex].playerSurname+".\n\nThe player will leave for his new team immediately.");
                    else // was unemployed 
                        menuItems[2].SetText(staticPlayersData[currentPlayerIndex].playerSurname+" has ACCEPTED your job offer.");
                    BuyOfferedPlayer(currentPlayerIndex, currentTeamId);
                }
                break;
            case Enums.Screen.SellPlayers:
                menuScrollY = saveMenuScrollY;
                currentNumberOfPage = (numPlayersInPlayersTeam / MaxPlayersInList) + 1;
                if (currentPage >= currentNumberOfPage)
                {
                    currentPage = currentNumberOfPage - 1;
                }
                
               //Generate List
               float yOffSell = 0.0f;
                int maxItemsSelling = numPlayersInPlayersTeam - (currentPage * MaxPlayersInList);
                if (maxItemsSelling > MaxPlayersInList) maxItemsSelling = MaxPlayersInList;

                int buttonY = -286;
                if (maxItemsSelling > 11)
                    buttonY = 395 - menuItemGenerator.playerTrainingYOffset - (22 * (maxItemsSelling + 1));
                menuItems[4].pos = new Vector2(menuItems[4].pos.x, -buttonY);
                menuItems[4].AdjustPosition(); menuItems[4].SetText("backButton");
                menuItems[5].pos = new Vector2(menuItems[5].pos.x, -buttonY);
                menuItems[5].AdjustPosition(); menuItems[5].SetText("Page " + (currentPage+1) + "/" + currentNumberOfPage);
                menuItems[6].pos = new Vector2(menuItems[6].pos.x, -buttonY);
                menuItems[6].AdjustPosition(); menuItems[6].SetText("pageButtonPrev");
                menuItems[7].pos = new Vector2(menuItems[7].pos.x, -buttonY);
                menuItems[7].AdjustPosition();menuItems[7].SetText("pageButtonNext");
                MenuScrollBar sellBar = menuItems[3].GetComponent<MenuScrollBar>();
                sellBar.minMaxRange = new Vector2(0, -(22 * maxItemsSelling)+menuItemGenerator.playerTrainingYOffset+116);
                if (sellBar.minMaxRange.y < -480) 
                    sellBar.minMaxRange = new Vector2(0, sellBar.minMaxRange.y+480);
                else
                    sellBar.minMaxRange = new Vector2(0, 0);
                
                for (int i = 0; i < maxItemsSelling; i++)
                {
                    int playerId = playersTeamPlayerIds[i + currentPage * MaxPlayersInList];
                    int playerDataIndex = GetPlayerDataIndexForPlayerID(playerId);
                   string nameString = String.Empty;
                   string playerLikesPositionString = String.Empty;
                   Color color = Color.white;
                    if (playerDataIndex != -1) // Handle Player name
                    {
                        string positionString = "--";
                       
                        playerLikesPositionString = FillPlayerLikesStringForPlayerIndex(playerDataIndex);
                        int formationIndex = FillPositionStringForPlayerIndexs(playerDataIndex, formations[(int)formationType], ref positionString);
                        if (formationIndex != -1) // Turn yellow
                        {
                            color = new Color(1.0f,1.0f,0.8f,1.0f);
                        }

                        if (dynamicPlayersData[playerDataIndex].weeksBannedOrInjured != 0) // Turn Red
                        {
                            color = new Color(1.0f,0.0f,0.0f,1.0f);
                        }
                        nameString = "("+positionString+") "+staticPlayersData[playerDataIndex].playerSurname;
                    }
                    
                   
                    int stars = (int)GetTeamLeagueAdjustedStarsRatingForPlayerIndex(playerDataIndex);
                    if (stars < 0) stars = 0; if (stars > 5) stars = 5;
                    menuItemGenerator.CreatePlayerTrainings(currentScreenDefinition, new Vector2(18.0f, yOffSell), stars, nameString,color,playerLikesPositionString,dynamicPlayersData[playerDataIndex]);
                    yOffSell -= 22f;
                }
                for (int j = 0; j < maxItemsSelling; j++)
                {
                    menuItemGenerator.GenerateMenuItem(currentScreenDefinition,MenuElement.TextBarHalf, new Vector2(0,-1*(110-menuItemGenerator.playerTrainingYOffset+22*j)),0,0," "+(j+1) + ")", Enums.MenuAction.CyclePlayerTransferStatus, j);
                } 
               
                break;
           case Enums.Screen.TrainPlayers:

                currentNumberOfPage = (numPlayersInPlayersTeam / MaxPlayersInList) + 1;
                if (currentPage >= currentNumberOfPage)
                {
                    currentPage = currentNumberOfPage - 1;
                }
               //Generate List
               float yOffTrain = 0.0f;
                int maxItems = numPlayersInPlayersTeam - (currentPage * MaxPlayersInList);
                if (maxItems > MaxPlayersInList) maxItems = MaxPlayersInList;
                //Training screen menu stuff
                int buttonTrainY = -286;
                if (maxItems > 11)
                    buttonTrainY = 395 - menuItemGenerator.playerTrainingYOffset - (22 * (maxItems + 1));
                menuItems[7].pos = new Vector2(menuItems[7].pos.x, -buttonTrainY);
                menuItems[7].AdjustPosition(); menuItems[7].SetText("backButton");
                menuItems[4].pos = new Vector2(menuItems[4].pos.x, -buttonTrainY);
                menuItems[4].AdjustPosition(); menuItems[4].SetText("Page " + (currentPage+1) + "/" + currentNumberOfPage);
                menuItems[5].pos = new Vector2(menuItems[5].pos.x, -buttonTrainY);
                menuItems[5].AdjustPosition(); menuItems[5].SetText("pageButtonPrev");
                menuItems[6].pos = new Vector2(menuItems[6].pos.x, -buttonTrainY);
                menuItems[6].AdjustPosition();menuItems[6].SetText("pageButtonNext");
                for (int i = 0; i < maxItems; i++)
                {
                    int playerId = playersTeamPlayerIds[i + currentPage * MaxPlayersInList];
                    int playerDataIndex = GetPlayerDataIndexForPlayerID(playerId);
                   string nameString = String.Empty;
                   string playerLikesPositionString = String.Empty;
                   Color color = Color.white;
                    if (playerDataIndex != -1) // Handle Player name
                    {
                        string positionString = "--";
                       
                        playerLikesPositionString = FillPlayerLikesStringForPlayerIndex(playerDataIndex);
                        int formationIndex = FillPositionStringForPlayerIndexs(playerDataIndex, formations[(int)formationType], ref positionString);
                        if (formationIndex != -1) // Turn yellow
                        {
                            color = new Color(1.0f,1.0f,0.8f,1.0f);
                        }

                        if (dynamicPlayersData[playerDataIndex].weeksBannedOrInjured != 0) // Turn Red
                        {
                            color = new Color(1.0f,0.0f,0.0f,1.0f);
                        }
                        nameString = "("+positionString+") "+staticPlayersData[playerDataIndex].playerSurname;
                    }
                    
                   
                    int stars = (int)GetTeamLeagueAdjustedStarsRatingForPlayerIndex(playerDataIndex);
                    if (stars < 0) stars = 0; if (stars > 5) stars = 5;
                    menuItemGenerator.CreatePlayerTrainings(currentScreenDefinition, new Vector2(18.0f, yOffTrain), stars, nameString,color,playerLikesPositionString,dynamicPlayersData[playerDataIndex]);
                    yOffTrain -= 22f;
                }
                for (int j = 0; j < maxItems; j++)
                {
                    menuItemGenerator.GenerateMenuItem(currentScreenDefinition,MenuElement.TextBarHalf, new Vector2(0,-1*(110-menuItemGenerator.playerTrainingYOffset+22*j)),0,0," "+(j+1) + ")", Enums.MenuAction.CyclePlayerTraining, j);
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
                price = menuItems[8].GetComponent<MenuItem>();
                price.SetText("$"+matchbreakerInfo[mbId].cost+"k");
                string matchbreakerCashTxt = "Cash: " + GetTeamCashBalance(playersTeam) + "k";
                menuItems[9].SetText(matchbreakerCashTxt);
                break;
           case Enums.Screen.PreMatchReview:
                for (int home = 0; home < numTeamsInScenarioLeague; home++)
                {
                    for (int away = 0; away < numTeamsInScenarioLeague; away++)
                    {
                        if (PremiumLeagueMatchesPlayed[home, away] == week)
                        {
                            bool foundPlayersMatch = false;
                            int homeTeamIndex = teamIndexsForScenarioLeague[home];
                            int awayTeamIndex = teamIndexsForScenarioLeague[away];
                            int homeTeamId = staticTeamsData[homeTeamIndex].teamId;
                            int awayTeamId = staticTeamsData[awayTeamIndex].teamId;

                            if (playersTeam == homeTeamId)
                            {
                                foundPlayersMatch = true;
                                oppositionTeamId = awayTeamId;
                            }

                            if (playersTeam == awayTeamId)
                            {
                                foundPlayersMatch = true;
                                oppositionTeamId = homeTeamId;
                            }

                            if (foundPlayersMatch)
                            {
                                playersMatch.homeTeam = homeTeamId;
                                playersMatch.awayTeam = awayTeamId;
                                string preMatchTeams = staticTeamsData[homeTeamIndex].teamName+"\nvs\n"+staticTeamsData[awayTeamIndex].teamName;
                                menuItems[2].SetText(preMatchTeams);
                                playersMatch.homeTeamName = staticTeamsData[homeTeamIndex].teamName;
                                playersMatch.awayTeamName = staticTeamsData[awayTeamIndex].teamName;
                                
                                playersMatch.homeTeam1StColour = staticTeamsData[homeTeamIndex].homeTeam1StColour;
                                playersMatch.homeTeam2NdColour = staticTeamsData[homeTeamIndex].homeTeam2NdColour; 
                                playersMatch.awayTeam1StColour = staticTeamsData[awayTeamIndex].awayTeam1StColour;
                                playersMatch.awayTeam2NdColour = staticTeamsData[awayTeamIndex].awayTeam2NdColour;

                                int managerIndex = GetIndexToManagerForTeamID(oppositionTeamId);
                                float style = staticManagersData[managerIndex].styleOffset;
                                string styleText = "Balanced";
                                if (style is <= 4.9f and >= -4.9f)
                                {
                                    styleText = "Balanced";
                                }
                                else
                                {
                                    styleText = style switch
                                    {
                                        < -9.9f => "Very Defensive",
                                        < -4.9f => "Defensive",
                                        > 9.9f => "Strong Attacking",
                                        > 4.9f => "Attacking",
                                        _ => ""
                                    };
                                }

                                string scoutReport = "Previous matches would indicate "+staticManagersData[managerIndex].managerSurname+" prefers to use a "+styleText+" style of play.";
                                menuItems[5].SetText(scoutReport);
                            }
                        }
                    }
                }
                 // fill formation details
                if (lastOppositionTeamAssignmentId != oppositionTeamId)
                {
                    lastOppositionTeamAssignmentId = oppositionTeamId;
                    int managerIndex = GetIndexToManagerForTeamID(oppositionTeamId);
                    oppositionTeamFormationType = GetCPUFormationForManagerIndex(managerIndex);
                    numPlayersInOppositionTeam = FillTeamPlayerArray(oppositionTeamPlayerIds, oppositionTeamId);
                    // An attempt to have the CPU manager select their team
                    AutofillFormationFromPlayerIDs(playersInOppositionFormation,oppositionTeamPlayerIds,numPlayersInOppositionTeam,oppositionTeamFormationType,oppositionTeamId);
                }
               break; 
            case Enums.Screen.OppositionFormation:
                RenderScene();
                //Set shaded box text
                int oppositionTeamIndex = GetTeamDataIndexForTeamID(oppositionTeamId);
                menuItems[2].SetText(staticTeamsData[oppositionTeamIndex].teamName +"\n" + formationStrings[(int)oppositionTeamFormationType]);
                int maxOppositionItems = numPlayersInOppositionTeam;
                if (maxOppositionItems > MaxPlayersInATeam)
                    maxOppositionItems = MaxPlayersInATeam;
                float yOffOpposition = 32.0f;
                Debug.Log("Max opposition items: "+ maxOppositionItems);
                for (int i = 0; i < maxOppositionItems; i++)
                {
                    playerNameUVOffsets[i] = yOffOpposition / 1024.0f;
                    int playerIndex = GetPlayerDataIndexForPlayerID(oppositionTeamPlayerIds[i]);
                    if (playerIndex != -1)
                    {
                        string playerStr = staticPlayersData[playerIndex].playerSurname;
                        float fontSize = (float)((18 * 1.3) / 2);
                        MenuItem item =menuItemGenerator.GenerateMenuItem(currentScreenDefinition, MenuElement.StaticText,
                            new Vector2(0, -yOffOpposition), 2, 0, playerStr, 0, 0, null,
                            fontSize);
                        yOffOpposition += fontSize;
                        item.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 12);
                    }
                }
                Debug.Assert(playerNameUVOffsets[maxOppositionItems] <= yOffOpposition/1024.0f);
                // Change destination for back/next buttons
                if (matchEngine.state == MatchEngineState.MatchOver)
                {
                    menuItems[3].param = (int)Enums.Screen.PreMatchReview;
                    menuItems[4].param = (int)Enums.Screen.SelectFormation;
                }
                else // If we are in a match, go back to the match! 
                {
                    menuItems[3].param = (int)Enums.Screen.MatchEngine;
                    menuItems[4].param = (int)Enums.Screen.MatchEngine;
                    matchEngine.updateTimer = -1; // force this here to quickly update the display when we return
                }
                break;
            case Enums.Screen.SelectFormation:
                if (matchEngine.state == MatchEngineState.MatchOver)
                {
                    menuItems[8].param = (int)Enums.Screen.OppositionFormation;
                }
                else // If we are in a match, go back to the match! 
                {
                    menuItems[8].param = (int)Enums.Screen.MatchEngine;
                    matchEngine.updateTimer = -1; // force this here to quickly update the display when we return
                }
                currentScreenSubState = 0;
                break;
            case Enums.Screen.AssignPlayers:
                RenderScene();
                int showFormation = 0;
                int selectPlayer = 1;

                if (currentScreenSubState == showFormation)
                {
                    // Hide page changer unless we need it
                    menuItems[6].gameObject.SetActive(false); // Middle button
                    menuItems[7].gameObject.SetActive(false); // back button
                    menuItems[8].gameObject.SetActive(false); // next button
                    int maxItemsAssign = numPlayersInOppositionTeam;
                    // trying to allow more entries on the list (actually a maximum of 67~68)
                    if (maxItemsAssign > MaxPlayersInATeam) maxItemsAssign = MaxPlayersOnScreen;
                    if (maxItemsAssign > 16)
                    {
                        menuItems[0].GetComponent<MenuScrollBar>().minMaxRange.y = -1 * (maxItemsAssign);
                        //update Page ?/? text
                        menuItems[6].SetText("Page " + (currentPage+1) + "/" + currentNumberOfPage);
                    }
                    
                    float yOffPlayer = 0.0f;
                    // for (int i = 0; i < maxItemsAssign; i++) // player name list
                    // { 
                    //     int playerDataIndex = GetPlayerDataIndexForPlayerID(playersTeamPlayerIds[i]);
                    //     float fontSize = (float)((18 * 1.3) / 2);
                    //     MenuItem currentItem = menuItemGenerator.GenerateMenuItem(currentScreenDefinition,MenuElement.StaticText, new Vector2(0,-1*(8+yOffPlayer)),1,0,staticPlayersData[playerDataIndex].playerSurname, MenuAction.Null, 0,null, fontSize);
                    //     currentItem.GetComponent<RectTransform>().sizeDelta = new Vector2(128, 128);
                    //     currentItem.affectedByScroll = true;
                    //     yOffPlayer += 22;
                    // }   
                    // playerNameUVOffsets[maxItemsAssign] = yOffPlayer/1024.0f;
                    for (int j = 0; j < maxItemsAssign; j++)
                    {
                       // menuItemGenerator.GenerateMenuItem(currentScreenDefinition,MenuElement.TextBarHalf, new Vector2(0,-1*(110-menuItemGenerator.playerTrainingYOffset+22*j)),0,0," "+(j+1) + ")", Enums.MenuAction.AssignPlayerToFormation, j);
                    } 
                }
                break;
            case Enums.Screen.MatchEngine:
                ResetMenuRadioButtons(menuItems);
                
                SetMenuRadioButtonsAtOccurance(menuItems,(int)playersMatchStrategy);
                //  only setup a match if there isn't one already in progress (allows display of other screens - including formation changing)
                if (matchEngine.state == MatchEngineState.MatchOver)
                    matchEngine.SetupForMatch(playersMatch.homeTeam, playersMatch.awayTeam);
                for (int i = 0; i < MaxPlayersInFormation; i++)
                {
                    if (playersMatch.homeTeam == playersTeam)
                    {
                        playersMatch.formationTypeHomeTeam = formationType;
                        playersMatch.formationTypeAwayTeam = oppositionTeamFormationType;
                        playersMatch.formationHomeTeam[i] = playersInFormation[i];
                        playersMatch.formationAwayTeam[i] = playersInOppositionFormation[i];
                        
                        menuItems[9].param = (int)Enums.Screen.SelectFormation;
                        menuItems[10].param = (int)Enums.Screen.OppositionFormation;
                    }
                    else if (playersMatch.awayTeam == playersTeam)
                    {
                        playersMatch.formationTypeAwayTeam = formationType;
                        playersMatch.formationTypeHomeTeam = oppositionTeamFormationType;
                        playersMatch.formationAwayTeam[i] = playersInFormation[i];
                        playersMatch.formationHomeTeam[i] = playersInOppositionFormation[i];
                        
                        menuItems[9].param = (int)Enums.Screen.OppositionFormation;
                        menuItems[10].param = (int)Enums.Screen.SelectFormation;
                    }
                }
                ButtonItem matchbreakerButton = menuItems[8].GetComponent<ButtonItem>();
                matchbreakerButton.flags &= ~MenuElementFlag.HideItem;
                if (playersMatchBreaker != -1)
                {
                    matchbreakerButton.button.texture = matchbreakerInfo[playersMatchBreaker].texture;
                }
                else
                {
                    matchbreakerButton.button.texture = textures[(int)Enums.Texture.ButtonNoMatchbreaker];
                }
                matchbreakerButton.SetText("");
                
                ButtonItem homeButton = menuItems[9].GetComponent<ButtonItem>();
                ButtonItem awayButton = menuItems[10].GetComponent<ButtonItem>();

                homeButton.HideItem(false);
                awayButton.HideItem(false);
                homeButton.button.texture = formationMenuIconBars[(int)playersMatch.formationTypeHomeTeam].texture;
                awayButton.button.texture = formationMenuIconBars[(int)playersMatch.formationTypeAwayTeam].texture;
                homeButton.SetText("");
                awayButton.SetText("");
                
                menuItems[1].HideItem(false);// show skip
                menuItems[2].HideItem(true); // hide next
                break;
            case Enums.Screen.ProcessMatchData:
                GoToMenu(Enums.Screen.PostMatchReview);
                break;
            case Enums.Screen.PostMatchReview:
                int savePlayersMatchHomeTeamScore = matchEngine.homeTeamScore;
                int savePlayersMatchAwayTeamScore = matchEngine.awayTeamScore;
                
                int leagueIndexH = GetLeagueDataIndexForTeam(playersMatch.homeTeam);
                int leagueIndexA = GetLeagueDataIndexForTeam(playersMatch.awayTeam);
                if (matchEngine.homeTeamScore == matchEngine.awayTeamScore)
                {// a draw
                    premiumLeagueData[leagueIndexH].leaguePoints += 1;
                    premiumLeagueData[leagueIndexA].leaguePoints += 1;
                    playerRating++;
                    if (playerRating > 255)
                        playerRating = 255;
                }
                else if (matchEngine.homeTeamScore > matchEngine.awayTeamScore)
                { //home win
                    premiumLeagueData[leagueIndexH].leaguePoints += 3;
                    premiumLeagueData[leagueIndexH].goalDifference += (matchEngine.homeTeamScore - matchEngine.awayTeamScore);
                    premiumLeagueData[leagueIndexA].goalDifference -= (matchEngine.homeTeamScore - matchEngine.awayTeamScore);

                    if (playersMatch.homeTeam == playersTeam)
                    {
                        playerRating += (matchEngine.homeTeamScore - matchEngine.awayTeamScore);
                        if (playerRating > 255)
                            playerRating = 255;
                    }
                    else
                    {
                        playerRating -= (matchEngine.homeTeamScore - matchEngine.awayTeamScore);
                        if (playerRating < 0)
                            playerRating = 0;
                    }
                }
                else
                {//away win 
                    premiumLeagueData[leagueIndexA].leaguePoints += 3;
                    premiumLeagueData[leagueIndexA].goalDifference += (matchEngine.awayTeamScore - matchEngine.homeTeamScore);
                    premiumLeagueData[leagueIndexH].goalDifference -= (matchEngine.awayTeamScore - matchEngine.homeTeamScore);

                    if (playersMatch.awayTeam == playersTeam)
                    {
                        playerRating -= (matchEngine.homeTeamScore - matchEngine.awayTeamScore);
                        if (playerRating > 255)
                            playerRating = 255;
                    }
                    else
                    {
                        playerRating += (matchEngine.homeTeamScore - matchEngine.awayTeamScore);
                        if (playerRating < 0)
                            playerRating = 0;
                    }
                }
                
                premiumLeagueData[leagueIndexH].goalsFor += matchEngine.homeTeamScore;
                premiumLeagueData[leagueIndexH].goalsAgainst += matchEngine.awayTeamScore;
                premiumLeagueData[leagueIndexH].matchesPlayed++;
                
                premiumLeagueData[leagueIndexA].goalsFor += matchEngine.awayTeamScore;
                premiumLeagueData[leagueIndexA].goalsAgainst += matchEngine.homeTeamScore;
                premiumLeagueData[leagueIndexA].matchesPlayed++;
                ProcessCPUMatchesForWeek(savePlayersMatchHomeTeamScore,savePlayersMatchAwayTeamScore);
                SortLeagueTable();
                week++;
                ProcessWeeksPlayerTrainingForLeagueId(playersLeague);
                ProcessWeeklyPlayerSalariesForLeagueId(playersLeague);
                CPUTeamConsiderTransferListsForWeek();
                SaveGameData();
                transferOfferOffersMadeThisTurn = 0;
                break;

            case Enums.Screen.TransferOffers:
                bool skipTransferScreen = true;
                transferOfferInterestedPlayerId = -1;
                transferOfferInterestedTeamIndex = -1;
                transferOfferOffersMadeThisTurn = 0;

                int numOnTransferList = -1;
                int[] transfersList = new int[MaxPlayersInATeam];

                if (IsAnyPlayerInTeamOnTheTransferList(playersTeamPlayerIds, numPlayersInPlayersTeam, transfersList, ref numOnTransferList))
                {
                    for (int i = 0; i < numTeamsInScenarioLeague; i++)
                    {
                        int teamId = premiumLeagueData[i].teamId;
                        PlayerFormation form = (PlayerFormation)DoesTeamNeedToPurchasePlayers(teamId);
                        
                        int chance = Random.Range(0, 100);
                        if (chance < 10 && transferOfferOffersMadeThisTurn < 3)
                        {
                            int numPlayersInTeam = CountNumberOfPlayersInTeam(teamId);
                            if (numPlayersInTeam < 32)
                            {
                                //budget check
                                int teamIndex = GetTeamDataIndexForTeamID(teamId);
                                int numWeeks = (((numTeamsInScenarioLeague - 1) * 2)-week);
                                int estimatedBalanceAtSeasonEnd = EstimateTeamIndexCashBalance(teamIndex, numWeeks);

                                if (estimatedBalanceAtSeasonEnd > 0)
                                {
                                    for (int j = 0; j < numOnTransferList; j++)
                                    {
                                        if (CheckPlayerIdIsHappyInFormation(transferList[j], form)>0)
                                        {
                                            int playerValue = DetermineValueOfPlayerID(transferList[j], (int)playersLeague);
                                            int teamCash = GetTeamCashBalance(teamId);
                                            int offerValue = playerValue;
                                            
                                            int dataIndex = GetPlayerDataIndexForPlayerID(transferList[j]);
                                            TransferStatus transferTerms = (TransferStatus)((dynamicPlayersData[dataIndex].trainingTransfer & transferMask) >> transferBitShift);
                                            if (transferTerms == TransferStatus.FreeTransfer)
                                            {
                                                offerValue = 0;
                                            }
                                            else
                                            {
                                                if (transferTerms == TransferStatus.OffersAtValue)
                                                    offerValue = playerValue;
                                                else
                                                {
                                                    offerValue = (int)(playerValue * 0.75f);
                                                }
                                            }

                                            if (offerValue < teamCash)
                                            {
                                                transferOfferOffersMadeThisTurn++;
                                                transferOfferOfferValue = offerValue;
                                                transferOfferInterestedTeamIndex = i;
                                                transferOfferInterestedPlayerId = transferList[j];
                                                skipTransferScreen = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //Theres an offer!
                    if (transferOfferInterestedPlayerId != -1)
                    {
                        int playerIndex = GetPlayerDataIndexForPlayerID(transferOfferInterestedPlayerId);
                        string likeStr = FillPlayerLikesStringForPlayerIndex(playerIndex);
                        string currPosStr = "";
                        FillPositionStringForPlayerIndexs(playerIndex,formations[(int)formationType],ref currPosStr);

                        string transferOffer = "You have received an offer of $"+transferOfferOfferValue+"k from "+staticTeamsData[transferOfferInterestedTeamIndex].teamName+" for:\n"+staticPlayersData[playerIndex].playerSurname+"\nPlays: "+likeStr+"   Pos: "+currPosStr+"\n\n\nDo you want to accept this offer for your player?";
                        menuItems[2].SetText(transferOffer);
                        
                        
                        int stars = (int)GetTeamLeagueAdjustedStarsRatingForPlayerIndex(playerIndex);
                        PlayerTraining transferPlayer = menuItemGenerator.CreatePlayerTrainings(currentScreenDefinition, new Vector2(0,-(356-menuItemGenerator.playerBuyingYOffset+12)),stars,"",Color.white,"",dynamicPlayersData[playerIndex]);
                        transferPlayer.transform.SetSiblingIndex(5);
                    }
                }
                if (skipTransferScreen)
                    GoToMenu(Enums.Screen.EndOfTurn);
                break;
            case Enums.Screen.TransferConfirm:
                int playerIndexTransfer = GetPlayerDataIndexForPlayerID(transferOfferInterestedPlayerId);
                string transferConfirm = "You have CONFIRMED an offer of $"+transferOfferOfferValue+"k from "+staticTeamsData[transferOfferInterestedTeamIndex].teamName+" for the player "+staticPlayersData[playerIndexTransfer].playerSurname+".\n\nThe player will leave for his new team immediately";
                menuItems[2].SetText(transferConfirm);

                int formIndex = IsPlayerIdInFormation(transferOfferInterestedPlayerId, playersInFormation);
                if (formIndex != -1)
                    playersInFormation[formIndex] = -1;
                
                dynamicPlayersData[playerIndexTransfer].trainingTransfer = (int)Training.Normal;
                int interestedTeamId = staticTeamsData[transferOfferInterestedTeamIndex].teamId;
                dynamicPlayersData[playerIndexTransfer].teamId = (short)interestedTeamId;
                numPlayersInPlayersTeam = FillTeamPlayerArray(playersTeamPlayerIds, playersTeam); // rebuild our team without our sold player

                AddTeamCashBalance(playersTeam, transferOfferOfferValue);
                AddTeamCashBalance(interestedTeamId, -transferOfferOfferValue);
                
                statsTurnIncome += transferOfferOfferValue;
                SaveGameData();
                break;
            case Enums.Screen.EndOfTurn:
                playersBalance[week] = GetTeamCashBalance(playersTeam);
                for (int i = 0; i < week + 1; i++)
                {
                    endOfTurnBalanceArray[i] = playersBalance[i];
                }
                menuGraph.array = endOfTurnBalanceArray;
                menuGraph.numInArray = week + 1;
                GraphItem graph = menuItems[5].GetComponent<GraphItem>();
                graph.SetGraph();
                string endOfTurnIncomeExpend = "Prev. Balance: $"+playersBalance[Math.Abs(week - 1)]+"k\n...\nTicket Sales ("+statsAttendance+"/"+statsStadiumSeats+"): $"+statsTurnIncomeTicketSales+"k\nSponsor/TV: $"+statsTurnIncomeSponsorsTV+"k\nOther Income: $"+statsTurnIncome+"k\n...\nSalary Expenditure: $"+statsTurnExpendSalary+"k\nOther Expenditure: $"+statsTurnExpend+"k\n...\nNew Balance: $"+playersBalance[week]+"k";
                menuItems[4].SetText(endOfTurnIncomeExpend);
                int currentPos = GetPositionInLeagueTableForTeamId(playersTeam);

                string diffString;
                int diff = currentPos - statsPrevLeaguePos;
                if (diff > 0)
                    diffString = "League Pos.: " + currentPos + " (-"+Math.Abs(diff)+")";
                else if (diff < 0)
                    diffString = "League Pos.: " + currentPos + " (+"+Math.Abs(diff)+")";
                else
                    diffString = "League Pos.: " + currentPos + " ("+diff+")";
                menuItems[2].SetText(diffString);
                break;
            case Enums.Screen.LeagueFinished:
                int currentPosition = GetPositionInLeagueTableForTeamId(playersTeam);
                string scenarioOutcome;
                if (currentPosition == 1)
                    scenarioOutcome = "Congratulations!\nYou have won the league!";
                else if (currentPosition <= 3)
                {
                    if (scenarioData[playersScenario].leagueIDPromotion != playersLeague)
                        scenarioOutcome = "Congratulations!\nYou have won promotion from the league.";
                    else 
                        scenarioOutcome = "Congratulations!\nYou finished in the top three of the league.";
                }
                else if (currentPosition > numTeamsInScenarioLeague - 3)
                {
                    if (scenarioData[playersScenario].leagueIDRelegation != playersLeague)
                        scenarioOutcome = "Bad Luck!\nYou have been relegated from the league.";
                    else 
                        scenarioOutcome = "Bad Luck!\nIt's a good job you can't be relegated from the league.";
                }
                else
                {
                    if (scenarioData[playersScenario].startMoneyAdjust < 0)
                        scenarioOutcome =
                            "You have failed to win the league.\nGiven your difficult start well done for avoiding the relegation zone.";
                    else
                        scenarioOutcome = "You have failed to win the league.\nAt least you avoided the relegation zone";
                }
                menuItems[2].SetText(scenarioOutcome);


                string endOfSeason;
                if (playersYearsToRetire <= 0)
                {
                    endOfSeason = "You have reached retirement age! Maybe you could be reborn via a different scenario?";
                    menuItems[6].menuAction = MenuAction.GotoMenu;
                    menuItems[6].param = (int)Enums.Screen.ChooseLeague;
                }
                else
                {
                    endOfSeason = "You have "+playersYearsToRetire+" years until you retire. Would you like to manage your team for another season?";
                    menuItems[6].menuAction = MenuAction.ProcessResetLeagueForNextYear;
                }
                menuItems[4].SetText(endOfSeason);
                break;
            case Enums.Screen.ProcessLeagueFinish:
                
                float wk = week;
                int percent = (int)((wk / ((numTeamsInScenarioLeague - 1) * 2)) * 100f);
                menuItems[1].SetText("PROCESSING END OF SEASON DATA\nPlease Wait... "+percent+"%");
                
                break;
        }
    }

    /// <summary>
    /// Extracted logic for buying a player.
    /// </summary>
    /// <param name="playerIndex">The purchased player</param>
    /// <param name="teamId">the previous team that sold the player</param>
    private void BuyOfferedPlayer(int playerIndex, int teamId)
    {
        dynamicPlayersData[playerIndex].trainingTransfer = (int)Training.Normal;
        dynamicPlayersData[playerIndex].teamId = (short)playersTeam;
        numPlayersInPlayersTeam = FillTeamPlayerArray(playersTeamPlayerIds, playersTeam);

        AddTeamCashBalance(playersTeam, -currentBuyPlayerOffer);
        if (teamId != -1) // the players old team profits
            AddTeamCashBalance(teamId, currentBuyPlayerOffer);

        if (teamId == oppositionTeamId)
        {
            // is player id already in a formation? if so REMOVE it!
            int formIndex = IsPlayerIdInFormation(currentBuyPlayerId, playersInOppositionFormation);
            if (formIndex != -1)
            {
                playersInOppositionFormation[formIndex] = -1;
                numPlayersInOppositionTeam = FillTeamPlayerArray(oppositionTeamPlayerIds, teamId);
                AutofillFormationFromPlayerIDs(playersInOppositionFormation,oppositionTeamPlayerIds,numPlayersInOppositionTeam,oppositionTeamFormationType,teamId);
            }
        }

        SaveGameData();
    }

    void CPUTeamConsiderTransferListsForWeek()
    {
        for (int teamIndex = 0; teamIndex < numberOfTeamsInArrays; teamIndex++)
        {
            int transferMood = 0;
            int teamId = staticTeamsData[teamIndex].teamId;
            int numWeeks = ((numTeamsInScenarioLeague - 1) * 2) - week;
            int estimatedBalanceAtSeasonEnd = EstimateTeamIndexCashBalance(teamIndex, numWeeks);

            if (estimatedBalanceAtSeasonEnd < 0 & numWeeks != 0)
            {
                int weeklyChange = (dynamicTeamsData[teamIndex].cashBalance - estimatedBalanceAtSeasonEnd / numWeeks);
                int numWeeksUntilBust = dynamicTeamsData[teamIndex].cashBalance / weeklyChange;
                if (numWeeksUntilBust < 8)
                    transferMood = -2; //sell star players
                else
                    transferMood = -1; //sell unneeded players
            }
            else if (estimatedBalanceAtSeasonEnd > dynamicTeamsData[teamIndex].cashBalance)
            {
                transferMood = 1; // might sell un-needed players maybe
            }
            else
            {
                if (estimatedBalanceAtSeasonEnd > (dynamicTeamsData[teamIndex].cashBalance/2))
                    transferMood = -1;
            }
            
            int[] teamPlayerIds = new int[MaxPlayersInATeam];
            int numPlayers = FillTeamPlayerArray(teamPlayerIds, teamId);
            for (int i = 0; i < numPlayers; i++)
            {
                int playerIndex = GetPlayerDataIndexForPlayerID(teamPlayerIds[i]);
                if ((dynamicPlayersData[playerIndex].trainingTransfer & transferMask) != 0)
                {
                    int chance = Random.Range(1, 100);
                    if (chance < 75)
                    {
                        if (transferMood > 0)
                            dynamicPlayersData[playerIndex].trainingTransfer = (dynamicPlayersData[playerIndex].trainingTransfer&trainingMask) | ((int)(TransferStatus.NotListed)<<transferBitShift);
                    }
                }
                else
                {
                    int chance = Random.Range(1, 100);
                    if (chance < 50)
                    {
                        if (transferMood < -1)
                            dynamicPlayersData[playerIndex].trainingTransfer = (dynamicPlayersData[playerIndex].trainingTransfer&trainingMask) | ((int)(TransferStatus.AnyOffers)<<transferBitShift);
                        else if (transferMood < 0)
                        {
                            int chance2 = Random.Range(1, 100);
                            if (chance2 < 50)
                            {
                                float playerStarRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(playerIndex);
                                if (playerStarRating <= 1.0f)
                                    dynamicPlayersData[playerIndex].trainingTransfer = (dynamicPlayersData[playerIndex].trainingTransfer&trainingMask) | ((int)(TransferStatus.OffersAtValue)<<transferBitShift);
                                else if (chance2 < 10)
                                {
                                    if (playerStarRating <= 2.0f)
                                        dynamicPlayersData[playerIndex].trainingTransfer = (dynamicPlayersData[playerIndex].trainingTransfer&trainingMask) | ((int)(TransferStatus.OffersAtValue)<<transferBitShift);
                                }
                            }
                        }
                        else
                        {
                            float playerStarRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(playerIndex);
                            if (playerStarRating == 0.0f)
                                dynamicPlayersData[playerIndex].trainingTransfer = (dynamicPlayersData[playerIndex].trainingTransfer&trainingMask) | ((int)(TransferStatus.FreeTransfer)<<transferBitShift);
                            else if (playerStarRating <= 0.1f)
                                dynamicPlayersData[playerIndex].trainingTransfer = (dynamicPlayersData[playerIndex].trainingTransfer&trainingMask) | ((int)(TransferStatus.OffersAtValue)<<transferBitShift);
                            
                            if (dynamicPlayersData[playerIndex].morale < -32)
                                dynamicPlayersData[playerIndex].trainingTransfer = (dynamicPlayersData[playerIndex].trainingTransfer&trainingMask) | ((int)(TransferStatus.OffersAtValue)<<transferBitShift);
                        }
                    }
                }
            }
        }
    }
    void ProcessWeeklyPlayerSalariesForLeagueId(LeagueID leagueID)
    {
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            if (dynamicPlayersData[i].teamId != -1)
            {
                if (GetTeamsLeagueID(dynamicPlayersData[i].teamId) == (int)leagueID)
                {
                    int teamId = dynamicPlayersData[i].teamId;
                    if (teamId != -1)
                    {
                        AddTeamCashBalance(teamId, -dynamicPlayersData[i].weeklySalary);
                        if (teamId == playersTeam)
                            statsTurnExpendSalary += dynamicPlayersData[i].weeklySalary;
                    }
                }
            }
        }
    }
    int CountPlayersOnTransferMarketExcludingTeam(int teamId)
    {
        int numPlayersFound = 0;
        numPlayersOnTransferList = 0;

        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            if (dynamicPlayersData[i].teamId != teamId)
            {
                if (dynamicPlayersData[i].teamId == -1)
                    dynamicPlayersData[i].trainingTransfer = ((int)TransferStatus.FreeTransfer)<<transferBitShift;

                if ((dynamicPlayersData[i].trainingTransfer & transferMask) != 0)
                {
                    int chance = Random.Range(1, 4);
                    if ((chance & 1) == 0)
                    {
                        if (numPlayersOnTransferList < MaxPlayers)
                        {
                            transferList[numPlayersOnTransferList] = staticPlayersData[i].playerId;
                            numPlayersOnTransferList++;
                        }
                        numPlayersFound++;
                    }
                }
            }
        }
        return numPlayersFound;
    }


    private int EstimateTeamIndexCashBalance(int teamIndex, int weeks)
    {
        int totalPeriodIncome = 0;
        int totalPeriodExpend = 0;
        int teamId = staticTeamsData[teamIndex].teamId;
        int leagueDataIndex = GetIndexToLeagueData((int)dynamicTeamsData[teamIndex].leagueID);
        
        //Calculate expenditure from salaries
        int[] teamPlayerIds = new int[MaxPlayersInATeam];
        int numPlayers = FillTeamPlayerArray(teamPlayerIds, teamId);
        int weeklySalaryCost = 0;
        for (int i = 0; i < numPlayers; i++)
        {
            int playerIndex = GetPlayerDataIndexForPlayerID(teamPlayerIds[i]);
            weeklySalaryCost += dynamicPlayersData[playerIndex].weeklySalary;
        }
        totalPeriodExpend += weeklySalaryCost * weeks;
        
        //Income 
        int maxAttendance = staticTeamsData[teamIndex].stadiumSeats;
        totalPeriodIncome += (int)(maxAttendance * 0.5f * staticLeaguesData[leagueDataIndex].ticketValue * weeks);
        totalPeriodIncome += (staticLeaguesData[leagueDataIndex].tvIncomePerTurn*weeks);
        totalPeriodIncome += ((staticLeaguesData[leagueDataIndex].maxSponsorIncomePerTurn -
                                     staticLeaguesData[leagueDataIndex].minSponsorIncomePerTurn)/2+staticLeaguesData[leagueDataIndex].minSponsorIncomePerTurn)*weeks;

        int balance = dynamicTeamsData[teamIndex].cashBalance;
        balance += totalPeriodIncome;
        balance -= totalPeriodExpend;
        return balance;
    }

    private int CountNumberOfPlayersInTeam(int teamId)
    {
        int numPlayersFound = 0;
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            if (dynamicPlayersData[i].teamId == teamId)
                numPlayersFound++;
        }
        return numPlayersFound;
    }

    /// <summary>
/// Checks if there is space on a team, and lists the needed formation types
/// </summary>
/// <param name="teamId"></param>
/// <returns>the neededPositions as an int flag</returns>
    private int DoesTeamNeedToPurchasePlayers(int teamId)
    {
        int neededPositions = 0;
        numPlayersInOppositionTeam = FillTeamPlayerArray(oppositionTeamPlayerIds, teamId);
        if (numPlayersInOppositionTeam < MaxPlayersInATeam)
        {
            int numCanPlayGoal = CountPlayersWhoCanPlayInPosition(PlayerFormation.Goalkeeper,oppositionTeamPlayerIds,numPlayersInOppositionTeam);
            if (numCanPlayGoal < 2)
                neededPositions |= (int)Enums.PlayerFormation.Goalkeeper;
            int numCanPlayDefense = CountPlayersWhoCanPlayInPosition(PlayerFormation.Defender,oppositionTeamPlayerIds,numPlayersInOppositionTeam);
            if (numCanPlayDefense < 6)
                neededPositions |= (int)Enums.PlayerFormation.Defender;
            int numCanPlayMidfield = CountPlayersWhoCanPlayInPosition(PlayerFormation.MidFielder,oppositionTeamPlayerIds,numPlayersInOppositionTeam);
            if (numCanPlayMidfield < 6)
                neededPositions |= (int)Enums.PlayerFormation.MidFielder;
            int numCanPlayAttacker = CountPlayersWhoCanPlayInPosition(PlayerFormation.Attacker,oppositionTeamPlayerIds,numPlayersInOppositionTeam);
            if (numCanPlayAttacker < 6)
                neededPositions |= (int)Enums.PlayerFormation.Attacker;
            
        }
        return neededPositions;
    }

    private int CountPlayersWhoCanPlayInPosition(PlayerFormation posType, int[] playerIds, int numPlayersId)
    {
        int result = 0;
        for (int k = 0; k < numPlayersId; k++)
        {
            int playerId = playerIds[k];
            int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
            if ((staticPlayersData[dataIndex].playerPositionFlags & posType) != (PlayerFormation)0)
                result++;
        }
        return result;
    }

    private bool IsAnyPlayerInTeamOnTheTransferList(int[] playerIds, int numPlayerIds, int[] transfers, ref int numTrans)
    {
        Debug.Log(playerIds.Length + " " + numPlayerIds + " " + transfers.Length + " " + numTrans);
        bool result = false;
        numTrans = 0;
        for (int k = 0; k < numPlayerIds; k++)
        {
            int playerId = playerIds[k];
            int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
            if (dataIndex != -1)
            {
                if ((dynamicPlayersData[dataIndex].trainingTransfer & transferMask) >> transferBitShift != 0)
                {
                    result = true;
                    transfers[numTrans] = playerId;
                    numTrans++;
                }
            }
        }
        return result;
    }

    private void ProcessCPUMatchesForWeek(int savePlayersMatchHomeTeamScore, int savePlayersMatchAwayTeamScore,bool draw=true)
    {
        float yOff = 72.0f;
        for (int home = 0; home < numTeamsInScenarioLeague; home++)
        {
            for (int away = 0; away < numTeamsInScenarioLeague; away++)
            {
                if (PremiumLeagueMatchesPlayed[home, away] == week)
                {
                    int homeScore = 0;
                    int homeTeamId = staticTeamsData[teamIndexsForScenarioLeague[home]].teamId;
                    int leagueIndexH = GetLeagueDataIndexForTeam(homeTeamId);
                    
                    int awayScore = 0;
                    int awayTeamId = staticTeamsData[teamIndexsForScenarioLeague[away]].teamId;
                    int leagueIndexA = GetLeagueDataIndexForTeam(awayTeamId);

                    if ((playersMatch.homeTeam == homeTeamId) && (playersMatch.awayTeam == awayTeamId))
                    {
                        homeScore = savePlayersMatchHomeTeamScore;
                        awayScore = savePlayersMatchAwayTeamScore;
                    }
                    else // simulated match for cpu v cpu
                    {
                        matchEngine.SetupForMatch(homeTeamId, awayTeamId);
                        int[] teamPlayerIds = new int[MaxPlayersInATeam];
                        
                        int managerHomeIndex = GetIndexToManagerForTeamID(homeTeamId);
                        Debug.Assert(managerHomeIndex != -1); // ensure its not unemployed
                        playersMatch.formationTypeHomeTeam = GetCPUFormationForManagerIndex(managerHomeIndex);
                        int numPlayersInTeam = FillTeamPlayerArray(teamPlayerIds, homeTeamId);
                        AutofillFormationFromPlayerIDs(playersMatch.formationHomeTeam, teamPlayerIds,numPlayersInTeam,playersMatch.formationTypeHomeTeam,homeTeamId);

                        int managerAwayIndex = GetIndexToManagerForTeamID(homeTeamId);
                        Debug.Assert(managerAwayIndex != -1); // ensure its not unemployed
                        playersMatch.formationTypeAwayTeam = GetCPUFormationForManagerIndex(managerAwayIndex);
                        numPlayersInTeam = FillTeamPlayerArray(teamPlayerIds, awayTeamId);
                        AutofillFormationFromPlayerIDs(playersMatch.formationAwayTeam, teamPlayerIds,numPlayersInTeam,playersMatch.formationTypeAwayTeam,awayTeamId);

                        while (matchEngine.state != MatchEngineState.MatchOver)
                        {
                            matchEngine.Render(matchEngine.skip,true);
                        }

                        if (matchEngine.homeTeamScore == matchEngine.awayTeamScore)
                        {
                            premiumLeagueData[leagueIndexH].leaguePoints += 1;
                            premiumLeagueData[leagueIndexA].leaguePoints += 1;
                        }
                        else if (matchEngine.homeTeamScore > matchEngine.awayTeamScore)
                        {
                            premiumLeagueData[leagueIndexH].leaguePoints += 3;
                            premiumLeagueData[leagueIndexH].goalDifference += (matchEngine.homeTeamScore - matchEngine.awayTeamScore);
                            premiumLeagueData[leagueIndexA].goalDifference -= (matchEngine.homeTeamScore - matchEngine.awayTeamScore);
                        }
                        else
                        {//away win 
                            premiumLeagueData[leagueIndexA].leaguePoints += 3;
                            premiumLeagueData[leagueIndexA].goalDifference += (matchEngine.awayTeamScore - matchEngine.homeTeamScore);
                            premiumLeagueData[leagueIndexH].goalDifference -= (matchEngine.awayTeamScore - matchEngine.homeTeamScore);
                        }
                        premiumLeagueData[leagueIndexH].goalsFor += matchEngine.homeTeamScore;
                        premiumLeagueData[leagueIndexH].goalsAgainst += matchEngine.awayTeamScore;
                    
                        premiumLeagueData[leagueIndexA].goalsFor += matchEngine.awayTeamScore;
                        premiumLeagueData[leagueIndexA].goalsAgainst += matchEngine.homeTeamScore;
                        if (leagueIndexH != -1)
                            premiumLeagueData[leagueIndexH].matchesPlayed++;
                        if (leagueIndexA != -1)
                            premiumLeagueData[leagueIndexA].matchesPlayed++;
                        homeScore = matchEngine.homeTeamScore;
                        awayScore = matchEngine.awayTeamScore;
                    }

                    UpdateConditionOfPlayersInTeam(homeTeamId, ConditionAdjustRecoverOverWeek);
                    UpdateBansInjuryAndFlagsOfPlayersInTeam(homeTeamId);
                    UpdateConditionOfPlayersInTeam(awayTeamId, ConditionAdjustRecoverOverWeek);
                    UpdateBansInjuryAndFlagsOfPlayersInTeam(awayTeamId);
                    int homeTeamIndex = GetTeamDataIndexForTeamID(homeTeamId);
                    int awayTeamIndex = GetTeamDataIndexForTeamID(awayTeamId);
                    
                    
                    // Render
                    if (draw)
                    {
                        MenuItem item = menuItemGenerator.GenerateMenuItem(currentScreenDefinition, MenuElement.StaticText,
                            new Vector2(32, -yOff),0,0,staticTeamsData[homeTeamIndex].teamName + " " + homeScore + " " + staticTeamsData[awayTeamIndex].teamName + " " + awayScore,MenuAction.Null,0,null,18*menuItemGenerator.standingsTextFontScale);
                        yOff += 16;
                        item.transform.SetSiblingIndex(3); // ensure it overlays the box
                        Color textColor = new Color(0.7f,0.8f,0.9f);
                        if (homeTeamId == playersTeam)
                            textColor = new Color(1.0f,1.0f,0.8f);
                        if (awayTeamId == playersTeam)
                            textColor = new Color(1.0f,1.0f,0.8f);
                        item.mText.color = (textColor);
                    }
                    
                    // Pay out goal score bonuses
                    int perGoalBonusCost = DefaultGoalScoreBonus;
                    AddTeamCashBalance(homeTeamId, (perGoalBonusCost*homeScore));
                    AddTeamCashBalance(awayTeamId, (perGoalBonusCost*awayScore));
                    if (homeTeamId == playersTeam)
                        statsTurnExpendSalary += (perGoalBonusCost * homeScore);
                    if (awayTeamId == playersTeam)
                        statsTurnExpendSalary += (perGoalBonusCost * awayScore);
                    
                    int leagueDataIndex = GetIndexToLeagueData((int)dynamicTeamsData[homeTeamIndex].leagueID);
                    int maxAttendance = staticTeamsData[homeTeamIndex].stadiumSeats;
                    float homeFanMorale = dynamicTeamsData[homeTeamIndex].fanMorale;
                    float awayFanMorale = dynamicTeamsData[awayTeamIndex].fanMorale;
                    float homeAttend = (8.0f + (homeFanMorale * 2.0f)) / 10.0f; // using a 10 range here so attenance should be between 60-100 per side
                    float awayAttend = (8.0f + (awayFanMorale * 2.0f)) / 10.0f; // using a 10 range here so attenance should be between 60-100 per side
                    int actualAttendance = (int)((maxAttendance) * ((homeAttend + awayAttend) / 2.0f));

                    if (((matchEngine.homeTeamMatchBreakerFlags & MatchBreakerFlags.MaximumAttendence) !=
                         (MatchBreakerFlags)0) ||
                        ((matchEngine.awayTeamMatchBreakerFlags & MatchBreakerFlags.MaximumAttendence) !=
                         (MatchBreakerFlags)0))
                        actualAttendance = maxAttendance;

                    int incomeFromMatch = (int)(actualAttendance * staticLeaguesData[leagueDataIndex].ticketValue);
                    AddTeamCashBalance(homeTeamId, (int)(incomeFromMatch * 0.6f));
                    AddTeamCashBalance(awayTeamId, (int)(incomeFromMatch * 0.4f));

                    if (homeTeamId == playersTeam)
                    {
                        statsTurnIncomeTicketSales += (int)(incomeFromMatch * 0.6f);
                        statsAttendance = actualAttendance;
                        statsStadiumSeats = maxAttendance;
                    }
                    if (awayTeamId == playersTeam)
                    {
                        statsTurnIncomeTicketSales += (int)(incomeFromMatch * 0.4f);
                        statsAttendance = actualAttendance;
                        statsStadiumSeats = maxAttendance;
                    }

                    int homeTeamTvSponsorIncome = staticLeaguesData[leagueDataIndex].tvIncomePerTurn;
                    int awayTeamTvSponsorIncome = staticLeaguesData[leagueDataIndex].tvIncomePerTurn;

                    homeTeamTvSponsorIncome += (staticLeaguesData[leagueDataIndex].maxSponsorIncomePerTurn - staticLeaguesData[leagueDataIndex].maxSponsorIncomePerTurn/2)+staticLeaguesData[leagueDataIndex].maxSponsorIncomePerTurn;
                    awayTeamTvSponsorIncome += (staticLeaguesData[leagueDataIndex].maxSponsorIncomePerTurn - staticLeaguesData[leagueDataIndex].maxSponsorIncomePerTurn/2)+staticLeaguesData[leagueDataIndex].maxSponsorIncomePerTurn;
                    
                    if ((matchEngine.homeTeamMatchBreakerFlags & MatchBreakerFlags.SponsorBonus) !=
                         (MatchBreakerFlags)0)
                        homeTeamTvSponsorIncome *= 3;
                    if ((matchEngine.awayTeamMatchBreakerFlags & MatchBreakerFlags.SponsorBonus) !=
                         (MatchBreakerFlags)0)
                        awayTeamTvSponsorIncome *= 3;
                    
                    AddTeamCashBalance(homeTeamId, homeTeamTvSponsorIncome);
                    AddTeamCashBalance(awayTeamId, awayTeamTvSponsorIncome);

                    if (homeTeamId == playersTeam)
                        statsTurnIncomeSponsorsTV += homeTeamTvSponsorIncome;
                    if (awayTeamId == playersTeam)
                        statsTurnIncomeSponsorsTV += awayTeamTvSponsorIncome;

                    UpdateTeamIndexFanMorale(homeTeamIndex,(homeScore*0.05f)- (awayScore*0.05f));
                    UpdateTeamIndexFanMorale(awayTeamIndex,(awayScore*0.05f)- (homeScore*0.05f));
                }
                
            }
        }
    }

    private float UpdateTeamIndexFanMorale(int teamIndex, float morale)
    {
        float result = dynamicTeamsData[teamIndex].fanMorale;
        result += morale;
        if (result > 1.0f)
            result = 1.0f;
        if (result < -1.0f)
            result = -1.0f;
        
        dynamicTeamsData[teamIndex].fanMorale = result;
        return result;
    }


    private void SortLeagueTable()
    {
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            for (int team = 0; team < numTeamsInScenarioLeague - 1; team++)
            {
                if (premiumLeagueData[team].leaguePoints < premiumLeagueData[team+1].leaguePoints)
                    SwapLeagueTableEntry(team,team+1);
                else if (premiumLeagueData[team].leaguePoints == premiumLeagueData[team + 1].leaguePoints)
                {
                    if (premiumLeagueData[team].goalDifference < premiumLeagueData[team + 1].goalDifference)
                        SwapLeagueTableEntry(team, team + 1);
                }
            }
        }
    }

    private int GetLeagueDataIndexForTeam(int teamId)
    {
        int leagueIndex = -1;
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
        {
            if (premiumLeagueData[i].teamId == teamId)
            {
                leagueIndex = i;
                break;
            }
        }
        return leagueIndex;
    }

    private void ProcessLeagueEndData()
    {
        Debug.Log("Processing end data");
        if ((week != -1) && week < ((numTeamsInScenarioLeague-1)*2))
        {
            ProcessCPUMatchesForWeek(0,0, false);
            SortLeagueTable();
            week++;
            ProcessWeeksPlayerTrainingForLeagueId(playersLeague);
            ProcessWeeklyPlayerSalariesForLeagueId(playersLeague);
            CPUTeamConsiderTransferListsForWeek();
        }
        else // done running cpu matches
        {
            if (week != -1) // not processing a league
            {
                int[] otherLeagueFinalStandings = new int[MaxTeamsInLeague];
                for (int i = 0; i < numTeamsInScenarioLeague; i++)
                    otherLeagueFinalStandings[i] = premiumLeagueData[i].teamId;
                //promotions
                switch (leagueEndSaveUsersLeague)
                {
                    case LeagueID.Premium:
                        //relegation
                        int rel1 = leagueEndUsersFinalStandings[17];
                        int rel2 = leagueEndUsersFinalStandings[18];
                        int rel3 = leagueEndUsersFinalStandings[19];

                        int tmIndex = GetTeamDataIndexForTeamID(rel1);
                        dynamicTeamsData[tmIndex].leagueID = LeagueID.Chumpionship;
                        tmIndex = GetTeamDataIndexForTeamID(rel2);
                        dynamicTeamsData[tmIndex].leagueID = LeagueID.Chumpionship;
                        tmIndex = GetTeamDataIndexForTeamID(rel3);
                        dynamicTeamsData[tmIndex].leagueID = LeagueID.Chumpionship;
                        //promotion
                        int promo1 = otherLeagueFinalStandings[0];
                        int promo2 = otherLeagueFinalStandings[1];
                        int promo3 = otherLeagueFinalStandings[2];
                        
                        tmIndex = GetTeamDataIndexForTeamID(promo1);
                        dynamicTeamsData[tmIndex].leagueID = LeagueID.Premium;
                        tmIndex = GetTeamDataIndexForTeamID(promo2);
                        dynamicTeamsData[tmIndex].leagueID = LeagueID.Premium;
                        tmIndex = GetTeamDataIndexForTeamID(promo3);
                        dynamicTeamsData[tmIndex].leagueID = LeagueID.Premium;
                        break;
                    case LeagueID.Chumpionship:
                        //relegation
                        int relg1 = otherLeagueFinalStandings[17];
                        int relg2 = otherLeagueFinalStandings[18];
                        int relg3 = otherLeagueFinalStandings[19];

                        
                        int chumpTmIndex = GetTeamDataIndexForTeamID(relg1);
                        dynamicTeamsData[chumpTmIndex].leagueID = LeagueID.Chumpionship;
                        chumpTmIndex = GetTeamDataIndexForTeamID(relg2);
                        dynamicTeamsData[chumpTmIndex].leagueID = LeagueID.Chumpionship;
                        chumpTmIndex = GetTeamDataIndexForTeamID(relg3);
                        dynamicTeamsData[chumpTmIndex].leagueID = LeagueID.Chumpionship;
                        //promotion
                        int chumpPromo1 = leagueEndUsersFinalStandings[0];
                        int chumpPromo2 = leagueEndUsersFinalStandings[1];
                        int chumpPromo3 = leagueEndUsersFinalStandings[2];
                        
                        chumpTmIndex = GetTeamDataIndexForTeamID(chumpPromo1);
                        dynamicTeamsData[chumpTmIndex].leagueID = LeagueID.Premium;
                        chumpTmIndex = GetTeamDataIndexForTeamID(chumpPromo2);
                        dynamicTeamsData[chumpTmIndex].leagueID = LeagueID.Premium;
                        chumpTmIndex = GetTeamDataIndexForTeamID(chumpPromo3);
                        dynamicTeamsData[chumpTmIndex].leagueID = LeagueID.Premium;
                        break;
                }
            }

            int teamIndex = GetTeamDataIndexForTeamID(playersTeam);
            playersLeague = dynamicTeamsData[teamIndex].leagueID;
            numTeamsInScenarioLeague = CountTeamsInLeague(playersLeague);
            FillTeamsInLeagueArray(teamIndexsForScenarioLeague,playersLeague);
            week = 0;
            Debug.Log("Resetting league points");
            ResetLeaguePoints();
            SaveGameData();
            LoadAndPrepareGame();
        }
    }
    void ProcessWeeksPlayerTrainingForLeagueId(LeagueID leagueID)
    {
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            if (dynamicPlayersData[i].teamId != -1)
            {
                if (GetTeamsLeagueID(dynamicPlayersData[i].teamId) == (int)leagueID)
                {
                    switch ((Training)(dynamicPlayersData[i].trainingTransfer & trainingMask))
                    {
                        case Training.None:
                            dynamicPlayersData[i].starsRating += Random.value * -0.05f;
                            dynamicPlayersData[i].condition += Random.value *  0.2f;
                            UpdatePlayerIndexMoraleByAmount(i,-1);
                            break;
                        case Training.Light:
                            dynamicPlayersData[i].starsRating += Random.value * -0.002f;
                            dynamicPlayersData[i].condition += Random.value *  0.02f;
                            break;
                        case Training.Normal:
                            break;
                        case Training.Intensive:
                            dynamicPlayersData[i].starsRating += Random.value * 0.05f;
                            dynamicPlayersData[i].condition += Random.value *  -0.2f;
                            break;
                    }
                    dynamicPlayersData[i].starsRating = Math.Clamp(dynamicPlayersData[i].starsRating, 0.0f, 14.9f);
                    dynamicPlayersData[i].condition = Math.Clamp(dynamicPlayersData[i].condition, 0.0f, 1.0f);
                }
            }
        }
    }

    /// <summary>
    /// scan a menu looking for radio buttons and reset all of them?
    /// </summary>
    /// <param name="menuItems">the list of menu items for the screen.</param>
    /// <param name="occ">the nth occurance</param>
    public void SetMenuRadioButtonsAtOccurance(MenuItem[] menuItems, int occ)
    {
        int j = 0;
        
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i].type == MenuElement.RadioButton)
            {
                if (menuItems[i].flags != MenuElementFlag.HideItem)
                {
                    if (j == occ)
                    {
                        bool hasButton = menuItems[i].gameObject.TryGetComponent(out ButtonItem button);
                        Debug.Log(button.button);
                        button.button.flags = 1;
                        button.SetText("");
                        Debug.Log("Updated "+menuItems[i]+" with tag " + j);
                        
                    }
                    j++;
                }
            }
        }
    }

    public void ResetMenuRadioButtons(MenuItem[] menuItems)
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i].type == MenuElement.RadioButton)
            {
                if (menuItems[i].flags != MenuElementFlag.HideItem)
                {
                
                    ButtonItem button = menuItems[i].gameObject.GetComponent<ButtonItem>();
                    button.button.flags = 0;
                    button.SetText("");
                }
            }
        }
    }
    public void SetOptionsRadioButtons(MenuItem[] items)
    {
        ResetMenuRadioButtons(items);
        SetMenuRadioButtonsAtOccurance(items, SFXEnabled ? 1 : 0);
        SetMenuRadioButtonsAtOccurance(items, VibrationEnabled ? 3 : 2);
    }

    private Formation GetCPUFormationForManagerIndex(int managerIndex)
    {
        float TEST_BALANCE_RANGE = 20.0f;
        float balance = Random.value * TEST_BALANCE_RANGE;
        balance -= (TEST_BALANCE_RANGE / 2);
        balance += 50.0f; // centre the balance 0-100% range
        balance += staticManagersData[managerIndex].styleOffset;
        balance /= 100.0f;
        int index = (int)((int)Formation.KFormationMax * balance);
        Formation result = cpuFormationBalance[index];
        return result;
    }

    private string FillPlayerLikesStringForPlayerIndex(int playerIndex)
    {
        char[] playerLikesPositionString = new char[24];
        int playerLikesStringOffset = 0;
        if ((staticPlayersData[playerIndex].playerPositionFlags & PlayerFormation.Goalkeeper) != 0)
        {
            playerLikesPositionString[0] = 'G';
            playerLikesPositionString[1] = 'K';
            playerLikesStringOffset = 2;
        }
        if ((staticPlayersData[playerIndex].playerPositionFlags & PlayerFormation.Defender) != 0)
        {
            playerLikesPositionString[playerLikesStringOffset++] = 'D';
        }
        if ((staticPlayersData[playerIndex].playerPositionFlags & PlayerFormation.MidFielder) != 0)
        {
            playerLikesPositionString[playerLikesStringOffset++] = 'M';
        }
        if ((staticPlayersData[playerIndex].playerPositionFlags & PlayerFormation.Attacker) != 0)
        {
            playerLikesPositionString[playerLikesStringOffset++] = 'F';
        }
        if ((staticPlayersData[playerIndex].playerPositionFlags & PlayerFormation.Left) != 0)
        {
            playerLikesPositionString[playerLikesStringOffset++] = 'L';
        }
        if ((staticPlayersData[playerIndex].playerPositionFlags & PlayerFormation.Center) != 0)
        {
            playerLikesPositionString[playerLikesStringOffset++] = 'C';
        }
        if ((staticPlayersData[playerIndex].playerPositionFlags & PlayerFormation.Right) != 0)
        {
            playerLikesPositionString[playerLikesStringOffset++] = 'R';
        }
        Debug.Log(playerLikesPositionString);
        return new string(playerLikesPositionString);
    }

    private int FillPositionStringForPlayerIndexs(int playerIndex, FormationData playersFormation, ref string positionString)
    {
        int formationIndex = -1;
        for (int inFormation = 0; inFormation < MaxPlayersInSquad; inFormation++)
        {
            if (playerIndex == playersInFormation[inFormation])
            {
                formationIndex = inFormation;
                break;
            }
        }
        if (formationIndex != -1) // has a formation
        {
            FormationInfo newPlayersFormation = playersFormation.formations[formationIndex];
     
            if ((newPlayersFormation.formation & PlayerFormation.Substitute) != 0)
            {
                positionString = "SUB";
            }
            if ((newPlayersFormation.formation & PlayerFormation.Goalkeeper) != 0)
            {
                positionString = "GK";
            }
            if ((newPlayersFormation.formation & PlayerFormation.Defender) != 0)
            {
                positionString = "D";
            }
            if ((newPlayersFormation.formation & PlayerFormation.MidFielder) != 0)
            {
                positionString = "M";
            }
            if ((newPlayersFormation.formation & PlayerFormation.Attacker) != 0)
            {
                positionString = "F";
            }
            if ((newPlayersFormation.formation & PlayerFormation.Left) != 0)
            {
                positionString += "L";
            }
            if ((newPlayersFormation.formation & PlayerFormation.Center) != 0)
            {
                positionString += "C";
            }
            if ((newPlayersFormation.formation & PlayerFormation.Right) != 0)
            {
                positionString += "R";
            }
            
               
        }

        if ((dynamicPlayersData[playerIndex].weeksBannedOrInjured & bannedMask) != 0)
        {
            positionString = "BAN";
        }
        if ((dynamicPlayersData[playerIndex].weeksBannedOrInjured & injuryMask) != 0)
        {
            positionString = "INJ";
        }
       
        return formationIndex;
        
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
    public int GetTeamCashBalance(int teamId)
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
        int managerIndex = GetIndexToManagerForTeamID(playersTeam);
        if (managerIndex != -1)
        {
            dynamicManagersData[managerIndex].teamId = -1; // set unemployed
        }
        week = 0;

        playerRating = playerRatingStartValue;
        playersSponsor = -1;
        playersWeeksWithSponsor = 0;
        playersMatchBreaker = -1;
        matchEngine.state = MatchEngineState.MatchOver;
        playersMatchStrategy = MatchStrategy.Balanced;

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
        CPUTeamConsiderTransferListsForWeek();
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
               playerId = ChooseGoalKeeperForFormation(formation, playerIds, numPlayers);
           }
           if ((formPosType & (int)PlayerFormation.Attacker) !=0)
           {
               playerId = ChooseForwardForFormation(formation, playerIds, numPlayers);
           }
           if ((formPosType & (int)PlayerFormation.MidFielder) !=0)
           {
               playerId = ChooseMidfieldForFormation(formation, playerIds, numPlayers);
           }
           if ((formPosType & (int)PlayerFormation.Defender) !=0)
           {
               playerId = ChooseDefenderForFormation(formation, playerIds, numPlayers);
           }

           if (playerId == -1)
           {
               for (int k = 0; k < numPlayers; k++)
               {
                   int checkId = playerIds[k];
                   if (IsPlayerIdInFormation(checkId,formation) == -1)
                   {
                       playerId = checkId;
                       break;
                   }
               }
           }
           formation[i] = playerId;
        }
    }

/// <summary>
/// Determine the goalkeeper candidate for the formation
/// </summary>
/// <param name="squad"></param>
/// <param name="playerIds"></param>
/// <param name="numPlayers"></param>
/// <returns>the index for the player to assign goalkeeper.</returns>
    private int ChooseGoalKeeperForFormation(int[] squad, int[] playerIds, int numPlayers)
    {
        int resultIndex = -1;
        float resultRating = 0.0f;

        for (int i = 0; i < numPlayers; i++)
        {
            int playerId = playerIds[i];
            for (int j = 0; j < MaxPlayersInSquad; j++)
                if (squad[j] == playerId) playerId = -1; // player was already in the squad.
            
            //Check if player is already assigned
            if (playerId != -1)
            {
                int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                float starsRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
                
                float positionScale = 1.0f;
                float outOfPositionScale = 1.0f;
                if ((int)CheckPlayerIndexIsHappyInPosition(playerId, PlayerFormation.Goalkeeper) < 1)
                    outOfPositionScale = 0.5f;
                
                if (dynamicPlayersData[dataIndex].weeksBannedOrInjured > 0)
                    starsRating = 0.0f;

                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Goalkeeper) > 0)
                    positionScale = 2.0f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Defender) > 0)
                    positionScale = 1.5f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.MidFielder) > 0)
                    positionScale = 0.75f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Attacker) > 0)
                    positionScale = 0.5f;
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

/// <summary>
/// Determine the forward candidate for the formation
/// </summary>
/// <param name="squad"></param>
/// <param name="playerIds"></param>
/// <param name="numPlayers"></param>
/// <returns>the index for the player to assign goalkeeper.</returns>
    private int ChooseForwardForFormation(int[] squad, int[] playerIds, int numPlayers)
    {
        int resultIndex = -1;
        float resultRating = 0.0f;

        for (int i = 0; i < numPlayers; i++)
        {
            int playerId = playerIds[i];
            for (int j = 0; j < MaxPlayersInSquad; j++)
                if (squad[j] == playerId) playerId = -1; // player was already in the squad.
            
            //Check if player is already assigned
            if (playerId != -1)
            {
                int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                float starsRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
                
                float positionScale = 1.0f;
                float outOfPositionScale = 1.0f;
                if ((int)CheckPlayerIndexIsHappyInPosition(playerId, PlayerFormation.Attacker) == 0)
                    outOfPositionScale = 0.5f;
                
                if (dynamicPlayersData[dataIndex].weeksBannedOrInjured > 0)
                    starsRating = 0.0f;

                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Attacker) > 0)
                    positionScale = 2.0f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.MidFielder) > 0)
                    positionScale = 1.5f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.MidFielder) > 0)
                    positionScale = 0.75f;
                if ((dynamicPlayersData[dataIndex].flags & YellowCardMask) !=0) 
                    positionScale *= 0.95f; // take the shine off this player a little
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
/// <summary>
/// Determine the forward candidate for the formation
/// </summary>
/// <param name="squad"></param>
/// <param name="playerIds"></param>
/// <param name="numPlayers"></param>
/// <returns>the index for the player to assign goalkeeper.</returns>
    private int ChooseDefenderForFormation(int[] squad, int[] playerIds, int numPlayers)
    {
        int resultIndex = -1;
        float resultRating = 0.0f;

        for (int i = 0; i < numPlayers; i++)
        {
            int playerId = playerIds[i];
            for (int j = 0; j < MaxPlayersInSquad; j++)
                if (squad[j] == playerId) playerId = -1; // player was already in the squad.
            
            //Check if player is already assigned
            if (playerId != -1)
            {
                int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                float starsRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
                
                float positionScale = 1.0f;
                float outOfPositionScale = 1.0f;
                if ((int)CheckPlayerIndexIsHappyInPosition(playerId, PlayerFormation.Defender) == 0)
                    outOfPositionScale = 0.5f;
                
                if (dynamicPlayersData[dataIndex].weeksBannedOrInjured > 0)
                    starsRating = 0.0f;

                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Defender) > 0)
                    positionScale = 2.25f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.MidFielder) > 0)
                    positionScale = 0.9f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Goalkeeper) > 0)
                    positionScale = 0.8f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Attacker) > 0)
                    positionScale = 0.7f;
                
                if ((dynamicPlayersData[dataIndex].flags & YellowCardMask) !=0) 
                    positionScale *= 0.95f; // take the shine off this player a little
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

/// <summary>
/// Determine the forward candidate for the formation
/// </summary>
/// <param name="squad"></param>
/// <param name="playerIds"></param>
/// <param name="numPlayers"></param>
/// <returns>the index for the player to assign goalkeeper.</returns>
    private int ChooseMidfieldForFormation(int[] squad, int[] playerIds, int numPlayers)
    {
        int resultIndex = -1;
        float resultRating = 0.0f;

        for (int i = 0; i < numPlayers; i++)
        {
            int playerId = playerIds[i];
            for (int j = 0; j < MaxPlayersInSquad; j++)
                if (squad[j] == playerId) playerId = -1; // player was already in the squad.
            
            //Check if player is already assigned
            if (playerId != -1)
            {
                int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                float starsRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
                
                float positionScale = 1.0f;
                float outOfPositionScale = 1.0f;
                if ((int)CheckPlayerIndexIsHappyInPosition(playerId, PlayerFormation.MidFielder) == 0)
                    outOfPositionScale = 0.5f;
                
                if (dynamicPlayersData[dataIndex].weeksBannedOrInjured > 0)
                    starsRating = 0.0f;

                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.MidFielder) > 0)
                    positionScale = 2.0f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Attacker) > 0)
                    positionScale = 1.5f;
                if ((staticPlayersData[dataIndex].playerPositionFlags & PlayerFormation.Defender) > 0)
                    positionScale = 1.25f;
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

/// <summary>
/// This is used to check if a player is happy in an assigned formation, by checking the player's position flags.
/// </summary>
/// <param name="playerId">the player</param>
/// <param name="position">The PlayerFormation</param>
/// <returns>0 if unhappy, 1 if happy</returns>
    private PlayerFormation CheckPlayerIndexIsHappyInPosition(int playerId, PlayerFormation position)
    {
        int playerIndex = GetPlayerDataIndexForPlayerID(playerId);
        if (position == PlayerFormation.Substitute) return PlayerFormation.Substitute;
        return staticPlayersData[playerIndex].playerPositionFlags & position;
    }   

    private ManagementStyle GetEnumManagerStyleForTeamId(int teamId)
    {
        
        int managerIndex = GetIndexToManagerForTeamID(teamId);
        if (managerIndex != -1)
        {
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
        }
       
        return ManagementStyle.Balanced;
    }
    
    /// <param name="teamId">The team to retrieve the manager of</param>
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
    /// <summary>
    /// Manager index variant for loading a savegame
    /// </summary>
    /// <param name="managerId"></param>
    /// <returns></returns>
    int GetIndexToManagerForManagerID(int managerId)
    {
        int result = -1;
        for (int i = 0; i < numberOfManagersInArrays; i++)
        {
            if (staticManagersData[i].managerId == managerId)
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

    public int DetermineValueOfPlayerID(int playerId, int leagueId)
    {
        
        int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
        float starsRating = GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
        
        int result = 0;
        result = (int)(starsRating * leagueToPlayerValue[leagueId]);
        result *= (int)dynamicPlayersData[dataIndex].condition;
        return result;
    }

    public int GetPlayerDataIndexForPlayerID(int playerId)
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
    public float GetTeamLeagueAdjustedStarsRatingForPlayerIndex(int dataIndex)
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

    public int GetTeamsLeagueID(int currentTeamId)
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

    public int GetTeamDataIndexForTeamID(int teamId)
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
            //Debug.Log(premiumLeagueData.Length + " " + staticTeamsData.Length + " " + dataIndex);
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
        if (cash >= cost)
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

    public void AssignPlayerToFormation(int playerId, int index)
    {
        // Unset players old formation if in any
        int oldPos = IsPlayerIdInFormation(playerId, playersInFormation);
        if (oldPos != -1)
            playersInFormation[oldPos] = -1;
        playersInFormation[index] = playerId;
    }

    public int IsPlayerIdInFormation(int playerFormId)
    {
        int result = -1;
        for (int i = 0; i < MaxPlayersInSquad; i++)
        {
            if (playersInFormation[i] == playerFormId)
            {
                result = i;
                break;
            }
        }
        return result;
    }
    public int IsPlayerIdInFormation(int playerFormId, int[] formation)
    {
        for (int i = 0; i < MaxPlayersInSquad; i++)
        {
            if (formation[i] == playerFormId)
            {
                return i;
            }
        }
        return -1;
    }

    public void SwapFormationCycle()
    {
        currentNumberOfPage = (numPlayersInPlayersTeam / MaxPlayersInList) + 1;
        if (currentPage >= currentNumberOfPage)
            currentPage = currentNumberOfPage -1;
        int maxItems = numPlayersInPlayersTeam - (currentPage * MaxPlayersInList);
        if (maxItems > MaxPlayersInList)
            maxItems = MaxPlayersInList;
        //show page view
        currentMenuItems[6].gameObject.SetActive(true);
        currentMenuItems[7].gameObject.SetActive(true);
        currentMenuItems[8].gameObject.SetActive(true);
        //update Page ?/? text
        currentMenuItems[6].SetText("Page " + (currentPage+1) + "/" + currentNumberOfPage);

        
        //Generate List
               RectTransform itemsForTraining = currentScreenDefinition.transform.GetChild(1).GetComponent<RectTransform>(); 
               float yOffTrain = 0.0f;
                //Training screen menu stuff
                itemsForTraining.sizeDelta = new Vector2(320f,menuItemGenerator.playerTrainingYOffset+395+(22*maxItems));
                
                for (int i = 0; i < maxItems; i++)
                {
                    int playerId = playersTeamPlayerIds[i + currentPage * MaxPlayersInList];
                    int playerDataIndex = GetPlayerDataIndexForPlayerID(playerId);
                       string nameString = String.Empty;
                       string playerLikesPositionString = String.Empty;
                       Color color = Color.white;
                    if (playerDataIndex != -1) // Handle Player name
                    {
                        if (matchEngine.state == Enums.MatchEngineState.MatchOver)
                        {
                            color = new Color(0.2f, 0.9f, 0.2f);
                        }
                        else
                        {
                            color = new Color(0.6f, 0.7f, 0.8f);
                        }
                        string positionString = "--";
                       
                        playerLikesPositionString = FillPlayerLikesStringForPlayerIndex(playerDataIndex);
                        int formationIndex = FillPositionStringForPlayerIndexs(playerDataIndex, formations[(int)formationType], ref positionString);
                        if (formationIndex != -1) // Turn yellow
                        {
                            color = new Color(1.0f,1.0f,0.8f,1.0f);
                        }

                        if (dynamicPlayersData[playerDataIndex].weeksBannedOrInjured != 0) // Turn Red
                        {
                            color = new Color(1.0f,0.0f,0.0f,1.0f);
                        }
                        nameString = "("+positionString+") "+staticPlayersData[playerDataIndex].playerSurname;
                    }


                    int stars = 0;
                    menuItemGenerator.CreatePlayerTrainings(currentScreenDefinition, new Vector2(0.0f, yOffTrain), stars, nameString,color,playerLikesPositionString,dynamicPlayersData[playerDataIndex]);
                    yOffTrain -= 22f;
                }
                for (int j = 0; j < maxItems; j++)
                {
                    menuItemGenerator.GenerateMenuItem(currentScreenDefinition,MenuElement.TextBarHalf, new Vector2(0,-1*(110-menuItemGenerator.playerTrainingYOffset+22*j)),0,0," ", Enums.MenuAction.AssignPlayerToFormation, j);
                } 
    }

    public int CountLegalPlayersOnPitchInSquad(int[] squad)
    {
        int result = 0;
        for (int i = 0; i < MaxPlayersInFormation; i++)
        {
            if (squad[i] != -1)
            {
                int playerId = squad[i];
                int dataIndex = GetPlayerDataIndexForPlayerID(playerId);
                if (dataIndex == -1)
                    continue;
                if ((dynamicPlayersData[dataIndex].weeksBannedOrInjured & bannedMask) != 0)
                    continue;
                if ((dynamicPlayersData[dataIndex].flags & RedCardMask) != 0)
                    continue;
                result++;
            }
        }
        return result;
    }

    public void UpdatePlayerIndexMoraleByAmount(int playerIndex, int amount)
    {
        dynamicPlayersData[playerIndex].morale += (short)amount;
        
        if (dynamicPlayersData[playerIndex].morale < -256)
            dynamicPlayersData[playerIndex].morale = -256;
        if (dynamicPlayersData[playerIndex].morale > 256)
            dynamicPlayersData[playerIndex].morale = 256;
    }

    public bool UpdateConditionOfPlayer(int dataIndex, float conditionAdjustment)
    {
        bool injured = false;
        float oldCondition = dynamicPlayersData[dataIndex].condition;
        dynamicPlayersData[dataIndex].condition += conditionAdjustment;
        //check for new injury
        if (dynamicPlayersData[dataIndex].condition < ShowInjuredRatio)
            if (oldCondition >= ShowInjuredRatio)
            {
                injured = true;
                UpdatePlayerIndexMoraleByAmount(dataIndex,-8);
            }

        if (dynamicPlayersData[dataIndex].condition < 0.0f)
            dynamicPlayersData[dataIndex].condition = 0.0f;
        if (dynamicPlayersData[dataIndex].condition > MaxPlayerCondition)
            dynamicPlayersData[dataIndex].condition = MaxPlayerCondition;

        return injured;
    }
    public void UpdateConditionOfPlayersInTeam(int teamId, float conditionAdjustment)
    {
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            if (dynamicPlayersData[i].teamId == teamId)
            {
                UpdateConditionOfPlayer(i, conditionAdjustment);
                UpdatePlayerIndexMoraleByAmount(i,-1);
            }
        }
    }
    public void UpdateBansInjuryAndFlagsOfPlayersInTeam(int teamId)
    {
        for (int i = 0; i < numberOfPlayersInArrays; i++)
        {
            if (dynamicPlayersData[i].teamId == teamId)
            {
                int dataIndex = i;
                dynamicPlayersData[dataIndex].flags &= ~(WarningMask | BeenOnPitchMask);
                dynamicPlayersData[dataIndex].flags &= ~(YellowCardMask | RedCardMask);
                // is player banned
                if ((dynamicPlayersData[dataIndex].flags & bannedMask) != 0)
                {
                    int weeks = (dynamicPlayersData[dataIndex].weeksBannedOrInjured & bannedMask) >> bannedBitShift;
                    weeks--;
                    UpdatePlayerIndexMoraleByAmount(dataIndex,-2);
                    if (weeks <= 0)
                    {
                        weeks = 0;
                    }

                    dynamicPlayersData[dataIndex].weeksBannedOrInjured =
                        (short)((dynamicPlayersData[dataIndex].weeksBannedOrInjured & injuryMask) | (weeks<<bannedBitShift));
                }
                if ((dynamicPlayersData[dataIndex].flags & injuryMask) != 0)
                {
                    int weeks = (dynamicPlayersData[dataIndex].weeksBannedOrInjured & injuryMask);
                    weeks--;
                    UpdatePlayerIndexMoraleByAmount(dataIndex,-4);
                    if (weeks <= 0)
                    {
                        weeks = 0;
                        dynamicPlayersData[dataIndex].condition += ShowInjuredRatio;
                        if (dynamicPlayersData[dataIndex].condition > MaxPlayerCondition)
                            dynamicPlayersData[dataIndex].condition = MaxPlayerCondition;
                    }

                    dynamicPlayersData[dataIndex].weeksBannedOrInjured =
                        (short)((dynamicPlayersData[dataIndex].weeksBannedOrInjured & bannedMask) | (weeks));
                }
                // should a player be banned from yellow cards
                if ((dynamicPlayersData[dataIndex].flags & YellowCardsUntilBanMask) == 0)
                {
                    premiumLeagueYellowCards[dataIndex] |= 5;
                    UpdatePlayerIndexMoraleByAmount(dataIndex,-4);
                    int weeks = (dynamicPlayersData[dataIndex].weeksBannedOrInjured & bannedMask) >> bannedBitShift;
                    weeks--;
                    UpdatePlayerIndexMoraleByAmount(dataIndex,-2);
                    if (weeks <= 0)
                    {
                        weeks = 0;
                    }

                    dynamicPlayersData[dataIndex].weeksBannedOrInjured =
                        (short)((dynamicPlayersData[dataIndex].weeksBannedOrInjured & injuryMask) | (weeks<<bannedBitShift));
                }
            }
        }
    }
    public void GivePlayerIndexAYellowCard(int playerIndex)
    {
        int numCards = dynamicPlayersData[playerIndex].flags & YellowCardMask;
        numCards++;
        dynamicPlayersData[playerIndex].flags |= (short)(numCards&YellowCardMask);
        if ((premiumLeagueYellowCards[playerIndex] & YellowCardsUntilBanMask) > 0)
        {
            int cardsTillBan = premiumLeagueYellowCards[playerIndex] * YellowCardsUntilBanMask;
            cardsTillBan--;
            premiumLeagueYellowCards[playerIndex] = (ushort)((premiumLeagueYellowCards[playerIndex] & YellowCardsUntilBanMask) |
                                                     (cardsTillBan & YellowCardsUntilBanMask));
        }
        //increase tournament counter too
        int yellowsInTournament = (premiumLeagueYellowCards[playerIndex] & YellowCardsInTournamentMask) >> YellowCardsInTournamentBitShift;
        yellowsInTournament++;
        premiumLeagueYellowCards[playerIndex] = (ushort)((premiumLeagueYellowCards[playerIndex] &
                                                          YellowCardsUntilBanMask) |
                                                         ((yellowsInTournament << YellowCardsInTournamentBitShift) &
                                                          YellowCardsInTournamentMask));

    }

    public void SetEndOfMatchButtonStates()
    {
        MenuItem[] menuItems = currentScreenDefinition.MenuItems.GetComponentsInChildren<MenuItem>();
        
        menuItems[1].HideItem(true);
        menuItems[2].HideItem(false);
        
        menuItems[8].HideItem(true);
        menuItems[9].HideItem(true);
        menuItems[10].HideItem(true);
        menuItems[11].HideItem(true);
        menuItems[12].HideItem(true);
    }
    public void PrepareNextYearOfPlay()
    {
        Debug.Log("Preparing next year of play");
        leagueEndNumTeamsInFinalStandings = numTeamsInScenarioLeague;
        for (int i = 0; i < numTeamsInScenarioLeague; i++)
            leagueEndUsersFinalStandings[i] = premiumLeagueData[i].teamId;

        int finishPos = GetPositionInLeagueTableForTeamId(playersTeam);
        if (finishPos == 1)
            playerRating += 4;
        else if (finishPos <= 3)
            playerRating += 2;
        else if (finishPos > (numTeamsInScenarioLeague - 3))
            playerRating -= 2;

        switch (playersLeague)
        {
            case LeagueID.Scotland:
                week = -1;
                break;
            case LeagueID.Usa:
                week = -1;
                break;
            default:
                leagueEndSaveUsersLeague = playersLeague;
                if (playersLeague == LeagueID.Premium)
                    playersLeague = LeagueID.Chumpionship;
                else if (playersLeague == LeagueID.Chumpionship)
                    playersLeague = LeagueID.Premium;

                numTeamsInScenarioLeague = CountTeamsInLeague(playersLeague);
                FillTeamsInLeagueArray(teamIndexsForScenarioLeague, playersLeague);
                week = 0;
                ResetLeaguePoints();
                BuildMatchSchedule(numTeamsInScenarioLeague);
                break;
        }
        GoToMenu(Enums.Screen.ProcessLeagueFinish);
    }
}