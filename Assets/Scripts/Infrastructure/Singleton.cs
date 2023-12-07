using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    public abstract class Singleton<T> : MonoBehaviour
        where T : Component
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;

                _instance = FindObjectOfType<T>();

                if (_instance != null) 
                    return _instance;

                return _instance = new GameObject
                {
                    name = typeof(T).Name
                }.AddComponent<T>();
            }
        }

        [UsedImplicitly]
        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                DontDestroyOnLoad(gameObject);

                OnAwake();
            }
            else
            {
                Destroy(gameObject.GetComponent<T>());
            }
        }

        protected virtual void OnAwake() { }
    }
}