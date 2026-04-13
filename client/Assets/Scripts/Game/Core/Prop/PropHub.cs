using System;
using System.Collections.Generic;
using UnityEngine;


public static class PropHub
{
    private static Dictionary<string, PropEntry> dic = new Dictionary<string, PropEntry>();

    public static void CreateProp<T>(string key, bool hasSignal = true)
    {
        if (!dic.ContainsKey(key))
            return;

    }

    public static bool HasProp(string key)
    {
        return dic.ContainsKey(key);
    }

    
    public static void SetProp(string key, object value)
    {
        
    }

    
    //public static PropHandle GetProp(string key)
    //{
        
    //}

    public static void RemoveProp(string key)
    {
        
    }

    public static void ClearAll()
    {
        
    }

}
