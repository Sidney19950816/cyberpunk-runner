using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Abstractions
{
    public abstract class Trigger : BaseBehaviour
    {
        public Action OnTrigger;
    }

    [RequireComponent(typeof(Collider))]
    public abstract class Trigger<T> : Trigger
        where T : MonoBehaviour
    {
        public Action<T> OnTriggerT;

        [UsedImplicitly]
        private void Awake()
        {
            if (!GetRequiredComponent<Collider>().isTrigger)
            {
                throw new Exception($"{GetType().Name} Collider isTrigger is false.");
            }
        }

        [UsedImplicitly]
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<T>(out var component)) 
                return;

            OnTrigger?.Invoke();
            OnTriggerT?.Invoke(component);
        }
    }
}