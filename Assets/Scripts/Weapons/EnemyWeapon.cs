using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyWeapon : Weapon
    {
        [Space, Header("LASER")]
        [SerializeField] private WeaponLaser _weaponLaser;

        [Space, Header("PARTICLES")]
        [SerializeField] private ParticleSystem _flashParticleSystem;

        private void Start()
        {
            _weaponLaser?.SetActive(false);
        }

        public void Fire(Transform target)
        {
            var projectile = GetProjectile(false);
            projectile.GetComponent<Projectile>().Initialize(target);

            if(_flashParticleSystem != null)
                Instantiate(_flashParticleSystem, ProjectileSpawnPoint.position, transform.rotation);
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
