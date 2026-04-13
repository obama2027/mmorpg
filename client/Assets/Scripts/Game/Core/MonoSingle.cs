using UnityEngine;

public abstract class MonoSingle<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject main = GameObject.Find("Main");
                if (main != null)
                {
                    _instance = main.GetComponent<T>();
                    if (_instance == null)
                    {
                        _instance = main.AddComponent<T>();
                    }
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this as T;
    }

    public virtual void Init(){
        
    }
}