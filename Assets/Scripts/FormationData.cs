using UnityEngine;

    [CreateAssetMenu(fileName = "Formation_", menuName = "Formation", order = 0)]
    public class FormationData : ScriptableObject
    {
        public FormationInfo[] formations;
    }
