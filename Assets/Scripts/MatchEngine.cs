using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Player;
using UnityEngine;

public class MatchEngine : MonoBehaviour
{
    public const int MaxPlayersInATeam = 64;
    [Header("Match Engine Data")]
    public Enums.MatchEngineState State;
    public int Turn;
    [Tooltip("How much time has been spent addressing fouls/injurys and other delays?")]
    public float InjuryTime;
    [Tooltip("Any extra time in minutes sourced from injury time")]
    public int ExtraTime;
    [Tooltip("Counts time between turns")]
    public float UpdateTimer;
    [Tooltip("So we can process each step within a turn - so messages don't appear at once")]
    public Enums.MatchEngineSubState SubTurnState;
    [Tooltip("Who has the ball.")]
    public int TeamInPossession;
    
    [Header("Home")]
    [Tooltip("Goals for home team")]
    public int HomeTeamScore;
    [Tooltip("-1,0,+1 to multiple with strategy/balance factors")]
    public int HomeStrategyBalance;
    public int HomeTeam;
    public int HomeTeamMatchBreakerFlags;
    public int HomeTeamMatchBreakerActivationTurn;
    public int MaxHomeTeamPlayersOnPitch;

    [Header("Away")]
    [Tooltip("Goals for away team")]
    public int AwayTeamScore;
    [Tooltip("-1,0,+1 to multiple with strategy/balance factors")]
    public int AwayStrategyBalance;
    public int AwayTeam;
    public int AwayTeamMatchBreakerFlags;
    public int AwayTeamMatchBreakerActivationTurn;
    public int MaxAwayTeamPlayersOnPitch;

    public int TurnTimeMultiplier;
    public int TurnTimeOffset;
    public int TurnsInPossession;
    public float FoulDamageDone;
    public int IndexOfPlayerCommitingFoul;
    public int PlayerWithBallIndex;
    [Tooltip("keep track of the yOff, so we can scroll up text if needed")]
    public float MatchStatusYOffset;
    public int ItemsInQuickPlayerList;
    [Tooltip("a shorter list to aid in faster player data index determination")]
    public IdToIndex[] QuickPlayerIdToIndexList = new IdToIndex[MaxPlayersInATeam*2];

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
