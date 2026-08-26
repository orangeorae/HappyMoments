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

    [Header("필드 소환")]
    [SerializeField] private SpawnModel spawnModel;  // 카드를 선택했을 때 실제 3D 모델을 배치해주는 스크립트

    //화면에 떠있는 카드 슬롯 저장리스트
    private List<SpawnCardSlot> slots = new List<SpawnCardSlot>(); 

    // 현재 시기에서 뽑힐 수 있는 아이템 목록 
    private List<ItemData> currentItems = new List<ItemData>();

    private SpawnCardSlot selectSlot;

    private void OnEnable()
    {
        Button_Spawn.onClick.AddListener(OnClick_SpawnButton);
    }

    private void OnDisable()
    {
        Button_Spawn.onClick.RemoveAllListeners();

        foreach (SpawnCardSlot slot in slots)
        {
            if(slot != null)
            {
                slot.OnCardSelect -= OnClick_CardSlot;
            }
        }
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

            cardSlot.OnCardSelect += OnClick_CardSlot;
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

        if(selectSlot  == null)
        {
            Debug.Log("[SpawnManager] 먼저 소환할 카드를 선택해주세요");
            return;
        }

        ItemData selectItem = selectSlot.CurrentItem;


        if(currentMoney < spawnCost)
        {
            Debug.Log("재화가 부족합니다.");
            return;
        }
        

        //3D 모델 소환
        bool hasSpawn = spawnModel.SpawnItemModel(selectItem);

       if(hasSpawn == false)
        {
            Debug.Log("[SpawnManager] 소환 실패 / 기존 카드를 유지합니다.");
            return;
        }
        currentMoney -= spawnCost;

        ItemData newItem = GetRandomItem();

        selectSlot.SetItem(newItem);

        selectSlot = null;

        Debug.Log($"[SpawnManager] {selectItem.ItemName} 소환 완료");

        Debug.Log($"[SpawnManager] {currentMoney} 소환 완료");
    }

    private void OnClick_CardSlot(SpawnCardSlot clickSlot)
    { 
        if(clickSlot == null)
        {
            return;
        }

        selectSlot = clickSlot;

        Debug.Log($"[SpawnManager] 카드 선택: {selectSlot.CurrentItem.ItemName}");
    }
}
