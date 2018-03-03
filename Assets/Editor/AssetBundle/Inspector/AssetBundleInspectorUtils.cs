using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// added by wsh @ 2018.01.06
/// 说明：AssetbundleInspector工具类
/// </summary>

namespace AssetBundles
{
    public class AssetBundleInspectorUtils
    {
        public const string DatabaseRoot = "Assets/Editor/AssetBundle/Database";

        static public bool CheckMaybeAssetBundleAsset(string assetPath)
        {
            return assetPath.StartsWith("Assets/" + AssetBundleConfig.AssetsFolderName);
        }

        static public string AssetPathToDatabasePath(string assetPath)
        {
            if (!CheckMaybeAssetBundleAsset(assetPath))
            {
                return null;
            }

            assetPath = assetPath.Replace("Assets/", "");
            return Path.Combine(DatabaseRoot, assetPath + ".asset");
        }
    }
}
