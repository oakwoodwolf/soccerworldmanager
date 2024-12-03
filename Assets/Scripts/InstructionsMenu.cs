using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class InstructionsMenu : MonoBehaviour
{
    [FormerlySerializedAs("GameManager")]
    public GameManager gameManager;
    public const int MaxInstructions = 4;
    [FormerlySerializedAs("InstructionScreen")]
    [Range(1, MaxInstructions)]
    public int instructionScreen = 1;

    public TMP_Text instructions;
    [TextArea(5,8)]
    public string[] instructionString = new string[MaxInstructions];
    // Start is called before the first frame update
    void Awake()
    {
        SetInstructionScreen(1);
    }

    public void SetInstructionScreen(int screen)
    {
        instructionScreen = screen;
        instructions.text = instructionString[screen - 1];
    }
    private void Exit()
    {
        SetInstructionScreen(1);
        gameManager.GoToMenu(Enums.Screen.Title);
    }
    public void OnClick(bool reverse)
    {
        int screenToGo = instructionScreen;
        if (reverse) //left
        {
            screenToGo--;
            if (screenToGo < 1)
            {
                Exit();
            }
            else
            {
                SetInstructionScreen(screenToGo);
            }
        }
        else //right
        {
            screenToGo++;
            if (screenToGo > MaxInstructions)
            {
                Exit();
            }
            else
            {
                SetInstructionScreen(screenToGo);
            }
        }
    }
}
