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
/// 1、安卓打包可以不用区分渠道，没有IOS那样的机器审核难以通过的问题
/// </summary>

public class PackageTool : EditorWindow
{
    static private BuildTarget buildTarget = BuildTarget.Android;
    static private ChannelType channelType = ChannelType.Test;
    static private string resVersion = "1.0.0";
    static private LocalServerType localServerType = LocalServerType.CurrentMachine;
    static private string localServerIP = "127.0.0.1";
    static private bool androidBuildABForPerChannel;
    static private bool iosBuildABForPerChannel;
    static private bool buildABSForPerChannel;

    [MenuItem("Tools/Package", false, 0)]
    static void Init() {
        EditorWindow.GetWindow(typeof(PackageTool));
    }

    void OnEnable()
    {
        buildTarget = EditorUserBuildSettings.activeBuildTarget;
        channelType = PackageUtils.GetCurSelectedChannel();

        localServerType = PackageUtils.GetLocalServerType();
        localServerIP = PackageUtils.GetLocalServerIP();

        androidBuildABForPerChannel = PackageUtils.GetAndroidBuildABForPerChannelSetting();
        iosBuildABForPerChannel = PackageUtils.GetIOSBuildABForPerChannelSetting();
        LoadCurrentVersionFile(true);
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target : ", buildTarget);
        GUILayout.Space(5);
        bool buildTargetSupport = false;
        if (buildTarget != BuildTarget.Android && buildTarget != BuildTarget.iOS)
        {
            GUILayout.Label("Error : Only android or iOS build target supported!!!");
        }
        else
        {
            buildTargetSupport = true;
            channelType = (ChannelType)EditorGUILayout.EnumPopup("Build Channel : ", channelType);
        }
        GUILayout.EndVertical();

        if (buildTargetSupport)
        {
            if (GUI.changed)
            {
                PackageUtils.SaveCurSelectedChannel(channelType);
            }

            DrawAssetBundlesConfigGUI();
            DrawConfigGUI();
            DrawLocalServerGUI();
            DrawAssetBundlesGUI();
            DrawXLuaGUI();
            DrawBuildPlayerGUI();
        }
    }

    #region AB相关配置
    void DrawAssetBundlesConfigGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[AssetBundles Config]-------------");
        GUILayout.Space(3);

        // 是否为每个channel打一个AB包
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Build For Per Channel : ", GUILayout.Width(150));
        if (buildTarget == BuildTarget.Android)
        {
            buildABSForPerChannel = EditorGUILayout.Toggle(androidBuildABForPerChannel, GUILayout.Width(50));
            if (buildABSForPerChannel != androidBuildABForPerChannel)
            {
                androidBuildABForPerChannel = buildABSForPerChannel;
                PackageUtils.SaveAndroidBuildABForPerChannelSetting(buildABSForPerChannel);
            }
        }
        else
        {
            buildABSForPerChannel = EditorGUILayout.Toggle(iosBuildABForPerChannel, GUILayout.Width(50));
            if (buildABSForPerChannel != iosBuildABForPerChannel)
            {
                iosBuildABForPerChannel = buildABSForPerChannel;
                PackageUtils.SaveIOSBuildABForPerChannelSetting(buildABSForPerChannel);
            }
        }
        if (GUILayout.Button("Run All Checkers", GUILayout.Width(200)))
        {
            bool checkChannel = PackageUtils.BuildAssetBundlesForPerChannel(buildTarget);
            PackageUtils.CheckAndRunAllCheckers(checkChannel, true);
        }
        GUILayout.EndHorizontal();
    }
    #endregion

    #region 资源配置GUI
    void DrawConfigGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Config]-------------");

        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        GUILayout.Label("res_version", GUILayout.Width(100));
        resVersion = GUILayout.TextField(resVersion, GUILayout.Width(100));
        GUILayout.Label("Auto increase sub version, otherwise modify the text directly!", GUILayout.Width(500));
        GUILayout.EndHorizontal();

        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        GUILayout.Label("notice_version", GUILayout.Width(100));
        GUILayout.Label("1.0.0", GUILayout.Width(100));
        GUILayout.Label("No supported yet!", GUILayout.Width(500));
        GUILayout.EndHorizontal();

        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        GUILayout.Label("app_version", GUILayout.Width(100));
        GUILayout.Label(PlayerSettings.bundleVersion, GUILayout.Width(100));
        GUILayout.Label("Auto increase sub version, otherwise go to PlayerSetting!", GUILayout.Width(500));
        GUILayout.EndHorizontal();

        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (PackageUtils.BuildAssetBundlesForPerChannel(buildTarget))
        {
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
        }
        else
        {
            if (GUILayout.Button("Load Version", GUILayout.Width(100)))
            {
                LoadCurrentVersionFile();
            }
            if (GUILayout.Button("Save Version", GUILayout.Width(100)))
            {
                SaveCurrentVersionFile();
            }
        }
        GUILayout.EndHorizontal();
    }
    #endregion

    #region 本地服务器配置GUI
    void DrawLocalServerGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[AssetBundles Local Server]-------------");
        GUILayout.Space(3);

        GUILayout.BeginHorizontal();
        var curSelected = (LocalServerType)EditorGUILayout.EnumPopup("Local Server Type : ", localServerType, GUILayout.Width(300));
        bool typeChanged = curSelected != localServerType;
        if (typeChanged)
        {
            PackageUtils.SaveLocalServerType(curSelected);

            localServerType = curSelected;
            localServerIP = PackageUtils.GetLocalServerIP();
        }
        if (localServerType == LocalServerType.CurrentMachine)
        {
            GUILayout.Label(localServerIP);
        }
        else
        {
            localServerIP = GUILayout.TextField(localServerIP, GUILayout.Width(100));
            if (GUILayout.Button("Save", GUILayout.Width(200)))
            {
                PackageUtils.SaveLocalServerIP(localServerIP);
            }
        }
        GUILayout.EndHorizontal();
    }
    #endregion

    #region AB相关操作GUI
    void DrawAssetBundlesGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Build AssetBundles]-------------");
        GUILayout.Space(3);
        
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (buildABSForPerChannel)
        {
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
                AssetBundleMenuItems.ToolsOpenOutput();
            }
            if (GUILayout.Button("Copy To StreamingAsset", GUILayout.Width(200)))
            {
                AssetBundleMenuItems.ToolsCopyAssetbundles();
            }
        }
        else
        {
            if (GUILayout.Button("Execute Build", GUILayout.Width(200)))
            {
                BuildAssetBundlesForCurrentChannel();
            }
            if (GUILayout.Button("Open Output Folder", GUILayout.Width(200)))
            {
                AssetBundleMenuItems.ToolsOpenOutput();
            }
            if (GUILayout.Button("Copy To StreamingAsset", GUILayout.Width(200)))
            {
                AssetBundleMenuItems.ToolsCopyAssetbundles();
            }
        }
        GUILayout.EndHorizontal();
    }
    #endregion

    #region xlua相关GUI
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
    #endregion

    #region 打包相关GUI
    void DrawBuildAndroidPlayerGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Build Android Player]-------------");
        GUILayout.Space(3);

        GUILayout.BeginHorizontal();
        if (PackageUtils.BuildAssetBundlesForPerChannel(buildTarget))
        {
            if (GUILayout.Button("Copy SDK Resources", GUILayout.Width(200)))
            {
                PackageUtils.CopyAndroidSDKResources(channelType.ToString());
            }
            if (GUILayout.Button("Current Channel Only", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += BuildAndroidPlayerForCurrentChannel;
            }
            if (GUILayout.Button("For All Channels", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += BuildAndroidPlayerForAllChannels;
            }
            if (GUILayout.Button("Open Output Folder", GUILayout.Width(200)))
            {
                var folder = PackageUtils.GetChannelOutputPath(buildTarget, channelType.ToString());
                EditorUtils.ExplorerFolder(folder);
            }
        }
        else
        {
            if (GUILayout.Button("Copy SDK Resource", GUILayout.Width(200)))
            {
                PackageUtils.CopyAndroidSDKResources(channelType.ToString());
            }
            if (GUILayout.Button("Execute Build", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += BuildAndroidPlayerForCurrentChannel;
            }
            if (GUILayout.Button("Open Output Folder", GUILayout.Width(200)))
            {
                var folder = PackageUtils.GetChannelOutputPath(buildTarget, channelType.ToString());
                EditorUtils.ExplorerFolder(folder);
            }
        }
        GUILayout.EndHorizontal();
    }

    void DrawBuildIOSPlayerGUI()
    {
        GUILayout.Space(3);
        GUILayout.Label("-------------[Build IOS Player]-------------");
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (PackageUtils.BuildAssetBundlesForPerChannel(buildTarget))
        {
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
        }
        else
        {
            if (GUILayout.Button("Execute Build", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += BuildIOSPlayerForCurrentChannel;
            }
            if (GUILayout.Button("Open Output Folder", GUILayout.Width(200)))
            {
                var folder = Path.Combine(System.Environment.CurrentDirectory, BuildPlayer.XCodeOutputPath);
                EditorUtils.ExplorerFolder(folder);
            }
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
    #endregion

    #region 资源配置操作
    public static string ReadVersionFile(BuildTarget target, ChannelType channel)
    {
        string rootPath = PackageUtils.GetAssetBundleOutputPath(target, channel.ToString());
        return GameUtility.SafeReadAllText(Path.Combine(rootPath, BuildUtils.ResVersionFileName));
    }

    public static void SaveVersionFile(BuildTarget target, ChannelType channel)
    {
        string rootPath = PackageUtils.GetAssetBundleOutputPath(target, channel.ToString());
        GameUtility.SafeWriteAllText(Path.Combine(rootPath, BuildUtils.ResVersionFileName), resVersion);
        GameUtility.SafeWriteAllText(Path.Combine(rootPath, BuildUtils.NoticeVersionFileName), resVersion);
        GameUtility.SafeWriteAllText(Path.Combine(rootPath, BuildUtils.AppVersionFileName), PlayerSettings.bundleVersion);
    }

    public static void LoadCurrentVersionFile(bool silence = false)
    {
        string readVersion = ReadVersionFile(buildTarget, channelType);
        if (string.IsNullOrEmpty(readVersion))
        {
            if (!silence)
            {
                var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
                EditorUtility.DisplayDialog("Error", string.Format("No version file  for : \n\nplatform : {0} \nchannel : {1} \n\n",
                    buildTargetName, channelType.ToString()), "Confirm");
            }
        }
        else
        {
            resVersion = readVersion;
            if (!silence)
            {
                EditorUtility.DisplayDialog("Success", "Load cur version file done!", "Confirm");
            }
        }
    }

    public static void SaveCurrentVersionFile(bool silence = false)
    {
        SaveVersionFile(buildTarget, channelType);
        if (!silence)
        {
            EditorUtility.DisplayDialog("Success", "Save version file done!", "Confirm");
        }
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
    #endregion

    #region AB相关操作
    public static void IncreaseResSubVersion()
    {
        // 每一次构建资源，子版本号自增，注意：前两个字段这里不做托管，自行编辑设置
        LoadCurrentVersionFile(true);
        string[] vers = resVersion.Split('.');
        if (vers.Length > 0)
        {
            int subVer = 0;
            int.TryParse(vers[vers.Length - 1], out subVer);
            vers[vers.Length - 1] = (subVer + 1).ToString();
        }
        resVersion = string.Join(".", vers);
        SaveCurrentVersionFile(true);
    }

    public static void BuildAssetBundlesForCurrentChannel()
    {
        var start = DateTime.Now;
        BuildPlayer.BuildAssetBundles(buildTarget, channelType.ToString());

        IncreaseResSubVersion();
        var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
        EditorUtility.DisplayDialog("Success", string.Format("Build AssetBundles for : \n\nplatform : {0} \nchannel : {1} \n\ndone! use {2}s", 
            buildTargetName, channelType, (DateTime.Now - start).TotalSeconds), "Confirm");
    }

    public static void BuildAssetBundlesForAllChannels()
    {
        var start = DateTime.Now;
        BuildPlayer.BuildAssetBundlesForAllChannels(buildTarget);

        IncreaseResSubVersion();
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
    #endregion

    #region 打包相关操作
    public static void IncreaseAppSubVersion()
    {
        // 每一次构建安装包，子版本号自增，注意：前两个字段这里不做托管，自行到PlayerSetting中设置
        string bundleVersion = PlayerSettings.bundleVersion;
        string[] vers = bundleVersion.Split('.');
        if (vers.Length > 0)
        {
            int subVer = 0;
            int.TryParse(vers[vers.Length - 1], out subVer);
            vers[vers.Length - 1] = (subVer + 1).ToString();
        }
        PlayerSettings.bundleVersion = string.Join(".", vers);
        SaveCurrentVersionFile(true);
    }

    public static void BuildAndroidPlayerForCurrentChannel()
    {
        if (CheckSymbolsToCancelBuild())
        {
            return;
        }

        var start = DateTime.Now;
        BuildPlayer.BuildAndroid(channelType.ToString(), channelType == ChannelType.Test);

        IncreaseAppSubVersion();
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

        IncreaseAppSubVersion();
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

        IncreaseAppSubVersion();
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

        IncreaseAppSubVersion();
        var buildTargetName = PackageUtils.GetPlatformName(buildTarget);
        EditorUtility.DisplayDialog("Success", string.Format("Build player for : \n\nplatform : {0} \nchannel : all \n\ndone! use {2}s", buildTargetName, (DateTime.Now - start).TotalSeconds), "Confirm");
    }
    #endregion
}
