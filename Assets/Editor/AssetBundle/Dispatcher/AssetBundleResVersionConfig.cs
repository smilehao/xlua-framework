using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// added by wsh @ 2018.08.17
/// 说明：Assetbundle资源版本号序列化文件
/// </summary>

namespace AssetBundles
{
    public class AssetBundleResVersionConfig : ScriptableObject
    {
        public const string RES_PATH = AssetBundleInspectorUtils.DatabaseRoot + "/AssetBundleResVersionConfig.asset";
        public string resVersion = "1.0.000";
    }
}
