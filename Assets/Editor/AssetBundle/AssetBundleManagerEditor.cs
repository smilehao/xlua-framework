using AssetBundles;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// added by wsh @ 2017.12.29
/// 功能：Assetbundle编辑器支持，方便调试用
/// </summary>


[CustomEditor(typeof(AssetBundleManager), true)]
public class AssetBundleManagerEditor : Editor
{
    static protected string[] displayTypes = new string[] {
        "Resident", "AssetBundles Caching", "Assets Caching",
        "Web Requesting", "Web Requester Queue", "Prosessing Web Requester",
        "Prosessing AssetBundle AsyncLoader", "Prosessing Asset AsyncLoader",
    };

    static protected int selectedTypeIndex = 6;
    static protected Dictionary<string, bool> abItemSate = new Dictionary<string, bool>();
    static protected Dictionary<string, bool> refrenceSate = new Dictionary<string, bool>();
    static protected Dictionary<string, bool> dependenciesSate = new Dictionary<string, bool>();
    static protected Dictionary<string, bool> abRefrenceSate = new Dictionary<string, bool>();
    static protected Dictionary<string, bool> webRequestRefrenceSate = new Dictionary<string, bool>();
    static protected Dictionary<string, bool> abLoaderfrenceSate = new Dictionary<string, bool>();

    static protected void ClearStates()
    {
        abItemSate.Clear();
        refrenceSate.Clear();
        dependenciesSate.Clear();
        abRefrenceSate.Clear();
        webRequestRefrenceSate.Clear();
        abLoaderfrenceSate.Clear();
    }

    static public void DrawPadding()
    {
        GUILayout.Space(18f);
    }

    public void OnEnable()
    {
        EditorApplication.update += Update;
    }

    public void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    public void Update()
    {
        Repaint();
    }
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("DisplayType:", GUILayout.MaxWidth(80f));
        var newSelectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, displayTypes);
        EditorGUILayout.EndHorizontal();

        if (newSelectedTypeIndex != selectedTypeIndex)
        {
            ClearStates();
        }
        selectedTypeIndex = newSelectedTypeIndex;
        OnRefresh(selectedTypeIndex);
    }

    public void OnRefresh(int selectedTypeIndex)
    {
        switch (selectedTypeIndex)
        {
            case 0:
                {
                    OnDrawAssetBundleResident();
                    break;
                }
            case 1:
                {
                    OnDrawAssetBundleCaching();
                    break;
                }
            case 2:
                {
                    OnDrawAssetCaching();
                    break;
                }
            case 3:
                {
                    OnDrawWebRequesting();
                    break;
                }
            case 4:
                {
                    OnDrawWebRequesterQueue();
                    break;
                }
            case 5:
                {
                    OnDrawProsessingWebRequester();
                    break;
                }
            case 6:
                {
                    OnDrawProsessingAssetBundleAsyncLoader();
                    break;
                }
            case 7:
                {
                    OnDrawProsessingAssetAsyncLoader();
                    break;
                }
        }
    }

    static public void DrawProperty(string title, string content)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(250f));
        EditorGUILayout.LabelField(title, GUILayout.MinWidth(200f));
        EditorGUILayout.LabelField(content, GUILayout.MinWidth(50f));
        EditorGUILayout.EndHorizontal();
    }

    static public bool DrawHeader(string text, Dictionary<string, bool> states, string key, bool forceOn, bool minimalistic, params GUILayoutOption[] options)
    {
        bool state = false;
        states.TryGetValue(key, out state);

        if (!minimalistic)
        {
            GUILayout.Space(3f);
        }
        if (!forceOn && !state)
        {
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        }
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state)
            {
                text = "\u25BC" + (char)0x200a + text;
            }
            else
            {
                text = "\u25BA" + (char)0x200a + text;
            }

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            state = GUILayout.Toggle(state, text, "PreToolbar2", options);
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state)
            {
                text = "\u25BC " + text;
            }
            else
            {
                text = "\u25BA " + text;
            }
            state = GUILayout.Toggle(state, text, "dragtab", options);
        }

        if (GUI.changed)
        {
            states[key] = state;
        }

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    static public bool DrawSubHeader(int level, string text, Dictionary<string, bool> states, string key, string subText)
    {
        if (string.IsNullOrEmpty(subText))
        {
            EditorGUILayout.BeginHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(250f));
        }
        EditorGUILayout.LabelField("", GUILayout.MinWidth(20 * level), GUILayout.MaxWidth(20 * level));
        var expanded = DrawHeader(text, states, key, false, true, GUILayout.MinWidth(200f));
        EditorGUILayout.LabelField(subText, GUILayout.MinWidth(50), GUILayout.MaxWidth(50));
        EditorGUILayout.EndHorizontal();
        return expanded;
    }

    static public void BeginContents(bool minimalistic)
    {
        if (!minimalistic)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        }
        else
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
            GUILayout.Space(10f);
        }
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    static public void EndContents(bool minimalistic)
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (!minimalistic)
        {
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(3f);
    }

    static public void DrawTextListContent(List<string> list, string prefix = null)
    {
        BeginContents(false);
        for (int i = 0; i < list.Count; i++)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                EditorGUILayout.LabelField(prefix + list[i], GUILayout.MinWidth(150f));
            }
            else
            {
                EditorGUILayout.LabelField(list[i], GUILayout.MinWidth(150f));
            }
        }
        EndContents(false);
    }

    static public void DrawTextArrayContent(string[] array, string prefix = null)
    {
        BeginContents(false);
        for (int i = 0; i < array.Length; i++)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                EditorGUILayout.LabelField(prefix + array[i], GUILayout.MinWidth(150f));
            }
            else
            {
                EditorGUILayout.LabelField(array[i], GUILayout.MinWidth(150f));
            }
        }
        EndContents(false);
    }
    
    protected void DrawAssetbundleRefrences(string assetbundleName, string key, int level = 0)
    {
        var instance = AssetBundleManager.Instance;
        var abRefrences = instance.GetAssetBundleRefrences(assetbundleName);
        var webRequestRefrences = instance.GetWebRequesterRefrences(assetbundleName);
        var abLoaderRefrences = instance.GetAssetBundleLoaderRefrences(assetbundleName);
        var expanded = false;

        expanded = DrawSubHeader(level + 1, "ABRefrence:", abRefrenceSate, key, abRefrences.Count.ToString());
        if (expanded && abRefrences.Count > 0)
        {
            DrawTextListContent(abRefrences);
        }

        expanded = DrawSubHeader(level + 1, "WebRequester:", webRequestRefrenceSate, key, webRequestRefrences.Count.ToString());
        if (expanded && webRequestRefrences.Count > 0)
        {
            DrawTextListContent(webRequestRefrences, "Sequence : ");
        }

        expanded = DrawSubHeader(level + 1, "ABLoader:", abLoaderfrenceSate, key, abLoaderRefrences.Count.ToString());
        if (expanded && abLoaderRefrences.Count > 0)
        {
            DrawTextListContent(abLoaderRefrences, "Sequence : ");
        }
    }

    protected void DrawAssetbundleContent(string assetbundleName, string key, int level)
    {
        var instance = AssetBundleManager.Instance;
        var expanded = false;
        BeginContents(false);

        var loaded = instance.GetAssetBundleCache(assetbundleName);
        EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(250f));
        EditorGUILayout.LabelField("", GUILayout.MinWidth(20 * level));
        DrawProperty("Has Loaded:", loaded ? "true" : "false");
        EditorGUILayout.EndHorizontal();

        var referencesCount = instance.GetAssetbundleRefrenceCount(assetbundleName);
        expanded = DrawSubHeader(level, "References Count:", refrenceSate, key, referencesCount.ToString());
        if (expanded)
        {
            DrawAssetbundleRefrences(assetbundleName, key, level);
        }

        var dependencies = instance.curManifest.GetAllDependencies(assetbundleName);
        var dependenciesCount = instance.GetAssetbundleDependenciesCount(assetbundleName);
        expanded = DrawSubHeader(level, "Dependencies Count:", dependenciesSate, key, dependenciesCount.ToString());
        if (expanded && dependenciesCount > 0)
        {
            for (int i = 0; i < dependencies.Length; i++)
            {
                var dependence = dependencies[i];
                if (!string.IsNullOrEmpty(dependence) && dependence != assetbundleName)
                {
                    DrawAssetbundleItem(dependence, dependence, abItemSate, assetbundleName + dependence, level + 1);
                }
            }
        }

        EndContents(false);
    }

    protected void DrawAssetbundleItem(string title, string assetbundleName, Dictionary<string, bool> states, string key, int level = 0)
    {
        var instance = AssetBundleManager.Instance;
        if (instance.IsAssetBundleLoaded(assetbundleName))
        {
            title += "[loaded]";
        }

        if (level == 0)
        {
            if (DrawHeader(title, states, key, false, false))
            {
                DrawAssetbundleContent(assetbundleName, key, level);
            }
        }
        else
        {
            if (DrawSubHeader(level, title, states, key, ""))
            {
                DrawAssetbundleContent(assetbundleName, key, level + 1);
            }
        }
    }

    protected void OnDrawAssetBundleResident()
    {
        var instance = AssetBundleManager.Instance;
        var resident = instance.GetAssetbundleResident();
        var iter = resident.GetEnumerator();
        EditorGUILayout.BeginVertical();
        while (iter.MoveNext())
        {
            var assetbundleName = iter.Current;
            DrawAssetbundleItem(assetbundleName, assetbundleName, abItemSate, assetbundleName);
        }
        EditorGUILayout.EndVertical();
    }

    protected void OnDrawAssetBundleCaching()
    {
        var instance = AssetBundleManager.Instance;
        var assetbundleCaching = instance.GetAssetbundleCaching();
        var iter = assetbundleCaching.GetEnumerator();
        EditorGUILayout.BeginVertical();
        while (iter.MoveNext())
        {
            var assetbundleName = iter.Current;
            DrawAssetbundleItem(assetbundleName, assetbundleName, abItemSate, assetbundleName);
        }
        EditorGUILayout.EndVertical();
    }

    protected void OnDrawAssetCaching()
    {
        var instance = AssetBundleManager.Instance;
        var assetCaching = instance.GetAssetCaching();
        var totalCount = instance.GetAssetCachingCount();
        var iter = assetCaching.GetEnumerator();
        EditorGUILayout.BeginVertical();
        DrawProperty("Total loaded assets count : ", totalCount.ToString());
        while (iter.MoveNext())
        {
            var assetbundleName = iter.Current.Key;
            var assetNameList = iter.Current.Value;
            string title = string.Format("{0}[{1}]", assetbundleName, assetNameList.Count);
            if (DrawHeader(title, abItemSate, assetbundleName, false, false))
            {
                DrawTextListContent(assetNameList);
            }
        }
        EditorGUILayout.EndVertical();
    }

    protected void OnDrawWebRequesting()
    {
        var instance = AssetBundleManager.Instance;
        var webRequesting = instance.GetWebRequesting();
        var iter = webRequesting.GetEnumerator();
        EditorGUILayout.BeginVertical();
        while (iter.MoveNext())
        {
            var assetbundleName = iter.Current.Key;
            var webRequester = iter.Current.Value;
            string title = string.Format("Sequence : {0} --- {1}", webRequester.Sequence, assetbundleName);
            DrawAssetbundleItem(title, assetbundleName, abItemSate, assetbundleName);
        }
        EditorGUILayout.EndVertical();
    }

    protected void OnDrawWebRequesterQueue()
    {
        var instance = AssetBundleManager.Instance;
        var requesterQueue = instance.GetWebRequestQueue();
        var iter = requesterQueue.GetEnumerator();
        EditorGUILayout.BeginVertical();
        while (iter.MoveNext())
        {
            var assetbundleName = iter.Current.assetbundleName;
            var webRequester = iter.Current;
            string title = string.Format("Sequence : {0} --- {1}", webRequester.Sequence, assetbundleName);
            DrawAssetbundleItem(title, assetbundleName, abItemSate, assetbundleName);
        }
        EditorGUILayout.EndVertical();
    }

    protected void OnDrawProsessingWebRequester()
    {
        var instance = AssetBundleManager.Instance;
        var prosessing = instance.GetProsessingWebRequester();
        var iter = prosessing.GetEnumerator();
        EditorGUILayout.BeginVertical();
        while (iter.MoveNext())
        {
            var assetbundleName = iter.Current.assetbundleName;
            var webRequester = iter.Current;
            string title = string.Format("Sequence : {0} --- {1}", webRequester.Sequence, assetbundleName);
            DrawAssetbundleItem(title, assetbundleName, abItemSate, assetbundleName);
        }
        EditorGUILayout.EndVertical();
    }
    
    protected void OnDrawProsessingAssetBundleAsyncLoader()
    {
        var instance = AssetBundleManager.Instance;
        var prosessing = instance.GetProsessingAssetBundleAsyncLoader();
        var iter = prosessing.GetEnumerator();
        EditorGUILayout.BeginVertical();
        while (iter.MoveNext())
        {
            var assetbundleName = iter.Current.assetbundleName;
            var loader = iter.Current;
            string title = string.Format("Sequence : {0} --- {1}", loader.Sequence, assetbundleName);
            DrawAssetbundleItem(title, assetbundleName, abItemSate, assetbundleName);
        }
        EditorGUILayout.EndVertical();
    }
    
    protected void OnDrawProsessingAssetAsyncLoader()
    {
        var instance = AssetBundleManager.Instance;
        var prosessing = instance.GetProsessingAssetAsyncLoader();
        var iter = prosessing.GetEnumerator();
        EditorGUILayout.BeginVertical();
        while (iter.MoveNext())
        {
            var assetName = iter.Current.AssetName;
            var loader = iter.Current;
            string title = string.Format("Sequence : {0} --- {1}", loader.Sequence, assetName);
            var assetbundleName = instance.GetAssetBundleName(assetName);
            DrawAssetbundleItem(title, assetbundleName, abItemSate, assetbundleName);
        }
        EditorGUILayout.EndVertical();
    }
}
