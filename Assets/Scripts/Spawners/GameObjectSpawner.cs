using Assets.Scripts.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Spawners
{
    public sealed class GameObjectSpawner : Spawner<GameObject>
    {
        [SerializeField] private Trigger<Bike> _trigger;

        private void Start()
        {
            _trigger.OnTrigger += Spawn;
        }

        protected override void OnSpawn(GameObject spawned)
        {
        }

        protected override void Destroy(GameObject spawned)
        {
            _trigger.OnTrigger -= Spawn;

            Destroy(spawned);
        }
    }
}