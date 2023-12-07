using System;
using Assets.Scripts.Spawners;
using Assets.Scripts.Triggers;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.World
{
    public sealed class Chunk : MonoBehaviour
    {
        private static readonly Vector3 DisabledPosition = new(0, -1000, 0);
        
        private ActionSceneSpawner[] _actionSceneSpawners;

        [SerializeField] [UsedImplicitly] private BikeTrigger _startTrigger;

        public Action OnChunkDisable;
        public Action<Chunk> OnChunkEnter;

        [SerializeField] [UsedImplicitly] private Transform _endPoint;
        
        public void Initialize()
        {
            _startTrigger.OnTrigger += () => OnChunkEnter?.Invoke(this);
            _startTrigger.OnTriggerT += bike => bike.SetResetPosition(_startTrigger.transform, _endPoint.transform);

            _actionSceneSpawners = gameObject.GetComponentsInChildren<ActionSceneSpawner>();
            OnChunkEnter += DisableCollider;
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            transform.position = DisabledPosition;
        }

        public void Destroy()
        {
            Debug.Log("Destroy CHUNK");
            Destroy(gameObject);
        }

        public void Enable(Chunk previous = null)
        {
            transform.position = previous == null ? Vector3.zero : previous._endPoint.position;
            transform.eulerAngles = previous == null ? Vector3.zero : previous._endPoint.eulerAngles;

            foreach (var actionSceneSpawner in _actionSceneSpawners)
                actionSceneSpawner.Spawn();

            gameObject.SetActive(true);
            _startTrigger.GetComponent<BoxCollider>().enabled = true;
        }

        private void DisableCollider(Chunk chunk)
        {
            _startTrigger.GetComponent<BoxCollider>().enabled = false;
        }
    }
}