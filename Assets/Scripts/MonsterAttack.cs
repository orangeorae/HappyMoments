using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    private int _attackDamage;

    private float _attackSpeed;

    private float _attackTimer;

   public void MonsterAttackInit(MonsterData monsterData) //몬스터 스텟 세팅 
    {
        if(monsterData == null)
        {
            return;
        }

        _attackDamage = monsterData.Attack;
        
        _attackSpeed = monsterData.AttackSpeed;

        _attackTimer = _attackSpeed;
    }

    private void Update()
    {
        if(Core.Instance == null)
        {
            return;
        }

        if (Core.Instance.IsDestroyed)
        {
            return;
        }

        _attackTimer -= Time.deltaTime;

        if(_attackTimer > 0f)
        {
            return;
        }

        Core.Instance.TakeDamage(_attackDamage);

        _attackTimer = _attackSpeed;
    }
}
