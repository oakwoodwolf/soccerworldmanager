using UnityEngine;


    [CreateAssetMenu(fileName = "Formation_", menuName = "Formation Info", order = 0)]
    public class FormationInfo : ScriptableObject
    {
        [SerializeField]
        private Vector2 pos;
        [SerializeField]
        private Enums.PlayerFormation formation;
      

        [SerializeField] public int value;
        private void OnValidate()
        {
            // Live calculate whenever a property changes in the Inspector
            value = (int)(formation);
        }
    }
