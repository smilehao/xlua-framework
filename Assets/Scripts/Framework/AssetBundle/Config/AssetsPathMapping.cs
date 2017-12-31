using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// added by wsh @ 2017.12.26
/// 功能： Assetbundle相关的Asset路径映射解析，每次在构建Assetbunlde完成自动生成，每次有资源更新时需要强行下载一次
/// 说明： 映射规则：
/// 1）对于Asset：Asset加载路径（相对于Assets文件夹）到Assetbundle名与Asset名的映射
/// 2）对于带有Variant的Assetbundle，做通用替换处理
/// 使用说明：
/// 1）asset加载：
///     假定AssetBundleConfig设置为AssetsFolderName = "AssetsPackage"，且：
///         A）assetbundle名称：assetspackage/materials/newfolder.assetbundle
///         B）assetbundle中含有资源：1390616300363.jpg与1390616300363.mat
///         C）Assets路径为：Assets/AssetsPackage/Materials/New Folder/1390616300363.jpg与Assets/AssetsPackage/Materials/New Folder/1390616300363.mat
///     则代码中需要的加载路径分别为：
///         Materials/New Folder/1390616300363.jpg与Materials/New Folder/1390616300363.mat
///     注意：Assets路径带文件类型后缀，且区分大小写
/// 2）带variant的Assetbundle资源加载：
///     假定设置为：
///         A）assetbundle名称：assetspackage/sampleAssets/tanks/variants/language，定义在以下两个路径
///             Assets/AssetsPackage/SampleAssets/Tanks/Variants/Language/Danish，Variant为danish
///             Assets/AssetsPackage/SampleAssets/Tanks/Variants/Language/English，Variant为english
///         B）assetbundle中资源：
///             Assets/AssetsPackage/SampleAssets/Tanks/Variants/Language/Danish/Canvas.prefab
///             Assets/AssetsPackage/SampleAssets/Tanks/Variants/Language/English/Canvas.prefab
///         C）使用时设置激活的Variant为danish或者english，则代码中需要的加载路径统一为：
///             Assets/AssetsPackage/SampleAssets/Tanks/Variants/Language/{Variant}/Canvas.prefab（其中"{Variant}" = AssetBundleConfig.VariantMapParttren）
/// </summary>

namespace AssetBundles
{
    public class ResourcesMapItem
    {
        public string assetbundleName;
        public string assetName;
    }
    
    public class AssetsPathMapping
    {
        protected const string PATTREN = AssetBundleConfig.CommonMapPattren;
        protected Dictionary<string, ResourcesMapItem> pathLookup = new Dictionary<string, ResourcesMapItem>();
        protected Dictionary<string, List<string>> assetsLookup = new Dictionary<string, List<string>>();
        protected Dictionary<string, string> assetbundleLookup = new Dictionary<string, string>();
        protected List<string> emptyList = new List<string>();

        public AssetsPathMapping()
        {
        }
        
        public void Initialize(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                UnityEngine.Debug.LogError("ResourceNameMap empty!!");
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
                    UnityEngine.Debug.LogError("splitArr length < 2 : " + map);
                    continue;
                }

                ResourcesMapItem item = new ResourcesMapItem();
                //如：assetspackage/ui/prefab/assetbundleupdaterpanel_prefab.assetbundle
                item.assetbundleName = splitArr[0];
                //如：Assets/AssetsPackage/UI/Prefab/AssetbundleUpdaterPanel.prefab
                item.assetName = splitArr[1];

                //如：Assets/AssetsPackage/
                string assetPath = null;
                string mapHead = string.Format("Assets/{0}/", AssetBundleConfig.AssetsFolderName);
                if (item.assetName.StartsWith(mapHead))
                {
                    //如：UI/Prefab/AssetbundleUpdaterPanel.prefabd
                    assetPath = GameUtility.FormatToUnityPath(item.assetName.Replace(mapHead, ""));
                }
                
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
