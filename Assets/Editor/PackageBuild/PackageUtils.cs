using UnityEditor;
using System.IO;
using GameChannel;
using System;
using AssetBundles;
using UnityEngine;

/// <summary>
/// added by wsh @ 2018.01.03
/// 功能： 打包相关配置和通用函数
/// </summary>

public class PackageUtils
{
    public static string GetCurPlatformName()
    {
        return GetPlatformName(EditorUserBuildSettings.activeBuildTarget);
    }
    
    public static string GetPlatformName(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            default:
                Logger.LogError("Error buildTarget!!!");
                return null;
        }
    }

    public static ChannelType GetCurSelectedChannel()
    {
        ChannelType channelType = ChannelType.Test;
        string channelName = EditorPrefs.GetString("ChannelName");
        if (Enum.IsDefined(typeof(ChannelType), channelName))
        {
            channelType = (ChannelType)Enum.Parse(typeof(ChannelType), channelName);
        }
        else
        {
            EditorPrefs.SetString("ChannelName", ChannelType.Test.ToString());
        }
        return channelType;
    }

    public static void SaveCurSelectedChannel(ChannelType channelType)
    {
        EditorPrefs.SetString("ChannelName", channelType.ToString());
    }

    public static string GetPlatformChannelPath(BuildTarget target, string channelName)
    {
        return Path.Combine(PackageUtils.GetPlatformName(target), channelName);
    }

    public static string GetCurPlatformChannelPath()
    {
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        var channelName = GetCurSelectedChannel().ToString();
        return GetPlatformChannelPath(buildTarget, channelName);
    }

    public static string GetBuildPlatformOutputPath(BuildTarget target, string channelName)
    {
        string outputPath = Path.Combine(AssetBundleConfig.AssetBundlesBuildOutputPath, GetPlatformChannelPath(target, channelName));
        GameUtility.CheckDirAndCreateWhenNeeded(outputPath);
        return outputPath;
    }

    public static string GetCurBuildSettingOutputPath()
    {
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        var channelType = GetCurSelectedChannel();
        return GetBuildPlatformOutputPath(buildTarget, channelType.ToString());
    }

    public static string GetCurBuildSettingOutputManifestPath()
    {
        string path = GetCurBuildSettingOutputPath();
        path = Path.Combine(path, GetCurSelectedChannel().ToString());
        return path;
    }

    public static string GetCurBuildSettingStreamingManifestPath()
    {
        string path = AssetBundleUtility.GetStreamingAssetsDataPath();
        path = Path.Combine(path, GetCurSelectedChannel().ToString());
        return path;
    }

    public static AssetBundleManifest GetManifestFormLocal(string manifestPath)
    {
        FileInfo fileInfo = new FileInfo(manifestPath);
        if (!fileInfo.Exists)
        {
            Debug.LogError("You need to build assetbundles first to get assetbundle dependencis info!");
            return null;
        }
        byte[] bytes = GameUtility.SafeReadAllBytes(fileInfo.FullName);
        if (bytes == null)
        {
            return null;
        }
        AssetBundle assetBundle = AssetBundle.LoadFromMemory(bytes);
        AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        assetBundle.Unload(false);
        return manifest;
    }

    public static void CopyAssetBundlesToStreamingAssets(BuildTarget buildTarget, string channelName)
    {
        string source = GetBuildPlatformOutputPath(buildTarget, channelName);
        string destination = AssetBundleUtility.GetStreamingAssetsDataPath();
        // 有毒，竟然在有的windows系统这个函数删除不了目录，不知道是不是Unity的Bug
        // GameUtility.SafeDeleteDir(destination);
        AssetDatabase.DeleteAsset(GameUtility.FullPathToAssetPath(destination));
        AssetDatabase.Refresh();

        try
        {
            FileUtil.CopyFileOrDirectoryFollowSymlinks(source, destination);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Something wrong, you need manual delete AssetBundles folder in StreamingAssets, err : " + ex);
            return;
        }

        var allManifest = GameUtility.GetSpecifyFilesInFolder(destination, new string[] { ".manifest" });
        if (allManifest != null && allManifest.Length > 0)
        {
            for (int i = 0; i < allManifest.Length; i++)
            {
                GameUtility.SafeDeleteFile(allManifest[i]);
            }
        }

        AssetDatabase.Refresh();
    }

    public static void CopyCurSettingAssetBundlesToStreamingAssets()
    {
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        var channelName = GetCurSelectedChannel().ToString();
        CopyAssetBundlesToStreamingAssets(buildTarget, channelName);
        Debug.Log("Copy channel assetbundles to streaming assets done!");
    }

    public static void CheckAndAddSymbolIfNeeded(BuildTarget buildTarget, string targetSymbol)
    {
        if (buildTarget != BuildTarget.Android && buildTarget != BuildTarget.iOS)
        {
            Debug.LogError("Only support Android and IOS !");
            return;
        }

        var buildTargetGroup = buildTarget == BuildTarget.Android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if (!symbols.Contains("HOTFIX_ENABLE"))
        {
            symbols = string.Format("{0};{1};", symbols, "HOTFIX_ENABLE");
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
    }

    public static void CopyJavaFolder(string source, string destination)
    {
        if (!Directory.Exists(source))
        {
            return;
        }
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
            AssetDatabase.Refresh();
        }

        string[] sourceDirs = Directory.GetDirectories(source);
        for (int i = 0; i < sourceDirs.Length; i++)
        {
            CopyJavaFolder(sourceDirs[i] + "/", destination + "/" + Path.GetFileName(sourceDirs[i]));
        }

        string[] sourceFiles = Directory.GetFiles(source);
        for (int j = 0; j < sourceFiles.Length; j++)
        {
            if (sourceFiles[j].Contains("classes.jar"))
            {
                continue;
            }
            File.Copy(sourceFiles[j], destination + "/" + Path.GetFileName(sourceFiles[j]), true);
        }
    }
}
