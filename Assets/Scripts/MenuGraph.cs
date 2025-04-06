using UnityEngine;


    [CreateAssetMenu(fileName = "MenuGraph", menuName = "Scriptable Objects/Graph")]
    public class MenuGraph : ScriptableObject
    {
        [SerializeField]
        public Vector2 size;
        public Vector2 defaultMinMax;
        [SerializeField]
        public uint flags;
        public int numInArray;
        public int[] array;
        public string title;
    }
