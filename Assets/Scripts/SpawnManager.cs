using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{

    [Header("카드 슬롯")]
    [SerializeField] private GameObject _cardSlotPrefab; // 카드 프리팹
    [SerializeField] private Transform _slotContainer; // 카드들이 놓일 부모 오브젝트
    [SerializeField] private int _slotCount = 4; // 카드 최대 수 

    private int _currentStage = 1;


    [Header("소환 버튼")]
    [SerializeField] private Button Button_Spawn;
    [SerializeField] private int _spawnCost = 10; //소환 비용
    [SerializeField] private int _currentMoney = 100; //임시로 관리 나중에 별도로 분리

    [Header("필드 소환(아이템)")]
    [SerializeField] private SpawnModel spawnModel;  // 카드를 선택했을 때 실제 3D 모델을 배치해주는 스크립트

    [Header("필드 소환(몬스터)")]
    [SerializeField] private SpawnMonster _spawnMonster;
    [SerializeField] private float _spawnMonsterSecond = 3f; // 몬스터 나오는 시간 간격



    //화면에 떠있는 카드 슬롯 저장리스트
    private List<SpawnCardSlot> _slots = new List<SpawnCardSlot>(); 

    // 현재 시기에서 뽑힐 수 있는 아이템 목록 
    private List<ItemData> _currentItems = new List<ItemData>();

    private SpawnCardSlot selectSlot; //선택된 슬롯

    private int _currentWave;

    private List<MonsterData> _currentMonsters = new List<MonsterData>();

    private int _currentMonsterIndex;

    private float _monsterSpawnTimer; // 다음 몬스토 소환까지 남은 시간을 재기 위함   

    private bool _isBossSpawned;

    private void OnEnable()
    {
        Button_Spawn.onClick.AddListener(OnClick_SpawnButton);
    }

    private void OnDisable()
    {
        if (Button_Spawn != null)
        {
            Button_Spawn.onClick.RemoveAllListeners();
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged -= HandleStageChanged;
            StageManager.Instance.OnWaveChanged -= HandleWaveChanged;
        }

        foreach (SpawnCardSlot slot in _slots)
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

        StageManager.Instance.OnStageChanged += HandleStageChanged;
        StageManager.Instance.OnWaveChanged += HandleWaveChanged;

        // 시작할 때 StageManager가 갖고있는 현재 스테이지 값으로 맞춰줌
        _currentStage = StageManager.Instance.CurrentStage;
        _currentWave = StageManager.Instance.CurrentWave;


        RefreshCard();
        RefreshMonsterList();
    }

    private void Update()
    {
        if(_currentWave == 3 && _isBossSpawned)
        {
            return;
        }

        _monsterSpawnTimer -= Time.deltaTime;

        if (_monsterSpawnTimer <= 0f)
        {
            SpawnNextMonster();
        }
    }

    private void CreateSlots() // 슬롯 생성 
    {
        for(int i =0; i< _slotCount; i++)
        {
            GameObject newSlot = Instantiate(_cardSlotPrefab, _slotContainer);
            SpawnCardSlot  cardSlot = newSlot.GetComponent<SpawnCardSlot>();
            _slots.Add(cardSlot);

            cardSlot.OnCardSelect += OnClick_CardSlot;
        }
    }

    //스테이지가 바뀌었을 때 뽑을 수 있는 아이템 새로고침
    private void RefreshCard()
    {
        _currentItems = DataManager.Instance.GetItemByStage(_currentStage);
        selectSlot = null;


        if (_currentItems == null || _currentItems.Count == 0) 
        {
            foreach (SpawnCardSlot slot in _slots) 
            {
                slot.Clear();
            }

            return;
        }

        foreach (SpawnCardSlot slot in _slots)
        {
            ItemData randomItem = GetRandomItem();
            slot.SetItem(randomItem);
        }

    }

    private ItemData GetRandomItem() // 확률에 맞는 아이템 구하기
    {
        int total = 0;
        foreach (ItemData item in _currentItems)
        {
            total += item.Probability; // 각 아이템의 확률 더해주기 
        }

        int randomNumber = Random.Range(0, total);

        int sum = 0;
        foreach(ItemData item in _currentItems)
        {
            sum += item.Probability;
            if(randomNumber < sum)
            {
                return item;
            }
        }

        return _currentItems[0]; //혹시 못찾으면 첫번째 아이템 반환
    }

    private void OnClick_SpawnButton()
    {

        if(selectSlot  == null)
        {
            Debug.Log("[SpawnManager] 먼저 소환할 카드를 선택해주세요");
            return;
        }

        ItemData selectItem = selectSlot.CurrentItem;


        if(_currentMoney < _spawnCost)
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
        _currentMoney -= _spawnCost;

        ItemData newItem = GetRandomItem();

        selectSlot.SetItem(newItem);

        selectSlot = null;

        Debug.Log($"[SpawnManager] {selectItem.ItemName} 소환 완료");

        Debug.Log($"[SpawnManager] {_currentMoney} 소환 완료");
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

    private void HandleStageChanged(int newStage)
    {
        _currentStage = newStage;
        RefreshCard();

        _currentWave = StageManager.Instance.CurrentWave;
        RefreshMonsterList();

        Debug.Log($"[SpawnManager] 스테이지 {_currentStage}로 갱신 -> 아이템 목록 재설정");
    }

    private void HandleWaveChanged(int newWave)
    {
        _currentWave = newWave;
        RefreshMonsterList();

        Debug.Log($"[SpawnManager] 웨이브 {_currentWave}로 갱신");
    }

    private void RefreshMonsterList() // 현재 스테이지와 웨이브에 맞는 몬스터 세팅
    {
        _currentMonsters = DataManager.Instance.GetMonsterByStage(_currentStage, _currentWave);
        _currentMonsterIndex = 0;
        _isBossSpawned = false;
        _monsterSpawnTimer = 0f;
    }

    private void SpawnNextMonster() //소환 지시 
    {
        if(_currentMonsters == null || _currentMonsters.Count == 0)
        {
            return;
        }

        MonsterData nextMonster = GetNextMonsterData();
           
        if( nextMonster == null)
        {
                return;
        }

            bool hasSpawn = _spawnMonster.SpawnMonsterInstance(nextMonster);    

            if(hasSpawn == false)
            {
                Debug.Log("[SpawnManager]몬스터 소환 실패");
                return;
            }

            _monsterSpawnTimer = _spawnMonsterSecond;
        }

     private MonsterData GetNextMonsterData() // 소환될 몬스터 뽑아주기 
    {
        
            if (_currentWave == 3)
            {
                foreach (MonsterData monsterData in _currentMonsters)
                {
                    if (monsterData.Tier == "3")
                    {
                        _isBossSpawned = true;
                        return monsterData;
                    }
                }

                return null;
            }

            MonsterData result = _currentMonsters[_currentMonsterIndex];

            _currentMonsterIndex++;

            if (_currentMonsterIndex >= _currentMonsters.Count)
            {
                _currentMonsterIndex = 0;
            }

            return result;
     }
}
