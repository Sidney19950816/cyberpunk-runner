using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class CharacterController : MonoBehaviour
    {
        protected const string IDLE = "Idle";
        protected const string AIM_FORWARD = "Aim Forward";

        [Space]
        [Header("Character Properties")]
        [SerializeField] private Animator _animator;
        [SerializeField] private FullBodyBipedIK _ik;
        [SerializeField] private AimIK _aimIk;
        [SerializeField] private AimController _aimController;

        private Rigidbody[] _rigidbodies;

        public Animator Animator => _animator;
        public FullBodyBipedIK IK => _ik;
        public AimIK AimIK => _aimIk;

        public AimController AimController => _aimController;

        protected virtual void Start()
        {
            _animator ??= GetComponent<Animator>();
            _ik ??= GetComponent<FullBodyBipedIK>();
            _aimIk ??= GetComponent<AimIK>();
            _aimController ??= GetComponent<AimController>();

            _rigidbodies = GetRigidbodies();
            _aimIk.enabled = false;
        }

        private Rigidbody[] GetRigidbodies()
        {
            var rigidbodiesList = new List<Rigidbody>();
            var allChildren = GetComponentsInChildren<Transform>();

            foreach (var child in allChildren)
            {
                var r = child.GetComponent<Rigidbody>();
                if (r != null)
                {
                    rigidbodiesList.Add(r);
                    r.isKinematic = true;
                    r.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            }

            return rigidbodiesList.ToArray();
        }

        protected virtual void OnDeath()
        {
            _animator.enabled = false;
            _ik.enabled = false;
            _aimIk.enabled = false;
            foreach (var r in _rigidbodies)
            {
                r.isKinematic = false;
                r.mass = 10;
            }
        }

        public virtual void SetAimTarget(Transform target)
        {
            AimIK.enabled = true;
            AimController.enabled = true;
            Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }
}
