using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GameChannel;
using AssetBundles;
using System;
using System.Text;

public class BuildPlayer : Editor
{
    public const string ApkOutputPath = "vAPK";
    public const string XCodeOutputPath = "vXCode";

    public static void WritePackageNameFile(BuildTarget buildTarget, string channelName)
    {
        var outputPath = PackageUtils.GetBuildPlatformOutputPath(buildTarget, channelName);
        GameUtility.SafeWriteAllText(Path.Combine(outputPath, BuildUtils.PackageNameFileName), channelName);
    }

    public static void WriteAssetBundleSize(BuildTarget buildTarget, string channelName)
    {
        var outputPath = PackageUtils.GetBuildPlatformOutputPath(buildTarget, channelName);
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
        var outputPath = PackageUtils.GetBuildPlatformOutputPath(buildTarget, channelName);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, buildOption, buildTarget);
        if (manifest != null && writeConfig)
        {
            AssetsPathMappingEditor.BuildPathMapping(manifest);
            VariantMappingEditor.BuildVariantMapping(manifest);
            BuildPipeline.BuildAssetBundles(outputPath, buildOption, buildTarget);
        }
        WritePackageNameFile(buildTarget, channelName);
        WriteAssetBundleSize(buildTarget, channelName);
        AssetDatabase.Refresh();
    }

    public static void BuildAssetBundles(BuildTarget buildTarget, string channelName)
    {
        var start = DateTime.Now;
        CheckAssetBundles.Run();
        Debug.Log("Finished CheckAssetBundles.Run! use " + (DateTime.Now - start).TotalSeconds + "s");

        start = DateTime.Now;
        CheckAssetBundles.SwitchChannel(channelName.ToString());
        Debug.Log("Finished CheckAssetBundles.SwitchChannel! use " + (DateTime.Now - start).TotalSeconds + "s");

        start = DateTime.Now;
        InnerBuildAssetBundles(buildTarget, channelName, true);
        Debug.Log("Finished InnerBuildAssetBundles! use " + (DateTime.Now - start).TotalSeconds + "s");

        var targetName = PackageUtils.GetPlatformName(buildTarget);
        Debug.Log(string.Format("Build assetbundles for platform : {0} and channel : {1} done!", targetName, channelName));
    }

    public static void BuildAssetBundlesForAllChannels(BuildTarget buildTarget)
    {
        var targetName = PackageUtils.GetPlatformName(buildTarget);

        var start = DateTime.Now;
        CheckAssetBundles.Run();
        Debug.Log("Finished CheckAssetBundles.Run! use " + (DateTime.Now - start).TotalSeconds + "s");

        int index = 0;
        double switchChannel = 0;
        double buildAssetbundles = 0;
        foreach (var current in (ChannelType[])Enum.GetValues(typeof(ChannelType)))
        {
            start = DateTime.Now;
            var channelName = current.ToString();
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

    public static void BuildAndroid(string channelName, bool isTest)
    {
        BuildTarget buildTarget = BuildTarget.Android;
        PackageUtils.CopyAssetBundlesToStreamingAssets(buildTarget, channelName);
        if (!isTest)
        {
            CopyAndroidSDKResources(channelName);
            LaunchAssetBundleServer.ClearAssetBundleServerURL();
        }
        else
        {
            LaunchAssetBundleServer.WriteAssetBundleServerURL();
        }

        string buildFolder = Path.Combine(System.Environment.CurrentDirectory, ApkOutputPath);
        GameUtility.CheckDirAndCreateWhenNeeded(buildFolder);
        BaseChannel channel = ChannelManager.instance.CreateChannel(channelName);
#if UNITY_5_6_OR_NEWER
        PlayerSettings.applicationIdentifier = channel.GetBundleID();
#else
        PlayerSettings.bundleIdentifier = channel.GetBundleID();
#endif
        PlayerSettings.productName = channel.GetPackageName();

        string savePath = null;
        if (channel.IsGooglePlay())
        {
            savePath = Path.Combine(buildFolder, channelName);
            GameUtility.SafeDeleteDir(savePath);
            BuildPipeline.BuildPlayer(GetBuildScenes(), savePath, buildTarget, BuildOptions.AcceptExternalModificationsToPlayer);
        }
        else
        {
            savePath = Path.Combine(buildFolder, channel.GetPackageName() + ".apk");
            BuildPipeline.BuildPlayer(GetBuildScenes(), savePath, buildTarget, BuildOptions.None);
        }
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
#if UNITY_5_6_OR_NEWER
        PlayerSettings.applicationIdentifier = channel.GetBundleID();
#else
        PlayerSettings.bundleIdentifier = channel.GetBundleID();
#endif
        PlayerSettings.productName = channel.GetPackageName();
        PackageUtils.CheckAndAddSymbolIfNeeded(buildTarget, channelName);
        BuildPipeline.BuildPlayer(GetBuildScenes(), buildFolder, buildTarget, BuildOptions.None);
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


    public static void CopyAndroidSDKResources(string channelName)
    {
        string targetPath = Path.Combine(Application.dataPath, "/Plugins/Android");
        GameUtility.SafeDeleteDir(targetPath);

        string resPath = Path.Combine(Environment.CurrentDirectory, "/Channel/UnityCallAndroid_" + channelName);
        if (!Directory.Exists(resPath))
        {
            resPath = Path.Combine(Environment.CurrentDirectory, "/Channel/UnityCallAndroid" + channelName);
        }

        EditorUtility.DisplayProgressBar("提示", "正在拷贝SDK资源，请稍等", 0f);
        PackageUtils.CopyJavaFolder(resPath + "/assets", targetPath + "/assets");
        EditorUtility.DisplayProgressBar("提示", "正在拷贝SDK资源，请稍等", 0.3f);
        PackageUtils.CopyJavaFolder(resPath + "/libs", targetPath + "/libs");
        EditorUtility.DisplayProgressBar("提示", "正在拷贝SDK资源，请稍等", 0.6f);
        PackageUtils.CopyJavaFolder(resPath + "/res", targetPath + "/res");
        if (File.Exists(resPath + "/bin/UnityCallAndroid.jar"))
        {
            File.Copy(resPath + "/bin/UnityCallAndroid.jar", targetPath + "/libs/UnityCallAndroid.jar", true);
        }
        if (File.Exists(resPath + "/AndroidManifest.xml"))
        {
            File.Copy(resPath + "/AndroidManifest.xml", targetPath + "/AndroidManifest.xml", true);
        }
        if (File.Exists(resPath + "/icon/icon.png"))
        {
            File.Copy(resPath + "/icon/icon.png", Application.dataPath + "/icon.png", true);
        }

        EditorUtility.DisplayProgressBar("提示", "正在拷贝SDK资源，请稍等", 1f);
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
}
