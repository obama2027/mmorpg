using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public static class UIBinder
{
    private static readonly Dictionary<string, Type> PrefixToComponentType =
        new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "btn", typeof(Button) },
            { "img", typeof(Image) },
        };

    public static void RegisterPrefix(string prefix, Type componentType)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException("prefix is null or empty.", nameof(prefix));
        }

        if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
        {
            throw new ArgumentException("componentType must derive from UnityEngine.Component.", nameof(componentType));
        }

        PrefixToComponentType[prefix] = componentType;
    }

    public static void Bind(Transform root, object uiContainer)
    {
        if (root == null || uiContainer == null)
        {
            return;
        }

        var fieldInfos = uiContainer.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fieldInfos == null || fieldInfos.Length == 0)
        {
            return;
        }

        var nodeMap = BuildNodeMap(root);
        foreach (var fieldInfo in fieldInfos)
        {
            if (fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
            {
                continue;
            }

            if (!TryGetExpectedType(fieldInfo.Name, out var expectedComponentType))
            {
                continue;
            }

            if (!expectedComponentType.IsAssignableFrom(fieldInfo.FieldType))
            {
                continue;
            }

            if (!nodeMap.TryGetValue(fieldInfo.Name, out var targetNode) || targetNode == null)
            {
                continue;
            }

            var component = targetNode.GetComponent(fieldInfo.FieldType);
            if (component == null)
            {
                Debug.LogWarning($"[UIBinder] Component missing: node={fieldInfo.Name}, type={fieldInfo.FieldType.Name}");
                continue;
            }

            fieldInfo.SetValue(uiContainer, component);
        }
    }

    private static Dictionary<string, Transform> BuildNodeMap(Transform root)
    {
        var map = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        var children = root.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (child == null)
            {
                continue;
            }

            if (!IsBindingNodeName(child.name))
            {
                continue;
            }

            if (!map.ContainsKey(child.name))
            {
                map.Add(child.name, child);
            }
        }

        return map;
    }

    private static bool IsBindingNodeName(string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return false;
        }

        foreach (var prefix in PrefixToComponentType.Keys)
        {
            if (nodeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetExpectedType(string fieldName, out Type expectedComponentType)
    {
        expectedComponentType = null;
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        foreach (var kv in PrefixToComponentType)
        {
            if (!fieldName.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            expectedComponentType = kv.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Collects nodes whose names match a registered prefix (e.g. btn*, img*) and stores the
    /// matching component in <paramref name="byNodeName"/>. First occurrence wins per node name.
    /// </summary>
    public static void BindAuto(Transform root, Dictionary<string, Component> byNodeName)
    {
        if (root == null || byNodeName == null)
        {
            return;
        }

        byNodeName.Clear();
        var children = root.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (child == null)
            {
                continue;
            }

            var nodeName = child.name;
            if (!IsBindingNodeName(nodeName) || byNodeName.ContainsKey(nodeName))
            {
                continue;
            }

            if (!TryGetExpectedType(nodeName, out var expectedComponentType))
            {
                continue;
            }

            var component = child.GetComponent(expectedComponentType);
            if (component == null)
            {
                Debug.LogWarning($"[UIBinder] BindAuto: component missing on node={nodeName}, type={expectedComponentType.Name}");
                continue;
            }

            byNodeName.Add(nodeName, component);
        }
    }
}

