using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IHealth
{
    [SerializeField] private float _max;
    [SerializeField] private float _current;

    public event Action HealthChanged;
    public float Current
    {
        get => _current;
        set => _current = value;
    }

    public float Max
    {
        get => _max;
        set => _max = value;
    }

    public void TakeDamage(float damage)
    {
        Current -= damage;

        HealthChanged?.Invoke();
    }

    public void InitializeHealth(float initialHealth)
    {
        Max += (initialHealth / RemoteConfigManager.Instance.EnemyHealthIncreaseInterval) * RemoteConfigManager.Instance.EnemyHealthIncrease;
        Current = Max;

        HealthChanged?.Invoke();
    }
}
