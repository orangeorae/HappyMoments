using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;
        GameUtil.LoadFullData();
    }

    [Serializable]
    private class SerializationWrapper<T>//JsonUtility의 한계를 극복하기 위한 Wrapper 클래스
    {
        public List<T> items; // wrappedJson에 넣은 items 키와 매칭 리스트
    }

    public Dictionary<string, ItemData> ItemDataList { get; private set; } = new Dictionary<string, ItemData>();

    private Dictionary<string, T> LoadData<T>(string tableName) where T : GameDataBase
    {
        // 경로 설정 (확장자 .json 제외)
        // Resources/JsonOutput 폴더
        string resourcePath = $"JsonOutput/{tableName}";

        // 리소스 로드
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        //  파일 존재 여부 체크
        if (textAsset == null)
        {
            Debug.LogError($"[Error] 리소스를 찾을 수 없습니다: Resources/{resourcePath}");
            return new Dictionary<string, T>();
        }

        try
        {
            string jsonString = textAsset.text;

            // JsonUtility용 Wrapper 트릭 적용
            string wrappedJson = "{\"items\":" + jsonString + "}";
            SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if (wrapper != null && wrapper.items != null)
            {
                Debug.Log($"{typeof(T).Name} 데이터를 {wrapper.items.Count}개 로드했습니다.");
                // ToDictionary를 사용하려면 각 클래스(T)에 Id 필드가 있어야 한다.
                return wrapper.items.ToDictionary(item => item.Id.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T>();
    }

    public void LoadItemData(string jsonPath) //아이템 데이터 로드 함수
    {
        ItemDataList = LoadData<ItemData>(jsonPath);
    }

    public ItemData GetItemData(string id) // TryGetValue를 통해  Dictionary에서  안전 + 빠르게 데이터 추출
    {
        if (ItemDataList == null || string.IsNullOrEmpty(id)) return null;

        return ItemDataList.TryGetValue(id, out var data) ? data : null;
    }
}
