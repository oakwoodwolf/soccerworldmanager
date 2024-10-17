using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Player;
using UnityEngine;

public class PlayersMatch : MonoBehaviour
{
    public const int MaxTeamnameLength = 32;
    [Header("Players Match Data")]

    [Header("Home")]
    public int HomeTeam;
    public Color HomeTeam1stColour;
    public Color HomeTeam2ndColour;
    public string HomeTeamName;
    public Enums.Formation FormationTypeHomeTeam;
    public int[] FormationHomeTeam = new int[14];

    [Header("Away")]
    public int AwayTeam;
    public Color AwayTeam1stColour;
    public Color AwayTeam2ndColour;
    public string AwayTeamName;
    public Enums.Formation FormationTypeAwayTeam;
    public int[] FormationAwayTeam = new int[14];

    public string ScourerName;
    public string FoulerName;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
