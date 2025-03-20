using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayersMatch : MonoBehaviour
{
    public const int MaxTeamnameLength = 32;
    [FormerlySerializedAs("HomeTeam")]
    [Header("Players Match Data")]

    [Header("Home")]
    public int homeTeam;
    [FormerlySerializedAs("HomeTeam1stColour")]
    public Color homeTeam1StColour;
    [FormerlySerializedAs("HomeTeam2ndColour")]
    public Color homeTeam2NdColour;
    [FormerlySerializedAs("HomeTeamName")]
    public string homeTeamName;
    [FormerlySerializedAs("FormationTypeHomeTeam")]
    public Enums.Formation formationTypeHomeTeam;
    [FormerlySerializedAs("FormationHomeTeam")]
    public int[] formationHomeTeam = new int[14];

    [FormerlySerializedAs("AwayTeam")]
    [Header("Away")]
    public int awayTeam;
    [FormerlySerializedAs("AwayTeam1stColour")]
    public Color awayTeam1StColour;
    [FormerlySerializedAs("AwayTeam2ndColour")]
    public Color awayTeam2NdColour;
    [FormerlySerializedAs("AwayTeamName")]
    public string awayTeamName;
    [FormerlySerializedAs("FormationTypeAwayTeam")]
    public Enums.Formation formationTypeAwayTeam;
    [FormerlySerializedAs("FormationAwayTeam")]
    public int[] formationAwayTeam = new int[14];

    [FormerlySerializedAs("scourerName")] [FormerlySerializedAs("ScourerName")]
    public string scorerName;
    [FormerlySerializedAs("FoulerName")]
    public string foulerName;
    

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
