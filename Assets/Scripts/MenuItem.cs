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
    protected TMP_Text mText;
    [FormerlySerializedAs("_gameManager")]
    [SerializeField]
    GameManager gameManager;
    [Header("Values")]
    public Enums.MenuElement type;
    public int alignment;
    public uint flags;
    public string text;
    public Enums.MenuAction menuAction;
    public Action<object> OnClick;
    protected Button Button;
    public int param;
    protected RectTransform RectTransform;
    
    [Header("Scroll")]
    public bool affectedByScroll;
    public void Awake()
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
            Debug.Log("Listener Attached");
        }
    }

    public virtual void OnValidate()
    {
        if (mText.text != text)
        {
            mText.text = text;
        }
        AdjustPosition();
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

    public void Update()
    {
        if (!affectedByScroll) return;
        RectTransform.anchoredPosition = new Vector3(pos.x, -pos.y-gameManager.menuScrollY, RectTransform.transform.position.z);
    }

    public virtual void SetText(string newText)
    {
        text = newText;
        if (mText != null) {mText.text = newText;}
        
    }

    public void OnDestroy()
    {
        if (Button != null) {Button.onClick.RemoveAllListeners();}
    }

    public virtual void HandleClick()
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
                break;
            case Enums.MenuAction.CyclePlayerTransferStatus:
                break;
            case Enums.MenuAction.BuyPlayerReview:
                break;
            case Enums.MenuAction.BuyPlayerUpdateOffer:
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
                break;
            case Enums.MenuAction.PrepareFormation:
                break;
            case Enums.MenuAction.AssignPlayerToFormation:
                break;
            case Enums.MenuAction.CheckTeamBeforeGotoMenu:
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
                    case 3: gameManager.VibrationEnabled = true; break;
                }
                this.flags = 1;
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
