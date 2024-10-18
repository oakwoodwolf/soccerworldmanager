using UnityEngine;

public class Enums : MonoBehaviour
{
    public enum Screen
    {
        Title, // formerly Test
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
    public enum MenuAction { 
        Null = 0,
        GotoMenu,
        CallFunction,
        PrepareChooseTeam,
        SelectTeamAndCreateGame,
        LoadAndContinueGame,
        CyclePlayerTraining,
        CyclePlayerTransferStatus,
        SetCurrentPage,
        BuyPlayerReview,
        BuyPlayerUpdateOffer,
        UpdateCurrentPage,
        AssignSponsor,
        BuyMatchbreaker,
        UseMatchBreaker,
        PrepareFormation,
        AssignPlayerToFormation,
        CheckTeamBeforeGotoMenu,        // TODO - validates submitted team then returns to match - if appropriate 
        RadioSelectOptions,
        RadioSelectMatchBalance,        // handle radio buttons for defensive/balanced/attacking play
        ProcessSkipMatch,               // TODO - handle skipping (finishing) a match, then do a GotoMenu to the post match screen
        ProcessResetLeagueForNextYear,  // TODO - If I do this as an action it might be easier to adjust it to whatever I need to do??? 

        OpenSafari,                     // open Gavin Wade search on iTunes?

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

    public enum MatchBreakerFlags
    {
        GuaranteedPossession20Mins = 1,
        HomeTeamSupport = 2,
        MaximumAttendence = 4,
        Frustration = 16,
        FlukeORama = 64,
        ExtraExtraTime = 128,
        ImmunityToCards = 256,
        ImmunityToInjury = 512,
        SponsorBonus = 1024,
        Stalemate = 4096,
    }

    public enum SponsorID
    {
        None = -1,
        OGMInsurance = 0,   // lots of cash
        MrMunchySnacks,  // slowly climbing income
        FootLoose,       // random income?
        MAX
    }
    public enum Formation
    {
        kFormation442 = 0,
        kFormation424,
        kFormation433,
        kFormation532,
        kFormation514,
        kFormation352,
        kFormation541,
        kFormationMAX
    }

    public enum PlayerFormation
    {
        Goalkeeper  = (1 << 0),
        Defender    = (1 << 1),
        MidFielder  = (1 << 2),
        Attacker    = (1 << 3),
        Substitute  = (1 << 4),
        Left        = (1 << 5),
        Right       = (1 << 6),
        Center      = (1 << 7),
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
