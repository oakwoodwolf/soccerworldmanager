using UnityEngine;

public class StaticManagerData : ScriptableObject
{
    public int managerId;
    public float styleOffset;
    public string managerSurname;

    public void LoadStaticManagerData(string[] txtData)
    {
        managerId = int.Parse(txtData[0]);
        managerSurname = txtData[2];
        styleOffset = int.Parse(txtData[3]);
        name = managerSurname;
    }
}
