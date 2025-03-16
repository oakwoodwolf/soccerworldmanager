using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MatchEngine : MonoBehaviour
{
    public const int MaxPlayersInATeam = 64;
    [FormerlySerializedAs("State")]
    [Header("Match Engine Data")]
    public Enums.MatchEngineState state;
    [FormerlySerializedAs("Turn")]
    public int turn;
    [FormerlySerializedAs("InjuryTime")]
    [Tooltip("How much time has been spent addressing fouls/injurys and other delays?")]
    public float injuryTime;
    [FormerlySerializedAs("ExtraTime")]
    [Tooltip("Any extra time in minutes sourced from injury time")]
    public int extraTime;
    [FormerlySerializedAs("UpdateTimer")]
    [Tooltip("Counts time between turns")]
    public float updateTimer;
    [FormerlySerializedAs("SubTurnState")]
    [Tooltip("So we can process each step within a turn - so messages don't appear at once")]
    public Enums.MatchEngineSubState subTurnState;
    [FormerlySerializedAs("TeamInPossession")]
    [Tooltip("Who has the ball.")]
    public int teamInPossession;
    
    [FormerlySerializedAs("HomeTeamScore")]
    [Header("Home")]
    [Tooltip("Goals for home team")]
    public int homeTeamScore;
    [FormerlySerializedAs("HomeStrategyBalance")]
    [Tooltip("-1,0,+1 to multiple with strategy/balance factors")]
    public int homeStrategyBalance;
    [FormerlySerializedAs("HomeTeam")]
    public int homeTeam;
    public Enums.MatchBreakerFlags homeTeamMatchBreakerFlags;
    [FormerlySerializedAs("HomeTeamMatchBreakerActivationTurn")]
    public int homeTeamMatchBreakerActivationTurn;
    [FormerlySerializedAs("MaxHomeTeamPlayersOnPitch")]
    public int maxHomeTeamPlayersOnPitch;

    [FormerlySerializedAs("AwayTeamScore")]
    [Header("Away")]
    [Tooltip("Goals for away team")]
    public int awayTeamScore;
    [FormerlySerializedAs("AwayStrategyBalance")]
    [Tooltip("-1,0,+1 to multiple with strategy/balance factors")]
    public int awayStrategyBalance;
    [FormerlySerializedAs("AwayTeam")]
    public int awayTeam;
    [FormerlySerializedAs("AwayTeamMatchBreakerFlags")]
    public Enums.MatchBreakerFlags awayTeamMatchBreakerFlags;
    [FormerlySerializedAs("AwayTeamMatchBreakerActivationTurn")]
    public int awayTeamMatchBreakerActivationTurn;
    [FormerlySerializedAs("MaxAwayTeamPlayersOnPitch")]
    public int maxAwayTeamPlayersOnPitch;

    [FormerlySerializedAs("TurnTimeMultiplier")]
    [Tooltip("*5 or *1, used to calc time to display (normal or extra time)")]
    public int turnTimeMultiplier =1;
    [FormerlySerializedAs("TurnTimeOffset")]
    [Tooltip("0 or 90, depending upon displaying in game or extra time")]
    [Range(0,90)]
    public int turnTimeOffset;
    [FormerlySerializedAs("TurnsInPossession")]
    public int turnsInPossession;
    [FormerlySerializedAs("IndexOfFouledPlayer")]
    [Tooltip("Index of a fouled player (else -1)")]
    public float indexOfFouledPlayer;
    [FormerlySerializedAs("FoulDamageDone")]
    [Tooltip("Strength of damage done")]
    public float foulDamageDone;
    [FormerlySerializedAs("IndexOfPlayerCommitingFoul")]
    [Tooltip("Who did the naughty!?")]
    public int indexOfPlayerCommitingFoul;
    [FormerlySerializedAs("PlayerWithBallIndex")]
    [Tooltip("index into player arrays for the player with the ball - used for penalty taking")]
    public int playerWithBallIndex;
    [FormerlySerializedAs("MatchStatusYOffset")]
    [Tooltip("keep track of the yOff, so we can scroll up text if needed")]
    public float matchStatusYOffset;
    [FormerlySerializedAs("ItemsInQuickPlayerList")]
    public int itemsInQuickPlayerList;
    [FormerlySerializedAs("QuickPlayerIdToIndexList")]
    [Tooltip("a shorter list to aid in faster player data index determination")]
    public IdToIndex[] quickPlayerIdToIndexList = new IdToIndex[MaxPlayersInATeam*2];

    [Header("Match Engine Timer")]
    public float kickOffDelay = 0.0f;
    public float defaultDelay = 2.0f;
    public float goalDelay = 4.0f;

    public float skip = 8.0f;
    public float crowdMurMurTimer;

    [Header("Turns")]
    public int turnsInFirstHalf = 9;
    public int turnsInSecondHalf = 18;
    public const int MaxMatchStrings = 16;
    public const int MatchStringsMask = 15;
        
    public string[] matchStrings = new string[MaxMatchStrings];
    public int matchStringIndex = 0;
    
    public int extraTimeWarningTurn = 17;
    
    private GameManager gameManager;
    private PlayersMatch playersMatch;
    public GameObject overlay;

    [Header("UI")]
    [SerializeField]
    private TMP_Text[] matchTexts;
    [SerializeField]
    [TextArea]
    private string[] matchStringsLosingPossession =
    {
        "Dispossessed!",
        "The ball goes loose...",
        "He's tackled.",
        "Great tackle from the %s midfielder!",
        "Excellent tackle from %s.",
        "Tackled."
    };
    [SerializeField]
    [TextArea]
    private string[] matchStringsKeepPossession =
    {
        "%s make a run...",
        "%s keep possession...",
        "Superb control from %s...",
        "The incoming tackles are avoided...",
        "They've still got the ball!",
        "%s have still got possession.",
        "%s retain the ball.",
        "The %s players are passing well...",
        "%s keep the quality passes coming...",
        "%s string together a series of short passes.",
    };
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();
        playersMatch = gameManager.playersMatch;
        overlay.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetupForMatch(int homeTeamId, int awayTeamId)
    {
        overlay.SetActive(false);
        state = Enums.MatchEngineState.StartFirstHalf;
        turn = 0;
        updateTimer = kickOffDelay;
        subTurnState = 0;
        injuryTime = 0.0f;
        extraTime = -1;
        homeTeam = homeTeamId;
        awayTeam = awayTeamId;
        // normally 11, but could be less if players are sent off. 
        maxHomeTeamPlayersOnPitch = 11;
        maxAwayTeamPlayersOnPitch = 11;
        // set a default balanced strategy for both sides
        homeStrategyBalance = 0;
        awayStrategyBalance = 0;
        //clear matchbreakers
        homeTeamMatchBreakerFlags = 0;
        awayTeamMatchBreakerFlags = 0;
        indexOfFouledPlayer = -1;
        // TODO - prepare quick access team data array(s)
        itemsInQuickPlayerList = 0;
        for (int i = 0; i < gameManager.numberOfPlayersInArrays; i++)
        {
            if ((gameManager.dynamicPlayersData[i].teamId == homeTeamId) || (gameManager.dynamicPlayersData[i].teamId == awayTeamId))
            {
                Debug.Assert(itemsInQuickPlayerList < (2*GameManager.MaxPlayersInATeam));
                quickPlayerIdToIndexList[itemsInQuickPlayerList] = ScriptableObject.CreateInstance<IdToIndex>();
                quickPlayerIdToIndexList[itemsInQuickPlayerList].itemId = gameManager.staticPlayersData[i].playerId;
                quickPlayerIdToIndexList[itemsInQuickPlayerList].itemIndex = i;
                itemsInQuickPlayerList++;
            }
        }

    }

    public void Render(float dTime, bool skipping = false)
    {
        if (!skipping) // Crowd noises
        {
            crowdMurMurTimer -= dTime;
            if (crowdMurMurTimer < 0)
            {
                crowdMurMurTimer += 4.8f;
                int index = Random.Range(0, 2);
                gameManager.SoundEngine_StartEffect(Enums.Sounds.Crowd1+index);
            }
        }

        {
            updateTimer -= dTime;
            if (skipping)
                updateTimer = -1;
            if (updateTimer <= 0.0f || skipping) // simulate 'occasional' update of content
            {
                updateTimer = defaultDelay;
                if (homeTeam == gameManager.playersTeam)
                    homeStrategyBalance = gameManager.playersMatchStrategy - (Enums.MatchStrategy)1;
                else
                    UpdateCPUTeamAI(homeTeam);
                if (awayTeam == gameManager.playersTeam)
                    awayStrategyBalance = gameManager.playersMatchStrategy - (Enums.MatchStrategy)1;
                else
                    UpdateCPUTeamAI(awayTeam);

                switch (state)
                {
                    case Enums.MatchEngineState.StartFirstHalf:
                        turnTimeMultiplier = 5;
                        turnTimeOffset = 0;
                        teamInPossession = 0; // 0 is home, 1 is away
                        turnsInPossession = 0;
                        if (!skipping)
                        {
                            gameManager.SoundEngine_StartEffect(Enums.Sounds.Whistle);
                            string matchString = "";
                            if (teamInPossession == 0)
                                 matchString = playersMatch.homeTeamName + " Kicks off";
                            else
                                 matchString = playersMatch.awayTeamName + " Kicks off";
                            PushMatchString(matchString);
                        }
                        state = Enums.MatchEngineState.InFirstHalf;
                        homeTeamScore = 0;
                        awayTeamScore = 0;
                        break;
                    case Enums.MatchEngineState.InFirstHalf:
                        if (turn >= turnsInFirstHalf)
                        {
                            turn = turnsInFirstHalf;
                            state = Enums.MatchEngineState.EndFirstHalf;
                            if (!skipping)
                            {
                                gameManager.SoundEngine_StartEffect(Enums.Sounds.Whistle_EndHalf);
                                string refString = "Ref blows his whistle!\n- That's the end of the first half.\n";
                                PushMatchString(refString);
                                matchStringIndex++;
                                matchStringIndex &= MatchStringsMask;
                                matchStrings[matchStringIndex] = "The score at halftime is ["+homeTeamScore+":"+awayTeamScore+"]\n";
                            }
                        }
                        else
                        {
                            UpdateMatchTurn(skipping);
                        }
                        break;
                    case Enums.MatchEngineState.EndFirstHalf:
                        break;
                    case Enums.MatchEngineState.StartSecondHalf:
                        break;
                    case Enums.MatchEngineState.InSecondHalf:
                        break;
                    case Enums.MatchEngineState.ExtraTime:
                        break;
                    case Enums.MatchEngineState.MatchOver:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!skipping)
                {
                    overlay.SetActive(true);
                    for (int i = 0; i < 6; i++)
                    {
                        matchTexts[i].text = matchStrings[(matchStringIndex-(5-i))&MatchStringsMask];
                    }
                    int homeTeamIndex = gameManager.GetTeamDataIndexForTeamID(homeTeam);
                    int awayTeamIndex = gameManager.GetTeamDataIndexForTeamID(awayTeam);
                    string menuBarText = gameManager.staticTeamsData[homeTeamIndex].teamName + " " + homeTeamScore + " - " + awayTeamScore + " " + gameManager.staticTeamsData[awayTeamIndex].teamName; 
                    gameManager.currentScreenDefinition.MenuItems.transform.GetChild(0).GetComponent<TitleBar>().text = menuBarText;
                }
            }
        }
    }

    private void PushMatchString(string matchString)
    {
        matchStringIndex++;
        matchStringIndex &= MatchStringsMask;
        matchStrings[matchStringIndex] = "(" + GetTime() + " mins) " + matchString + "\n";
    }

    private void UpdateMatchTurn(bool skipping)
    {
        switch (subTurnState)
        {
            case Enums.MatchEngineSubState.DeterminePossession:
                int prevTeamInPossession = teamInPossession;
                teamInPossession = DeterminePossession();
                // !!!Match Breaker Effect!!! - Keep possession for 20 mins
                if ((prevTeamInPossession == 0) &&
                    (homeTeamMatchBreakerFlags & Enums.MatchBreakerFlags.GuaranteedPossession20Mins) != 0)
                {
                    if (turn < (homeTeamMatchBreakerActivationTurn + 4))
                    {
                        teamInPossession = 0;
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
                    }
                }
                if ((prevTeamInPossession == 1) &&
                    (awayTeamMatchBreakerFlags & Enums.MatchBreakerFlags.GuaranteedPossession20Mins) != 0)
                {
                    if (turn < (awayTeamMatchBreakerActivationTurn + 4))
                    {
                        teamInPossession = 1;
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
                    }
                }

                if (prevTeamInPossession != -1)
                {
                    if (prevTeamInPossession != teamInPossession)
                    {
                        turnsInPossession = 0;
                        if (!skipping)
                        {
                            
                            int index = Random.Range(0, matchStringsLosingPossession.Length);
                            if (teamInPossession == 0)
                            {
                                string losePossession = matchStringsLosingPossession[index].Replace("%s", playersMatch.homeTeamName) + " " + playersMatch.awayTeamName + " have lost the ball.";
                                PushMatchString(losePossession);
                            }
                            else
                            {
                                string losePossession = matchStringsLosingPossession[index].Replace("%s", playersMatch.awayTeamName) + " " + playersMatch.homeTeamName + " have lost the ball.";
                                PushMatchString(losePossession);
                            }
                        } 
                    }
                    else
                    {
                        turnsInPossession++;
                        if (!skipping)
                        {
                            string keepPossession = "";
                            int index = Random.Range(0, matchStringsKeepPossession.Length);
                            if (teamInPossession == 0)
                            {
                               keepPossession = matchStringsKeepPossession[index].Replace("%s", playersMatch.homeTeamName);
                            }
                            else
                            { 
                                keepPossession = matchStringsKeepPossession[index].Replace("%s", playersMatch.awayTeamName);
                            }
                            PushMatchString(keepPossession);
                        }
                    }
                    subTurnState = Enums.MatchEngineSubState.FoulCheck;
                }
                    
                break;
            case Enums.MatchEngineSubState.FoulCheck:
                break;
            case Enums.MatchEngineSubState.ProcessFoul:
                break;
            case Enums.MatchEngineSubState.PromptFormationFix:
                break;
            case Enums.MatchEngineSubState.RefIssueCard:
                break;
            case Enums.MatchEngineSubState.TakingPenalty:
                break;
            case Enums.MatchEngineSubState.DetermineGoalFromPenalty:
                break;
            case Enums.MatchEngineSubState.DetermineShooting:
                break;
            case Enums.MatchEngineSubState.DetermineGoal:
                break;
            case Enums.MatchEngineSubState.RestartPlayAfterGoal:
                break;
            case Enums.MatchEngineSubState.TakeFreeKick:
                break;
            case Enums.MatchEngineSubState.AdvanceTurn:
                break;
            case Enums.MatchEngineSubState.Max:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private int DeterminePossession()
    {
        int result;
        int teamHTotalSkill = GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam,playersMatch.formationTypeHomeTeam);
        int teamATotalSkill = GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam,playersMatch.formationTypeAwayTeam);
        teamHTotalSkill += (teamHTotalSkill/10);
        
        int teamSkillTotal = teamHTotalSkill + teamATotalSkill;
        
        int possession = (int)(Random.value * teamATotalSkill);

        if (possession <= teamHTotalSkill)
            result = 0;
        else
            result = 1;
        return result;
    }

    private int GetSkillPointsForPlayersOnPitch(int[] players, Enums.Formation formationType, int posMask=0)
    {
        float totalSkillPoints = 0;
        FormationData formationInfo = gameManager.formations[(int)formationType];
        for (int i = 0; i < GameManager.MaxPlayersInFormation; i++)
        {
            if ((formationInfo.formations[i].formation & (Enums.PlayerFormation)posMask) != (Enums.PlayerFormation)0)
            {
                int playerId = players[i];
                if (playerId != -1)
                {
                    int dataIndex = GetPlayerDataIndexForPlayerId(playerId);
                    float outOfPositionScale = 1.0f;
                    if (gameManager.CheckPlayerIndexIsHappyInFormation(dataIndex, formationInfo.formations[i]) == 0)
                        outOfPositionScale = 0.5f;
                    float starsRating = gameManager.GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
                    if (gameManager.dynamicPlayersData[dataIndex].weeksBannedOrInjured != 0)
                        starsRating = 0.0f;
                    totalSkillPoints += (starsRating * gameManager.dynamicPlayersData[dataIndex].condition)*outOfPositionScale;
                }
            }
        }
        return (int)totalSkillPoints;
    }

    private int GetPlayerDataIndexForPlayerId(int playerId)
    {
        int playerIndex = -1;
        for (int i = 0; i < itemsInQuickPlayerList; i++)
        {
            if (quickPlayerIdToIndexList[i].itemId == playerId)
            {
                playerIndex = quickPlayerIdToIndexList[i].itemIndex;
                break;
            }
        }
        return playerIndex;
    }

    private int GetTime()
    {
        return turnTimeOffset + (turn * turnTimeMultiplier);
    }

    /// <summary>
    /// Updates the AI team's strategy depending on the goal difference.
    /// </summary>
    /// <param name="cpuTeam"></param>
    private void UpdateCPUTeamAI(int cpuTeam)
    {
        int stratBalance = 0;
        if (homeTeam == cpuTeam)
        {
            int goalDiff = homeTeamScore - awayTeamScore;
            if (goalDiff < 0) // are we behind
            {
                if (Math.Abs(goalDiff) < 2)
                {
                    if (turn >= turnsInSecondHalf - 3)
                        stratBalance += 1;
                    
                }
                else
                {
                    if (awayStrategyBalance <=0) //non-attacking opponent
                        stratBalance += 1; // go attacking
                    else
                        stratBalance -= 1;
                }
            }
            homeStrategyBalance = 0;
            if (stratBalance > 0)
                homeStrategyBalance = 1;
            if (stratBalance < 0)
                homeStrategyBalance = -1;
        }
        if (awayTeam == cpuTeam)
        {
            int goalDiff = awayTeamScore - homeTeamScore;
            if (goalDiff < 0) // are we behind
            {
                if (Math.Abs(goalDiff) < 2)
                {
                    if (turn >= turnsInSecondHalf - 3)
                        stratBalance += 1;
                    
                }
                else
                {
                    if (homeStrategyBalance <=0) //non-attacking opponent
                        stratBalance += 1; // go attacking
                    else
                        stratBalance -= 1;
                }
            }
            awayStrategyBalance = 0;
            if (stratBalance > 0)
                awayStrategyBalance = 1;
            if (stratBalance < 0)
                awayStrategyBalance = -1;
        }
    }
}
