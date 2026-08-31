using UnityEngine;
using UnityEngine.AddressableAssets;

public class MonsterHealth : MonoBehaviour
{
    private MonsterData _monsterData;
    private int _currentHealth;
    private bool _isDead;

    public int CurrentHealth
    {
        get { return _currentHealth; } 
    }

    public bool IsDead
    {
        get { return _isDead; }
    }

    public void MonsterHealthInit(MonsterData monsterData)
    {
        if (monsterData == null) 
        {
            return;
        }

        _monsterData = monsterData;
        _currentHealth = _monsterData.Health;
        _isDead = false;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead)
        {
            return;
        }

        if(damage <= 0)
        {
            return;
        }

        _currentHealth -= damage;

        if(_currentHealth < 0)
        {
            _currentHealth = 0;
        }

        if(_currentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        Debug.Log("[MonsterHealth] 몬스터 사망: " + gameObject.name);

        Addressables.ReleaseInstance(gameObject);
    }
}
