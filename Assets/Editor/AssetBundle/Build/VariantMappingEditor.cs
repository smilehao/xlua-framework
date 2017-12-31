using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// added by wsh @ 2017.12.26
/// 功能：Assetbundle相关的Variant路径映射，每次在构建Assetbunlde完成后需要更新一次映射
/// 说明：映射规则：每一个带Variant的Assetbundle到定义Assetbundle的文件或者文件夹的映射
/// </summary>

namespace AssetBundles
{
    public class VariantMappingEditor
    {
        const string PATTREN = AssetBundleConfig.CommonMapPattren;
        public static List<string> mappingList = new List<string>();

        public static void BuildVariantMapping(BuildTarget target, AssetBundleManifest manifest)
        {
            mappingList.Clear();
            string rootPath = AssetBundleUtility.GetBuildPlatformOutputPath(target);
            string outputFilePath = System.IO.Path.Combine(rootPath, AssetBundleConfig.VariantsMapFileName);          
            string[] allVariants = manifest.GetAllAssetBundlesWithVariant();

            //处理带variants的assetbundle
            foreach (string assetbundle in allVariants)
            {
                //该assetbundle中包含的所有asset的路径（相对于Assets文件夹），如名为banner/lang的assetbundle下：
                //Assets/AssetsPackage/SampleAssets/Tanks/Variants/Language/Danish/Canvas.prefab
                //Assets/AssetsPackage/SampleAssets/Tanks/Variants/Language/English/Canvas.prefab
                //在代码使用的加载路径中，它们被统一处理为
                //Assets/AssetsPackage/SampleAssets/Tanks/Variants/Language/{Variant}/Canvas.prefab
                //所以，在映射中两者分别应该对应
                //Danish
                //English
                //以便客户端在加载资源时对加载路径进行正确还原
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetbundle);
                if (assetPaths == null || assetPaths.Length == 0)
                {
                    UnityEngine.Debug.LogError("Empty assetbundle with variant : " + assetbundle);
                    continue;
                }
                //自本节点向上找到Assetbundle所在
                AssetBundleImporter assetbundleImporter = AssetBundleImporter.GetAtPath(assetPaths[0]);
                while (assetbundleImporter != null && string.IsNullOrEmpty(assetbundleImporter.assetBundleVariant))
                {
                    assetbundleImporter = assetbundleImporter.GetParent();
                }
                if (assetbundleImporter == null || string.IsNullOrEmpty(assetbundleImporter.assetBundleVariant))
                {
                    UnityEngine.Debug.LogError("Can not find assetbundle with variant : " + assetbundle);
                    continue;
                }
                string assetbundlePath = assetbundleImporter.assetPath;
                if (assetbundlePath.EndsWith("/"))
                {
                    assetbundlePath = assetbundlePath.Substring(0, assetbundlePath.Length - 1);
                }
                //由于各个Variant的内部结构必须完全一致，而Load时也必须完全填写，所以这里不需要关注到assetbundle具体的每个资源
                string nowNode = System.IO.Path.GetFileName(assetbundlePath);
                string mappingItem = string.Format("{0}{1}{2}", assetbundle, PATTREN, nowNode);
                mappingList.Add(mappingItem);
            }
            mappingList.Sort();
            if (!GameUtility.SafeWriteAllLines(outputFilePath, mappingList.ToArray()))
            {
                Debug.LogError("BuildVariantMapping failed!!! try rebuild it again!");
            }
            else
            {
                Debug.Log("BuildVariantMapping success!!!");
            }
        }
    }
}
