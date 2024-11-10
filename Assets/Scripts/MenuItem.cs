using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;

public class MenuItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_Text;

    [SerializeField]
    private GameObject[] prefabs;

    int menuItemType;
    Vector2 position;
    int alignment;
    uint flags;
    string text;

    public MenuItem(int type, Vector2 pos, int align, uint flag, string text)
    {
        this.menuItemType = type;
        this.position = pos;
        this.alignment = align;
        this.flags = flag;
        this.text = text;

        Instantiate(prefabs[type]);
        MenuItem menuItem = GetComponent<MenuItem>();
        menuItem = this;

            
    }
}
