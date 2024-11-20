using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;

public class MenuItemGenerator : MonoBehaviour
{

    [SerializeField]
    private GameObject[] prefabs = new GameObject[(int)Enums.MenuElement.Max];

    public void GenerateMenuItem(Transform screen, Enums.MenuElement type, Vector2 pos, int align, uint flag, string text)
    {

        GameObject newobj = Instantiate(prefabs[(int)type]);
        newobj.transform.SetParent(screen, false);
        newobj.transform.position = pos;
        
        MenuItem menuItem = newobj.GetComponent<MenuItem>();
        menuItem.text = text;

    }
}
