using System;
using UnityEngine;

public class BikeHealth : MonoBehaviour, IHealth
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

    public void InitializeHealth(float initialHealth)
    {
        _current = _max = initialHealth;
    }

    public void TakeDamage(float damage)
    {
        _current -= damage;

        HealthChanged?.Invoke();
    }
}
