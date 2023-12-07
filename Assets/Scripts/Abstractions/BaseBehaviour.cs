using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Abstractions
{
    public abstract class BaseBehaviour : MonoBehaviour
    {
        public T GetRequiredComponent<T>()
            where T : Component
        {
            return GetComponents<T>().SingleOrDefault() ??
                   throw new Exception($"Component {typeof(T).Name} not found on {name}.");
        }

        // TODO: Replace with DI
        public T Find<T>()
            where T : Component
        {
            return FindObjectsOfType<T>().Single();
        }
    }
}