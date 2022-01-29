using UnityEngine;

namespace DeformationSnow
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Static Data", order = 0)]
    public class PlayerData : ScriptableObject
    {
        public float shiftBoost = 100f;
        public float speed = 1400f;
        public float startBoost = 2500f;
    }
}