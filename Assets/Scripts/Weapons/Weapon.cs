using UnityEngine;

namespace Assets.Scripts
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private GameObject _projectileObject;
        [SerializeField] private Transform _projectileSpawnPoint;

        [Space, Header("AUDIO SOURCE")]
        [SerializeField] protected AudioSource audioSource;

        [Space, Header("AUDIO CLIP")]
        [SerializeField] protected AudioClip weaponFireAudio;

        public Transform ProjectileSpawnPoint => _projectileSpawnPoint;

        /// <summary>
        /// Instantiates a projectile GameObject and optionally parents it to the weapon's transform.
        /// </summary>
        /// <param name="parentToWeapon">Specifies whether the instantiated projectile should be parented to the weapon.</param>
        /// <returns>The instantiated projectile GameObject.</returns>
        protected GameObject GetProjectile(bool parentToWeapon = true)
        {
            if(parentToWeapon)
                return Instantiate(_projectileObject, _projectileSpawnPoint);
            else
                return Instantiate(_projectileObject, _projectileSpawnPoint.position, Quaternion.identity);
        }
    }
}
