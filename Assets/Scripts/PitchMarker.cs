using UnityEngine;

public class PitchMarker : MenuItem
{
   
    public RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public override void HandleClick()
    {
        if (gameManager.currentScreen == Enums.Screen.AssignPlayers)
        {
            gameManager.currentScreenSubState = 1; //select player
            gameManager.formationCycle = param;
            gameManager.SwapFormationCycle();
        }
        
    }
    
}
