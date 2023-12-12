using Assets.Scripts.Abstractions;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Spawners
{
    public sealed class ActionSceneSpawner : Spawner<ActionScene>
    {
        protected override void OnSpawn(ActionScene spawned)
        {
            if(spawned.Trigger != null)
            {
                spawned.Trigger.OnTriggerT += spawned.BeginScene;
                spawned.gameObject.SetActive(true);
            }
        }

        protected override void Destroy(ActionScene spawned)
        {
            spawned.Trigger.OnTriggerT -= spawned.BeginScene;
            Destroy(spawned.gameObject);
        }
    }
}