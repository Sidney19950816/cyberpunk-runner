using Assets.Scripts;
using UnityEngine;
using Assets.Scripts.Managers;
using Assets.Scripts.Events;

[ExecuteInEditMode]
public abstract class Projectile : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidBody;

    public Rigidbody Rigidbody => _rigidBody;

    /// <summary>
    /// Initializes the projectile and sends the projectile to the static target based on speed
    /// </summary>
    public virtual void Initialize(Vector3 targetPosition)
    {
    }

    /// <summary>
    /// Initializes the projectile and sends the projectile to the dynamic target based on speed
    /// </summary>
    public virtual void Initialize(Transform target)
    {
    }

    protected abstract void OnTriggerEnter(Collider other);

    /// <summary>
    /// Applies an impulse force to the Rigidbody in the specified direction and rotates the object to look at the given target.
    /// </summary>
    /// <param name="direction">The direction in which to apply the force.</param>
    /// <param name="target">The target Vector3 to look at.</param>
    protected void ApplyForceAndLookAt(Vector3 direction, Vector3 target)
    {
        Rigidbody.AddForce(direction * Time.fixedDeltaTime, ForceMode.Impulse);
        transform.LookAt(target);
    }
}
