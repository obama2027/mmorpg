using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launch : MonoBehaviour
{
    [SerializeField] private bool isDebug = true;

    private void Start()
    {
        Debug.Log($"[Launch] isDebug={Config.isDebug}");
        Debug.Log($"[Launch] persistentDataPath={Application.persistentDataPath}");

        if (Config.isDebug)
        {
            EnterGame();
        }
        else
        {
            StartCoroutine(StartCheck());
        }
    }

    private IEnumerator StartCheck()
    {
        yield return PersistentAssetCopier.CopyIfNeeded();
        yield return BaseDllUpdater.CheckAndUpdateIfNeeded();
        yield return BaseDllLoader.RunBaseHotUpdate();
        Debug.Log("[Launch] Base hot update completed. Launch flow finished.");
    }

    private void EnterGame()
    {
        string MainSceneAssetPath = "Assets/GameAssets/Scenes/Main/Main.scene";
        var op = SceneManager.LoadSceneAsync(MainSceneAssetPath, LoadSceneMode.Single);
        if (op == null)
        {
            Debug.LogError($"[GameManager] Load Main.scene failed, scene={MainSceneAssetPath}");
        }
    }
}
