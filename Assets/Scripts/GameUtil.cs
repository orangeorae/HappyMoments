using UnityEngine;

public class GameUtil
{
    public static void LoadFullData()// 게임 로딩할 때 불러올 데이터
    {
        DataManager.Instance.LoadItemData("ItemData");
        DataManager.Instance.LoadMonsterData("MonsterData");

    }
}
