using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Events;
using System;

namespace Assets.Scripts
{
    public class PlayerWeapon : Weapon
    {
        [SerializeField] private AudioClip weaponReloadAudio;

        public Action<int> OnProjectileUpdateAction;

        public WeaponMode WeaponMode { get; private set; }

        private int initialBulletsCount;

        private Coroutine fireCoroutine;
        private Coroutine reloadCoroutine;

        [Serializable]
        public struct WeaponParams
        {
            [Header("Current bullets count in the magazine")]
            public int BulletsCount;

            [Header("Delay the weapon reload when the bullet count is 0")]
            public float ReloadDelay;

            [Header("The time it takes to load a bullet into a magazine")]
            public float BulletLoadTime;

            [Header("The damage that the weapon deals to the enemy")]
            public float Damage;

            [Header("The percentage chance that the player will deal critical damage")]
            public float CritChance;

            [Header("Starting Headshot multiplier value")]
            public int HeadshotMultiplier;

            [Header("Rokens given per Enemy kill")]
            public int RokensPerKill;

            [Header("The weapons index")]
            public int Index;
        }

        private WeaponParams weaponParameters;
        public WeaponParams WeaponParameters { get { return weaponParameters; } }

        public void Initialize(WeaponData weaponData)
        {
            Dictionary<UserWeaponUpgrade, float> upgrades = new Dictionary<UserWeaponUpgrade, float>();
            foreach (WeaponUpgradeData upgradeData in weaponData.upgrades)
            {
                upgrades.Add(upgradeData.enumId, upgradeData.value);
            }

            weaponParameters = new WeaponParams
            {
                Index = (int)weaponData.enumId,
                BulletsCount = weaponData.bulletsCount,
                ReloadDelay = upgrades[UserWeaponUpgrade.Reload],
                Damage = upgrades[UserWeaponUpgrade.Damage],
                CritChance = upgrades[UserWeaponUpgrade.Critical],
                BulletLoadTime = 0.5f,
                HeadshotMultiplier = weaponData.headshotMultiplier,
                RokensPerKill = weaponData.rokensPerKill,
            };

            initialBulletsCount = weaponData.bulletsCount;
            WeaponMode = weaponData.enumMode;

            transform.localScale = Vector3.one * 0.1f;
        }

        public void Fire(Vector3 position)
        {
            if(reloadCoroutine != null && weaponParameters.BulletsCount > 0)
                StopCoroutine(reloadCoroutine);

            switch(WeaponMode)
            {
                case WeaponMode.SemiAutomatic:
                    SingleFire(position);
                    break;
                case WeaponMode.BurstFire:
                    if (fireCoroutine == null)
                        fireCoroutine = StartCoroutine(BurstFireCoroutine(position));
                    break;
                case WeaponMode.SpreadFire:
                    SpreadFire(position);
                    break;
                case WeaponMode.Automatic:
                    if(fireCoroutine == null)
                        fireCoroutine = StartCoroutine(AutoFireCoroutine(position));
                    break;
            }
        }

        private Vector3 GetBulletSpreadDirection()
        {
            int bulletSpreadAngle = 30;
            Vector2 randomPointOnCircle = UnityEngine.Random.insideUnitCircle.normalized * bulletSpreadAngle;
            Vector3 spreadDirection = new Vector3(randomPointOnCircle.x, randomPointOnCircle.y, 1f).normalized;
            return spreadDirection;
        }

        private void Reload()
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
            fireCoroutine = null;
        }

        public void Reset()
        {
            UpdateProjectile(initialBulletsCount);
            fireCoroutine = null;
        }

        private IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(weaponParameters.ReloadDelay * Time.timeScale);

            while (weaponParameters.BulletsCount < initialBulletsCount)
            {
                UpdateProjectile(1);
                Util.PlaySound(weaponReloadAudio, audioSource);
                yield return new WaitForSeconds(weaponParameters.BulletLoadTime * Time.timeScale);
            }
        }

        private void UpdateProjectile(int value)
        {
            weaponParameters.BulletsCount = Mathf.Clamp(weaponParameters.BulletsCount += value, 0, initialBulletsCount);

            OnProjectileUpdateAction?.Invoke(weaponParameters.BulletsCount);

            if (weaponParameters.BulletsCount <= 0)
                Reload();
        }

        private void SingleFire(Vector3 position, GameObject projectile = null)
        {
            if (weaponParameters.BulletsCount <= 0)
                return;

            GameObject p = projectile == null ? GetProjectile(false) : projectile;
            p.GetComponent<PlayerBullet>().Initialize(position, weaponParameters);
            UpdateProjectile(-1);
            Util.Vibrate();
            Util.PlaySound(weaponFireAudio, audioSource);
        }

        private void SpreadFire(Vector3 position)
        {
            SingleFire(position);

            for (int i = 0; i < 2; i++)
            {
                Vector3 spreadDirection = GetBulletSpreadDirection();
                Vector3 spreadPosition = position + spreadDirection * UnityEngine.Random.Range(0.1f, 0.5f);
                GetProjectile(false).GetComponent<PlayerBullet>().Initialize(spreadPosition, weaponParameters);
            }
        }

        private IEnumerator AutoFireCoroutine(Vector3 position)
        {
            SingleFire(position);

            yield return new WaitForSeconds(0.2f * Time.timeScale);

            // Stop firing
            fireCoroutine = null;
        }

        private IEnumerator BurstFireCoroutine(Vector3 position)
        {
            SingleFire(position);

            List<GameObject> projectiles = new List<GameObject>();
            for (int i = 0; i < 2; i++)
            {
                projectiles.Add(GetProjectile(false));
                projectiles[i].SetActive(false);
            }

            yield return new WaitForSeconds(0.3f * Time.timeScale);
            projectiles[0].SetActive(true);
            SingleFire(position, projectiles[0]);
            yield return new WaitForSeconds(0.3f * Time.timeScale);
            projectiles[1].SetActive(true);
            SingleFire(position, projectiles[1]);
            yield return new WaitForSeconds(0.3f * Time.timeScale);

            // Stop firing
            fireCoroutine = null;
        }
    }
}

