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
    private TMP_Text mText;
    [FormerlySerializedAs("_gameManager")]
    [SerializeField]
    GameManager gameManager;
    public int alignment;
    public uint flags;
    public string text;
    public Enums.MenuAction menuAction;
    public Action<object> OnClick;
    public int param;
    public void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>().GetComponent<GameManager>();
    }
    public virtual void AddListener(Enums.MenuAction action, int clickParam)
    { 
        this.param = clickParam;
        this.menuAction = action;
        Button onClickTarget = GetComponent<Button>();
        if (onClickTarget != null)
        {
            onClickTarget.onClick.AddListener(HandleClick);
        } else
        {
            Debug.LogError("No button component attached");
        }
    }
    public virtual void SetText(string newText)
    {
        this.text = newText;
        this.mText.text = newText;
        
    }
    public virtual void HandleClick()
    {
        switch (menuAction) {
            case Enums.MenuAction.SelectTeamAndCreateGame:
                gameManager.CreateGameUsingTeam(param, gameManager.playersScenario);
                break;
            }
    }

}
