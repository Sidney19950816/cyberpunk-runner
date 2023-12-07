using System;
using System.Collections;
using Assets.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Grenade : Projectile
    {
        [Header("UI/Particles")]
        [SerializeField] private Image _targetImage;
        [SerializeField] private ParticleSystem _explosionParticle;

        [Space]
        [Header("Main Parameters")]
        [SerializeField] private SphereCollider _sphereCollider;
        [SerializeField] private float _height;
        [SerializeField] private float _explosionRadius;

        [Space, Header("Camera Parameters")]
        [SerializeField] private float _cameraShakeDuration;
        [SerializeField] private float _cameraShakeAmplitude;

        [Header("AUDIO CLIPS")]
        [SerializeField] private AudioClip explosionAudio;

        public override void Initialize(Vector3 targetPosition)
        {
            Rigidbody.velocity = CalculateLaunchData(targetPosition, targetPosition.y + _height).initialVelocity;
            InitTargetImage(targetPosition);
        }

        public override void Initialize(Transform target)
        {
            Vector3 direction = target.transform.forward;
            int x = direction.x > 0 ? (direction.x > 0.5f ? 1 : 0) : (direction.x < -0.5f ? -1 : 0);
            int z = direction.z > 0 ? (direction.z > 0.5f ? 1 : 0) : (direction.z < -0.5f ? -1 : 0);
            direction = new Vector3(x, direction.y, z);
            float targetVelocity = target.GetComponent<Rigidbody>()?.velocity.magnitude ?? 0;
            Vector3 pos = target.position + direction * (targetVelocity * CalculateLaunchData(target.position, target.position.y + _height).timeToTarget);
            Rigidbody.velocity = CalculateLaunchData(pos, target.position.y + _height).initialVelocity;
            InitTargetImage(pos);
        }

        struct LaunchData
        {
            public readonly Vector3 initialVelocity;
            public readonly float timeToTarget;

            public LaunchData(Vector3 initialVelocity, float timeToTarget)
            {
                this.initialVelocity = initialVelocity;
                this.timeToTarget = timeToTarget;
            }
        }

        LaunchData CalculateLaunchData(Vector3 target, float h)
        {
            float displacementY = target.y - transform.position.y;
            Vector3 displacementXZ = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z);
            float time = Mathf.Sqrt(-2 * h / -Physics.gravity.magnitude) + Mathf.Sqrt(2 * (displacementY - h) / -Physics.gravity.magnitude);
            Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * -Physics.gravity.magnitude * h); 
            Vector3 velocityXZ = displacementXZ / time;

            return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(-Physics.gravity.magnitude), time);
        }

        private void InitTargetImage(Vector3 position)
        {
            _targetImage.color = Color.red;
            _targetImage.transform.parent.SetParent(transform.parent, false);
            //_targetImage.transform.parent.parent = transform.parent;
            _targetImage.transform.parent.position = new Vector3(_targetImage.transform.parent.position.x, 0, _targetImage.transform.parent.position.z);
            _targetImage.transform.position = new Vector3(position.x, 0.1f, position.z); // Find a better way to determine the Y position for the targetImage
        }

        private IEnumerator Explode(float time)
        {
            _explosionParticle.Play();
            AudioManager.Instance.PlaySound(explosionAudio);
            GetComponent<MeshRenderer>().enabled = false;
            Util.CameraShake(this, _cameraShakeDuration, _cameraShakeAmplitude);
            Util.Vibrate();
            //Implement an explosion sound

            float timeElapsed = 0;
            while (timeElapsed < time)
            {
                _sphereCollider.radius = Mathf.Lerp(_sphereCollider.radius, _explosionRadius, timeElapsed / time);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(UtilConstants.PLAYER))
            {
                StateManager.SetState(new GameOverState(other.GetComponent<ArcadeBike>()));
                Instantiate(Resources.Load(UtilConstants.PROJECTILE_HIT_PARTICLE_PATH) as GameObject, transform.position, transform.rotation);
                Destroy(gameObject);
            }

            if (_targetImage != null)
            {
                Rigidbody.isKinematic = true;
                StartCoroutine(Explode(_explosionParticle.main.startLifetime.constant / 2));
                Destroy(_targetImage.transform.parent.gameObject);
            }
        }
    }
}
