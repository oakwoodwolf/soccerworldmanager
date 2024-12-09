using UnityEngine;


    [CreateAssetMenu(fileName = "Formation_", menuName = "Formation Info", order = 0)]
    public class FormationInfo : ScriptableObject
    {
        [SerializeField]
        public Vector2 pos;
        [SerializeField]
        public Enums.PlayerFormation formation;
      

        [SerializeField] public int value;
        private void OnValidate()
        {
            // Live calculate whenever a property changes in the Inspector
            value = (int)(formation);
        }
    }
