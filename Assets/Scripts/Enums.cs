using System;
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

    public enum Texture
    {
    Title = 0,
	Background,
	Lander,
	Base,
	MainThrust,
	LeftThrust,
	RightThrust,
	Explosion,
	FuelBar,
	FuelLevel,
	LightGreen,
	LightRed,
	LabelSpeed,
	LabelAngle,
	LabelPosition,

	MunchHead1,
	MunchHead1Closed,
	MunchHead1Urgh,
	MunchHead1Munch1,
	MunchHead1Munch2,
	ItemCake, 
	ItemSock, 
	ItemPizza,
	ItemDyno,
	ItemWorm,

	ItemTomato,
	ItemChilli,
	ItemBurger,


	StageComplete,
	GameOver,
	OutOfTime,
	TouchToContinue,

	PortionIcon,
	PortionIconFilled,

	MatchBg,
	Complete,
	
	ButtonPlayGame,
	ButtonPlayMarthathon,
	ButtonOptions,

	ButtonResumeGame,
	ButtonQuitGame,
	
	ButtonBackToMenu,
	
	GamePaused,
	Options,	
	
	ScoreAndLevel,
	
	Fade,
	
	AccelError,
	
	Lite,
	
	ColorTones,
	MotionBlur,

	MenuBar,
	MenuBarSelection,	
	MenuBarFade,
	
	ScrollBarBack,
	
	ButtonNext,
	Button96X32,
	
	Radio96X32,
	Radio96X32On,
	
	LogoIcon,
	
	ButtonEpl,
	ButtonEplMinus15Pnts,
	ButtonLeagueUsa,
	
	Pitch,
	
	Formation442,
	Formation424,
	Formation433,
	Formation532,
	Formation514,
	Formation352,
	Formation541,
	
	Stars1,
	Stars2,
	Stars3,
	Stars4,
	Stars5,
	
	TrainingResting,
	TrainingLight,
	TrainingNormal,
	TrainingIntensive,
	
	Pie10,
	Pie20,
	Pie30,
	Pie40,
	Pie50,
	Pie60,
	Pie70,
	Pie80,
	Pie90,
	Pie100,
	
	SellingNotListed,
	SellingFreeTransfer,
	SellingOffersAtValue,
	SellingAnyOffers,
	
	ArrowLeft,
	ArrowRight,
	
	ButtonCashBurner,
	ButtonGarbleAndStammer,
	ButtonNeveron,
	ButtonPrintsOfPersia,
	ButtonBlueSuedeChoux,
	ButtonGStringParade,
	ButtonFootLoose,
	ButtonOiK,
	ButtonSoaringCoasts,
	ButtonStressMart,
	ButtonBernieBerryAndCo,
	ButtonChroniclesOfNaan,
	ButtonGbtvRepeats,
	ButtonNfg,
	ButtonSiBurns,
	ButtonTentsAtmosphere,
	ButtonTheDogsBotox,
	ButtonThinkingOfEwe,
	ButtonWtfInsurance,
	ButtonMrMunchySnacks,
	ButtonError,
	
	ButtonTimewaste,
	ButtonFrustration,
	ButtonLocal,
	ButtonFluke,
	ButtonHometeam,
	ButtonSponsor,
	ButtonStalemate,
	ButtonGenius,
	ButtonClean,
	ButtonNocards,
	
	Cards1Yellow,
	Cards2Yellow,
	Cards1Red,
	
	ShirtTest,
	ShirtOutline,
	Shirt1Stcolour,
	Shirt2Ndcolour,
	
	FirstAid,
	
	ButtonNoMatchbreaker,
	
	SearchAppStore,
	
	KNumTextures
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
    public enum MenuElement {
        //	Terminate = -1,
        Null = 0,

        TextBar,
        TextBarHalf,           // half size/width version of TextBar
        TextBarHalfDouble,     // half size/width version of TextBar
        IconBar,
        Button,
        RadioButton,

        ShadedBox,
        StaticText,

        TitleBar,

        Graph,

        ScrollBar,

        Max
    };
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
        Usa = 50,
    }
[Flags]
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
        Stalemate = 4096, /// no goals scored for 30 mins (either team)
    }

    public enum Training
    {
        None = 0,
        Light,
        Normal,
        Intensive,
    }
    [Flags]
    public enum MenuElementFlag
    {
        HideItem				=(1<<0),
        ButtonSmallerFont		=(1<<1),
        MatchEngineTitleBar	=(1<<2),
        Size64TitleBarIcon	=(1<<3),
        HideTextItem			=(1<<4),	
        NotSelectable			=(1<<5),	
    }

    public enum SponsorID
    {
        None = -1,
        OgmInsurance = 0,   // lots of cash
        MrMunchySnacks,  // slowly climbing income
        FootLoose,       // random income?
        Max
    }
    public enum Formation
    {
        KFormation442 = 0,
        KFormation424,
        KFormation433,
        KFormation532,
        KFormation514,
        KFormation352,
        KFormation541,
        KFormationMax
    }

    public enum Sounds
    {
        Start = 0,
        Success,
        Chomp,
        Belch,
        Hic,
        Bomb,
        Toot,
        Splat,
        CrowdAw,
        Eat,	
        DropItem,	
        LevelUp,	
        MenuClick,	
        UseMatchBreaker,				// pressing the button
        MatchBreakerEffect,			// when something happens during the mysterious match....
        BadInput,					// user tries to do something they can't do - such as buy a matchbreaker beyond budget
        Crowd1,
        Crowd2,
        Crowd3,
        Crowd_Goal,
        Crowd_MissedGoal,
        Whistle_Foul,
        Whistle_EndHalf,
        Whistle_EndGame,
        Whistle,
    }

    [Flags]
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
