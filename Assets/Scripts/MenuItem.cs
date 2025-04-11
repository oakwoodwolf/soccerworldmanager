using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    [Header("References")]
    public Vector2 pos;

    [FormerlySerializedAs("m_Text")]
    [SerializeField]
    public TMP_Text mText;
    [FormerlySerializedAs("_gameManager")]
    [SerializeField]
    protected GameManager gameManager;
    [Header("Values")]
    public Enums.MenuElement type;
    public int alignment;
    public Enums.MenuElementFlag flags;
    public string text;
    public Enums.MenuAction menuAction;
    public Action<object> OnClick;
    protected Button Button;
    public int param;
    protected RectTransform RectTransform;
    
    [Header("Scroll")]
    public bool affectedByScroll;
    public virtual void Awake()
    {
        Button = GetComponent<Button>();
        RectTransform = GetComponent<RectTransform>();
        gameManager = FindFirstObjectByType<GameManager>().GetComponent<GameManager>();
        AddListener(menuAction, param);
    }
    public virtual void AddListener(Enums.MenuAction action, int clickParam)
    { 
        this.param = clickParam;
        this.menuAction = action;
        Button = GetComponent<Button>();
        if (Button != null)
        {
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(HandleClick);
            //Debug.Log("Listener Attached");
        }
    }

    public virtual void OnValidate()
    {
        AdjustPosition();
        switch (type)
        {
            case Enums.MenuElement.ShadedBox:
                break;
            case Enums.MenuElement.ScrollBar:
                break;
            case Enums.MenuElement.Null:
                break;
            default:
                if (mText.text != text)
                {
                    mText.text = text;
                }
                break;
        }
        
    }
    
    /// <summary>
    /// This uses the position of menuItem to set Unity's RectTransform, allowing you to feed the old position values into Unity
    /// </summary>
    protected void AdjustPosition()
    {
        if (RectTransform == null)
            RectTransform = GetComponent<RectTransform>();
        if (!Mathf.Approximately(RectTransform.transform.position.x, pos.x) || !Mathf.Approximately(RectTransform.transform.position.y, -pos.y))
        {
            RectTransform.anchorMax = new Vector2(0,1);
            RectTransform.anchorMin = new Vector2(0,1);
            RectTransform.anchoredPosition = new Vector3(pos.x, -pos.y, RectTransform.transform.position.z);
        }

    }

    public virtual void Update()
    {
        if (!affectedByScroll) return;
        RectTransform.anchoredPosition = new Vector3(pos.x, -pos.y-gameManager.menuScrollY, RectTransform.transform.position.z);
    }

    public virtual void SetText(string newText)
    {
        text = newText;
        if (mText != null)
        {
            mText.text = newText;
            switch (alignment)
            {
                case 2:  mText.alignment = TextAlignmentOptions.Top; break;
                case 1:  mText.alignment = TextAlignmentOptions.TopRight; break;
            }
                 
        }
        
    }

    public void OnDestroy()
    {
        if (Button != null) {Button.onClick.RemoveAllListeners();}
    }

    public virtual void HandleClick()
    {
        if (flags != Enums.MenuElementFlag.HideItem)
        {
             Debug.Log($"Button clicked with action: {menuAction}, param: {param}");
            if (menuAction != Enums.MenuAction.Null) gameManager.SoundEngine_StartEffect(Enums.Sounds.MenuClick);
            switch (menuAction) 
            {
                case Enums.MenuAction.SelectTeamAndCreateGame:
                    gameManager.CreateGameUsingTeam(param, gameManager.playersScenario);
                    break;
                case Enums.MenuAction.GotoMenu:
                    gameManager.GoToMenu(param);
                    break;
                case Enums.MenuAction.PrepareChooseTeam:
                    gameManager.PrepareChooseTeamMenu(param);
                    break;
                case Enums.MenuAction.LoadAndContinueGame:
                    gameManager.LoadAndPrepareGame();
                    break;
                case Enums.MenuAction.CallFunction:
                    gameManager.StartGame();
                    break;
                case Enums.MenuAction.CyclePlayerTraining:
                    int playerIndex = param;
                    playerIndex += gameManager.currentPage * GameManager.MaxPlayersInList;
                    int playerId = gameManager.playersTeamPlayerIds[playerIndex];
                    int dataIndex = gameManager.GetPlayerDataIndexForPlayerID(playerId);
                    int training = gameManager.dynamicPlayersData[dataIndex].trainingTransfer & GameManager.trainingMask;
                    training++;
                    if (training > (int)Enums.Training.Intensive)
                        training = (int)Enums.Training.None;
                    gameManager.dynamicPlayersData[dataIndex].trainingTransfer &= GameManager.transferMask;
                    gameManager.dynamicPlayersData[dataIndex].trainingTransfer |= training;
                    break;
                case Enums.MenuAction.CyclePlayerTransferStatus:
                    break;
                case Enums.MenuAction.BuyPlayerReview:
                    break;
                case Enums.MenuAction.BuyPlayerUpdateOffer:
                    int decinc = param;
                    int change = 1;
                    if (gameManager.currentBuyPlayerOffer > 9)
                        change = 5;
                    if (gameManager.currentBuyPlayerOffer > 99)
                        change = 10;
                    if (gameManager.currentBuyPlayerOffer > 999)
                        change = 100;
                    if (gameManager.currentBuyPlayerOffer > 9999)
                        change = 1000;
                    if (gameManager.currentBuyPlayerOffer > 99999)
                        change = 10000;
                    gameManager.currentBuyPlayerOffer += (decinc * change);
                    
                    int availableCash = gameManager.GetTeamCashBalance(gameManager.playersTeam);
                    if (gameManager.currentBuyPlayerOffer < 0)
                        gameManager.currentBuyPlayerOffer = 0;
                    if (gameManager.currentBuyPlayerOffer > availableCash)
                        gameManager.currentBuyPlayerOffer = availableCash;
                    gameManager.GoToMenu(Enums.Screen.BuyPlayerOffer);
                    break;
                case Enums.MenuAction.UpdateCurrentPage:
                    gameManager.currentPage += param;
                    if (gameManager.currentPage < 0)
                        gameManager.currentPage = gameManager.currentNumberOfPage - 1;
                    if (gameManager.currentPage > gameManager.currentNumberOfPage - 1)
                        gameManager.currentPage = 0;
                    gameManager.GoToMenu(gameManager.currentScreen);
                    break;
                case Enums.MenuAction.SetCurrentPage:
                    gameManager.currentPage = param;
                    gameManager.GoToMenu(gameManager.currentScreen);
                    break;
                case Enums.MenuAction.AssignSponsor:
                    gameManager.BuySponsor(param);
                    break;
                case Enums.MenuAction.BuyMatchbreaker:
                    gameManager.BuyMatchbreaker(param);
                    break;
                case Enums.MenuAction.UseMatchBreaker:
                    if (gameManager.playersMatchBreaker != -1)
                    {
                        if (gameManager.matchEngine.homeTeam == gameManager.playersTeam)
                        {
                            gameManager.matchEngine.homeTeamMatchBreakerFlags = gameManager.matchbreakerInfo[gameManager.playersMatchBreaker].flags;
                            gameManager.matchEngine.homeTeamMatchBreakerActivationTurn = gameManager.matchEngine.turn;
                        }
                        else if (gameManager.matchEngine.awayTeam == gameManager.playersTeam)
                        {
                            gameManager.matchEngine.awayTeamMatchBreakerFlags = gameManager.matchbreakerInfo[gameManager.playersMatchBreaker].flags;
                            gameManager.matchEngine.awayTeamMatchBreakerActivationTurn = gameManager.matchEngine.turn;
                        }
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.UseMatchBreaker);
                    }
                    break;
                case Enums.MenuAction.PrepareFormation:
                    gameManager.formationType = (Enums.Formation)param;
                    gameManager.GoToMenu(Enums.Screen.AssignPlayers);
                    break;
                case Enums.MenuAction.AssignPlayerToFormation:
                    int playerFormIndex = param;
                    int playerFormId = gameManager.playersTeamPlayerIds[playerFormIndex];
                    // As we're clicking to assign a player to the formation, set the substate back to showing a formation
                    gameManager.currentScreenSubState = 0; //kAssignPlayersSubState_ShowFormation
                    // Check if we're assigning players before or during a match
                    if (gameManager.matchEngine.state == Enums.MatchEngineState.MatchOver)
                    {
                        gameManager.AssignPlayerToFormation(playerFormId, gameManager.formationCycle);
                        gameManager.GoToMenu(Enums.Screen.AssignPlayers);
                    }
                    else // during match
                    {
                       int formIndex = gameManager.IsPlayerIdInFormation(playerFormId);
                       if (formIndex != -1)
                       {
                           int currentPlayerIdAtPosition = gameManager.playersInFormation[gameManager.formationCycle];
                           //Assign available player
                           gameManager.AssignPlayerToFormation(playerFormId, gameManager.formationCycle);
                           // Swap old player
                           gameManager.AssignPlayerToFormation(currentPlayerIdAtPosition, formIndex);
                           gameManager.GoToMenu(Enums.Screen.AssignPlayers);
                       }
                    }
                    break;
                case Enums.MenuAction.CheckTeamBeforeGotoMenu:
                    bool ok = false;
                    if (gameManager.matchEngine.state == Enums.MatchEngineState.MatchOver)
                    {
                        ok = true;
                    }
                    else
                    {
                        if (gameManager.matchEngine.homeTeam == gameManager.playersTeam)
                        {
                            if (gameManager.CountLegalPlayersOnPitchInSquad(gameManager.playersMatch.formationHomeTeam) <= gameManager.matchEngine.maxHomeTeamPlayersOnPitch)
                                ok = true;
                        }
                        else
                        {
                            if (gameManager.CountLegalPlayersOnPitchInSquad(gameManager.playersMatch.formationAwayTeam) <= gameManager.matchEngine.maxAwayTeamPlayersOnPitch)
                                ok = true;
                        }
                    }
                    if (ok)
                        gameManager.GoToMenu(param);
                    else
                    {
                        gameManager.SoundEngine_StartEffect(Enums.Sounds.BadInput);
                    }
                    break;
                case Enums.MenuAction.RadioSelectOptions:
                    switch (param)
                    {
                        case 0: gameManager.SFXEnabled = false; break;
                        case 1:
                            gameManager.SFXEnabled = true;
                            gameManager.SoundEngine_StartEffect(Enums.Sounds.Splat);
                            break;
                        case 2: gameManager.VibrationEnabled = false; break;
                        case 3: gameManager.VibrationEnabled = true;
                            Handheld.Vibrate(); break;
                    }
                    gameManager.SetOptionsRadioButtons(gameManager.currentScreenDefinition.MenuItems.GetComponentsInChildren<MenuItem>());
                    break;
                case Enums.MenuAction.RadioSelectMatchBalance:
                    MenuItem[] items = gameManager.currentScreenDefinition.MenuItems.GetComponentsInChildren<MenuItem>();
                    gameManager.ResetMenuRadioButtons(items);
                    gameManager.SetMenuRadioButtonsAtOccurance(items,param);
                    gameManager.playersMatchStrategy = (Enums.MatchStrategy)param;
                    
                    break;
                case Enums.MenuAction.ProcessSkipMatch:
                    while (gameManager.matchEngine.state != Enums.MatchEngineState.MatchOver)
                        gameManager.matchEngine.Render(8.0f,true);
                    gameManager.GoToMenu(Enums.Screen.ProcessMatchData);
                    break;
                case Enums.MenuAction.ProcessResetLeagueForNextYear:
                    if (gameManager.playersYearsToRetire > 0)
                        gameManager.playersYearsToRetire--;
                    gameManager.PrepareNextYearOfPlay();
                    break;
                case Enums.MenuAction.OpenSafari:
                    break;
                case Enums.MenuAction.Max:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
       
    }

    public virtual void HideItem(bool hide)
    {
        if (hide)
        {
            flags |= Enums.MenuElementFlag.HideItem;
            Image[] list = GetComponentsInChildren<Image>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].enabled = false;
            }
            mText?.gameObject.SetActive(false);
        }
        else
        {
            flags &= ~Enums.MenuElementFlag.HideItem;
            Image[] list = GetComponentsInChildren<Image>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].enabled = true;
            }
            mText?.gameObject.SetActive(true);
        }
    }

    public override string ToString()
    {
        return type.ToString() + " " +text;
    }
}
