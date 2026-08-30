using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    private CirclePath _circlePath;

    private float _moveSpeed;

    private float _currentAngle; // 현재 위치

    private float _circleRadius; // 반지름크기

 
    //몬스터가 처음 소환될 때 초기 설정 세팅
    public void MonsterMoveInit(MonsterData monsterData, CirclePath circlePath, float startAngle)
    {
        _circlePath = circlePath; // 걸어갈 길 지정
        _moveSpeed = monsterData.Speed; // csv 파일에서 몬스터 속도 가져오기
        _circleRadius = _circlePath.Radius; 
        _currentAngle = startAngle;

        transform.position = _circlePath.GetPathPosition(_currentAngle);
    }

    private void Update()
    {
        if(_circlePath == null || _circlePath.Radius  <= 0f)
        {
            return;
        }

        float angleSpeed = _moveSpeed / _circleRadius;
        _currentAngle += angleSpeed * Time.deltaTime; //프레임마다 걸린 시간만큼 각도를  더해서 전진 시키기

        transform.position = _circlePath.GetPathPosition(_currentAngle); // 계산된 각도를 바탕으로 좌표 갱신

    }

}
