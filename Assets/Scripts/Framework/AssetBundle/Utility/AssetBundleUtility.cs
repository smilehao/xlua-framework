using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.Net;
using System.Net.Sockets;
#endif
using System.IO;

/// <summary>
/// added by wsh @ 2017.12.25
/// 功能： Assetbundle相关的通用静态函数，提供运行时，或者Editor中使用到的有关Assetbundle操作和路径处理的函数
/// </summary>

namespace AssetBundles
{
    public class AssetBundleUtility
    {
        public static string GetCurPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformName(EditorUserBuildSettings.activeBuildTarget);
#else
			return GetPlatformName(Application.platform);
#endif
        }

#if UNITY_EDITOR
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
#endif
        
        private static string GetPlatformName(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                default:
                    Logger.LogError("Error platform!!!");
                    return null;
            }
        }
        
#if UNITY_EDITOR
        public static string GetBuildPlatformOutputPath(BuildTarget target)
        {
            string outputPath = Path.Combine(AssetBundleConfig.AssetBundlesBuildOutputPath, GetPlatformName(target));
            GameUtility.CheckDirAndCreateWhenNeeded(outputPath);
            return outputPath;
        }
#endif
        
        public static string GetPlatformStreamingAssetsFilePath(string assetPath = null)
        {
#if UNITY_EDITOR
            var target = EditorUserBuildSettings.activeBuildTarget;
            string outputPath = Path.Combine("file://" + Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
            outputPath = Path.Combine(outputPath, GetPlatformName(target));
#else
            var platform = Application.platform;
#if UNITY_IPHONE || UNITY_IOS
            string outputPath = Path.Combine("file://" + Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
#elif UNITY_ANDROID
            string outputPath = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
#else
            Logger.LogError("Unsupported platform!!!");
#endif
            outputPath = Path.Combine(outputPath, GetPlatformName(platform));
#endif
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
            return outputPath;
        }
        
        public static string GetPlatformPersistentFilePath(string assetPath = null)
        {
            return "file://" + GetPlatformPersistentDataPath(assetPath);
        }

        public static string GetPlatformPersistentDataPath(string assetPath = null)
        {
#if UNITY_EDITOR
            var target = EditorUserBuildSettings.activeBuildTarget;
            string outputPath = Path.Combine(Application.persistentDataPath, AssetBundleConfig.AssetBundlesFolderName);
            outputPath = Path.Combine(outputPath, GetPlatformName(target));
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
            return GameUtility.FormatToSysFilePath(outputPath);
#else
            var platform = Application.platform;
            string outputPath = Path.Combine(Application.persistentDataPath, AssetBundleConfig.AssetBundlesFolderName);
            outputPath = Path.Combine(outputPath, GetPlatformName(platform));
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
            return outputPath;
#endif
        }

        public static bool CheckPlatformPersistentFileExsits(string filePath)
        {
            var path = GetPlatformPersistentDataPath(filePath);
            return File.Exists(path);
        }

        // 注意：这个路径是给WWW读文件使用的url，如果要直接磁盘写persistentDataPath，使用GetPlatformPersistentDataPath
        public static string GetPlatformFileUrl(string filePath)
        {
            if (CheckPlatformPersistentFileExsits(filePath))
            {
                return GetPlatformPersistentFilePath(filePath);
            }
            else
            {
                return GetPlatformStreamingAssetsFilePath(filePath);
            }
        }

        // 检测AB包所在Asset路径有效性
        public static bool CheckAssetBundleAssetPathValid(string assetbundleAssetPath)
        {
            if (string.IsNullOrEmpty(assetbundleAssetPath))
            {
                Debug.LogError("assetbundleAssetPath null!");
                return false;
            }
            
            if (assetbundleAssetPath.Contains(" "))
            {
                Debug.LogError("assetbundleAssetPath contains empty char!");
                return false;
            }

            if (assetbundleAssetPath.Contains("."))
            {
                Debug.LogError("assetbundleAssetPath contains '.'!");
                return false;
            }

            return true;
        }

        // 注意：这里不处理Variant，且是Assetbundle在Assets中的路径，游戏逻辑层别使用，这里只用于特殊情况
        public static string AssetBundleAssetPathToAssetBundleName(string assetPath)
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                //remove root "Assets/"
                assetPath = assetPath.Replace("Assets/", "");
                //no " "
                assetPath = assetPath.Replace(" ", "");
                //there should not be any '.' in the assetbundle name
                //otherwise the variant handling in client may go wrong
                assetPath = assetPath.Replace(".", "_");
                //add after suffix ".assetbundle" to the end
                assetPath = assetPath + AssetBundleConfig.AssetBundleSuffix;
                return assetPath.ToLower();
            }
            return null;
        }

        // 相对于AssetBundleConfig.AssetsFolderName下的路径转Unity中Asset的绝对路径
        public static string RelativeAssetPathToAbsoluteAssetPath(string assetPath)
        {
            return "Assets/" + AssetBundleConfig.AssetsFolderName + "/" + assetPath;
        }

#if UNITY_EDITOR
        public static void WriteAssetBundleServerURL()
        {
            string assetBundleServerUrl = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
            assetBundleServerUrl = Path.Combine(assetBundleServerUrl, GetCurPlatformName());
            assetBundleServerUrl = Path.Combine(assetBundleServerUrl, AssetBundleConfig.AssetBundleServerUrlFileName);
            GameUtility.SafeWriteAllText(assetBundleServerUrl, GetAssetBundleServerURL());
        }

        public static string GetAssetBundleServerURL()
        {
            string downloadURL = string.Empty;
#pragma warning disable 0162
            if (AssetBundleConfig.isDebug == true)
            {
                // 注意：这里获取所有内网地址后选择一个最小的，因为可能存在虚拟机网卡
                var ips = new List<string>();
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ips.Add(ip.ToString());
                    }
                }
                ips.Sort();
                if (ips.Count <= 0)
                {
                    Logger.LogError("Get inter network ip failed!");
                }
                else
                {
                    downloadURL = "http://" + ips[0] + ":7888/";
                }
            }
            else
            {
                downloadURL = AssetBundleConfig.RemoteServerUrl;
            }
#pragma warning disable 0162
            return downloadURL;
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

        public static void CopyPlatformAssetBundlesToStreamingAssets()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            string source = GetBuildPlatformOutputPath(target);

            string outputPath = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
            GameUtility.SafeClearDir(outputPath);

            string destination = Path.Combine(outputPath, GetPlatformName(target));
            FileUtil.CopyFileOrDirectoryFollowSymlinks(source, destination);

            var allManifest = GameUtility.GetSpecifyFilesInFolder(destination, new string[] { ".manifest" });
            if (allManifest != null && allManifest.Length > 0)
            {
                for (int i = 0; i < allManifest.Length; i++)
                {
                    GameUtility.SafeDeleteFile(allManifest[i]);
                }
            }

            Debug.Log("Copy platform assetbundles to streaming assets done!");
        }
#endif
    }
}