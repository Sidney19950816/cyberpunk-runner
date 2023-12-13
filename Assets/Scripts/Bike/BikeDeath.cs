using Assets.Scripts;
using System;
using System.Collections;
using UnityEngine;

public class BikeDeath : MonoBehaviour
{
    [SerializeField] private BikeHealth _health;

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

        Died?.Invoke();
        Died = null;

        StateManager.SetState(new GameOverState(GetComponent<Bike>())); // TODO
    }
}
