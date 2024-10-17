using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructionsMenu : MonoBehaviour
{
    public GameManager GameManager;
    public const int maxInstructions = 4;
    [Range(1, maxInstructions)]
    public int InstructionScreen = 1;

    public TMP_Text instructions;
    [TextArea(5,8)]
    public string[] instructionString = new string[maxInstructions];
    // Start is called before the first frame update
    void Awake()
    {
        SetInstructionScreen(1);
    }

    public void SetInstructionScreen(int screen)
    {
        InstructionScreen = screen;
        instructions.text = instructionString[screen - 1];
    }
    private void Exit()
    {
        SetInstructionScreen(1);
        GameManager.GoToMenu(Enums.Screen.Title);
    }
    public void OnClick(bool reverse)
    {
        int screenToGo = InstructionScreen;
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
            if (screenToGo > maxInstructions)
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
