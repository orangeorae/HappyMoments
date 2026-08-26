using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnModel : MonoBehaviour
{
    [Header("소환 위치 오브젝트")]
    [SerializeField] private GameObject[] spawnPoint; // 소환 위치로 사용할 오브젝트들 

    [SerializeField] private float spawnHeight = 0f; //바닥 기준으로 띄울 높이 

    private float currentModelScale = 1.0f; //모델 크기 

    private Vector3 currentSpawnPosition;

    private Quaternion currentSpawnBaseRotation; // 모델의 원래 회전값 저장할 변수
    private bool[] isUseSeat; // 각 소환 위치가 사용중인지 체크하기 위함 

    private void Awake()
    {
        isUseSeat = new bool[spawnPoint.Length]; 
    }

    public bool SpawnItemModel(ItemData item) //아이템 모델 빈자리에 소환 
    {
        int emptyIndex = EmptySpawnIndex();

        if (emptyIndex == -1)
        {
            Debug.Log("빈 소환자리가 없습니다.");

            return false;
        }

        isUseSeat[emptyIndex] = true; //선택한 자리 true로 변경

        GameObject spawnModel = spawnPoint[emptyIndex];

        currentModelScale = item.ModelScale;

        currentSpawnPosition = GetSpawnTopCenter(spawnModel);
        currentSpawnBaseRotation = spawnModel.transform.rotation;

        Addressables.InstantiateAsync(item.ModelPath).Completed +=OnModelSpawnData; // 비동기 로드 후 모델에 위치 크기 적용

        Debug.Log($"[SpawnModel] {item.ItemName} 소환 완료 -> {spawnModel.name}");

        return true;
    }

    private Vector3 GetSpawnTopCenter(GameObject spawnPoint) // 소환 위치의 중앙 좌표 계산
    {
        Renderer pointRenderer = spawnPoint.GetComponentInChildren<Renderer>();
        if(pointRenderer == null)
        {
           return spawnPoint.transform.position;
        }

        Bounds bounds = pointRenderer.bounds;

        Vector3 topCenter = bounds.center;

        topCenter.y = bounds.max.y + spawnHeight;
   
        return topCenter;

    }
    private void OnModelSpawnData(AsyncOperationHandle<GameObject> handle) //모델 로드가 끝났을 때 위치, 회전, 크기 적용
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject spawnModel = handle.Result;


            Quaternion modelBaseRotation = spawnModel.transform.rotation;
            
            spawnModel.transform.rotation = currentSpawnBaseRotation * modelBaseRotation;

            spawnModel.transform.localScale = new Vector3(currentModelScale, currentModelScale, currentModelScale);

            spawnModel.transform.position = currentSpawnPosition;

            
        }
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
