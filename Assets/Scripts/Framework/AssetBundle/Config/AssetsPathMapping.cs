using System.Collections.Generic;
using XLua;

/// <summary>
/// added by wsh @ 2017.12.26
/// 功能： Assetbundle相关的Asset路径映射解析，每次在构建Assetbunlde完成自动生成，每次有资源更新时需要强行下载一次
/// 说明： 映射规则：
/// 1）对于Asset：Asset加载路径（相对于Assets文件夹）到Assetbundle名与Asset名的映射
/// 2）对于带有Variant的Assetbundle，做通用替换处理
/// 注意：Assets路径带文件类型后缀，且区分大小写
/// 使用说明：
/// 1）asset加载：
///     假定AssetBundleConfig设置为AssetsFolderName = "AssetsPackage"，且：
///         A）assetbundle名称：assetspackage/ui/prefabs/view/uiloading_prefab.assetbundle
///         B）assetbundle中资源：UILoading.prefab
///         C）Assets路径为：Assets/AssetsPackage/UI/Prefabs/View/UILoading.prefab
///     则代码中需要的加载路径为：UI/Prefabs/View/UILoading.prefab
/// 2）带variant的Assetbundle资源加载：
///     假定设置为：
///         A）assetbundle名称：assetspackage/ui/prefabs/language，定义在以下两个子路径
///             Assets/AssetsPackage/UI/Prefabs/Language/[Chinese]，variant = chinese
///             Assets/AssetsPackage/UI/Prefabs/Language/[English]，variant = english
///         B）assetbundle中资源：
///             Assets/AssetsPackage/UI/Prefabs/Language/[Chinese]/TestVariant.prefab
///             Assets/AssetsPackage/UI/Prefabs/Language/[English]/TestVariant.prefab
///         C）使用时设置激活的Variant为chinese或者english，则代码中需要的加载路径统一为：
///             Assets/AssetsPackage/UI/Prefabs/TestVariant.prefab===>即variant目录（[Chinese]、[English]）将被忽略，使逻辑层代码不需要关注variant带来的路径差异
/// TODO：
/// 1、后续看是否有必要全部把路径处理为小写，因为ToLower有GC分配，暂时不做这方面工作
/// </summary>

namespace AssetBundles
{
    public class ResourcesMapItem
    {
        public string assetbundleName;
        public string assetName;
    }

    [Hotfix]
    [LuaCallCSharp]
    public class AssetsPathMapping
    {
        protected const string PATTREN = AssetBundleConfig.CommonMapPattren;
        protected Dictionary<string, ResourcesMapItem> pathLookup = new Dictionary<string, ResourcesMapItem>();
        protected Dictionary<string, List<string>> assetsLookup = new Dictionary<string, List<string>>();
        protected Dictionary<string, string> assetbundleLookup = new Dictionary<string, string>();
        protected List<string> emptyList = new List<string>();

        public AssetsPathMapping()
        {
            AssetName = AssetBundleUtility.PackagePathToAssetsPath(AssetBundleConfig.AssetsPathMapFileName);
            AssetbundleName = AssetBundleUtility.AssetBundlePathToAssetBundleName(AssetName);
        }

        public string AssetbundleName
        {
            get;
            protected set;
        }

        public string AssetName
        {
            get;
            protected set;
        }

        public void Initialize(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                Logger.LogError("ResourceNameMap empty!!");
                return;
            }

            content = content.Replace("\r\n", "\n");
            string[] mapList = content.Split('\n');
            foreach (string map in mapList)
            {
                if (string.IsNullOrEmpty(map))
                {
                    continue;
                }

                string[] splitArr = map.Split(new[] { PATTREN }, System.StringSplitOptions.None);
                if (splitArr.Length < 2)
                {
                    Logger.LogError("splitArr length < 2 : " + map);
                    continue;
                }

                ResourcesMapItem item = new ResourcesMapItem();
                // 如：ui/prefab/assetbundleupdaterpanel_prefab.assetbundle
                item.assetbundleName = splitArr[0];
                // 如：UI/Prefab/AssetbundleUpdaterPanel.prefab
                item.assetName = splitArr[1];
                
                var assetPath = item.assetName;
                pathLookup.Add(assetPath, item);
                List<string> assetsList = null;
                assetsLookup.TryGetValue(item.assetbundleName, out assetsList);
                if (assetsList == null)
                {
                    assetsList = new List<string>();
                }
                if (!assetsList.Contains(item.assetName))
                {
                    assetsList.Add(item.assetName);
                }
                assetsLookup[item.assetbundleName] = assetsList;
                assetbundleLookup.Add(item.assetName, item.assetbundleName);
            }
        }
        
        public bool MapAssetPath(string assetPath, out string assetbundleName, out string assetName)
        {
            assetbundleName = null;
            assetName = null;
            ResourcesMapItem item = null;
            if (pathLookup.TryGetValue(assetPath, out item))
            {
                assetbundleName = item.assetbundleName;
                assetName = item.assetName;
                return true;
            }
            return false;
        }
        
        public List<string> GetAllAssetNames(string assetbundleName)
        {
            List<string> allAssets = null;
            assetsLookup.TryGetValue(assetbundleName, out allAssets);
            return allAssets == null ? emptyList : allAssets;
        }

        public string GetAssetBundleName(string assetName)
        {
            string assetbundleName = null;
            assetbundleLookup.TryGetValue(assetName, out assetbundleName);
            return assetbundleName;
        }
    }
}
