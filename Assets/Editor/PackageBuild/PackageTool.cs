using UnityEditor;
using UnityEngine;
using System.IO;
using GameChannel;
using AssetBundles;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// added by wsh @ 2018.01.03
/// 说明：打包工具
/// TODO：
/// 1、安装打包可以不用区分渠道，没有IOS那样的机器审核难以通过的问题
/// </summary>

public class PackageTool : EditorWindow
{
    static private BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
    static private ChannelType channelType = ChannelType.Test;
    static private string resVersion = "1.0.0";

    static PackageTool()
    {
        channelType = PackageUtils.GetCurSelectedChannel();
    }

    [MenuItem("Tools/Package", false, 0)]
    static void Init() {
        EditorWindow.GetWindow(typeof(PackageTool));
    }

    void DrawConfigGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Config]-------------");

        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        GUILayout.Label("res_version", GUILayout.Width(100));
        resVersion = GUILayout.TextField(resVersion, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        GUILayout.Label("notice_version", GUILayout.Width(100));
        GUILayout.Label("1.0.0", GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        GUILayout.Label("app_version", GUILayout.Width(100));
        GUILayout.Label(PlayerSettings.bundleVersion, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Current", GUILayout.Width(100)))
        {
            LoadCurrentVersionFile();
        }
        if (GUILayout.Button("Save Current", GUILayout.Width(100)))
        {
            SaveCurrentVersionFile();
        }
        if (GUILayout.Button("Validate All", GUILayout.Width(100)))
        {
            ValidateAllVersionFile();
        }
        if (GUILayout.Button("Save For All", GUILayout.Width(100)))
        {
            SaveAllVersionFile();
        }
        GUILayout.EndHorizontal();
    }
    
    void DrawAssetBundlesGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Build AssetBundles]-------------");
        GUILayout.Space(3);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Current Channel Only", GUILayout.Width(200)))
        {
            BuildAssetBundlesForCurrentChannel();
        }
        if (GUILayout.Button("For All Channels", GUILayout.Width(200)))
        {
            BuildAssetBundlesForAllChannels();
        }
        if (GUILayout.Button("Open Current Output", GUILayout.Width(200)))
        {
            AssetBundleMenuItems.ToolsToolsOpenOutput();
        }
        if (GUILayout.Button("Copy To StreamingAsset", GUILayout.Width(200)))
        {
            AssetBundleMenuItems.ToolsToolsCopyAssetbundles();
        }
        GUILayout.EndHorizontal();
    }

    void DrawXLuaGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Gen XLua Code]-------------");
        GUILayout.Space(3);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Code", GUILayout.Width(200)))
        {
            GenXLuaCode(buildTarget);
        }
        GUILayout.EndHorizontal();
    }

    void DrawBuildAndroidPlayerGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Build Android Player]-------------");
        GUILayout.Space(3);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Current Channel Only", GUILayout.Width(200)))
        {
            EditorApplication.delayCall += BuildAndroidPlayerForCurrentChannel;
        }
        if (GUILayout.Button("For All Channels", GUILayout.Width(200)))
        {
            EditorApplication.delayCall += BuildAndroidPlayerForAllChannels;
        }
        if (GUILayout.Button("Open Current Output", GUILayout.Width(200)))
        {
            var folder = Path.Combine(System.Environment.CurrentDirectory, BuildPlayer.ApkOutputPath);
            EditorUtils.ExplorerFolder(folder);
        }
        GUILayout.EndHorizontal();
    }

    void DrawBuildIOSPlayerGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Build IOS Player]-------------");
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Current Channel Only", GUILayout.Width(200)))
        {
            EditorApplication.delayCall += BuildIOSPlayerForCurrentChannel;
        }
        if (GUILayout.Button("For All Channels", GUILayout.Width(200)))
        {
            EditorApplication.delayCall += BuildIOSPlayerForAllChannels;
        }
        if (GUILayout.Button("Open Current Output", GUILayout.Width(200)))
        {
            var folder = Path.Combine(System.Environment.CurrentDirectory, BuildPlayer.XCodeOutputPath);
            EditorUtils.ExplorerFolder(folder);
        }
        GUILayout.EndHorizontal();
    }

    void DrawBuildPlayerGUI()
    {
        if (buildTarget == BuildTarget.Android)
        {
            DrawBuildAndroidPlayerGUI();
        }
        else if (buildTarget == BuildTarget.iOS)
        {
            DrawBuildIOSPlayerGUI();
        }
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target : ", buildTarget);
        GUILayout.Space(5);
        channelType = (ChannelType)EditorGUILayout.EnumPopup("Build Channel : ", channelType);
        GUILayout.EndVertical();

        if (GUI.changed)
        {
            PackageUtils.SaveCurSelectedChannel(channelType);
        }

        DrawConfigGUI();
        DrawAssetBundlesGUI();
        DrawXLuaGUI();
        DrawBuildPlayerGUI();
    }

    public static string ReadVersionFile(BuildTarget target, ChannelType channel)
    {
        string rootPath = PackageUtils.GetBuildPlatformOutputPath(target, channel.ToString());
        return GameUtility.SafeReadAllText(Path.Combine(rootPath, BuildUtils.ResVersionFileName));
    }

    public static void SaveVersionFile(BuildTarget target, ChannelType channel)
    {
        string rootPath = PackageUtils.GetBuildPlatformOutputPath(target, channel.ToString());
        GameUtility.SafeWriteAllText(Path.Combine(rootPath, BuildUtils.ResVersionFileName), resVersion);
        GameUtility.SafeWriteAllText(Path.Combine(rootPath, BuildUtils.NoticeVersionFileName), resVersion);
        GameUtility.SafeWriteAllText(Path.Combine(rootPath, BuildUtils.AppVersionFileName), PlayerSettings.bundleVersion);
    }

    public static void LoadCurrentVersionFile()
    {
        string readVersion = ReadVersionFile(buildTarget, channelType);
        if (string.IsNullOrEmpty(readVersion))
        {
            var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
            EditorUtility.DisplayDialog("Error", string.Format("No version file  for : \n\nplatform : {0} \nchannel : {1} \n\n", buildTargetName, channelType.ToString()), "Confirm");
        }
        else
        {
            resVersion = readVersion;
            EditorUtility.DisplayDialog("Success", "Load cur version file done!", "Confirm");
        }
    }

    public static void SaveCurrentVersionFile()
    {
        SaveVersionFile(buildTarget, channelType);
        EditorUtility.DisplayDialog("Success", "Save version file done!", "Confirm");
    }

    public static void ValidateAllVersionFile()
    {
        Dictionary<string, List<ChannelType>> versionMap = new Dictionary<string, List<ChannelType>>();
        List<ChannelType> channelList = null;
        foreach (var current in (ChannelType[])Enum.GetValues(typeof(ChannelType)))
        {
            string readVersion = ReadVersionFile(buildTarget, current);
            if (readVersion == null)
            {
                readVersion = "";
            }
            versionMap.TryGetValue(readVersion, out channelList);
            if (channelList == null)
            {
                channelList = new List<ChannelType>();
            }
            channelList.Add(current);
            versionMap[readVersion] = channelList;
        }

        StringBuilder sb = new StringBuilder();
        foreach (var current in versionMap)
        {
            var version = current.Key;
            var channels = current.Value;
            sb.AppendFormat("Version : {0}\n", version);
            sb.AppendFormat("{0} Channels : ", channels.Count);
            foreach (var channel in channels)
            {
                sb.AppendFormat("{0}, ", channel.ToString());
            }
            sb.AppendLine("\n-------------------------------------\n");
        }
        EditorUtility.DisplayDialog("Result", sb.ToString(), "Confirm");
    }

    void SaveAllVersionFile()
    {
        foreach (var current in (ChannelType[])Enum.GetValues(typeof(ChannelType)))
        {
            SaveVersionFile(buildTarget, current);
        }
        EditorUtility.DisplayDialog("Success", "Save all version files done!", "Confirm");
    }
    
    public static void BuildAssetBundlesForCurrentChannel()
    {
        var start = DateTime.Now;
        BuildPlayer.BuildAssetBundles(buildTarget, channelType.ToString());

        var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
        EditorUtility.DisplayDialog("Success", string.Format("Build AssetBundles for : \n\nplatform : {0} \nchannel : {1} \n\ndone! use {2}s", buildTargetName, channelType, (DateTime.Now - start).TotalSeconds), "Confirm");
    }

    public static void BuildAssetBundlesForAllChannels()
    {
        var start = DateTime.Now;
        BuildPlayer.BuildAssetBundlesForAllChannels(buildTarget);

        var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
        EditorUtility.DisplayDialog("Success", string.Format("Build AssetBundles for : \n\nplatform : {0} \nchannel : all \n\ndone! use {1}s", buildTargetName, (DateTime.Now - start).TotalSeconds), "Confirm");
    }

    public static void GenXLuaCode(BuildTarget buildTarget)
    {
        PackageUtils.CheckAndAddSymbolIfNeeded(buildTarget, "HOTFIX_ENABLE");
        CSObjectWrapEditor.Generator.ClearAll();
        CSObjectWrapEditor.Generator.GenAll();
    }

    public static bool CheckSymbolsToCancelBuild()
    {
        var buildTargetGroup = buildTarget == BuildTarget.Android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        var replace = symbols.Replace("HOTFIX_ENABLE", "");
        replace = symbols.Replace(";", "").Trim();
        if (!string.IsNullOrEmpty(replace))
        {
            int checkClear = EditorUtility.DisplayDialogComplex("Build Symbol Warning",
                string.Format("Now symbols : \n\n{0}\n\nClear all symbols except \"HOTFIX_ENABLE\" ?", symbols),
                "Yes", "No", "Cancel");
            if (checkClear == 0)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, "HOTFIX_ENABLE");
            }
            return checkClear == 2;
        }
        return false;
    }

    public static void BuildAndroidPlayerForCurrentChannel()
    {
        if (CheckSymbolsToCancelBuild())
        {
            return;
        }

        var start = DateTime.Now;
        BuildPlayer.BuildAndroid(channelType.ToString(), channelType == ChannelType.Test);

        var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
        EditorUtility.DisplayDialog("Success", string.Format("Build player for : \n\nplatform : {0} \nchannel : {1} \n\ndone! use {2}s", buildTargetName, channelType, (DateTime.Now - start).TotalSeconds), "Confirm");
    }

    public static void BuildAndroidPlayerForAllChannels()
    {
        if (CheckSymbolsToCancelBuild())
        {
            return;
        }

        var start = DateTime.Now;
        foreach (var current in (ChannelType[])Enum.GetValues(typeof(ChannelType)))
        {
            BuildPlayer.BuildAndroid(current.ToString(), current == ChannelType.Test);
        }

        var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
        EditorUtility.DisplayDialog("Success", string.Format("Build player for : \n\nplatform : {0} \nchannel : all \n\ndone! use {2}s", buildTargetName, (DateTime.Now - start).TotalSeconds), "Confirm");
    }

    public static void BuildIOSPlayerForCurrentChannel()
    {
        if (CheckSymbolsToCancelBuild())
        {
            return;
        }

        var start = DateTime.Now;
        BuildPlayer.BuildXCode(channelType.ToString(), channelType == ChannelType.Test);

        var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
        EditorUtility.DisplayDialog("Success", string.Format("Build player for : \n\nplatform : {0} \nchannel : {1} \n\ndone! use {2}s", buildTargetName, channelType, (DateTime.Now - start).TotalSeconds), "Confirm");
    }

    public static void BuildIOSPlayerForAllChannels()
    {
        if (CheckSymbolsToCancelBuild())
        {
            return;
        }

        var start = DateTime.Now;
        foreach (var current in (ChannelType[])Enum.GetValues(typeof(ChannelType)))
        {
            BuildPlayer.BuildXCode(current.ToString(), channelType == ChannelType.Test);
        }

        var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
        EditorUtility.DisplayDialog("Success", string.Format("Build player for : \n\nplatform : {0} \nchannel : all \n\ndone! use {2}s", buildTargetName, (DateTime.Now - start).TotalSeconds), "Confirm");
    }
}
