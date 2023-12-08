using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace Assets.Scripts
{
    public class Enemy : CharacterController, IDamageable
    {
        [Space, Header("Enemy Properties")]
        [SerializeField] private GameObject _head;
        [SerializeField] protected EnemyWeapon weapon;

        [Space, Header("Health")]
        [SerializeField] private float _maxHealth;
        [SerializeField] private Image _healthBar;

        private EnemyAim _enemyAim;

        public Action<Enemy> OnDeathAction;
        public Action OnHeadshot;

        private Collider _collider;

        public GameObject Head => _head;
        public EnemyWeapon Weapon => weapon;

        public EnemyAim Aim => _enemyAim;

        public float Health { get; private set; }
        public float MaxHealth { get { return _maxHealth; } }

        public Vector3 GetBoundsCenter()
        {
            return _collider.bounds.center;
        }

        [UsedImplicitly]
        protected override void Start()
        {
            base.Start();

            _collider = GetComponent<Collider>();
            OnHeadshot += OnDeath;

            Health = MaxHealth;
            _healthBar.transform.parent.SetActive(false);

            Weapon.WeaponLaser?.SetActive(false);
            _enemyAim = GetComponent<EnemyAim>()
                .With(e => e.Initialize(Animator, IK, AimController));
        }

        private void OnEnable()
        {
            Animator.SetFloat(IDLE, 0);
            Animator.SetFloat(AIM_FORWARD, 0);
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            OnDeathAction?.Invoke(this);

            if(Weapon != null)
                Destroy(Weapon.gameObject);
            _healthBar.transform.parent.SetActive(false);

            OnDeathAction = null;
            OnHeadshot = null;
            StartCoroutine(DestroyOnDelay());

        }
        protected virtual IEnumerator DestroyOnDelay()
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        public void SetHealth(int value)
        {
            _maxHealth += (value / RemoteConfigManager.Instance.EnemyHealthIncreaseInterval) * RemoteConfigManager.Instance.EnemyHealthIncrease;
            Health = MaxHealth;

            _healthBar.transform.parent.SetActive(true);
        }

        public void TakeDamage(float amount = 0)
        {
            Health -= Mathf.Clamp(amount, 0, MaxHealth);

            _healthBar.DOFillAmount(Health / MaxHealth, 1 * Time.timeScale);

            if (Health <= 0)
            {
                OnDeath();
            }
        }

        public void Die()
        {
            OnDeath();
        }
    }
}
