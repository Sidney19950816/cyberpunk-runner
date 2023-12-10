using Assets.Scripts.Events;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerBullet : Projectile
    {
        private const float BULLET_FORCE = 100;

        [SerializeField] private Transform particlesParent;

        private Vector3 targetPosition;

        private PlayerWeapon.WeaponParams weaponParams;

        private void FixedUpdate()
        {
            if (targetPosition == Vector3.zero)
                return;

            Vector3 direction = targetPosition - transform.position;
            ApplyForceAndLookAt(BULLET_FORCE / Time.timeScale * direction, targetPosition);
        }

        public void Initialize(Vector3 targetPosition, PlayerWeapon.WeaponParams weaponParams)
        {
            this.weaponParams = weaponParams;
            this.targetPosition = targetPosition;

            Instantiate(Resources.Load($"Particles/Weapon/Projectiles/Projectile{weaponParams.Index}"), particlesParent);
            Instantiate(Resources.Load($"Particles/Weapon/Flash/Flash{weaponParams.Index}"), particlesParent.position, Quaternion.identity);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            Enemy enemy = other.GetComponent<Enemy>() ?? Util.FindParentWithTag(other.gameObject, UtilConstants.ENEMY)?.GetComponent<Enemy>();
            if (enemy != null)
            {
                float critPercentage = weaponParams.CritChance;
                float critChance = Random.value < critPercentage / 100 ? (enemy.Health.Max * critPercentage) / 100 : 0;
                if (other.gameObject.GetComponent<EnemyHead>() != null)
                {
                    enemy.Head.HandleHeadshot();
                }
                else
                {
                    enemy.Health.TakeDamage(weaponParams.Damage + critChance);
                }

                if (other.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForce(transform.forward * 10, ForceMode.Impulse);
                }

                Instantiate(Resources.Load("Particles/BloodSplat") as GameObject, transform.position, transform.rotation);
            }

            if (other.GetComponent<PlayerBullet>() == null)
            {
                Instantiate(Resources.Load($"Particles/Weapon/Hit/Hit{weaponParams.Index}"), transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}