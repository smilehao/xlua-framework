using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace AssetBundles
{
    [InitializeOnLoad]
    public class AssetBundleMenuItems
    {
        //%:ctrl,#:shift,&:alt
        const string kSimulateMode = "AssetBundles/Switch Model/Simulate Mode";
        const string kEditorMode = "AssetBundles/Switch Model/Editor Mode";
        const string kBuildAndoridAssetbundle = "AssetBundles/Build AssetBundles/For Andorid";
        const string kBuildIOSAssetbundle = "AssetBundles/Build AssetBundles/For IOS";
        const string kBuildPlayer = "AssetBundles/Build Player/BuildPlayer";
        const string kBuildStandalonePlayer = "AssetBundles/Build Player/BuildStandalonePlayer";
        const string kToolsClearOutput = "AssetBundles/Tools/Clear Output";
        const string kToolsClearStreamingAssets = "AssetBundles/Tools/Clear Streaming Assets";
        const string kToolsClearPersistentAssets = "AssetBundles/Tools/Clear Persistent Assets";


        const string kCreateAssetbundleForCurrent = "Assets/AssetBundles/Create Assetbundle For Current &#z";
        const string kCreateAssetbundleForChildren = "Assets/AssetBundles/Create Assetbundle For Children &#x";
        const string kAssetDependencis = "Assets/AssetBundles/Asset Dependencis &#h";
        const string kAssetbundleAllDependencis = "Assets/AssetBundles/Assetbundle All Dependencis &#j";
        const string kAssetbundleDirectDependencis = "Assets/AssetBundles/Assetbundle Direct Dependencis &#k";

        // unity editor启动和运行时调用
        static AssetBundleMenuItems()
        {
            // 1、模拟模式下在电脑上模拟手机资源更新过程，如果需要更新最新ab，需要手动构建；如果根本没有ab，则构建一次
            // 2、模拟模式下需要用到streamingAsset，没有资源则拷贝一次，之后总是从本地服务器下载ab到persistentDataPath
            var platformName = AssetBundleUtility.GetCurPlatformName();
            var outputManifest = Path.Combine(AssetBundleConfig.AssetBundlesBuildOutputPath, platformName);
            outputManifest = Path.Combine(outputManifest, platformName);
            if (!File.Exists(outputManifest))
            {
                AssetBundleBuildScript.BuildAssetBundles();
            }
            var streamingManifest = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
            streamingManifest = Path.Combine(streamingManifest, platformName);
            streamingManifest = Path.Combine(streamingManifest, platformName);
            if (!File.Exists(streamingManifest))
            {
                AssetBundleUtility.CopyPlatformAssetBundlesToStreamingAssets();
                AssetDatabase.Refresh();
            }
            LaunchAssetBundleServer.CheckAndDoRunning();
        }
        
        [MenuItem(kEditorMode)]
        public static void ToggleEditorMode()
        {
            AssetBundleConfig.IsEditorMode = !AssetBundleConfig.IsEditorMode;
            LaunchAssetBundleServer.CheckAndDoRunning();
        }

        [MenuItem(kEditorMode, true)]
        public static bool ToggleEditorModeValidate()
        {
            Menu.SetChecked(kEditorMode, AssetBundleConfig.IsEditorMode);
            return true;
        }

        [MenuItem(kSimulateMode)]
        public static void ToggleSimulateMode()
        {
            AssetBundleConfig.IsSimulateMode = !AssetBundleConfig.IsSimulateMode;
            LaunchAssetBundleServer.CheckAndDoRunning();
        }

        [MenuItem(kSimulateMode, true)]
        public static bool ToggleSimulateModeValidate()
        {
            Menu.SetChecked(kSimulateMode, AssetBundleConfig.IsSimulateMode);
            return true;
        }

        [MenuItem(kBuildAndoridAssetbundle)]
        static public void BuildAndoridAssetBundles()
        {
            AssetBundleBuildScript.BuildAssetBundles(BuildTarget.Android);
        }
        
        [MenuItem(kBuildIOSAssetbundle)]
        static public void BuildIOSAssetBundles()
        {
            AssetBundleBuildScript.BuildAssetBundles(BuildTarget.iOS);
        }
        
        [MenuItem(kBuildPlayer)]
        static public void BuildPlayer()
        {
            AssetBundleBuildScript.BuildPlayer();
        }
        
        [MenuItem(kBuildStandalonePlayer)]
        static public void BuildStandalonePlayer()
        {
            AssetBundleBuildScript.BuildStandalonePlayer();
        }

        [MenuItem(kToolsClearOutput)]
        static public void ToolsClearOutput()
        {
            bool checkClear = EditorUtility.DisplayDialog("ClearOutput Warning",
                "Clear output assetbundles will force to rebuild all assetbundles, continue ?",
                "Yes", "No");
            if (!checkClear)
            {
                return;
            }
            string outputPath = Path.Combine(AssetBundleConfig.AssetBundlesBuildOutputPath, AssetBundleUtility.GetCurPlatformName());
            GameUtility.SafeDeleteDir(outputPath);
            Debug.Log(string.Format("Clear {0} assetbundle output done!", AssetBundleUtility.GetCurPlatformName()));
        }

        [MenuItem(kToolsClearStreamingAssets)]
        static public void ToolsClearStreamingAssets()
        {
            bool checkClear = EditorUtility.DisplayDialog("ClearStreamingAssets Warning",
                "Clear streaming assets assetbundles will lost the latest player build info, continue ?",
                "Yes", "No");
            if (!checkClear)
            {
                return;
            }
            string outputPath = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
            outputPath = Path.Combine(outputPath, AssetBundleUtility.GetCurPlatformName());
            GameUtility.SafeDeleteDir(outputPath);
            AssetDatabase.Refresh();
            Debug.Log(string.Format("Clear {0} assetbundle streaming assets done!", AssetBundleUtility.GetCurPlatformName()));
        }

        [MenuItem(kToolsClearPersistentAssets)]
        static public void ToolsClearPersistentAssets()
        {
            bool checkClear = EditorUtility.DisplayDialog("ClearPersistentAssets Warning",
                "Clear persistent assetbundles will force to update all assetbundles that difference with streaming assets assetbundles, continue ?",
                "Yes", "No");
            if (!checkClear)
            {
                return;
            }

            string outputPath = Path.Combine(Application.persistentDataPath, AssetBundleConfig.AssetBundlesFolderName);
            outputPath = Path.Combine(outputPath, AssetBundleUtility.GetCurPlatformName());
            GameUtility.SafeDeleteDir(outputPath);
            Debug.Log(string.Format("Clear {0} assetbundle persistent assets done!", AssetBundleUtility.GetCurPlatformName()));
        }

        [MenuItem(kCreateAssetbundleForCurrent)]
        static public void CreateAssetbundleForCurrent()
        {
            if (AssetBundleEditorHelper.HasValidSelection())
            {
                bool checkCreate = EditorUtility.DisplayDialog("CreateAssetbundleForCurrent Warning",
                    "Create assetbundle for cur selected objects will remove assetbundles in their children and parents,continue ?",
                    "Yes", "No");
                if (!checkCreate)
                {
                    return;
                }
                Object[] selObjs = Selection.objects;
                AssetBundleEditorHelper.CreateAssetbundleForCurrent(selObjs);
                List<string> removeList = AssetBundleEditorHelper.RemoveAssetbundleInParents(selObjs);
                removeList.AddRange(AssetBundleEditorHelper.RemoveAssetbundleInChildren(selObjs));
                string removeStr = string.Empty;
                int i = 0;
                foreach(string str in removeList)
                {
                    removeStr += string.Format("[{0}]{1}\n",++i,str);
                }
                Debug.Log(string.Format("CreateAssetbundleForCurrent done!\nRemove list :"+
                    "\n-------------------------------------------\n" +  
                    "{0}" +
                    "\n-------------------------------------------\n",
                    removeStr));
            }
        }
        
        [MenuItem(kCreateAssetbundleForChildren)]
        static public void CreateAssetbundleForChildren()
        {
            if (AssetBundleEditorHelper.HasValidSelection())
            {
                bool checkCreate = EditorUtility.DisplayDialog("CreateAssetbundleForChildren Warning",
                    "Create assetbundle for all chilren files of cur selected objects will remove assetbundles in all children dir,continue ?",
                    "Yes", "No");
                if (!checkCreate)
                {
                    return;
                }
                Object[] selObjs = Selection.objects;
                AssetBundleEditorHelper.CreateAssetbundleForChildrenFiles(selObjs);
                List<string> removeList = AssetBundleEditorHelper.RemoveAssetbundleInParents(selObjs);
                removeList.AddRange(AssetBundleEditorHelper.RemoveAssetbundleInChildren(selObjs,true,AssetBundleEditorHelper.REMOVE_TYPE.CHILDREN_DIR));
                string removeStr = string.Empty;
                int i = 0;
                foreach (string str in removeList)
                {
                    removeStr += string.Format("[{0}]{1}\n", ++i, str);
                }
                Debug.Log(string.Format("CreateAssetbundleForChildren done!\nRemove list :" +
                    "\n-------------------------------------------\n" +
                    "{0}" +
                    "\n-------------------------------------------\n",
                    removeStr));
            }
        }

        [MenuItem(kAssetDependencis)]
        static public void ListAssetDependencis()
        {
            if (AssetBundleEditorHelper.HasValidSelection())
            {
                Object[] selObjs = Selection.objects;
                string depsStr = AssetBundleEditorHelper.GetDependencyText(selObjs, false);
                string selStr = string.Empty;
                int i = 0;
                foreach (Object obj in selObjs)
                {
                    selStr += string.Format("[{0}]{1};", ++i, AssetDatabase.GetAssetPath(obj));
                }
                Debug.Log(string.Format("Selection({0}) depends on the following assets:" + 
                    "\n-------------------------------------------\n" + 
                    "{1}" + 
                    "\n-------------------------------------------\n",
                    selStr,
                    depsStr));
                AssetBundleEditorHelper.SelectDependency(selObjs,false);
            }
        }
        
        [MenuItem(kAssetbundleAllDependencis)]
        static public void ListAssetbundleAllDependencis()
        {
            ListAssetbundleDependencis(true);
        }
        
        [MenuItem(kAssetbundleDirectDependencis)]
        static public void ListAssetbundleDirectDependencis()
        {
            ListAssetbundleDependencis(false);
        }
        
        static public void ListAssetbundleDependencis(bool isAll)
        {
            if (AssetBundleEditorHelper.HasValidSelection())
            {
                string localFilePath = AssetBundleUtility.GetBuildPlatformOutputPath(EditorUserBuildSettings.activeBuildTarget);
                localFilePath = Path.Combine(localFilePath, AssetBundleUtility.GetCurPlatformName());

                Object[] selObjs = Selection.objects;
                var depsList = AssetBundleEditorHelper.GetDependancisFormBuildManifest(localFilePath, selObjs, isAll);
                depsList.Sort();

                string depsStr = string.Empty;
                int i = 0;
                foreach (string str in depsList)
                {
                    depsStr += string.Format("[{0}]{1}\n", ++i, str);
                }

                string selStr = string.Empty;
                i = 0;
                foreach (Object obj in selObjs)
                {
                    selStr += string.Format("[{0}]{1};", ++i, AssetDatabase.GetAssetPath(obj));
                }
                Debug.Log(string.Format("Selection({0}) directly depends on the following assetbundles:" +
                    "\n-------------------------------------------\n" +
                    "{1}" +
                    "\n-------------------------------------------\n",
                    selStr,
                    depsStr));
            }
        }
    }
}