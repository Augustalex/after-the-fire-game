using UnityEngine;

namespace DeformationSnow
{
    [CreateAssetMenu(fileName = "GenerationData", menuName = "Static Data", order = 0)]
    public class GenerationData : ScriptableObject
    {
        public float heightOffset = -1f;
        
        public float cellNoiseScale = 12f;
        public float cellNoiseAmplitude = 4f;
        public float cellNoiseOffset = -1f;
        
        public float perlinNoiseScale = .05f;
        public float perlinNoiseAmplitude = 2f;
        public float perlinNoiseOffset = 2f;
    }
}