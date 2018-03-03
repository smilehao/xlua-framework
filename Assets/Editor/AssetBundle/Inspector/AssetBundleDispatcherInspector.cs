using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// added by wsh @ 2018.01.06
/// 说明：Assetbundle分发器Inspector，为其提供可视化的编辑界面
/// TODO：
/// 1、还未完成，目前只是做了基本的配置功能
/// </summary>

namespace AssetBundles
{
    [CustomEditor(typeof(DefaultAsset), true)]
    public class AssetBundleDispatcherInspector : Editor
    {
        AssetBundleDispatcherConfig dispatcherConfig = null;
        string packagePath = null;
        string targetAssetPath = null;
        string databaseAssetPath = null;

        static Dictionary<string, bool> inspectorSate = new Dictionary<string, bool>();
        AssetBundleDispatcherFilterType filterType = AssetBundleDispatcherFilterType.Root;
        bool configChanged = false;

        void OnEnable()
        {
            Initialize();
        }

        void Initialize()
        {
            configChanged = false;
            filterType = AssetBundleDispatcherFilterType.Root;
            targetAssetPath = AssetDatabase.GetAssetPath(target);
            if (!AssetBundleUtility.IsPackagePath(targetAssetPath))
            {
                return;
            }

            packagePath = AssetBundleUtility.AssetsPathToPackagePath(targetAssetPath);
            databaseAssetPath = AssetBundleInspectorUtils.AssetPathToDatabasePath(targetAssetPath);
            dispatcherConfig = AssetDatabase.LoadAssetAtPath<AssetBundleDispatcherConfig>(databaseAssetPath);
            if (dispatcherConfig != null)
            {
                dispatcherConfig.Load();
                filterType = dispatcherConfig.Type;
            }
        }

        void DrawCreateAssetBundleDispatcher()
        {
            if (GUILayout.Button("Create AssetBundle Dispatcher"))
            {
                var dir = Path.GetDirectoryName(databaseAssetPath);
                GameUtility.CheckDirAndCreateWhenNeeded(dir);

                var instance = CreateInstance<AssetBundleDispatcherConfig>();
                AssetDatabase.CreateAsset(instance, databaseAssetPath);
                AssetDatabase.Refresh();

                Initialize();
                Repaint();
            }
        }

        void DrawFilterItem(AssetBundleCheckerFilter checkerFilter)
        {
            GUILayout.BeginVertical(); 
            var relativePath = GUILayoutUtils.DrawInputField("RelativePath:", checkerFilter.RelativePath, 300f, 80f);
            var objectFilter = GUILayoutUtils.DrawInputField("ObjectFilter:", checkerFilter.ObjectFilter, 300f, 80f);
            if (relativePath != checkerFilter.RelativePath)
            {
                configChanged = true;
                checkerFilter.RelativePath = relativePath;
            }
            if (objectFilter != checkerFilter.ObjectFilter)
            {
                configChanged = true;
                checkerFilter.ObjectFilter = objectFilter;
            }
            GUILayout.EndVertical();
        }

        void DrawFilterTypesList(List<AssetBundleCheckerFilter> checkerFilters)
        {
            GUILayout.BeginVertical(EditorStyles.textField);
            GUILayout.Space(3);

            EditorGUILayout.Separator();
            for (int i = 0; i < checkerFilters.Count; i++)
            {
                var curFilter = checkerFilters[i];
                var relativePath = string.IsNullOrEmpty(curFilter.RelativePath) ? "root" : curFilter.RelativePath;
                var objectFilter = string.IsNullOrEmpty(curFilter.ObjectFilter) ? "all" : curFilter.ObjectFilter;
                var filterType = relativePath + ": <" + objectFilter + ">";
                var stateKey = "CheckerFilters" + i.ToString();
                if (GUILayoutUtils.DrawRemovableSubHeader(1, filterType, inspectorSate, stateKey, () =>
                {
                    configChanged = true;
                    checkerFilters.RemoveAt(i);
                    i--;
                }))
                {
                    DrawFilterItem(curFilter);
                }
                EditorGUILayout.Separator();
            }
            if (GUILayout.Button("Add"))
            {
                configChanged = true;
                checkerFilters.Add(new AssetBundleCheckerFilter("", "t:prefab"));
            }
            EditorGUILayout.Separator();

            GUILayout.Space(3);
            GUILayout.EndVertical();
        }

        void DrawAssetDispatcherConfig()
        {
            GUILayoutUtils.BeginContents(false);

            GUILayoutUtils.DrawProperty("Path:", AssetBundleUtility.AssetsPathToPackagePath(targetAssetPath), 300f, 80f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("FilterType:", GUILayout.MaxWidth(80f));
            var selectType = (AssetBundleDispatcherFilterType)EditorGUILayout.EnumPopup(filterType);
            if (selectType != filterType)
            {
                filterType = selectType;
                configChanged = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            var filtersCount = dispatcherConfig.CheckerFilters.Count;
            if (GUILayoutUtils.DrawSubHeader(0, "CheckerFilters:", inspectorSate, "CheckerFilters", filtersCount.ToString()))
            {
                DrawFilterTypesList(dispatcherConfig.CheckerFilters);
            }

            Color color = GUI.color;
            if (configChanged)
            {
                GUI.color = color * new Color(1, 1, 0.5f);
            }
            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply"))
            {
                Apply();
            }
            GUI.color = new Color(1, 0.5f, 0.5f);
            if (GUILayout.Button("Remove"))
            {
                Remove();
            }
            GUI.color = color;
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            GUILayoutUtils.EndContents(false);
        }

        void Apply()
        {
            dispatcherConfig.PackagePath = packagePath;
            dispatcherConfig.Type = filterType;
            dispatcherConfig.Apply();
            EditorUtility.SetDirty(dispatcherConfig);
            AssetDatabase.SaveAssets();

            Initialize();
            Repaint();
            configChanged = false;
        }

        void Remove()
        {
            bool checkRemove = EditorUtility.DisplayDialog("Remove Warning",
                "Sure to remove the AssetBundle dispatcher ?",
                "Confirm", "Cancel");
            if (!checkRemove)
            {
                return;
            }
            GameUtility.SafeDeleteFile(databaseAssetPath);
            AssetDatabase.Refresh();

            Initialize();
            Repaint();
            configChanged = false;
        }

        void DrawAssetBundleDispatcherInspector()
        {
            if (GUILayoutUtils.DrawHeader("AssetBundle Dispatcher : ", inspectorSate, "DispatcherConfig", true, false))
            {
                DrawAssetDispatcherConfig();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!AssetBundleInspectorUtils.CheckMaybeAssetBundleAsset(targetAssetPath))
            {
                return;
            }
            GUI.enabled = true;

            if (dispatcherConfig == null)
            {
                DrawCreateAssetBundleDispatcher();
            }
            else
            {
                DrawAssetBundleDispatcherInspector();
            }
        }
        
        void OnDisable()
        {
            if (configChanged)
            {
                bool checkApply = EditorUtility.DisplayDialog("Modify Warning",
                    "You have modified the AssetBundle dispatcher setting, Apply it ?",
                    "Confirm", "Cancel");
                if (checkApply)
                {
                    Apply();
                }
            }
            dispatcherConfig = null;
            inspectorSate.Clear();
        }
    }
}