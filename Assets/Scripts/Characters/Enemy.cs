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

        public Action<Enemy> OnDeathAction;
        public Action OnHeadshot;

        private Collider _collider;

        public GameObject Head => _head;
        public EnemyWeapon Weapon => weapon;

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

        public override void SetAimTarget(Transform target)
        {
            base.SetAimTarget(target);

            StartCoroutine(SetIKLowerBodyWeightCoroutine(1, 0, 1));
            StartCoroutine(SetIKUpperBodyWeightCoroutine(1, 0, 3));
            StartCoroutine(WaitForTarget(target, 1));

            Weapon.WeaponLaser?.SetActive(true);
        }

        private IEnumerator WaitForTarget(Transform target, float duration)
        {
            float elapsedTime = 0f;
            float randomDuration = UnityEngine.Random.Range(0, 0.5f);

            while (elapsedTime < duration)
            {
                transform.LookAt(target);

                if(elapsedTime > randomDuration)
                    Animator.SetFloat(AIM_FORWARD, 1);

                elapsedTime += Time.deltaTime / Time.timeScale;
                yield return null;
            }

            if (AimController != null)
                AimController.target = target;
        }

        private IEnumerator SetIKLowerBodyWeightCoroutine(float startValue, float targetValue, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                SetIKLowerBodyWeight(Mathf.Lerp(startValue, targetValue, elapsedTime / duration));

                elapsedTime += Time.deltaTime / Time.timeScale;
                yield return null;
            }

            SetIKLowerBodyWeight(targetValue);
        }

        private IEnumerator SetIKUpperBodyWeightCoroutine(float startValue, float targetValue, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                SetIKUpperBodyWeight(Mathf.Lerp(startValue, targetValue, elapsedTime / duration));

                elapsedTime += Time.deltaTime / Time.timeScale;
                yield return null;
            }

            SetIKUpperBodyWeight(targetValue);
        }

        private void SetIKLowerBodyWeight(float value)
        {
            IK.solver.bodyEffector.positionWeight = value;

            IK.solver.rightFootEffector.positionWeight = value;
            IK.solver.rightFootEffector.rotationWeight = value;

            IK.solver.leftFootEffector.positionWeight = value;
            IK.solver.leftFootEffector.rotationWeight = value;
        }

        private void SetIKUpperBodyWeight(float value)
        {
            IK.solver.rightShoulderEffector.positionWeight = value;
            IK.solver.rightHandEffector.positionWeight = value;
            IK.solver.rightHandEffector.rotationWeight = value;
            IK.solver.rightArmMapping.weight = value;
            IK.solver.rightArmChain.pull = value;

            IK.solver.leftShoulderEffector.positionWeight = value;
            IK.solver.leftHandEffector.positionWeight = value;
            IK.solver.leftHandEffector.rotationWeight = value;
            IK.solver.leftArmMapping.weight = value;
            IK.solver.leftArmChain.pull = value;
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
