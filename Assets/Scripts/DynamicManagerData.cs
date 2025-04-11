using UnityEngine;

[System.Serializable]
public class DynamicManagerData : ScriptableObject
{
    public int managerId;
    public int teamId;

    public void LoadDynamicManagerData(string[] txtData)
    {
        teamId = int.Parse(txtData[1]);
    }
}
