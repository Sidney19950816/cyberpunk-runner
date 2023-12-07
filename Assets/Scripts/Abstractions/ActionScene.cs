using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Abstractions
{
    public abstract class ActionScene : Scripts.BaseBehaviour
    {
        [SerializeField] [UsedImplicitly] private Light[] _lights;
        [SerializeField] [UsedImplicitly] private Trigger<Bike> _trigger;

        public Trigger<Bike> Trigger => _trigger;

        protected Bike Bike { get; private set; }

        private BakePrefabLightmaps _bakePrefabLightmaps;

        [UsedImplicitly]
        protected void Start()
        {
            _bakePrefabLightmaps = GetComponent<BakePrefabLightmaps>();

            SetLights(false);

            OnStart();
        }

        public void BeginScene(Bike bike)
        {
            Bike = bike;

            SetLights(true);
            _trigger.gameObject.SetActive(false);

            OnSceneBegin();
        }

        public void EndScene()
        {
            SetLights(false);

            OnSceneEnd();
        }

        protected abstract void OnStart();

        protected abstract void OnSceneBegin();
        protected abstract void OnSceneEnd();

        private void SetLights(bool state)
        {
            //Temporary check until we decide on using baked prefabs or dynamic lights for all the prefabs
            if(_bakePrefabLightmaps != null)
            {
                _bakePrefabLightmaps.SetLightmaps(state);
            }
            else
            {
                foreach (var l in _lights) l.enabled = state;
            }
        }
    }
}