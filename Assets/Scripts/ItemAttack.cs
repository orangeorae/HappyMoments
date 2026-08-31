using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ItemAttack : MonoBehaviour
{
    private int _attackDamage;
    private float _attackSpeed;
    private float _attackTimer;

    private SphereCollider _attackRangeCollider; // 레이더 역할을 할 콜리더 

    private List<MonsterHealth> _monstersInRange = new List<MonsterHealth>(); // 사거리 안에 들어온 몬스터들 리스트 

    private void Awake()
    {
        _attackRangeCollider = gameObject.AddComponent<SphereCollider>(); // 콜리더 자동등록
        _attackRangeCollider.isTrigger = true;

        Rigidbody rigidbodyComponent = gameObject.AddComponent<Rigidbody>(); // 트리거 정상 작동을 위해서는 Rigidbody 필수 
        rigidbodyComponent.isKinematic = true;
        rigidbodyComponent.useGravity = false;
    }

    public void ItemAttackInit(ItemData itemData) //아이템 세팅
    {
        _attackDamage = itemData.Damage;
        _attackSpeed = itemData.AttackSpeed;

        _attackTimer = 0f;

        /*아이템 마다 모델 크기가  다 다르기 때문에
        콜라이더 로컬 반지름을 스케일로 나눠줘야 실제 월드 범위가 공격범위와 같아진다.*/
        float scale = transform.localScale.x;
        if(scale <= 0f)
        {
            scale = 1f;
        }

        _attackRangeCollider.radius = itemData.AttackRange / scale;
    }

    private void Update()
    {
        if(_attackRangeCollider.radius <= 0f)
        {
            return;
        }

        _attackTimer -= Time.deltaTime;

        if( _attackTimer > 0f)
        {
            return ;
        }
        AttackMonster();

        _attackTimer = _attackSpeed;
    }

    private void AttackMonster()
    {

        MonsterHealth targetMonster = GetClosestMonster();

        if (targetMonster == null)
        {
            return;
        }

        targetMonster.TakeDamage(_attackDamage);
    }

    private MonsterHealth GetClosestMonster()
    {
        MonsterHealth closestMonster = null;

        float closestDistance = 0f;

        /*리스트 삭제는 반드시 뒤에서 부터 해야한다,
         * 앞에서 부터 지우면 리스트의 순서가 밀리거나 몬스터를 건너뛰어 에러가 발생할 수 있다.*/
        for(int i = _monstersInRange.Count -1; i>=0; i--) 
        {
            MonsterHealth monsterHealth = _monstersInRange[i];

            if(monsterHealth == null || monsterHealth.IsDead)
            {
                _monstersInRange.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(transform.position, monsterHealth.transform.position);

            if (closestMonster == null || distance < closestDistance)
            {
                closestMonster = monsterHealth;
                closestDistance = distance;
            }
        
        }

        return closestMonster;
    }

    private void OnTriggerEnter(Collider other)
    {
        MonsterHealth monsterHealth = other.GetComponent<MonsterHealth>();
        if (monsterHealth == null)
        {
            return;
        }

        if(_monstersInRange.Contains(monsterHealth) == false)
        {
            _monstersInRange.Add(monsterHealth);
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        MonsterHealth monsterHealth = other.GetComponent<MonsterHealth>();

        if (monsterHealth == null)
        {
            return;
        }
        _monstersInRange.Remove(monsterHealth);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,_attackRangeCollider.radius * transform.localScale.x);
    }
}
