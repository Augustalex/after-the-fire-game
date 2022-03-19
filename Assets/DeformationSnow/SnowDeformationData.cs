using UnityEngine;

namespace DeformationSnow
{
    [CreateAssetMenu(fileName = "SnowDeformationData", menuName = "SnowDeformationData", order = 0)]
    public class SnowDeformationData : ScriptableObject
    {
        public int renderFrameLag = 11;
        public float interpolationVelocityMultiplier = 6f;
    }
}