using AssetBundles;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// added by wsh @ 2017.12.29
/// 功能：Assetbundle编辑器支持，方便调试用
/// </summary>

namespace AssetBundles
{
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
            base.OnInspectorGUI();
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
            if (!AssetBundleConfig.IsSimulateMode)
            {
                return;
            }

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

        protected void DrawAssetbundleRefrences(string assetbundleName, string key, int level = 0)
        {
            var instance = AssetBundleManager.Instance;
            var abRefrences = instance.GetAssetBundleRefrences(assetbundleName);
            var webRequestRefrences = instance.GetWebRequesterRefrences(assetbundleName);
            var abLoaderRefrences = instance.GetAssetBundleLoaderRefrences(assetbundleName);
            var expanded = false;

            expanded = GUILayoutUtils.DrawSubHeader(level + 1, "ABRefrence:", abRefrenceSate, key, abRefrences.Count.ToString());
            if (expanded && abRefrences.Count > 0)
            {
                GUILayoutUtils.DrawTextListContent(abRefrences);
            }

            expanded = GUILayoutUtils.DrawSubHeader(level + 1, "WebRequester:", webRequestRefrenceSate, key, webRequestRefrences.Count.ToString());
            if (expanded && webRequestRefrences.Count > 0)
            {
                GUILayoutUtils.DrawTextListContent(webRequestRefrences, "Sequence : ");
            }

            expanded = GUILayoutUtils.DrawSubHeader(level + 1, "ABLoader:", abLoaderfrenceSate, key, abLoaderRefrences.Count.ToString());
            if (expanded && abLoaderRefrences.Count > 0)
            {
                GUILayoutUtils.DrawTextListContent(abLoaderRefrences, "Sequence : ");
            }
        }

        protected void DrawAssetbundleContent(string assetbundleName, string key, int level)
        {
            var instance = AssetBundleManager.Instance;
            var expanded = false;
            GUILayoutUtils.BeginContents(false);

            var loaded = instance.GetAssetBundleCache(assetbundleName);
            EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(250f));
            EditorGUILayout.LabelField("", GUILayout.MinWidth(20 * level));
            GUILayoutUtils.DrawProperty("Has Loaded:", loaded ? "true" : "false");
            EditorGUILayout.EndHorizontal();

            var referencesCount = instance.GetAssetbundleRefrenceCount(assetbundleName);
            expanded = GUILayoutUtils.DrawSubHeader(level, "References Count:", refrenceSate, key, referencesCount.ToString());
            if (expanded)
            {
                DrawAssetbundleRefrences(assetbundleName, key, level);
            }

            var dependencies = instance.curManifest.GetAllDependencies(assetbundleName);
            var dependenciesCount = instance.GetAssetbundleDependenciesCount(assetbundleName);
            expanded = GUILayoutUtils.DrawSubHeader(level, "Dependencies Count:", dependenciesSate, key, dependenciesCount.ToString());
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

            GUILayoutUtils.EndContents(false);
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
                if (GUILayoutUtils.DrawHeader(title, states, key, false, false))
                {
                    DrawAssetbundleContent(assetbundleName, key, level);
                }
            }
            else
            {
                if (GUILayoutUtils.DrawSubHeader(level, title, states, key, ""))
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
            GUILayoutUtils.DrawProperty("Total loaded assets count : ", totalCount.ToString());
            while (iter.MoveNext())
            {
                var assetbundleName = iter.Current.Key;
                var assetNameList = iter.Current.Value;
                string title = string.Format("{0}[{1}]", assetbundleName, assetNameList.Count);
                if (GUILayoutUtils.DrawHeader(title, abItemSate, assetbundleName, false, false))
                {
                    GUILayoutUtils.DrawTextListContent(assetNameList);
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
}
