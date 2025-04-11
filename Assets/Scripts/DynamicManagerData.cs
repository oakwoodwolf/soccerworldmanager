using UnityEngine;

public class DynamicManagerData : ScriptableObject
{
    public int teamId;

    public void LoadDynamicManagerData(string[] txtData)
    {
        teamId = int.Parse(txtData[1]);
    }
}
