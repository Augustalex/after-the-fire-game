using UnityEngine;

namespace Core
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                // if (_instance == null) Debug.LogError(typeof(T).ToString() + " is not in scene. Make sure the script order is correct, or add this class before the Default Time in Script Execution order");

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _instance = this as T;

            CustomAwake();
        }

        protected virtual void CustomAwake()
        {
        }
    }
}