using UnityEngine;

public class Enums : MonoBehaviour
{
    public enum Screen
    {
        Test,
        Confirm,
        Instructions,
        Instructions2,
        Instructions3,
        Instructions4,
        Instructions5,
        Instructions6,
        Options,
        EditData,
        ChooseLeague,	
        ChooseTeam,	
        PreTurn,		// status overview?
        Standings,		// league table (or as appropriate)
        WeekPreview,	// preview of matches to be played th
        OtherBusiness,
        BuyPlayers,		// 
        BuyPlayerOffer,	//
        BuyPlayerConfirmOffer,	//
        SellPlayers,	// 
        TrainPlayers,	// Player training menu
        AssignSponsor,		//
        BuyMatchBreaker,	//
        PreMatchReview,
        OppositionFormation, // TODO - show opposition format
        SelectFormation,
        AssignPlayers,
        ConfirmGotoMatch,	
        MatchEngine,
        ProcessMatchData,	
        PostMatchReview,
        TransferOffers,		
        TransferConfirm,
        EndOfTurn,
        LeagueFinished,		
        ProcessLeagueFinish,
        LoadGameError,		
        Max
    }
    public enum MatchStrategy { Defensive, Balanced, Attacking }
    public enum State
    {
        StandBy,
        Running,
        Success,
        Failure,
        PreLevelPrompt,
        PostLevelScreen,

        ShowTitleScreen,
        EnterHighScore,
        DisplayHighScore,
        OptionsScreen,
        AccelerometerFailure,
        GameCompletedScreen
    }
    public enum SubState
    {
        InGame,
        Paused,
        DisplayingLevelComplete,
        DisplayingGameOver,
        DisplayingOutOfTime
    }
    public enum MatchEngineState
    {
        StartFirstHalf,
        InFirstHalf,
        EndFirstHalf,
        StartSecondHalf,
        InSecondHalf,
        ExtraTime,
        MatchOver
    }
    public enum MatchEngineSubState
    {
        DeterminePossession,
        FoulCheck,
        ProcessFoul,
        PromptFormationFix,
        RefIssueCard,
        TakingPenalty,
        DetermineGoalFromPenalty,
        DetermineShooting,
        DetermineGoal,
        RestartPlayAfterGoal,
        TakeFreeKick,
        AdvanceTurn,
        Max
    }

    public enum LeagueID
    {
        None = -1,
        Premium = 0,
        Chumpionship,
        Scotland = 10,
        USA = 50,
    }

    public enum SponsorID
    {
        None = -1,
        OGM_Insurance = 0,   // lots of cash
        MrMunchySnacks,  // slowly climbing income
        FootLoose,       // random income?
        MAX
    }
    public enum Formation
    {
        kFormation_442 = 0,
        kFormation_424,
        kFormation_433,
        kFormation_532,
        kFormation_514,
        kFormation_352,
        kFormation_541,
        kFormation_MAX
    }
    public enum GameMode
    {
        Standard,
        Marathon,
    }
    public enum ManagementStyle
    {
        VeryDefensive = -2,
        Defensive = -1,
        Balanced = 0,
        Attacking = 1,
        StrongAttacking = 2,
    }
}
