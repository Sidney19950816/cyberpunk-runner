using Assets.Scripts.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Abstractions
{
    public abstract class Spawner<T> : MonoBehaviour
        where T : Object
    {
        [SerializeField] [UsedImplicitly] private T[] _prefabs;

        private T _spawned;
        
        protected abstract void OnSpawn(T spawned);
        protected abstract void Destroy(T spawned);

        public void Spawn()
        {
            if (_spawned != null)
            {
                Destroy(_spawned);
            }

            if (_prefabs.Length == 0)
                return;

            _spawned = Instantiate(_prefabs.GetRandom(), transform);

            OnSpawn(_spawned);
        }
    }
}