using UnityEngine;
using static Enums;

public class DynamicPlayerData : ScriptableObject
{
    public float starsRating;
    public float condition;
    public short teamId;
    public int trainingTransfer;
    public short weeklySalary;
    public short morale;
    public short weeksBannedOrInjured;
    public ushort flags;

    public void LoadDynamicPlayerData(string[] txtData)
    {
        teamId = short.Parse(txtData[1]);
        condition = GameManager.MaxPlayerCondition;
        morale = 0;
        weeksBannedOrInjured = 0;
        trainingTransfer = (int)Enums.Training.Normal | ((int)(Enums.TransferStatus.NotListed) << 8);
        flags = 0;
        starsRating = float.Parse(txtData[4]);
    }
}
