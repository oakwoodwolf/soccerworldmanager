using System.Collections.Generic;
/// <summary>
/// Altenative to List<DynamicPlayerData> that actually works with the jsonUtility.
/// </summary>
[System.Serializable]
public class PlayerSave
{
    public List<SavePlayerData> data;
}

