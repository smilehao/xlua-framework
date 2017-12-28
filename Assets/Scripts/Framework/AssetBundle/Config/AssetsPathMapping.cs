using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 作者： wsh@2016.06.27
/// 功能： Assetbundle相关的Asset路径映射解析，每次在构建Assetbunlde完成自动生成，每次有资源更新时需要强行下载一次
/// 说明： 映射规则：
///         1）类型：level、asset
///         2）对于场景：Level名称到Assetbundle名与Asset名的映射
///         3）对于Asset：Asset加载路径（相对于Assets文件夹）到Assetbundle名与Asset名的映射
///         4）对于带有Variant的Assetbundle，做通用替换处理
/// 使用举例：
///         1）level加载：直接使用level名称，与普通场景加载方式填写的level名称无差别
///         2）asset加载：
///             假定设置为：
///                 A）assetbundle名称：materials/newfolder.assetbundle
///                 B）assetbundle中含有资源：1390616300363.jpg与1390616300363.mat
///                 C）对于路径为：Assets/ResourcesRemap/Materials/New Folder/1390616300363.jpg与Assets/ResourcesRemap/Materials/New Folder/1390616300363.mat
///             则代码中需要的加载路径分别为：
///                 Assets/ResourcesRemap/Materials/New Folder/1390616300363.jpg与Materials/New Folder/1390616300363.mat
///             注意：资源路径如果在Assets/ResourcesRemap下，则Assetbundle加载路径可与Resources.Load接口一样去书写，即：
///                 Materials/New Folder/1390616300363.jpg与Materials/New Folder/1390616300363.mat
///         3）带variant的Assetbundle中的资源加载：对level和asset做相应改变
///             假定设置为：
///                 A）assetbundle名称：banner/lang，定义在以下两个路径
///                     Assets/ResourcesRemap/AssetBundleSample/SampleAssets/Tanks/Variants/Language/Danish，Variant为danish
///                     Assets/ResourcesRemap/AssetBundleSample/SampleAssets/Tanks/Variants/Language/English，Variant为english
///                 B）assetbundle中资源：Assets/ResourcesRemap/AssetBundleSample/SampleAssets/Tanks/Variants/Language/Danish/Canvas.prefab
///                                       Assets/ResourcesRemap/AssetBundleSample/SampleAssets/Tanks/Variants/Language/English/Canvas.prefab
///                 C）使用时设置激活的Variant为danish或者english
///             则代码中需要的加载路径统一为：
///                 Assets/ResourcesRemap/AssetBundleSample/SampleAssets/Tanks/Variants/Language/{Variant}/Canvas.prefab（其中"{Variant}" = AssetBundleConfig.VariantReplaceParttren）
/// 注意： AssetBundleConfig.VariantReplaceParttren存在的必要性是：
///         1）为了让AssetBundle的资源加载和Resources下的资源加载使用统一的加载目录，从而可以共享一份代码，无需差别对待
///         2）当使用统一路径加载Assetbundle失败时，会默认从Resources下加载资源
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
                //如：_resources/ui/prefab/assetbundleupdaterpanel_prefab.assetbundle
                item.assetbundleName = splitArr[0];
                //如：Assets/_Resources/UI/Prefab/AssetbundleUpdaterPanel.prefab
                item.assetName = splitArr[1];

                //如：Assets/_Resources/
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
    }
}
