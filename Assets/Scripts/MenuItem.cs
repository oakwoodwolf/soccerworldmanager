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
    [FormerlySerializedAs("m_Text")]
    [SerializeField]
    protected TMP_Text mText;
    [FormerlySerializedAs("_gameManager")]
    [SerializeField]
    GameManager gameManager;
    public int alignment;
    public uint flags;
    public string text;
    public Enums.MenuAction menuAction;
    public Action<object> OnClick;
    protected Button Button;
    public int param;
    public void Awake()
    {
        Button = GetComponent<Button>();
        gameManager = FindFirstObjectByType<GameManager>().GetComponent<GameManager>();
    }
    public virtual void AddListener(Enums.MenuAction action, int clickParam)
    { 
        this.param = clickParam;
        this.menuAction = action;
        Button = GetComponent<Button>();
        if (Button != null)
        {
            Button.onClick.AddListener(HandleClick);
            Debug.Log("Listener Attached");
        } else
        {
            Debug.LogError("No button component attached");
        }
    }
    public virtual void SetText(string newText)
    {
        this.text = newText;
        if (this.mText != null) {this.mText.text = newText;}
        
    }

    public void OnDestroy()
    {
        Button.onClick.RemoveAllListeners();
    }

    public virtual void HandleClick()
    {
        Debug.Log($"Button clicked with action: {menuAction}, param: {param}");

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
            case Enums.MenuAction.CyclePlayerTraining:
                break;
            case Enums.MenuAction.CyclePlayerTransferStatus:
                break;
            case Enums.MenuAction.SetCurrentPage:
                break;
            case Enums.MenuAction.BuyPlayerReview:
                break;
            case Enums.MenuAction.BuyPlayerUpdateOffer:
                break;
            case Enums.MenuAction.UpdateCurrentPage:
                break;
            case Enums.MenuAction.AssignSponsor:
                break;
            case Enums.MenuAction.BuyMatchbreaker:
                break;
            case Enums.MenuAction.UseMatchBreaker:
                break;
            case Enums.MenuAction.PrepareFormation:
                break;
            case Enums.MenuAction.AssignPlayerToFormation:
                break;
            case Enums.MenuAction.CheckTeamBeforeGotoMenu:
                break;
            case Enums.MenuAction.RadioSelectOptions:
                break;
            case Enums.MenuAction.RadioSelectMatchBalance:
                break;
            case Enums.MenuAction.ProcessSkipMatch:
                break;
            case Enums.MenuAction.ProcessResetLeagueForNextYear:
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
