using System;
using System.Collections;
using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [SerializeField] private EnemyHealth _health;

    public event Action Died;

    private void Start() =>
      _health.HealthChanged += HealthChanged;

    private void OnDestroy() =>
      _health.HealthChanged -= HealthChanged;

    private void HealthChanged()
    {
        if (_health.Current <= 0)
            Die();
    }

    private void Die()
    {
        _health.HealthChanged -= HealthChanged;

        StartCoroutine(DestroyTimer());

        Died?.Invoke();
        Died = null;
    }

    private IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
