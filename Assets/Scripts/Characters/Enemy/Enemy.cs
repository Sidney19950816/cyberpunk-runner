using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace Assets.Scripts
{
    public class Enemy : CharacterController
    {
        [Space, Header("Enemy Properties")]
        [SerializeField] private EnemyHead _head;
        [SerializeField] protected EnemyWeapon _weapon;

        private EnemyAim _enemyAim;
        private EnemyDeath _enemyDeath;
        private IHealth _health;

        public EnemyHead Head => _head;
        public EnemyWeapon Weapon => _weapon;

        public EnemyAim Aim => _enemyAim;

        public IHealth Health => _health;

        [UsedImplicitly]
        protected override void Start()
        {
            base.Start();

            _enemyAim = GetComponent<EnemyAim>()
                .With(e => e.Initialize(Animator, IK, AimController));

            _health = GetComponent<IHealth>();

            _enemyDeath = GetComponent<EnemyDeath>();
            _enemyDeath.Died += DestroyWeapon;
            _enemyDeath.Died += OnDeath;
        }

        private void OnEnable()
        {
            Animator.SetFloat(IDLE, 0);
            Animator.SetFloat(AIM_FORWARD, 0);
        }

        protected virtual IEnumerator DestroyOnDelay()
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        private void DestroyWeapon()
        {
            if (Weapon != null)
                Destroy(Weapon.gameObject);
        }
    }
}
