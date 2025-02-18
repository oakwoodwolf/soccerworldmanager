using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.Serialization;

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

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
