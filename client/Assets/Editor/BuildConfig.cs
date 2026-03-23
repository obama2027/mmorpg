using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BuildConfig", menuName = "MMORPG/Bundle/BuildConfig")]
public sealed class BuildConfig : ScriptableObject
{
    public const string AssetPath = "Assets/Env/BuildConfig.asset";

    [SerializeField]
    private string version = "1.0.0";

    [SerializeField]
    private bool editorDevelopmentMode = true;

    [SerializeField]
    private List<string> buildRootList = new List<string>();

    public string Version => version;
    public bool EditorDevelopmentMode => editorDevelopmentMode;
    public IReadOnlyList<string> BuildRootList => buildRootList;
}