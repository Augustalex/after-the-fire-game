using UnityEngine;

namespace Player
{
    public class PlayerSize : MonoBehaviour
    {
        private float _size;

        public void Change(float value)
        {
            _size = value;
        }
        
        public bool HasSnow()
        {
            return _size > 0.01f;
        }


        public float Size()
        {
            return _size;
        }
    }
}