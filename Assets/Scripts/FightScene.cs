using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Abstractions;
using Cinemachine;
using JetBrains.Annotations;
using PG;
using UnityEngine;

namespace Assets.Scripts
{
    public class FightScene : ActionScene
    {
        [SerializeField] [UsedImplicitly] private float _remainingTime = 10;

        [SerializeField] [UsedImplicitly] private Transform _cameraLookAt;

        private readonly List<Enemy> _enemies = new();
        private bool _fightStarted;

        [UsedImplicitly]
        private void Update()
        {
            if (_fightStarted && Time.timeScale > 0)
            {
                if((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) && Bike.Player.Weapon.WeaponMode == WeaponMode.Automatic) && _remainingTime > 0)
                {
                    var ray = Managers.GameSceneManager.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out var hitInfo))
                    {
                        Bike.Player.Weapon.Fire(hitInfo.point);
                    }
                }

                _remainingTime -= Time.unscaledDeltaTime;

                if(_remainingTime < 0)
                {
                    foreach(Enemy e in _enemies)
                    {
                        e.Weapon.Fire(Bike.Player.transform);
                    }
                    _fightStarted = false;
                    Bike.Collider.enabled = true;
                }

                if (_enemies.OfType<EnemyPathFollower>().Any()) // better solution
                {
                    SetCameraLookAt(GetCameraLookAtTransform());
                }
            }
        }
    
        private void OnEnemyKill(Enemy enemy)
        {
            if(_fightStarted)
            {
                _enemies.Remove(enemy);

                if (_enemies.Count <= 0)
                {
                    EndScene();
                }
            }

            enemy.OnDeathAction -= OnEnemyKill;
            EventsService.EnemyTypeAsync(Bike.GetPassedChunksCount() / RemoteConfigManager.Instance.EnemyHealthIncreaseInterval + 1);
        }

        private void SetCameraLookAt(Transform lookAtTransform)
        {
            Managers.GameSceneManager.Instance.AimVirtualCamera.LookAt = lookAtTransform;

            var direction = lookAtTransform.position - Bike.transform.position;

            var follow = Managers.GameSceneManager.Instance.AimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            var dotResult = Vector3.Dot(Bike.transform.right, direction.normalized);

            var offset = 0.5f * Bike.FightOffset / 100;

            follow.CameraSide = dotResult switch
            {
                < -0.3f => 0.5f - offset,
                > 0.3f => 0.5f + offset,
                _ => 0.5f
            };
        }

        private Transform GetCameraLookAtTransform()
        {
            if (_cameraLookAt != null)
            {
                _cameraLookAt.position = GetCenterOfEnemies() + Vector3.up;
                return _cameraLookAt;
            }

            _cameraLookAt = new GameObject("Camera Look At")
            {
                transform =
                {
                    position = GetCenterOfEnemies() + Vector3.up,
                    parent = transform
                }
            }.transform;

            return _cameraLookAt;
        }
        
        private Enemy GetCenterEnemy()
        {
            var center = GetCenterOfEnemies();

            return _enemies
                .ToDictionary(e => e, e => Vector3.Distance(e.transform.position, center))
                .Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
        }

        private Vector3 GetCenterOfEnemies()
        {
            return _enemies.Aggregate(Vector3.zero, (c, e) => c + e.transform.position) /
                   _enemies.Count;
        }

        protected override void OnStart()
        {
            if (Trigger == null)
                return;

            _enemies.AddRange(GetComponentsInChildren<Enemy>());
            Trigger.OnTriggerT += BeginScene;
        }

        protected override void OnSceneBegin()
        {
            if (_fightStarted) 
                return;

            _fightStarted = true;

            StateManager.SetState(new FightState(Bike, _enemies, _remainingTime));

            SetCameraLookAt(GetCameraLookAtTransform());
            foreach (var e in _enemies)
            {
                e.gameObject.SetActive(true);
                e.OnDeathAction += OnEnemyKill;
                e.SetHealth(Bike.GetPassedChunksCount());
                e.SetAimTarget(Bike.Player.transform);
            }

            Bike.Player.SetAimTarget(GetCenterEnemy().transform);
        }

        protected override void OnSceneEnd()
        {
            StateManager.SetState(new GameState(Bike.GetComponent<ArcadeBike>()));

            _fightStarted = false;
            Bike.Collider.enabled = true;
            Bike.Player.ResetAimTarget();
        }
    }
}
