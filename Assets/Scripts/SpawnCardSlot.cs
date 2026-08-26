using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class SpawnCardSlot : MonoBehaviour
{
    public Image Image_Icon;
    public Text Text_Name;
    public Text Text_Tier;

    [Header("카드 선택")]
    [SerializeField] private Button Button_Select;
    public ItemData CurrentItem {  get; private set; }
    public bool IsEmpty //슬롯이 비어있는지 여부 알기 위함
    {
        get { return CurrentItem == null; }
    }

    public event Action<SpawnCardSlot> OnCardSelect; //카드 선택 시 발생 이벤트 

    private void OnEnable()
    {
        Button_Select.onClick.AddListener(OnClick_Card);
    }

    private void OnDisable()
    {
        Button_Select.onClick.RemoveAllListeners();
    }

    private void OnClick_Card()
    {
        if (IsEmpty)
        {
            return;
        }

            OnCardSelect?.Invoke(this);
    }

    public void SetItem(ItemData item) //아이템 세팅 
    {
        CurrentItem = item;

        if(item == null)
        {
            Clear();
            return;
        }

        Text_Name.text = item.ItemName;
        Text_Tier.text = item.Tier;

        if(Image_Icon != null)
        {
            Image_Icon.enabled = false;
        }

        Addressables.LoadAssetAsync<Sprite>(item.IconPath).Completed += OnIconSpriteLoad;

    }

    private void OnIconSpriteLoad(AsyncOperationHandle<Sprite> handle)
    {
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            if(Image_Icon != null){
                Image_Icon.sprite = handle.Result;
                Image_Icon.enabled = true;
            }
        }
        else
        {
            Debug.LogError($"[SpawnCardSlot] 카드 슬롯 이미지 Addressable 로드 실패");
        }
    }

    public void Clear()
    {
        CurrentItem = null;
        Image_Icon.sprite = null;
        Image_Icon.enabled = false;
        Text_Name.text = "";
        Text_Tier.text = "";
    }
}
