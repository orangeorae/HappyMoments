using UnityEngine;

[System.Serializable]
public class GameDataBase //JSON 기획 데이터 클래스의 최상위 부모 클래스
{
    public string Id;
}


[System.Serializable]
public class ItemData : GameDataBase //아이템 세부 기획 데이터 클래스
{
    public int Stage;
    public string StageName;
    public string Tier;
    public string ItemName;
    public int Probability;
    public string IconPath;
    public string ModelPath;
    public float ModelScale;

    public int Damage;
    public float AttackRange;
    public float AttackSpeed;

}

[System.Serializable]
public class MonsterData : GameDataBase //몬스터 세부 기획 데이터 클래스
{
    public int Stage;
    public string StageName;
    public int Wave;
    public string Tier;
    public string Name;
    public int Health;
    public int Attack;
    public float Speed;
    public float AttackSpeed;
    public string ModelPath;

}
