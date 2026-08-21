using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{

    [Header("카드 슬롯")]
    [SerializeField] private GameObject cardSlotPrefab; // 카드 프리팹
    [SerializeField] private Transform slotContainer; // 카드들이 놓일 부모 오브젝트
    [SerializeField] private int slotCount = 4; // 카드 최대 수 

    [Header("현재 시기")]
    [SerializeField] private int currentStage = 0;


    [Header("소환 버튼")]
    [SerializeField] private Button Button_Spawn;
    [SerializeField] private int spawnCost = 10; //소환 비용
    [SerializeField] private int currentMoney = 100; //임시로 관리 나중에 별도로 분리

    //화면에 떠있는 카드 슬롯 저장리스트
    private List<SpawnCardSlot> slots = new List<SpawnCardSlot>(); 

    // 현재 시기에서 뽑힐 수 있는 아이템 목록 
    private List<ItemData> currentItems = new List<ItemData>();

    private void OnEnable()
    {
        Button_Spawn.onClick.AddListener(OnClick_SpawnButton);
    }

    private void OnDisable()
    {
        Button_Spawn.onClick.RemoveAllListeners();
    }



    private void Start()
    {
        CreateSlots();

        //지금 시기에 맞는 아이템 가져오기 
        currentItems = DataManager.Instance.GetItemByStage(currentStage);

        for(int i =0; i< slots.Count; i++) // 슬롯을 랜덤 아이템으로 채우기 
        {
            ItemData randomItem = GetRandomItem(); //아이템 하나 뽑기
            slots[i].SetItem(randomItem); // 순차적으로 슬롯에 아이템 보여주기 
        }
    }

    private void CreateSlots() // 슬롯 생성 
    {
        for(int i =0; i< slotCount; i++)
        {
            GameObject newSlot = Instantiate(cardSlotPrefab, slotContainer);
            SpawnCardSlot  cardSlot = newSlot.GetComponent<SpawnCardSlot>();
            slots.Add(cardSlot);
        }
    }

    private ItemData GetRandomItem() // 확률에 맞는 아이템 구하기
    {
        int total = 0;
        foreach (ItemData item in currentItems)
        {
            total += item.Probability; // 각 아이템의 확률 더해주기 
        }

        int randomNumber = Random.Range(0, total);

        int sum = 0;
        foreach(ItemData item in currentItems)
        {
            sum += item.Probability;
            if(randomNumber < sum)
            {
                return item;
            }
        }

        return currentItems[0];
    }

    private void OnClick_SpawnButton()
    {
        if(currentMoney < spawnCost)
        {
            Debug.Log("재화가 부족합니다.");
            return;
        }

        int  emptySlotIndex = EmptySlotIndex();

        if(emptySlotIndex == -1)
        {
            Debug.Log("빈 슬롯이 없습니다.");
            return;
        }

        ItemData randomItem = GetRandomItem(); //아이템 뽑기 

        SpawnCardSlot targetSlot = slots[emptySlotIndex]; // 찾은 인덱스의 슬롯 가져오기 
        targetSlot.SetItem(randomItem); // 빈 슬롯에 랜덤 아이템 설정 

        currentMoney -= spawnCost;
    }

    // 슬롯리스트에서 비어있는 슬롯 인덱스 찾기 
    private int EmptySlotIndex()
    {
        for (int i = 0; i < slots.Count; i++) {
            if(slots[i] != null && slots[i].IsEmpty)
            {
                return i;
            }
        }
        return -1;
    }
}
