using System;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyBullet : Projectile
    {
        private Transform target;

        private void FixedUpdate()
        {
            if (target == null)
                return;

            if (StateManager.CurrentState is not FightState)
                Destroy(gameObject);

            Vector3 direction = target.position - transform.position;
            ApplyForceAndLookAt(1000 * direction, target.position);
        }

        public override void Initialize(Transform target)
        {
            this.target = target;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(UtilConstants.PLAYER))
            {
                StateManager.SetState(new GameOverState(other.GetComponent<ArcadeBike>()));
                Instantiate(Resources.Load($"Particles/Weapon/Hit/EnemyHit"), transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
}