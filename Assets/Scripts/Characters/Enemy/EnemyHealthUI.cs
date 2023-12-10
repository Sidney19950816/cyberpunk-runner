using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Image _healthBar;

    private IHealth _health;

    public void Construct(IHealth health)
    {
        _health = health;

        _health.HealthChanged += UpdateHpBar;
    }

    private void Start()
    {
        IHealth health = GetComponent<IHealth>();

        if (health != null)
        {
            Construct(health);

            _healthBar.transform.parent.SetActive(false);
        }
    }

    private void UpdateHpBar()
    {
        if(_health.Current / _health.Max < _healthBar.fillAmount)
        {
            _healthBar
                .DOFillAmount(_health.Current / _health.Max, 1 * Time.timeScale);
        }

        _healthBar.transform.parent
            .SetActive(_health.Current <= 0 ? false : true);
    }
}
