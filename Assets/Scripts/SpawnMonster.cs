using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnMonster : MonoBehaviour
{
    private MonsterData _loadingMonsterData;

    private float _loadingStartAngle; //몬스터 소환 위치 저장

    public bool SpawnMonsterInstance(MonsterData monsterData) //매니저한테 몬스터 데이터를 넘겨받고 실행 
    {
        if(CirclePath.Instance == null)
        {
            Debug.Log("[SpawnMonster] CirclePath가 준비되지 않았습니다.");
            return false;
        }

        _loadingStartAngle = Random.Range(0f, Mathf.PI * 2f); //무작위 각도 뽑기(0~ 360)
        Vector3 spawnPosition = CirclePath.Instance.GetPathPosition(_loadingStartAngle);// 정해진 각도를 이용해 실제 맵의 위치 구하기

        _loadingMonsterData = monsterData; //로딩이 끝난 뒤 몬스터 정보를 넘겨주기 위해

        Addressables.InstantiateAsync(monsterData.ModelPath, spawnPosition, Quaternion.identity).Completed += OnMonsterSpawned;

        return true;
    }

   //3D 모델 로딩이 완전히 끝났을 때 자동으로 실행해주는 콜백함수
    private void OnMonsterSpawned(AsyncOperationHandle<GameObject> handle)
    {

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("[SpawnMonster] 모델 로드 실패: " + _loadingMonsterData.ModelPath);
            return;
        }



        GameObject monsterObject = handle.Result;

        MonsterMove monsterMove = monsterObject.GetComponent<MonsterMove>();
        MonsterAttack monsterAttack = monsterObject.GetComponent<MonsterAttack>();

        if (monsterMove == null)
        {
            monsterMove = monsterObject.AddComponent<MonsterMove>();
        }

        if (monsterAttack == null)
        {
            monsterAttack = monsterObject.AddComponent<MonsterAttack>();
        }
        //몬스터 초기 정보 설정
        monsterMove.MonsterMoveInit(_loadingMonsterData, CirclePath.Instance, _loadingStartAngle);

        monsterAttack.MonsterAttackInit(_loadingMonsterData);

    }
}
