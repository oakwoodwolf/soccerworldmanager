using UnityEngine;

[CreateAssetMenu(fileName = "MenuButton", menuName = "Scriptable Objects/Button")]

    public class MenuButton : ScriptableObject
    {
        public Sprite texture;
        public Sprite textureSelected;
        public Vector2 offset;
        public Vector2 size;
        [Tooltip("if 1, it is a radio button that is on")]
        public int flags;
        public string text;
    }
