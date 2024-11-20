using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_Text;
    GameManager gameManager;
    public int alignment;
    public uint flags;
    public string text;

    public void UpdateItem(Enums.MenuAction clickListener, int clickParam)
    { 
        gameManager = FindFirstObjectByType<GameManager>();
        Button onClickTarget = GetComponent<Button>();
        UnityEngine.Events.UnityAction call = null;
        switch (clickListener) {
            case Enums.MenuAction.GotoMenu:
                onClickTarget.onClick.AddListener(delegate { gameManager.GoToMenu(clickParam); });
                break;
            default:
                break;
        }
        
    }


}
