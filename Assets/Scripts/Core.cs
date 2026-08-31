using System;
using UnityEngine;
using UnityEngine.UI;

public class Core : MonoBehaviour
{
    public static Core Instance { get; private set; }  

    [Header("코어 체력")]
    [SerializeField] private int _maxHealth = 100;

    [Header("코어 HP Slider")]
    [SerializeField] private Slider _hpSlider;

    private int _currentHealth;

    private bool _isDestroyed;

    public int MaxHealth
    {
        get { return _maxHealth; }
    }

    public int CurrentHealth
    {
        get { return _currentHealth; }
    }

    public bool IsDestroyed
    {
        get { return _isDestroyed; }
    }

    // 추후 확장을 위함 
    //public event Action<int, int> OnHealthChanged;

    //public event Action OnCoreDestroyed;

    private void Awake()
    {
        Instance = this;

        ResetHealth();
        SetSlider();
    }

    public void TakeDamage(int damage)
    {
        if (_isDestroyed)
        {
            return;
        }

        _currentHealth -=damage;

        if(_currentHealth < 0)
        {
            _currentHealth = 0;
        }

        //OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        UpdateSliderUI();

        Debug.Log($"[CoreHealth] 코어 피해: {damage}/ 남은 체력: {_currentHealth}");

        if(_currentHealth == 0)
        {
            DestroyCore();
        }
    }

    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
        _isDestroyed = false;

       // OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void DestroyCore()
    {
        if (_isDestroyed)
        {
            return;
        }

        _isDestroyed = true;

        Debug.Log("[CoreHealth] 코어가 파괴 되었습니다.");
      //  OnCoreDestroyed?.Invoke();
    }

    private void SetSlider()
    {
        if (_hpSlider == null)
        {
            return;
        }

        _hpSlider.minValue = 0f;
        _hpSlider.maxValue = 1f;

        UpdateSliderUI();
    }

    private void UpdateSliderUI()
    {
        if(_hpSlider == null || _maxHealth <= 0)
        {
            return;
        }

        float healthHandle = (float)_currentHealth / _maxHealth;

        _hpSlider.value = healthHandle;
    }
}
