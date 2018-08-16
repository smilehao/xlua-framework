using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GameChannel;
using AssetBundles;
using System;
using System.Text;

/// <summary>
/// added by wsh @ 2018.01.03
/// 功能： 打包相关配置和通用函数
/// 
/// 注意：
/// 1）如果为每个渠道分别打AB包，则将渠道名打入各个AB包<---为了解决iOS各个渠道包的提审问题
/// 
/// </summary>

public class BuildPlayer : Editor
{
    public const string XCodeOutputPath = "vXCode";

    public static void WriteChannelNameFile(BuildTarget buildTarget, string channelName)
    {
        var outputPath = PackageUtils.GetAssetBundleOutputPath(buildTarget, channelName);
        GameUtility.SafeWriteAllText(Path.Combine(outputPath, BuildUtils.ChannelNameFileName), channelName);
    }

    public static void WriteAssetBundleSize(BuildTarget buildTarget, string channelName)
    {
        var outputPath = PackageUtils.GetAssetBundleOutputPath(buildTarget, channelName);
        var allAssetbundles = GameUtility.GetSpecifyFilesInFolder(outputPath, new string[] { ".assetbundle" });
        StringBuilder sb = new StringBuilder();
        if (allAssetbundles != null && allAssetbundles.Length > 0)
        {
            foreach (var assetbundle in allAssetbundles)
            {
                FileInfo fileInfo = new FileInfo(assetbundle);
                int size = (int)(fileInfo.Length / 1024) + 1;
                var path = assetbundle.Substring(outputPath.Length + 1);
                sb.AppendFormat("{0}{1}{2}\n", GameUtility.FormatToUnityPath(path), AssetBundleConfig.CommonMapPattren, size);
            }
        }
        string content = sb.ToString().Trim();
        GameUtility.SafeWriteAllText(Path.Combine(outputPath, BuildUtils.AssetBundlesSizeFileName), content);
    }
    
    private static void InnerBuildAssetBundles(BuildTarget buildTarget, string channelName, bool writeConfig)
    {
        BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.IgnoreTypeTreeChanges | BuildAssetBundleOptions.DeterministicAssetBundle;
        string outputPath = PackageUtils.GetAssetBundleOutputPath(buildTarget, channelName);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, buildOption, buildTarget);
        if (manifest != null && writeConfig)
        {
            AssetsPathMappingEditor.BuildPathMapping(manifest);
            VariantMappingEditor.BuildVariantMapping(manifest);
            BuildPipeline.BuildAssetBundles(outputPath, buildOption, buildTarget);
        }
        WriteChannelNameFile(buildTarget, channelName);
        WriteAssetBundleSize(buildTarget, channelName);
        AssetDatabase.Refresh();
    }

    public static void BuildAssetBundles(BuildTarget buildTarget, string channelName)
    {
        bool buildForPerChannel = PackageUtils.BuildAssetBundlesForPerChannel(buildTarget);

        XLuaMenu.CopyLuaFilesToAssetsPackage();
        PackageUtils.CheckAndRunAllCheckers(buildForPerChannel, false);

        DateTime start = DateTime.Now;
        if (buildForPerChannel)
        {
            start = DateTime.Now;
            CheckAssetBundles.SwitchChannel(channelName);
            Debug.Log("Finished CheckAssetBundles.SwitchChannel! use " + (DateTime.Now - start).TotalSeconds + "s");
        }

        start = DateTime.Now;
        InnerBuildAssetBundles(buildTarget, channelName, true);
        Debug.Log("Finished InnerBuildAssetBundles! use " + (DateTime.Now - start).TotalSeconds + "s");

        var targetName = PackageUtils.GetPlatformName(buildTarget);
        Debug.Log(string.Format("Build assetbundles for platform : {0} and channel : {1} done!", targetName, channelName));
    }

    public static void BuildAssetBundlesForAllChannels(BuildTarget buildTarget)
    {
        bool buildForPerChannel = PackageUtils.BuildAssetBundlesForPerChannel(buildTarget);
        var targetName = PackageUtils.GetPlatformName(buildTarget);
        Debug.Assert(buildForPerChannel == true);

        XLuaMenu.CopyLuaFilesToAssetsPackage();
        PackageUtils.CheckAndRunAllCheckers(buildForPerChannel, false);

        int index = 0;
        double switchChannel = 0;
        double buildAssetbundles = 0;
        var start = DateTime.Now;
        foreach (var current in (ChannelType[])Enum.GetValues(typeof(ChannelType)))
        {
            var channelName = current.ToString();

            start = DateTime.Now;
            CheckAssetBundles.SwitchChannel(channelName);
            switchChannel = (DateTime.Now - start).TotalSeconds;

            start = DateTime.Now;
            InnerBuildAssetBundles(buildTarget, channelName, index == 0);
            buildAssetbundles = (DateTime.Now - start).TotalSeconds;

            index++;
            Debug.Log(string.Format("{0}.Build assetbundles for platform : {1} and channel : {2} done! use time : switchChannel = {3}s , build assetbundls = {4} s", index, targetName, channelName, switchChannel, buildAssetbundles));
        }

        Debug.Log(string.Format("Build assetbundles for platform : {0} for all {1} channels done!", targetName, index));
    }

    public static void BuildAssetBundlesForCurSetting()
    {
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        string channelName = PackageUtils.GetCurSelectedChannel().ToString();
        BuildAssetBundles(buildTarget, channelName);
    }

    private static void SetPlayerSetting(BaseChannel channel)
    {
        if (channel != null)
        {
#if UNITY_5_6_OR_NEWER
            PlayerSettings.applicationIdentifier = channel.GetBundleID();
#else
            PlayerSettings.bundleIdentifier = channel.GetBundleID();
#endif
            PlayerSettings.productName = channel.GetProductName();
            PlayerSettings.companyName = channel.GetCompanyName();
        }
    }

    public static void BuildAndroid(string channelName, bool isTest)
    {
        BuildTarget buildTarget = BuildTarget.Android;
        PackageUtils.CopyAssetBundlesToStreamingAssets(buildTarget, channelName);
        if (!isTest)
        {
            LaunchAssetBundleServer.ClearAssetBundleServerURL();
        }
        else
        {
            LaunchAssetBundleServer.WriteAssetBundleServerURL();
        }
        
        BaseChannel channel = ChannelManager.instance.CreateChannel(channelName);
        SetPlayerSetting(channel);

        string savePath = PackageUtils.GetChannelOutputPath(buildTarget, channelName);
        string appName = channel.GetProductName() + ".apk";
        if (channel.IsGooglePlay())
        {
            savePath = Path.Combine(savePath, "GooglePlay");
            GameUtility.SafeDeleteDir(savePath);
            BuildPipeline.BuildPlayer(GetBuildScenes(), savePath, buildTarget, BuildOptions.AcceptExternalModificationsToPlayer);
        }
        else
        {
            savePath = Path.Combine(savePath, appName);
            BuildPipeline.BuildPlayer(GetBuildScenes(), savePath, buildTarget, BuildOptions.None);
        }
        string outputPath = Path.Combine(Application.persistentDataPath, AssetBundleConfig.AssetBundlesFolderName);
        GameUtility.SafeDeleteDir(outputPath);
        Debug.Log(string.Format("Build android player for : {0} done! output ：{1}", channelName, savePath));
    }
    
    public static void BuildXCode(string channelName, bool isTest)
    {
        BuildTarget buildTarget = BuildTarget.iOS;
        PackageUtils.CopyAssetBundlesToStreamingAssets(buildTarget, channelName);
        if (!isTest)
        {
            LaunchAssetBundleServer.ClearAssetBundleServerURL();
        }
        else
        {
            LaunchAssetBundleServer.WriteAssetBundleServerURL();
        }

        string buildFolder = Path.Combine(System.Environment.CurrentDirectory, XCodeOutputPath);
        buildFolder = Path.Combine(buildFolder, channelName);
        GameUtility.CheckDirAndCreateWhenNeeded(buildFolder);

        string iconPath = "Assets/Editor/icon/ios/{0}/{1}.png";
        string[] iconSizes = new string[] { "180", "167","152", "144", "120", "114", "76", "72", "57" };
        List<Texture2D> iconList = new List<Texture2D>();
        for (int i = 0; i < iconSizes.Length; i++)
        {
            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(string.Format(iconPath, channelName, iconSizes[i]), typeof(Texture2D));
            iconList.Add(texture);
        }
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, iconList.ToArray());

        BaseChannel channel = ChannelManager.instance.CreateChannel(channelName);
        SetPlayerSetting(channel);

        PackageUtils.CheckAndAddSymbolIfNeeded(buildTarget, channelName);
        BuildPipeline.BuildPlayer(GetBuildScenes(), buildFolder, buildTarget, BuildOptions.None);

        string outputPath = Path.Combine(Application.persistentDataPath, AssetBundleConfig.AssetBundlesFolderName);
        GameUtility.SafeDeleteDir(outputPath);
    }
	
	static string[] GetBuildScenes()
	{
		List<string> names = new List<string>();
		foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e != null && e.enabled)
            {
                names.Add(e.path);
            }
        }
        return names.ToArray();
    }
}
