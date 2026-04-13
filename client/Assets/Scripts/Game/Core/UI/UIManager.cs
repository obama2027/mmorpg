using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIManager : MonoSingle<UIManager>
{

    [SerializeField] private Transform uiRoot;
    [SerializeField] private int windowOrderStep = 100;
    [SerializeField] private List<UIConfig> configs = new List<UIConfig>
    {
        new UIConfig
        {
            type = UIType.Login,
            defaultLayer = UILayer.FullScreen,
            bundlePath = "GameAssets/UI/Login",
            assetPath = "Prefab/Login.prefab",
        },
        new UIConfig
        {
            type = UIType.Main,
            defaultLayer = UILayer.FullScreen,
            bundlePath = "GameAssets/UI/Main",
            assetPath = "Prefab/Main.prefab",
        },
    };

    private readonly Dictionary<UIType, UIConfig> _configMap = new Dictionary<UIType, UIConfig>();
    private readonly Dictionary<UIType, UIWindow> _windowMap = new Dictionary<UIType, UIWindow>();
    private readonly Dictionary<UILayer, Transform> _layerRoots = new Dictionary<UILayer, Transform>();
    private readonly Dictionary<UILayer, int> _layerOrderIndex = new Dictionary<UILayer, int>();

    private Canvas _rootCanvas;

    private static readonly Dictionary<UILayer, string> LayerNodeNames = new Dictionary<UILayer, string>
    {
        { UILayer.Background, "Layer_Background" },
        { UILayer.MainHud, "Layer_MainHud" },
        { UILayer.FullScreen, "Layer_Fullscreen" },
        { UILayer.Popup, "Layer_Popup" },
        { UILayer.System, "Layer_System" },
        { UILayer.Guide, "Layer_Guide" },
        { UILayer.Toast, "Layer_Toast" },
        { UILayer.Blocker, "Layer_Blocker" },
    };

    private bool _isInit = false;
    public override void Init()
    {
        if(_isInit)return;
        _isInit = true;
        EnsureRootAndLayers();
        BuildConfigMap();
    }

    public async Task<UIWindow> Open(UIType type, UILayer? forceLayer = null, object args = null)
    {
        if (!_configMap.TryGetValue(type, out var cfg))
        {
            Debug.LogError($"[UIManager] Config not found: {type}");
            return null;
        }

        if (_windowMap.TryGetValue(type, out var existing) && existing != null)
        {
            var targetLayer = forceLayer ?? existing.Layer;
            var targetRoot = GetLayerRoot(targetLayer);
            if (existing.transform.parent != targetRoot)
            {
                existing.transform.SetParent(targetRoot, false);
            }

            FitWindowRootToLayer(existing.gameObject);
            existing.Layer = targetLayer;
            ApplyWindowSorting(existing.gameObject, targetLayer);
            if (!existing.gameObject.activeSelf)
            {
                existing.gameObject.SetActive(true);
            }

            existing.OnShow(args);
            return existing;
        }

        if (GameAssetService.Instance == null || !GameAssetService.Instance.IsReady)
        {
            Debug.LogError("[UIManager] GameAssetService is not ready.");
            return null;
        }

        AssetHandle<GameObject> handle;
        try
        {
            handle = await GameAssetService.Instance.LoadAssetByPathAsync<GameObject>(cfg.bundlePath, cfg.assetPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UIManager] Load UI prefab failed: type={type}, error={ex.Message}");
            return null;
        }

        if (handle == null || handle.Asset == null)
        {
            Debug.LogError($"[UIManager] UI prefab not found: type={type}, path={cfg.bundlePath}/{cfg.assetPath}");
            return null;
        }

        var layer = forceLayer ?? cfg.defaultLayer;
        var parent = GetLayerRoot(layer);
        var go = Instantiate(handle.Asset, parent, false);
        var window = go.GetComponent<UIWindow>();
        if (window == null)
        {
            Debug.LogError($"[UIManager] UIWindow component missing: type={type}, prefab={go.name}");
            Destroy(go);
            return null;
        }

        FitWindowRootToLayer(go);
        window.Type = type;
        window.Layer = layer;

        ApplyWindowSorting(go, layer);
        window.IsCreated = true;
        window.OnCreate();
        window.OnShow(args);
        _windowMap[type] = window;
        return window;
    }

    public void Close(UIType type, bool destroy = false)
    {
        if (!_windowMap.TryGetValue(type, out var window) || window == null)
        {
            return;
        }

        window.OnHide();
        if (!destroy)
        {
            window.gameObject.SetActive(false);
            return;
        }

        window.OnDestroyUI();
        _windowMap.Remove(type);
        Destroy(window.gameObject);
    }

    public void CloseAll(bool destroy = false)
    {
        var keys = new List<UIType>(_windowMap.Keys);
        for (var i = 0; i < keys.Count; i++)
        {
            Close(keys[i], destroy);
        }
    }

    /// <summary>
    /// Stretch window root to the layer parent (full rect) and normalize scale so it displays centered in the layer.
    /// </summary>
    private static void FitWindowRootToLayer(GameObject windowRoot)
    {
        var rt = windowRoot.GetComponent<RectTransform>();
        if (rt == null)
        {
            return;
        }

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition3D = Vector3.zero;
        rt.localScale = Vector3.one;
    }

    private void ApplyWindowSorting(GameObject uiObject, UILayer layer)
    {
        var order = NextOrder(layer);
        var canvas = uiObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = uiObject.AddComponent<Canvas>();
        }

        // Each window owns its sorting segment to avoid cross-window FX overlap.
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;
        canvas.renderMode = _rootCanvas != null ? _rootCanvas.renderMode : RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = _rootCanvas != null ? _rootCanvas.worldCamera : null;
        canvas.planeDistance = 100f;

        if (uiObject.GetComponent<GraphicRaycaster>() == null)
        {
            uiObject.AddComponent<GraphicRaycaster>();
        }
    }

    private int NextOrder(UILayer layer)
    {
        if (!_layerOrderIndex.TryGetValue(layer, out var index))
        {
            index = 0;
        }

        _layerOrderIndex[layer] = index + 1;
        return (int)layer + index * Mathf.Max(1, windowOrderStep);
    }

    private Transform GetLayerRoot(UILayer layer)
    {
        if (_layerRoots.TryGetValue(layer, out var root) && root != null)
        {
            return root;
        }

        throw new InvalidOperationException($"UI layer root missing: {layer}");
    }

    private void BuildConfigMap()
    {
        _configMap.Clear();
        for (var i = 0; i < configs.Count; i++)
        {
            var cfg = configs[i];
            if (cfg == null || cfg.type == UIType.None)
            {
                continue;
            }

            _configMap[cfg.type] = cfg;
        }
    }

    private void EnsureRootAndLayers()
    {
        if (uiRoot == null)
        {
            var rootGo = GameObject.Find("UIRoot");
            if (rootGo != null)
            {
                uiRoot = rootGo.transform;
            }
        }

        if (uiRoot == null)
        {
            var go = new GameObject("UIRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            uiRoot = go.transform;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2622f, 1206f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        _rootCanvas = uiRoot.GetComponent<Canvas>();
        if (_rootCanvas == null)
        {
            _rootCanvas = uiRoot.gameObject.AddComponent<Canvas>();
        }

        if (uiRoot.GetComponent<GraphicRaycaster>() == null)
        {
            uiRoot.gameObject.AddComponent<GraphicRaycaster>();
        }

        _layerRoots.Clear();
        foreach (var kv in LayerNodeNames)
        {
            var child = uiRoot.Find(kv.Value);
            if (child == null)
            {
                var go = new GameObject(kv.Value, typeof(RectTransform));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(uiRoot, false);
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                child = rt;
            }

            _layerRoots[kv.Key] = child;
        }
    }
}

