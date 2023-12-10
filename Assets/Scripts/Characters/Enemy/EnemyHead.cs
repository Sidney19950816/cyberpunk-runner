using System;
using UnityEngine;

public class EnemyHead : MonoBehaviour
{
    [SerializeField] private EnemyHealth _enemyHealth;

    public event Action OnHeadshot;

    public void HandleHeadshot()
    {
        _enemyHealth.TakeDamage(_enemyHealth.Max);

        OnHeadshot?.Invoke();
    }
}
