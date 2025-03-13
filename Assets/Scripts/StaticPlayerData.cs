using UnityEngine;

public class StaticPlayerData : ScriptableObject
{
    public int playerId;
    public Enums.PlayerFormation playerPositionFlags;
    public string playerSurname;

    public void LoadStaticPlayerData(string[] txtData)
    {
        playerId = int.Parse(txtData[0]);
        playerSurname = txtData[2];
        name = playerSurname;
        switch (txtData[3]) // Handle positions, stored as chars in the file, must be turned into ints!
        {
            case "G":
                playerPositionFlags |= Enums.PlayerFormation.Goalkeeper;
                break;
            case "D":
                playerPositionFlags |= Enums.PlayerFormation.Defender;
                break;
            case "M":
                playerPositionFlags |= Enums.PlayerFormation.MidFielder;
                break;
            case "S":
                playerPositionFlags |= Enums.PlayerFormation.Attacker;
                break;
            case "F":
                playerPositionFlags |= Enums.PlayerFormation.Attacker;
                break;
            case "L":
                playerPositionFlags |= Enums.PlayerFormation.Left;
                break;
            case "C":
                playerPositionFlags |= Enums.PlayerFormation.Center;
                break;
            case "R":
                playerPositionFlags |= Enums.PlayerFormation.Right;
                break;
        }
    }
}
