using UnityEngine;
using static Enums;

public class DynamicPlayerData : ScriptableObject
{
    public float starsRating;
    public float condition;
    public short teamId;
    public short trainingTransfer;
    public short weeklySalary;
    public short morale;
    public short weeksBannedOrInjured;
    public ushort flags;

    public void LoadDynamicPlayerData(string[] txtData)
    {
        teamId = short.Parse(txtData[1]);
        starsRating = float.Parse(txtData[4]);
    }
}
