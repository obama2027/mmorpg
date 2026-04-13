using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class UIWindow : MonoBehaviour
{
    private readonly Dictionary<string, Component> _uiByNodeName =
        new Dictionary<string, Component>(StringComparer.OrdinalIgnoreCase);

    private readonly List<(Button button, UnityAction action)> _buttonClickHandlers = new List<(Button, UnityAction)>();

    public UIType Type { get; internal set; }
    public UILayer Layer { get; internal set; }
    public bool IsCreated { get; internal set; }

    /// <summary>Auto-bound nodes (prefix rules from <see cref="UIBinder"/>), keyed by hierarchy node name.</summary>
    protected IReadOnlyDictionary<string, Component> UIByNodeName => _uiByNodeName;

    protected Button GetButton(string nodeName)
    {
        return _uiByNodeName.TryGetValue(nodeName, out var c) ? c as Button : null;
    }

    protected Image GetImage(string nodeName)
    {
        return _uiByNodeName.TryGetValue(nodeName, out var c) ? c as Image : null;
    }

    protected T GetUiComponent<T>(string nodeName) where T : Component
    {
        return _uiByNodeName.TryGetValue(nodeName, out var c) ? c as T : null;
    }

    // Prefab instantiated and dependencies prepared.
    public virtual void OnCreate()
    {
        UIBinder.BindAuto(transform, _uiByNodeName);
        RegisterAutoBoundButtons();
    }

    // Logical open/show.
    public virtual void OnShow(object args)
    {
    }

    // Logical close/hide.
    public virtual void OnHide()
    {
    }

    // Final destroy callback before GameObject.Destroy.
    public virtual void OnDestroyUI()
    {
        UnregisterAutoBoundButtons();
        _uiByNodeName.Clear();
    }

    /// <summary>
    /// Framework dispatches clicks for every auto-bound <see cref="Button"/> (e.g. node names with btn* prefix).
    /// </summary>
    protected virtual void OnBtnClick(Button btn)
    {
    }

    private void RegisterAutoBoundButtons()
    {
        foreach (var kv in _uiByNodeName)
        {
            var btn = kv.Value as Button;
            if (btn == null)
            {
                continue;
            }

            var captured = btn;
            UnityAction handler = () => OnBtnClick(captured);
            btn.onClick.AddListener(handler);
            _buttonClickHandlers.Add((btn, handler));
        }
    }

    private void UnregisterAutoBoundButtons()
    {
        for (var i = 0; i < _buttonClickHandlers.Count; i++)
        {
            var entry = _buttonClickHandlers[i];
            if (entry.button != null)
            {
                entry.button.onClick.RemoveListener(entry.action);
            }
        }

        _buttonClickHandlers.Clear();
    }
}
