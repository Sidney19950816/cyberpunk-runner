using UnityEngine;
using PathCreation;
using Assets.Scripts.Triggers;
using Assets.Scripts.Managers;
using System.Collections;

namespace Assets.Scripts
{
    public class EnemyPathFollower : Enemy
    {
        [Header("Bike Trigger")]
        [SerializeField] private BikeTrigger _bikeTrigger;

        [Header("Fight Scene")]
        [SerializeField] private FightScene _fightScene;

        [Header("Fight Scene Weapon")]
        [SerializeField] private EnemyWeapon fightSceneWeapon;

        [Header("Path Creator Properties")]
        [SerializeField] private PathCreator[] _pathCreators;
        [SerializeField] private EndOfPathInstruction _endOfPathInstruction;

        [Min(1)]
        [SerializeField] private float _projectileDelay = 2;

        [SerializeField] private float fightSceneDistance;

        private PathCreator pathCreator;
        private Bike playerBike;
        private float distanceTravelled;
        private float projectileDelay;

        private bool fightSceneStarted;

        protected override void Start()
        {
            base.Start();

            _bikeTrigger.OnTriggerT += bike => OnBikeTrigger(bike);

            if (_pathCreators != null)
            {
                pathCreator = _pathCreators[Random.Range(0, _pathCreators.Length)];
                pathCreator.pathUpdated += OnPathChanged;
            }
        }

        private void Update()
        {
            if (pathCreator == null)
                return;
            if (playerBike?.Rigidbody == null)
                return;
            if (StateManager.CurrentState is GameOverState)
                return;

            VertexPath path = pathCreator.path;
            distanceTravelled += playerBike.Rigidbody.velocity.magnitude * Time.deltaTime;
            transform.parent.position = path.GetPointAtDistance(distanceTravelled, _endOfPathInstruction);
            transform.parent.rotation = path.GetRotationAtDistance(distanceTravelled, _endOfPathInstruction);

            if (distanceTravelled / path.length >= 1 && Vector3.Distance(transform.parent.position, playerBike.transform.position) < fightSceneDistance
                && !fightSceneStarted)
            {
                fightSceneStarted = true;
                fightSceneWeapon.gameObject.SetActive(true);
                _weapon = fightSceneWeapon;

                _fightScene.BeginScene(playerBike);
            }

            projectileDelay -= Time.deltaTime;
            if (projectileDelay < 0 && distanceTravelled / path.length < 0.9f)
            {
                Weapon.Fire(playerBike.transform);
                projectileDelay = Random.Range(1, _projectileDelay);
            }
        }

        private void OnBikeTrigger(Bike bike)
        {
            playerBike = bike;
            gameObject.SetActive(true);
            _bikeTrigger = null;
            projectileDelay = Random.Range(1, _projectileDelay);
        }

        public void OnPathChanged()
        {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.parent.position);
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            Destroy(transform.parent.gameObject);
        }
    }
}
