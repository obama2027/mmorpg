using System.Collections;
using UnityEngine;

public class Launch : MonoBehaviour
{
    [SerializeField] private bool isDebug = false;
    [SerializeField] private string resServerBaseUrl = "http://localhost:8080";

    private void Start()
    {
        Debug.Log($"[Launch] persistentDataPath={Application.persistentDataPath}");

        if (isDebug)
        {
            Debug.Log("[Launch] isDebug=true, skip initial copy.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(resServerBaseUrl))
        {
            BaseDllUpdater.ResServerBaseUrl = resServerBaseUrl.Trim();
        }

        StartCoroutine(InitializePersistentAssetsIfNeeded());
    }

    private IEnumerator InitializePersistentAssetsIfNeeded()
    {
        yield return PersistentAssetCopier.CopyIfNeeded();
        yield return BaseDllUpdater.CheckAndUpdateIfNeeded();
    }
}
