using UnityEngine;

[System.Serializable]
public class GameDataBase //JSON 기획 데이터 클래스의 최상위 부모 클래스
{
    public string Id;
}


[System.Serializable]
public class ItemData : GameDataBase //농작물 세부 기획 데이터 클래스
{
    public int Stage;
    public string Tier;
    public string ItemName;
    public int Probability;
    public string IconPath;

}
