using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace AssetBundles
{
    //public class AssetBundleBuildScript
    //{
    //    public static void BuildAssetBundles()
    //    {
    //        BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
    //    }
        
    //    public static void BuildAssetBundles(BuildTarget buildTarget)
    //    {
    //        string outputPath = AssetBundleUtility.GetBuildPlatformOutputPath(buildTarget);
    //        BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.IgnoreTypeTreeChanges;
    //        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, buildOption, buildTarget);
    //        if (manifest != null)
    //        {
    //            AssetsPathMappingEditor.BuildPathMapping(buildTarget, manifest);
    //            VariantMappingEditor.BuildVariantMapping(buildTarget, manifest);
    //            BuildPipeline.BuildAssetBundles(outputPath, buildOption, buildTarget);
    //            Debug.Log("BuildAssetBundles success!!!");
    //        }
    //        AssetDatabase.Refresh();
    //    }

    //    public static void BuildPlayer()
    //    {
    //        var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
    //        if (outputPath.Length == 0)
    //            return;

    //        string[] levels = GetLevelsFromBuildSettings();
    //        if (levels.Length == 0)
    //        {
    //            Debug.Log("Nothing to build.");
    //            return;
    //        }

    //        string targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
    //        if (targetName == null)
    //        {
    //            return;
    //        }
    //        GameUtility.SafeDeleteFile(outputPath + targetName);

    //        // Build and copy AssetBundles.
    //        BuildAssetBundles();
    //        // makesure the local server URL is up-to-dat
    //        AssetBundleUtility.WriteAssetBundleServerURL();
    //        AssetDatabase.Refresh();

    //        BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
    //        BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
    //    }
        
    //    public static void BuildStandalonePlayer()
    //    {
    //        var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
    //        if (outputPath.Length == 0)
    //            return;

    //        string[] levels = GetLevelsFromBuildSettings();
    //        if (levels.Length == 0)
    //        {
    //            Debug.Log("Nothing to build.");
    //            return;
    //        }

    //        string targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
    //        if (targetName == null)
    //        {
    //            return;
    //        }
    //        GameUtility.SafeDeleteFile(outputPath + targetName);

    //        // Build and copy AssetBundles.
    //        BuildAssetBundles();
    //        AssetBundleUtility.CopyPlatformAssetBundlesToStreamingAssets();
    //        // makesure the local server URL is up-to-dat
    //        AssetBundleUtility.WriteAssetBundleServerURL();
    //        AssetDatabase.Refresh();

    //        BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
    //        BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
    //    }
        
    //    public static string GetBuildTargetName(BuildTarget target)
    //    {
    //        switch (target)
    //        {
    //            case BuildTarget.Android:
    //                return string.Format("/{0}.apk",AssetBundleConfig.PlayerBuildName);
    //            case BuildTarget.iOS:
    //                return string.Format("/{0}.ipa", AssetBundleConfig.PlayerBuildName);
    //            default:
    //                Debug.Log("Target not implemented.");
    //                return null;
    //        }
    //    }
        
    //    static string[] GetLevelsFromBuildSettings()
    //    {
    //        List<string> levels = new List<string>();
    //        for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
    //        {
    //            if (EditorBuildSettings.scenes[i].enabled)
    //                levels.Add(EditorBuildSettings.scenes[i].path);
    //        }

    //        return levels.ToArray();
    //    }
    //}
}