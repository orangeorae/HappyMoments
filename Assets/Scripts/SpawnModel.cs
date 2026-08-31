using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnModel : MonoBehaviour
{
    [Header("소환 위치 오브젝트")]
    [SerializeField] private GameObject[] spawnPoint; // 소환 위치로 사용할 오브젝트들 

    [SerializeField] private float spawnHeight = 0f; //바닥 기준으로 띄울 높이 

    private bool[] isUseSeat; // 각 소환 위치가 사용중인지 체크하기 위함 


    /* 어드레서블은 로드가 끝날 때까지 시간이 걸리니까
     놓을 곳, 크기 정보를 미리 저장해 뒀다가 로드가 끝내면 꺼내쓰기 위함 */
    private GameObject _loadingSpawnPoint;

    private float _loadingModelScale;

    private ItemData _loadingItemData;
    

    private void Awake()
    {
        isUseSeat = new bool[spawnPoint.Length]; 
    }

    public bool SpawnItemModel(ItemData item) //아이템 모델 빈자리에 소환 요청
    {
        int emptyIndex = EmptySpawnIndex();

        if (emptyIndex == -1)
        {
            Debug.Log("빈 소환자리가 없습니다.");

            return false;
        }

        isUseSeat[emptyIndex] = true; //선택한 자리 true로 변경

        GameObject selectSpawnPoint = spawnPoint[emptyIndex];
        
        _loadingSpawnPoint = selectSpawnPoint;
        _loadingModelScale = item.ModelScale;
        _loadingItemData = item;
        Addressables.InstantiateAsync(item.ModelPath).Completed +=OnModelSpawnData; // 비동기 로드 후 모델에 위치 크기 적용

        return true;
    }

   
    private void OnModelSpawnData(AsyncOperationHandle<GameObject> handle) //모델 로드가 끝났을 때 위치, 회전, 크기 적용
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject spawnModel = handle.Result;
            GameObject selectSpawnPoint = _loadingSpawnPoint; // 저장해둔 그루터기 넣기

            //회전 적용
            // 프리팹 자체가 가지고 있는 회전 값에 그루터기의 기울어진 각도를 곱해서 표면에 맞추도록 함
            Quaternion modelBaseRotation = spawnModel.transform.rotation;
            spawnModel.transform.rotation = selectSpawnPoint.transform.rotation * modelBaseRotation;

            //크기 적용
            spawnModel.transform.localScale = Vector3.one * _loadingModelScale;

            //일단 그루터기 좌표에 먼저 놓기(모델 원점 위치)
            spawnModel.transform.position = selectSpawnPoint.transform.position;
            
            // 모델의 실제 렌더링 크기를 재서 그루터기 위에 맞게 위치 보정
            DetailSpawnPoint(spawnModel, selectSpawnPoint);

            //모델을 그루터기 자식으로 넣기 위함 (true를 넘기면 지금 위치, 회전값을 그대로 유지한 채 부모만 바뀌도록)
            spawnModel.transform.SetParent(selectSpawnPoint.transform, true);

            ItemAttack itemAttack = spawnModel.GetComponent<ItemAttack>();

            if (itemAttack == null) 
            {
                itemAttack = spawnModel.AddComponent<ItemAttack>();
            }

            itemAttack.ItemAttackInit(_loadingItemData);
        }
    }
    
    /* 모델의 실제 Bounds를 측정해서 그루터기 위해 정확하게 올리기 위함
     * 그냥 모델의 원점(pivot) 좌표를 그루터기 좌표에 맞추면
     * 원점이 모델 중앙이 아니라 다른곳에 있는 모델은 위치가 이상하게 나오기 때문(그루터기가 기울어져있는 것도 고려)*/

    private void DetailSpawnPoint(GameObject spawnModel, GameObject spawnPointObject)
    {
        Renderer[] modelRenderers = spawnModel.GetComponentsInChildren<Renderer>(); //모델의 Renderer 찾아오기

        Renderer pointRenderer = spawnPointObject.GetComponentInChildren<Renderer>(); // 그루터기의 Renderer 찾아오기

        if(modelRenderers.Length == 0 || pointRenderer == null)
        {
            Debug.LogWarning($"[SpawnModel] {spawnModel.name} 또는 {spawnPointObject.name}에 Renderer가 없어 위치 보정 불가");
            return;
        }

        //모델의 여러 Renderer를 하나로 합쳐서 모델 전체를 감싸는 Bounds를 구한다.
        // Encapsulate 는 기존 Bounds에 새 Bounds를 합쳐서 둘 다 포함하는 더 큰 Bounds로 확장해주는 함수
        Bounds modelBounds = modelRenderers[0].bounds;

        foreach (Renderer renderer in modelRenderers)
        {
            modelBounds.Encapsulate(renderer.bounds);
        }

        //그루터기 Bounds
        Bounds pointBounds = pointRenderer.bounds;

        //그루터기 자체의 방향을 세 축으로 값을 얻는다.
        Vector3 up = spawnPointObject.transform.up.normalized;
        Vector3 right = spawnPointObject.transform.right.normalized;
        Vector3 forward = spawnPointObject.transform.forward.normalized;


        /* modelMinUp -> 모델을 up 방향으로 쟀을 때 가장 낮은 지점 (모델의 바닥)
         * pointMaxUp -> 그루터기를 up 방향으로 쟀을 때 가장 높은 지점 (그루터기의 맨 윗면)
          _ (언더바)는 값은 받아야하는 구조지만 이 값은 쓸 일이 없을 때 대체용*/
        GetExtentAxis(modelBounds, up, out float modelMinUp, out _);
        GetExtentAxis(pointBounds, up, out _, out float pointMaxUp);

        //살짝 더 그루터기 윗면에서 모델을 띄우고 싶을 때를 위함 
        float offsetUp = pointMaxUp - modelMinUp + spawnHeight;
        

        // 오른쪽이랑 앞 방향 보정해서 모델 중심을 그루터기 중심에 맞추기 위함
        float offsetRight = GetCenterAxis(pointBounds, right) - GetCenterAxis(modelBounds, right);
        float offsetForward = GetCenterAxis(pointBounds, forward) - GetCenterAxis(modelBounds, forward);

        Vector3 offset = (up * offsetUp) + (right * offsetRight) + (forward * offsetForward);

        spawnModel.transform.position += offset; // 계산한 이동량만큼 모델 옮기기
    }


    // Bounds를 특정 방향으로 투영했을 때 가장 작은 값과 가장 큰 값을 구하는 함수
    private void GetExtentAxis(Bounds bounds, Vector3 axis, out float min, out float max)
    {
        float center = Vector3.Dot(bounds.center, axis); // Ex) Vector3.Dot(점, 방향) -> 그 점이 그 방향으로 얼마나 멀리있는지 숫자 하나로 알려주는 것

        //박스의 절반 너비를 axis 방향 기준으로 환산
        float halfWidth = Mathf.Abs(axis.x * bounds.extents.x) +Mathf.Abs(axis.y * bounds.extents.y) + Mathf.Abs(axis.z * bounds.extents.z);

        min = center - halfWidth;
        max = center + halfWidth;
    }

    //Bounds의 중심점을 axis 방향으로 투영한 값 (중심 맞추기)
    private float GetCenterAxis(Bounds bounds, Vector3 axis)
    {
        return Vector3.Dot(bounds.center, axis);
    }
    private int EmptySpawnIndex() // 비어있는 소환 자리중 하나를 랜덤으로 골라 인덱스 반환
    {
        int emptyCount = 0;

        foreach (bool isSeat in isUseSeat)
        {
            if (isSeat == false)
            {
                emptyCount++;
            }
        }

        if (emptyCount == 0)
        {
            return -1;
        }

        int randomSeat = Random.Range(0, emptyCount);

        int currentEmptyCount = 0;

        for (int i = 0; i < isUseSeat.Length; i++)
        {
            if (isUseSeat[i] == false)
            {
                if (currentEmptyCount == randomSeat)
                {
                    return i;
                }

                currentEmptyCount++;
            }
        }

        return -1;
    }


}
