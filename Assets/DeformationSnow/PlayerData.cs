using UnityEngine;

namespace DeformationSnow
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "PlayerData", order = 0)]
    public class PlayerData : ScriptableObject
    {
        public float shiftBoost = 100f;
        public float speed = 1400f;
        public float startBoost = 2500f;
        public float minSpeed = 9f;
        
        // Draft - in prio order from top to bottom
        public float inAirDrag = 1f;
        public float onIceDrag = 0f;
        public float highSpeedDragVelocityThreshold = 10f;
        public float highSpeedDrag = 2f;
        public float boostDrag = 2f;
        public float movingDrag = 2f;
        public float stillDrag = 0.1f;
        
        // In Air Effects
        public float gravity = 200f;
        public float gravityMultiplierBase = 4f;
        public float gravityMultiplierGrowthExponent = 1.5f;
        public float gravityMultiplierMax = 10f;
        
        // Jump
        public float jumpForce = 8f;
        public float jumpDirectionalPush = 5f;
        
        public float extraDownwardForceOnIsland = 200f;
        
        // Ice
        public float onIceMovementMultiplier = .4f;
        public float onIceMinRandomMotion = 100f;
        public float onIceMaxRandomMotion = 2000f;
        
        public float treeHitStunTime = 2f;
        public float hitTreeReturnForceMultiplier = 1.5f;
    }
}