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
    public const int TeamHome = 0;
    public const int TeamAway = 1;
    public const float ConditionAdjustPerTurn = -0.016f;
        
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
    public int indexOfFouledPlayer;
    [FormerlySerializedAs("FoulDamageDone")]
    [Tooltip("Strength of damage done")]
    public float foulDamageDone;
    [FormerlySerializedAs("IndexOfPlayerCommitingFoul")]
    [Tooltip("Who did the naughty!?")]
    public int indexOfPlayerCommitingFoul;
    public DynamicPlayerData playerCommitingFoul;
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

    [SerializeField] [TextArea] private string[] matchStringsTeamFouled =
    {
        "The %s player is brought down. Free kick given.",
        "%s midfielder is hacked by an opposition player. Free kick to %s.",
        "A horrible foul on the %s player! The crowd are appauled!",
        "The %s player is fouled. Free kick awarded.",
        // Penalty!!!
        "The %s striker has been fouled in the area! Penalty to %s!",
        "The opposition defender handled the ball in the area! Penalty to %s!",
    };
    [SerializeField]
    [TextArea]
    private string[] matchStringsChanceCreated =
    {
        "%s lines up a shot...",
        "%s from outside the penalty area...",
        "Inside the 6-yard box, surely a goal for %s...",
        "The cross is received by %s from the left...",
        "It's crossed in towards %s",
        "%s's got space! The shot is taken...",
        "From miles away... It's %s",
        "Surely %s can't miss from here...",
        "%s's through on goal..."
    };
    [SerializeField]
    [TextArea]
    private string[] matchStringsGoalScored =
    {
        "Goal scored!",
        "Straight in the back of the net!",
        "It's blocked, but he taps it in!",
        "What a goal!",
        "Goal for %s!",
        "It goes in off the post! What a lucky goal!",
        "Goal!",
        "Incredible goal!",
        "Goooooaaaaaaaaaallllllllllll!",
        "Superb finish!",
        "And it's in!",
        "Cracking goal!",
        "Great left-footed finish!",
        "It's tucked away stylishly!"
    };
    [SerializeField]
    [TextArea]
    private string[] matchStringsGoalMissed =
    {
        "The ball has gone wide!",
        "The shot goes over the bar!",
        "It hits the woodwork!",
        "It misses by miles!",
        "It misses the target.",
        "The shot has no power.",
        "The ball is sliced wide.",
        "Too high!",
    };
    [SerializeField]
    [TextArea]
    private string[] matchStringsTakeFreeKick =
    {
        "Free kick given to %s within shooting distance.",
        "%s are awarded a free kick within range of the goal.",
        "%s take a free kick.",
        "%s take the free kick.",
        "A free kick for %s.",
    };
    [SerializeField]
    [TextArea]
    private string[] matchStringsRestartPlayAfterGoal =
    {
        "get the action underway.",
        "take the centre kick.",
        "take centre.",
        "commence the action.",
        "start with possession.",
        "begin play.",
    };
    [SerializeField]
    [TextArea]
    private string[] matchStringsPlayerInjured =
    {
        "%s needs to be stretchered off.",
        "%s's going to need stretchering off.",
        "%s won't be finishing the match.",
        "%s is stretchered off.",
        "The injury is too serious for %s to carry on."
    };
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();
        playersMatch = gameManager.playersMatch;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.currentScreen!=Enums.Screen.MatchEngine)
        {
            // additional clearing
            for (int j = 0; j < 6; j++)
            {
                matchTexts[j].text = "-";
            }
            overlay.SetActive(false);
        }
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
        
        int indexH = gameManager.GetTeamDataIndexForTeamID(homeTeamId);
        int indexA = gameManager.GetTeamDataIndexForTeamID(awayTeamId);
        Debug.Log("Preparing Match for " + gameManager.dynamicTeamsData[indexH].name + " " + gameManager.dynamicTeamsData[indexA].name);
        for (int i = 0; i < gameManager.numberOfPlayersInArrays; i++)
        {
            if ((gameManager.dynamicPlayersData[i].teamId == homeTeamId) || (gameManager.dynamicPlayersData[i].teamId == awayTeamId))
            {
                Debug.Assert(itemsInQuickPlayerList < (2*GameManager.MaxPlayersInATeam));
                quickPlayerIdToIndexList[itemsInQuickPlayerList] = ScriptableObject.CreateInstance<IdToIndex>();
                quickPlayerIdToIndexList[itemsInQuickPlayerList].name = gameManager.staticPlayersData[i].playerSurname;
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
                        teamInPossession = TeamHome; // 0 is home, 1 is away
                        turnsInPossession = 0;
                        if (!skipping)
                        {
                            
                            gameManager.SoundEngine_StartEffect(Enums.Sounds.Whistle);
                            string matchString = "";
                            if (teamInPossession == TeamHome)
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
                        state = Enums.MatchEngineState.StartSecondHalf;
                        
                        break;
                    case Enums.MatchEngineState.StartSecondHalf:
                        if (!skipping)
                        {
                            gameManager.SoundEngine_StartEffect(Enums.Sounds.Whistle);
                            teamInPossession = TeamAway;
                            turnsInPossession = 0;
                            string matchString = "";
                            if (teamInPossession == 0)
                                matchString = playersMatch.homeTeamName + " get us going for the second half.";
                            else
                                matchString = playersMatch.awayTeamName + " get us going for the second half.";
                            PushMatchString(matchString);
                        }
                        state = Enums.MatchEngineState.InSecondHalf;
                        break;
                    case Enums.MatchEngineState.InSecondHalf:
                        if (turn == extraTimeWarningTurn &&
                            subTurnState == Enums.MatchEngineSubState.DeterminePossession && extraTime == -1)
                        {
                            extraTime = (int)injuryTime;
                            if (((homeTeamMatchBreakerFlags & Enums.MatchBreakerFlags.ExtraExtraTime) !=
                                 (Enums.MatchBreakerFlags)0) ||
                                ((awayTeamMatchBreakerFlags & Enums.MatchBreakerFlags.ExtraExtraTime) != 0))
                                extraTime += 5;
                            if (!skipping)
                            {
                                if (extraTime > 0)
                                    PushMatchString("The 4th official indicates there will be "+extraTime+" min(s) of extra time.");
                                else
                                    PushMatchString("The 4th official indicates there will be no extra time played.");
                            }

                        }
                        if (turn >= turnsInSecondHalf)
                        {
                            turn = turnsInSecondHalf;
                            if (extraTime > 0)
                            {
                                state = Enums.MatchEngineState.ExtraTime;
                                turn = 0;
                                turnTimeMultiplier = 1;
                                turnTimeOffset = 90;
                            }
                            else
                            {
                                state = Enums.MatchEngineState.MatchOver;
                                if (!skipping)
                                {
                                    gameManager.SoundEngine_StartEffect(Enums.Sounds.Whistle_EndGame);
                                    string refString = "The Ref blows his whistle!\n- That's the end of the Match!\n";
                                    PushMatchString(refString);
                                    matchStringIndex++;
                                    matchStringIndex &= MatchStringsMask;
                                    matchStrings[matchStringIndex] = "The final score is ["+homeTeamScore+":"+awayTeamScore+"]\n";
                                    gameManager.SetEndOfMatchButtonStates();
                                }
                            }
                            
                        }
                        else
                        {
                            UpdateMatchTurn(skipping);
                        }
                        break;
                    case Enums.MatchEngineState.ExtraTime:
                        if (turn >= extraTime)
                        {

                            state = Enums.MatchEngineState.MatchOver;
                            if (!skipping)
                            {
                                gameManager.SoundEngine_StartEffect(Enums.Sounds.Whistle_EndGame);
                                string refString = "The Ref blows his whistle!\n- That's the end of the Match!\n";
                                PushMatchString(refString);
                                matchStringIndex++;
                                matchStringIndex &= MatchStringsMask;
                                matchStrings[matchStringIndex] = "The final score is ["+homeTeamScore+":"+awayTeamScore+"]\n";
                                gameManager.SetEndOfMatchButtonStates();
                            }
                            
                            
                        }
                        else
                        {
                            UpdateMatchTurn(skipping);
                        }
                        break;
                    case Enums.MatchEngineState.MatchOver:
                        gameManager.SetEndOfMatchButtonStates();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!skipping)
                {
                    overlay.SetActive(true);
                    float off = 0f;
                    for (int i = 0; i < 6; i++)
                    {
                        matchTexts[i].text = matchStrings[(matchStringIndex-(5-i))&MatchStringsMask];
                        matchTexts[i].rectTransform.anchoredPosition = new Vector2(0,-off);
                        matchTexts[i].ForceMeshUpdate();
                        off += (17 * (matchTexts[i].textInfo.lineCount));
                        Debug.Log(matchTexts[i].textInfo.lineCount + " " + matchTexts[i].text);
                    }
                    int homeTeamIndex = gameManager.GetTeamDataIndexForTeamID(homeTeam);
                    int awayTeamIndex = gameManager.GetTeamDataIndexForTeamID(awayTeam);
                    string menuBarText = gameManager.staticTeamsData[homeTeamIndex].teamName + " " + homeTeamScore + " - " + awayTeamScore + " " + gameManager.staticTeamsData[awayTeamIndex].teamName; 
                    gameManager.currentScreenDefinition.MenuItems.transform.GetChild(0).GetComponent<TitleBar>().SetText(menuBarText);
                }
            }
        }
    }

    /// <summary>
    /// Wraps the boilerplate for the match string to be drawn, adding the time as a prefix.
    /// </summary>
    /// <param name="matchString"></param>
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
                        teamInPossession = TeamHome;
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
                    }
                }
                if ((prevTeamInPossession == TeamAway) &&
                    (awayTeamMatchBreakerFlags & Enums.MatchBreakerFlags.GuaranteedPossession20Mins) != 0)
                {
                    if (turn < (awayTeamMatchBreakerActivationTurn + 4))
                    {
                        teamInPossession = TeamAway;
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
                            if (teamInPossession == TeamHome)
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
                            if (teamInPossession == TeamHome)
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
                if (DetermineFouls())
                {
                    int index = Random.Range(0, 4);
                    bool penalty = false;
                    if ((turnsInPossession & 3) == 3)
                    {
                        index &= 1;
                        index += 4;
                        penalty = true;
                    }

                    if (!skipping)
                    {
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.Whistle_Foul);
                        string fouledString = "";
                        if (teamInPossession == TeamHome)
                            fouledString = matchStringsTeamFouled[index].Replace("%s", playersMatch.homeTeamName);
                        else
                            fouledString = matchStringsTeamFouled[index].Replace("%s", playersMatch.awayTeamName);
                        if (penalty)
                            fouledString = "PENALTY: " + fouledString;
                        else
                            fouledString = "FOUL: " + fouledString;
                        PushMatchString(fouledString);
                    }

                    indexOfFouledPlayer = DeterminePlayerWithTheBall();
                    DeterminePlayerIndexWhoCausedFoul();
                    if (penalty)
                    {
                        updateTimer /= 2.0f;
                        subTurnState = Enums.MatchEngineSubState.TakingPenalty;
                    }
                    else
                    {
                        subTurnState = Enums.MatchEngineSubState.ProcessFoul;
                    }
                }
                else
                {
                    subTurnState = Enums.MatchEngineSubState.DetermineShooting;
                }
                break;
            case Enums.MatchEngineSubState.ProcessFoul:
                foulDamageDone = Random.value * 0.25f;
                bool injured = false;
                if (((homeTeamMatchBreakerFlags & Enums.MatchBreakerFlags.ImmunityToInjury) !=
                     (Enums.MatchBreakerFlags)0) ||
                    ((awayTeamMatchBreakerFlags & Enums.MatchBreakerFlags.ImmunityToInjury) !=
                     (Enums.MatchBreakerFlags)0))
                {
                    injured = false;
                    gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
                }
                else
                    injured = gameManager.UpdateConditionOfPlayer(indexOfFouledPlayer, -foulDamageDone);

                if (injured)
                {
                    if (!skipping)
                    {
                        int index = Random.Range(0, matchStringsPlayerInjured.Length);
                        string injuredString = matchStringsPlayerInjured[index].Replace("%s", gameManager.staticPlayersData[indexOfFouledPlayer].playerSurname);
                        PushMatchString(injuredString);
                    }
                    gameManager.dynamicPlayersData[indexOfFouledPlayer].weeksBannedOrInjured = (short)((gameManager.dynamicPlayersData[indexOfFouledPlayer].weeksBannedOrInjured & GameManager.bannedMask) | (1+1));
                }
                subTurnState = Enums.MatchEngineSubState.RefIssueCard;
                break;
            case Enums.MatchEngineSubState.PromptFormationFix:
                if (!skipping)
                {
                    string fixString = "";
                    if (playersMatch.homeTeam == gameManager.playersTeam)
                        fixString = playersMatch.homeTeamName;
                    else
                        fixString = playersMatch.awayTeamName;
                    PushMatchString(fixString + " make a change");
                }
                turn++;
                subTurnState = Enums.MatchEngineSubState.TakeFreeKick;
                break;
            case Enums.MatchEngineSubState.RefIssueCard:
                float refReaction = foulDamageDone;
                
                //Matchbreaker influence
                if (teamInPossession == TeamHome)
                {
                    if ((homeTeamMatchBreakerFlags & Enums.MatchBreakerFlags.Frustration) != (Enums.MatchBreakerFlags)0)
                    {
                        refReaction *= 1.5f;
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
                    }
                    if ((awayTeamMatchBreakerFlags & Enums.MatchBreakerFlags.ImmunityToCards) != (Enums.MatchBreakerFlags)0)
                    {
                        refReaction = 0.05f;
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
                    }
                }
                else
                {
                    if ((awayTeamMatchBreakerFlags & Enums.MatchBreakerFlags.Frustration) != (Enums.MatchBreakerFlags)0)
                    {
                        refReaction *= 1.5f;
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
                    }
                    if ((homeTeamMatchBreakerFlags & Enums.MatchBreakerFlags.ImmunityToCards) != (Enums.MatchBreakerFlags)0)
                    {
                        refReaction = 0.05f;
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
                    }
                }

                if (refReaction < 0.1f) // no reaction
                {
                    
                }
                else if (refReaction < 1.2f) // yellow card
                {
                    if ((gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].flags & GameManager.WarningMask) !=
                        0)
                    {
                        int numCards = gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].flags & GameManager.YellowCardMask;
                        if (numCards < 1)
                        {
                            gameManager.GivePlayerIndexAYellowCard(indexOfPlayerCommitingFoul);
                            if (!skipping)
                            {
                                string yellowCardString = "The ref wants to speak to %s. He reaches for a card... it's Yellow!".Replace("%s",playersMatch.foulerName);
                                PushMatchString(yellowCardString);
                            }
                        }
                        else
                        {
                            gameManager.GivePlayerIndexAYellowCard(indexOfPlayerCommitingFoul);
                            gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].flags |= GameManager.RedCardMask;
                            if (teamInPossession == TeamHome)
                                maxAwayTeamPlayersOnPitch--;
                            else
                                maxHomeTeamPlayersOnPitch--;
                            gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].weeksBannedOrInjured = (short)((gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].weeksBannedOrInjured & GameManager.injuryMask) | ((1+1)<<GameManager.bannedBitShift));
                            
                            if (!skipping)
                            {
                                string yellowCardString = "The ref reaches for a card... it's Yellow for %s,\nQuickly followed by a Red!\n".Replace("%s",playersMatch.foulerName);
                                PushMatchString(yellowCardString);
                            }
                        }
                    }
                    else
                    {
                        gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].flags |= GameManager.WarningMask;
                        if (!skipping)
                        {
                            string warningString =
                                playersMatch.foulerName + " gets away with a warning from the ref!\n";
                            PushMatchString(warningString);
                        }
                    }
                }
                else if (refReaction >= 0.24f) // red card
                {
                    gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].flags |= GameManager.RedCardMask;
                    
                    if (teamInPossession == TeamHome)
                        maxAwayTeamPlayersOnPitch--;
                    else
                        maxHomeTeamPlayersOnPitch--;

                    int weeksBan = (int)((foulDamageDone - 0.24f) * 299.0f);
                    weeksBan++;
                    gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].weeksBannedOrInjured = (short)((gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].weeksBannedOrInjured & GameManager.injuryMask) | ((weeksBan+1)<<GameManager.bannedBitShift));
                    
                    if (!skipping)
                    {
                        string warningString =
                            "That's a Red Card for " + playersMatch.foulerName;
                        PushMatchString(warningString);
                    }
                }
                
                //player is injured
                if (gameManager.dynamicPlayersData[indexOfFouledPlayer].condition < GameManager.ShowInjuredRatio)
                {
                    int fouledTeamId = gameManager.dynamicPlayersData[indexOfFouledPlayer].teamId;
                    if (fouledTeamId == gameManager.playersTeam)
                        subTurnState = Enums.MatchEngineSubState.PromptFormationFix;
                    else
                    {
                        turn++;
                        subTurnState = Enums.MatchEngineSubState.TakeFreeKick;
                    }
                }
                else
                {
                    if ((gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].flags & GameManager.RedCardMask) != 0)
                    {  
                        int offendingTeamId = gameManager.dynamicPlayersData[indexOfPlayerCommitingFoul].teamId;
                        if (offendingTeamId == gameManager.playersTeam)
                            subTurnState = Enums.MatchEngineSubState.PromptFormationFix;
                        else
                        {
                            turn++;
                            subTurnState = Enums.MatchEngineSubState.TakeFreeKick;
                        }
                    }
                    else
                    {
                        turn++;
                        subTurnState = Enums.MatchEngineSubState.TakeFreeKick;
                    }
                }
                
                break;
            case Enums.MatchEngineSubState.TakingPenalty:
                if (!skipping)
                {
                    matchStringIndex++;
                    matchStringIndex &= MatchStringsMask;
                    matchStrings[matchStringIndex] = "- " + playersMatch.scorerName + " steps up to take it\n";
                }
                updateTimer /= 2.0f;
                subTurnState = Enums.MatchEngineSubState.DetermineGoal;
                break;
            case Enums.MatchEngineSubState.DetermineGoalFromPenalty:
                Debug.Assert(false);
                break;
            case Enums.MatchEngineSubState.DetermineShooting:
                if (DetermineIfShotTaken())
                {
                    
                    if (!skipping)
                    {
                        DeterminePlayerWithTheBall();
                        int index = Random.Range(0, matchStringsChanceCreated.Length);
                        string chanceString = matchStringsChanceCreated[index].Replace("%s", playersMatch.scorerName);
                        string prefix = "";
                        if (teamInPossession == TeamHome)
                            prefix = playersMatch.homeTeamName;
                        else
                            prefix = playersMatch.awayTeamName;
                        PushMatchString(prefix + " have a chance... " + chanceString);
                        
                    }
                    updateTimer /= 2.0f;
                    subTurnState = Enums.MatchEngineSubState.DetermineGoal;
                }
                else
                {
                    subTurnState = Enums.MatchEngineSubState.AdvanceTurn;
                }
                break;
            case Enums.MatchEngineSubState.DetermineGoal:
                if (DetermineIfGoalScored())
                {
                    
                    string goalString = "";
                    int index = Random.Range(0, matchStringsGoalScored.Length);
                    if (teamInPossession == TeamHome)
                    {
                        homeTeamScore++;
                        goalString = matchStringsGoalScored[index].Replace("%s", playersMatch.homeTeamName);
                        teamInPossession = TeamAway;
                    }
                    else
                    {
                        awayTeamScore++;
                        goalString = matchStringsGoalScored[index].Replace("%s", playersMatch.awayTeamName);
                        teamInPossession = TeamHome;
                    }
                    if (!skipping)
                    {
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.Crowd_Goal);
                        if (gameManager.VibrationEnabled) 
                            Handheld.Vibrate();
                        matchStringIndex++;
                        matchStringIndex &= MatchStringsMask;
                        matchStrings[matchStringIndex] = "- GOAL! ["+homeTeamScore+":"+awayTeamScore+"] " + goalString + "\n";
                    }
                    updateTimer = goalDelay;
                    turn++;
                    subTurnState = Enums.MatchEngineSubState.RestartPlayAfterGoal;
                }
                else
                {
                    if (!skipping)
                    {
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.Crowd_MissedGoal);
                        int index = Random.Range(0, matchStringsGoalMissed.Length);
                        matchStringIndex++;
                        matchStringIndex &= MatchStringsMask;
                        matchStrings[matchStringIndex] = "- " + matchStringsGoalMissed[index] + "\n";
                    }
                    subTurnState = Enums.MatchEngineSubState.AdvanceTurn;
                }
                break;
            case Enums.MatchEngineSubState.RestartPlayAfterGoal:
                if (!skipping)
                {
                    gameManager.SoundEngine_StartEffect(Enums.Sounds.Whistle);
                    int index = Random.Range(0, matchStringsRestartPlayAfterGoal.Length);
                    string playString =  (teamInPossession == TeamHome ? playersMatch.homeTeamName : playersMatch.awayTeamName) + " " + matchStringsRestartPlayAfterGoal[index];
                    PushMatchString(playString);
                }
                subTurnState = Enums.MatchEngineSubState.DeterminePossession;
                break;
            case Enums.MatchEngineSubState.TakeFreeKick:
                if (!skipping)
                {
                    int index = Random.Range(1,matchStringsTakeFreeKick.Length);
                    string takeKickString = matchStringsTakeFreeKick[index].Replace("%s", teamInPossession == TeamHome ? playersMatch.homeTeamName : playersMatch.awayTeamName);
                    PushMatchString(takeKickString);
                }
                subTurnState = Enums.MatchEngineSubState.DeterminePossession;
                break;
            case Enums.MatchEngineSubState.AdvanceTurn:
                UpdateTeamConditionOfPlayers(homeTeam,playersMatch.formationHomeTeam,GameManager.MaxPlayersInFormation,ConditionAdjustPerTurn);
                UpdateTeamConditionOfPlayers(awayTeam,playersMatch.formationAwayTeam,GameManager.MaxPlayersInFormation,ConditionAdjustPerTurn);

                subTurnState = Enums.MatchEngineSubState.DeterminePossession;
                turn++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Update 'condition' of players in a formation
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="formation"></param>
    /// <param name="positions"></param>
    /// <param name="conditionAdjustment"></param>
    private void UpdateTeamConditionOfPlayers(int teamId, int[] formation, int positions, float conditionAdjustment)
    {
        Debug.Assert(positions <= GameManager.MaxPlayersInFormation);
        for (int i = 0; i < positions; i++)
        {
            int playerId = formation[i];
            if (playerId != -1)
            {
                int dataIndex = GetPlayerDataIndexForPlayerId(playerId);
                if (dataIndex != -1)
                {
                    bool injured = gameManager.UpdateConditionOfPlayer(dataIndex,conditionAdjustment);
                    if (injured)
                    {
                        //These are unimplemented in the original source too/
                        if (teamId == gameManager.playersTeam)
                        {
                            
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        gameManager.UpdatePlayerIndexMoraleByAmount(dataIndex,1);
                    }
                }
            }
        }
    }

    private bool DetermineIfShotTaken()
    {
        bool result = false;
        int teamMidfieldSkill;
        if (teamInPossession == TeamHome)
            teamMidfieldSkill = GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam,Enums.PlayerFormation.MidFielder,playersMatch.formationTypeHomeTeam);
        else
            teamMidfieldSkill = GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam,Enums.PlayerFormation.MidFielder,playersMatch.formationTypeAwayTeam);
        int maxMidfieldSkill = 30;
        int chance = (int)(Random.value * maxMidfieldSkill);
        
        //Home Team support matchbreaker!
        if ((teamInPossession == TeamHome) && ((homeTeamMatchBreakerFlags & Enums.MatchBreakerFlags.HomeTeamSupport) != 0))
        {
            // half whatever we currently have to increase odds
            if (chance > 0)
                chance /= 2;
            gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
        }
        chance += (1 * (-homeStrategyBalance));
        chance += (1 * (-awayStrategyBalance));
        if (chance <= teamMidfieldSkill)
            result = true;
        return result;
    }

    
    private bool DetermineIfGoalScored()
    {
        bool result = false;
        int attackingSkill;
        if (teamInPossession == TeamHome)
            attackingSkill = GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam, Enums.PlayerFormation.Attacker,playersMatch.formationTypeHomeTeam);
        else
            attackingSkill = GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam, Enums.PlayerFormation.Attacker,playersMatch.formationTypeAwayTeam);
        // Evaluate the defences
        int defenceSkill;
        float goalieScale = 1.0f;
        
        if (teamInPossession == TeamHome)
        {
            defenceSkill = GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam, Enums.PlayerFormation.Defender,playersMatch.formationTypeAwayTeam);
            goalieScale += 0.1f * GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam, Enums.PlayerFormation.Goalkeeper,playersMatch.formationTypeAwayTeam);
        }
        else
        {
            defenceSkill = GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam, Enums.PlayerFormation.Defender,playersMatch.formationTypeHomeTeam);
            goalieScale += 0.1f * GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam, Enums.PlayerFormation.Goalkeeper,playersMatch.formationTypeHomeTeam);
        }
        defenceSkill = (int)((float)defenceSkill * goalieScale); // apply goalie influence
        
        int skillrange = attackingSkill + defenceSkill;
        int chance = (int)(Random.value * skillrange);
        
        //FlukeORama
        if ((teamInPossession == TeamHome) && ((homeTeamMatchBreakerFlags & Enums.MatchBreakerFlags.FlukeORama) != 0))
        {
            if (turn < (homeTeamMatchBreakerActivationTurn + 6))
            {
                // half whatever we currently have to increase odds
                if (chance > 0)
                    chance /= 2;
                gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
            }
            else
                homeTeamMatchBreakerFlags &= ~Enums.MatchBreakerFlags.FlukeORama;
        }
        if ((teamInPossession == TeamAway) && ((awayTeamMatchBreakerFlags & Enums.MatchBreakerFlags.FlukeORama) != 0))
        {
            if (turn < (awayTeamMatchBreakerActivationTurn + 6))
            {
                // half whatever we currently have to increase odds
                if (chance > 0)
                    chance /= 2;
                gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
            }
            else
                awayTeamMatchBreakerFlags &= ~Enums.MatchBreakerFlags.FlukeORama;
        }
        
        chance += (1 * (-homeStrategyBalance));
        chance += (1 * (-awayStrategyBalance));
        if (chance <= attackingSkill)
            result = true;
        
        
        //Stalemate
        if ((homeTeamMatchBreakerFlags & Enums.MatchBreakerFlags.Stalemate) != 0)
        {
            if (turn < (homeTeamMatchBreakerActivationTurn + 6))
            {
                result = false;
                gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
            }
            else
                homeTeamMatchBreakerFlags &= ~Enums.MatchBreakerFlags.Stalemate;
        }
        if ((awayTeamMatchBreakerFlags & Enums.MatchBreakerFlags.Stalemate) != 0)
        {
            if (turn < (awayTeamMatchBreakerActivationTurn + 6))
            {
                result = false;
                gameManager.SoundEngine_StartEffect(Enums.Sounds.MatchBreakerEffect);
            }
            else
                awayTeamMatchBreakerFlags &= ~Enums.MatchBreakerFlags.Stalemate;
        }
        return result;
    }
    private int DeterminePlayerIndexWhoCausedFoul()
    {
        int foulerIndex = -1;
        int withBallId = gameManager.staticPlayersData[playerWithBallIndex].playerId;
        Enums.Formation formType;
        int[] squad;
        if (teamInPossession == TeamHome)
        {
            squad = playersMatch.formationHomeTeam;
            formType = playersMatch.formationTypeHomeTeam;
        }
        else
        {
            squad = playersMatch.formationAwayTeam;
            formType = playersMatch.formationTypeAwayTeam;
        }

        int withFormIndex;
        for (withFormIndex = 0; withFormIndex < GameManager.MaxPlayersInSquad; withFormIndex++)
        {
            // squad array holds ids
            if (squad[withFormIndex] == withBallId)
                break; 		// out of loop
        }

        FormationData formationInfo = gameManager.formations[(int)formType];
        Enums.PlayerFormation positionOfPlayerWithBall = formationInfo.formations[withFormIndex].formation;
        Enums.PlayerFormation positionOfFoulingPlayer = Enums.PlayerFormation.Defender;

        if ((positionOfPlayerWithBall & Enums.PlayerFormation.Attacker) != (Enums.PlayerFormation)0)
        {
            int chance = Random.Range(0, 11); //6/11=defender, 3/11=midfield, 1/11=goalkeeper
            
            if (chance < 6)
                positionOfFoulingPlayer = Enums.PlayerFormation.Defender;
            else if (chance < 9)
                positionOfFoulingPlayer = Enums.PlayerFormation.MidFielder;
            else
                positionOfFoulingPlayer = Enums.PlayerFormation.Goalkeeper;
        }
        else if ((positionOfPlayerWithBall & Enums.PlayerFormation.MidFielder) != (Enums.PlayerFormation)0)
        {
            int chance = Random.Range(0, 11); //6/11=defender, 3/11=midfield, 1/11=goalkeeper
            
            if (chance < 6)
                positionOfFoulingPlayer = Enums.PlayerFormation.MidFielder;
            else if (chance < 9)
                positionOfFoulingPlayer = Enums.PlayerFormation.Defender;
            else
                positionOfFoulingPlayer = Enums.PlayerFormation.Attacker;
        }
        else if ((positionOfPlayerWithBall & Enums.PlayerFormation.Defender) != (Enums.PlayerFormation)0)
        {
            int chance = Random.Range(0, 11); //6/11=defender, 3/11=midfield, 1/11=goalkeeper
            
            if (chance < 6)
                positionOfFoulingPlayer = Enums.PlayerFormation.Attacker;
            else if (chance < 9)
                positionOfFoulingPlayer = Enums.PlayerFormation.MidFielder;
            else
                positionOfFoulingPlayer = Enums.PlayerFormation.Defender;
        }
        Debug.Log(positionOfFoulingPlayer.ToString());
        
        // Scan list for players in needed position
        int skill;
        if (teamInPossession == TeamHome)
            skill = GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam,positionOfFoulingPlayer,playersMatch.formationTypeAwayTeam);
        else
            skill = GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam,positionOfFoulingPlayer,playersMatch.formationTypeHomeTeam);
        
        //if (skill > 0)
        //{
            if (teamInPossession == TeamHome)
                foulerIndex = GetPlayerIndexInFormationAtSkillOffset(playersMatch.formationAwayTeam, skill,
                    positionOfFoulingPlayer, playersMatch.formationTypeAwayTeam, awayTeam);
            else
                foulerIndex = GetPlayerIndexInFormationAtSkillOffset(playersMatch.formationHomeTeam, skill,
                    positionOfFoulingPlayer, playersMatch.formationTypeHomeTeam, homeTeam);
        //}
        Debug.Log("skill " + skill + "\t fouling player index:" + foulerIndex);
        Debug.Assert(foulerIndex != -1);
        if (foulerIndex == -1)
            playersMatch.foulerName = "[Err Det Foul Plyr]";
        else
        {
            playersMatch.foulerName = gameManager.staticPlayersData[foulerIndex].playerSurname;
            playerCommitingFoul = gameManager.dynamicPlayersData[foulerIndex];
        }
            
        indexOfPlayerCommitingFoul = foulerIndex;
        
        return foulerIndex;
    }

    private int DeterminePlayerWithTheBall()
    {
        int possessionAttack = 0;
        int possessionMid = 0;
        int possessionDefence = 0;

        if (teamInPossession == 0)
        {
            possessionAttack = GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam, Enums.PlayerFormation.Attacker, playersMatch.formationTypeHomeTeam);
            possessionMid = GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam, Enums.PlayerFormation.MidFielder, playersMatch.formationTypeHomeTeam);
            possessionDefence = GetSkillPointsForPlayersOnPitch(playersMatch.formationHomeTeam, Enums.PlayerFormation.Defender, playersMatch.formationTypeHomeTeam);
        }
        else
        {
            possessionAttack = GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam, Enums.PlayerFormation.Attacker, playersMatch.formationTypeAwayTeam);
            possessionMid = GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam, Enums.PlayerFormation.MidFielder, playersMatch.formationTypeAwayTeam);
            possessionDefence = GetSkillPointsForPlayersOnPitch(playersMatch.formationAwayTeam, Enums.PlayerFormation.Defender, playersMatch.formationTypeAwayTeam);
        }
            
        // bias against defence
        possessionDefence = (int)(possessionDefence * 0.5f); // half the defence contribution
        possessionAttack = (int)(possessionAttack * 0.5f); // increase contribution of strikers

        int scorerIndex = -1;
        int whorange = possessionAttack + possessionMid + possessionDefence;
        float chance = Random.Range(0f, whorange);
        if (chance <= possessionAttack)
        { // score by forward
            float whoskill = Random.Range(0f, possessionAttack/2f);
            if (teamInPossession == TeamHome)
                scorerIndex = GetPlayerIndexInFormationAtSkillOffset(playersMatch.formationHomeTeam, whoskill, Enums.PlayerFormation.Attacker, playersMatch.formationTypeHomeTeam, homeTeam);
            else
                scorerIndex = GetPlayerIndexInFormationAtSkillOffset(playersMatch.formationAwayTeam, whoskill, Enums.PlayerFormation.Attacker, playersMatch.formationTypeAwayTeam, awayTeam);
        }
        else if (chance <= (possessionAttack + possessionMid))
        { // score by mid
            float whoskill = Random.Range(0f, possessionMid);
            if (teamInPossession == TeamHome)
                scorerIndex = GetPlayerIndexInFormationAtSkillOffset(playersMatch.formationHomeTeam, whoskill, Enums.PlayerFormation.MidFielder, playersMatch.formationTypeHomeTeam, homeTeam);
            else
                scorerIndex = GetPlayerIndexInFormationAtSkillOffset(playersMatch.formationAwayTeam, whoskill, Enums.PlayerFormation.MidFielder, playersMatch.formationTypeAwayTeam, awayTeam);
        }
        else 
        { // score by defender
            float whoskill = Random.Range(0f, possessionDefence*2f);
            if (teamInPossession == TeamHome)
                scorerIndex = GetPlayerIndexInFormationAtSkillOffset(playersMatch.formationHomeTeam, whoskill, Enums.PlayerFormation.Defender, playersMatch.formationTypeHomeTeam, homeTeam);
            else
                scorerIndex = GetPlayerIndexInFormationAtSkillOffset(playersMatch.formationAwayTeam, whoskill, Enums.PlayerFormation.Defender, playersMatch.formationTypeAwayTeam, awayTeam);
        }

        if (scorerIndex == -1)
            playersMatch.scorerName = "Err Det Plyr";
        else
            playersMatch.scorerName = gameManager.staticPlayersData[scorerIndex].playerSurname;
        
        playerWithBallIndex = scorerIndex;
        return playerWithBallIndex;
    }

    private int GetPlayerIndexInFormationAtSkillOffset(int[] players, float targetSkill, Enums.PlayerFormation posMask, Enums.Formation formationType, int teamId)
    {
        int resultIndex = -1;
        float totalSkillPoints = 0;
        FormationData formationInfo = gameManager.formations[(int)formationType];
        for (int i = 0; i < GameManager.MaxPlayersInFormation; i++)
        {
            if ((formationInfo.formations[i].formation & posMask) != (Enums.PlayerFormation)0)
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
                    
                    float prevSkillPoints = totalSkillPoints;
                    totalSkillPoints += (starsRating * gameManager.dynamicPlayersData[dataIndex].condition)*outOfPositionScale;
                    // Does this skill range span the target amount?
                    if ((prevSkillPoints <= targetSkill) && (totalSkillPoints >= targetSkill))
                    {
                        resultIndex = dataIndex;
                        Debug.Assert(gameManager.dynamicPlayersData[resultIndex].teamId == teamId);
                        break;
                    }
                }
            }
        }
        return resultIndex;
    }


    private bool DetermineFouls()
    {
       bool result = false;
       // Neil suggested alternate checks, but lets try a 50/50 check
       if (Random.value <= 0.5f)
       {
           int aggressionRatio = 5;
           int chance = Random.Range(0, 11); // 11 = number of players in team
           if (chance <= aggressionRatio)
           {
               result = true;
               injuryTime += 0.5f;
           }
       }
       return result;
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
    private int GetSkillPointsForPlayersOnPitch(int[] players,  Enums.PlayerFormation posMask, Enums.Formation formationType)
    {
        float totalSkillPoints = 0;
        FormationData formationInfo = gameManager.formations[(int)formationType];
        for (int i = 0; i < GameManager.MaxPlayersInFormation; i++)
        {
            if ((formationInfo.formations[i].formation & posMask) != 0)
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
                    Debug.Log("Getting skill points for " + gameManager.dynamicPlayersData[dataIndex].name + gameManager.dynamicPlayersData[dataIndex].condition);
                    totalSkillPoints += (starsRating * gameManager.dynamicPlayersData[dataIndex].condition) * outOfPositionScale;
                }
            }
        }
        return (int)totalSkillPoints;
    }
    private int GetSkillPointsForPlayersOnPitch(int[] players, Enums.Formation formationType)
    {
        float totalSkillPoints = 0;
        FormationData formationInfo = gameManager.formations[(int)formationType];
        for (int i = 0; i < GameManager.MaxPlayersInFormation; i++)
        {
            int playerId = players[i];
            if (playerId != -1)
            {
                int dataIndex = GetPlayerDataIndexForPlayerId(playerId);
                Debug.Assert(dataIndex != -1);
                float outOfPositionScale = 1.0f;
                if (gameManager.CheckPlayerIndexIsHappyInFormation(dataIndex, formationInfo.formations[i]) == 0)
                    outOfPositionScale = 0.5f;
                float starsRating = gameManager.GetTeamLeagueAdjustedStarsRatingForPlayerIndex(dataIndex);
                if (gameManager.dynamicPlayersData[dataIndex].weeksBannedOrInjured != 0)
                    starsRating = 0.0f;
                totalSkillPoints += (starsRating * gameManager.dynamicPlayersData[dataIndex].condition)*outOfPositionScale;
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