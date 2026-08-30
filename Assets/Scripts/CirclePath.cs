using UnityEngine;

public class CirclePath : MonoBehaviour
{
    private const float RoadCenterRadius = 0.85f; // 바닥 크기 대비 실제 길이 위치할 비율 

    private float PathHeight = 0.02f;

    public static CirclePath Instance { get; private set; }

    private Vector3 _localCenterPosition; // 바닥의 중심점

    private float _localRadius; //바닥의 반지름

    private float _worldRadius; // 실제 반지름 크기

    public Vector3 CenterPosition // 원형 길의 중심 구하기 
    {
        get { return GetPathPosition(0f) - transform.right * _worldRadius; } 
    }

    public float Radius // 다른 스크립트에서 이동속도를 각도를 반영한 속도로 바꿀 때 사용할 실제 반지름 
    {
        get { return _worldRadius; }
    }

    private void Awake()
    {
        Instance = this; //게임 시작 시 자신을 등록 

        MeshFilter meshFilter = GetComponent<MeshFilter>();


        if(meshFilter == null)
        {
            return;
        }

        if(meshFilter.sharedMesh == null)
        {
            return;
        }

        Bounds localBounds = meshFilter.sharedMesh.bounds; //바닥의 경계 영역 정보 가져오기

        _localCenterPosition = localBounds.center; // 바닥 정중앙좌표를 궤도의 중심으로 저장

        float localHalfSize = Mathf.Min(localBounds.extents.x, localBounds.extents.z); // x와 z 중 좀 더 작은 값을 기준으로 절반 크기 구하기 

        _localRadius = localHalfSize * RoadCenterRadius; // 구해진 절반 크기에 실제 위치 비율을 곱해서 바깥으로 떨어지지 않도록 함

        float planeSizeX = Mathf.Abs(transform.lossyScale.x); // 유니티 화면에서 오브젝트의 스케일을 늘리거나 줄였을 경우에 대비해 실제 비율 가져오기
        float planeSizeZ =Mathf.Abs(transform.lossyScale.z);

        float averageScale = (planeSizeX + planeSizeZ) * 0.5f; // x와 z의 평균값 구하기

        _worldRadius = _localRadius * averageScale; // 로컬 반지름에 Scale 곱해서 게임 월드 반지름 구하기
    }

    public Vector3 GetPathPosition(float angle) // 몬스터의 현재 각도를 넣으면 해당 각도에 맞는 3D 월드 좌표를 반환
    {
        float x = Mathf.Cos(angle) * _localRadius;
        float z = Mathf.Sin(angle) * _localRadius;

        Vector3 localPosition = _localCenterPosition + new Vector3(x, 0f, z);

        //TransformPoint를 써서 오브젝트 내부 로컬 좌표를 게임 세상의 실제 월드 좌표로 반환
        Vector3 worldPosition = transform.TransformPoint(localPosition); 

        worldPosition += transform.up * PathHeight;

        return worldPosition;
    }
}
