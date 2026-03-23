using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class LoadDll : MonoBehaviour
{
    void Start()
    {
#if !UNITY_EDITOR
        byte[] dll = File.ReadAllBytes($"{Application.streamingAssetsPath}/HotUpdate.dll.bytes");
        Assembly hotUpdateAss = Assembly.Load(dll);
#else
        Assembly hotUpdateAss = AppDomain.CurrentDomain.GetAssemblies()
            .First(a => a.GetName().Name == "HotUpdate");
#endif

        Type type = hotUpdateAss.GetType("Hello");
        type.GetMethod("Run").Invoke(null, null);
    }
}