using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get { return instance; }
    }

    //virtual -> 자식 클래스가 나중에 확장활 수 있게 
    protected virtual void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as T; // 아직 인스턴스가 없으면 이 오브젝트를 유일한 인스턴스로 등록
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
