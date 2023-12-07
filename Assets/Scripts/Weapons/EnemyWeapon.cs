using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyWeapon : Weapon
    {
        [Space, Header("LASER")]
        [SerializeField] private WeaponLaser weaponLaser;

        [Space, Header("PARTICLES")]
        [SerializeField] private ParticleSystem flashParticleSystem;

        public WeaponLaser WeaponLaser => weaponLaser;

        public void Fire(Transform target)
        {
            var projectile = GetProjectile(false);
            projectile.GetComponent<Projectile>().Initialize(target);
            WeaponLaser?.SetActive(false);

            if(flashParticleSystem != null)
                Instantiate(flashParticleSystem, ProjectileSpawnPoint.position, transform.rotation);
            audioSource?.PlayOneShot(weaponFireAudio);
        }

        public void FireAfter(Transform target, float time)
        {
            StartCoroutine(FireCoroutine(target, time));
        }

        private IEnumerator FireCoroutine(Transform target, float time)
        {
            yield return new WaitForSecondsRealtime(time);

            Fire(target);
        }
    }
}
