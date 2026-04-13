using System;
using UnityEngine;

public class Main : MonoBehaviour
{

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        GameEntry.Instance.Init();
    }
}
