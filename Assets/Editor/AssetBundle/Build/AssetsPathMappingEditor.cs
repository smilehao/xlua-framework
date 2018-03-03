using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// added by wsh @ 2017.12.25
/// 功能：Assetbundle相关的Asset路径映射，每次在构建Assetbunlde完成后需要更新一次映射
/// 注意：所有配置文件不用路径映射
/// TODO：
/// 1、暂时不考虑支持variant了
/// </summary>

namespace AssetBundles
{
    public class AssetsPathMappingEditor
    {
        const string PATTREN = AssetBundleConfig.CommonMapPattren;
        public static List<string> mappingList = new List<string>();
        
        public static void BuildPathMapping(AssetBundleManifest manifest)
        {
            mappingList.Clear();
            string outputFilePath = AssetBundleUtility.PackagePathToAssetsPath(AssetBundleConfig.AssetsPathMapFileName);
            
            string[] allAssetbundles = manifest.GetAllAssetBundles();
            string[] allVariants = manifest.GetAllAssetBundlesWithVariant();
            
            List<string> assetbundlesWithoutVariant = null;
            List<string> variantWithoutDeplicate = null;
            ProsessVariant(allAssetbundles, allVariants, out assetbundlesWithoutVariant, out variantWithoutDeplicate);

            // 处理所有不带variants的assetbundle
            foreach (string assetbundle in assetbundlesWithoutVariant)
            {
                // 该assetbundle中包含的所有asset的路径（相对于Assets文件夹），如：
                // Assets/AssetsPackage/UI/Prefabs/View/UILoading.prefab
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetbundle);
                foreach (string assetPath in assetPaths)
                {
                    string packagePath = AssetBundleUtility.AssetsPathToPackagePath(assetPath);
                    string mappingItem = string.Format("{0}{1}{2}", assetbundle, PATTREN, packagePath);
                    mappingList.Add(mappingItem);
                }
            }
            // 处理带variants的assetbundle（已经去重）
            // string variant = "[" + AssetBundleConfig.VariantMapParttren + "]";
            foreach (string assetbundle in variantWithoutDeplicate)
            {
                // 该assetbundle中包含的所有asset的路径（相对于Assets文件夹），如：
                // Assets/AssetsPackage/UI/Prefabs/Language/[Chinese]/TestVariant.prefab
                // Assets/AssetsPackage/UI/Prefabs/Language/[English]/TestVariant.prefab
                // 由于已经去重，以上条目有且仅有一条出现
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetbundle);
                if (assetPaths == null || assetPaths.Length == 0)
                {
                    UnityEngine.Debug.LogError("Empty assetbundle with variant : " + assetbundle);
                    continue;
                }
                // 自本节点向上找到Assetbundle所在
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
                // 将拿掉[Variant]目录名如：
                // Assets/AssetsPackage/UI/Prefabs/Language/TestVariant.prefab
                // 用此种方式可以统一路径，使加载Assetbundle时的路径与具体激活的variant无关
                string nowRoot = GameUtility.FormatToUnityPath(System.IO.Path.GetDirectoryName(assetbundlePath));
                foreach (string assetPath in assetPaths)
                {
                    string nowAsset = assetPath.Replace(assetbundlePath, "");
                    string nowAssetPath = nowRoot + nowAsset;
                    string packagePath = AssetBundleUtility.AssetsPathToPackagePath(nowAssetPath);
                    string mappingItem = string.Format("{0}{1}{2}", RemoveVariantSuffix(assetbundle), PATTREN, packagePath);
                    mappingList.Add(mappingItem);
                }
            }
            mappingList.Sort();

            if (!GameUtility.SafeWriteAllLines(outputFilePath, mappingList.ToArray()))
            {
                Debug.LogError("BuildPathMapping failed!!! try rebuild it again!");
            }
            else
            {
                AssetDatabase.Refresh();
                AssetBundleEditorHelper.CreateAssetbundleForCurrent(outputFilePath);
                Debug.Log("BuildPathMapping success...");
            }
            AssetDatabase.Refresh();
        }
        
        // 处理所有的Variant：相同Assetbundle，不同Variant只需保留一份不带Variant的映射
        public static void ProsessVariant(string[] allAssetbundle, string[] allVariants, out List<string> assetbundlesWithoutVariant, out List<string> variantWithoutDeplicate)
        {
            assetbundlesWithoutVariant = new List<string>();
            // 抽取出所有不带Variant的Assetbundle
            foreach (string assetbundle in allAssetbundle)
            {
                if (!System.Array.Exists(allVariants, element => element.Equals(assetbundle)))
                {
                    assetbundlesWithoutVariant.Add(assetbundle);
                }
            }
            // 过滤所有varians后缀
            List<string> variantsProsessed = new List<string>();
            for (int i = 0; i < allVariants.Length; i++)
            {
                variantsProsessed.Add(RemoveVariantSuffix(allVariants[i]));
            }
            // 过滤所有重复的varians
            List<string> variantsNoDuplicate = new List<string>();
            foreach (string variant in variantsProsessed)
            {
                if (!variantsNoDuplicate.Contains(variant))
                {
                    variantsNoDuplicate.Add(variant);
                }
            }
            // 取出同Assetbundle名称，不同variant的唯一代表
            variantWithoutDeplicate = new List<string>();
            foreach (string variant in allVariants)
            {
                if (variantsNoDuplicate.Contains(RemoveVariantSuffix(variant)))
                {
                    variantWithoutDeplicate.Add(variant);
                    variantsNoDuplicate.Remove(RemoveVariantSuffix(variant));
                }
            }
        }
        
        public static string RemoveVariantSuffix(string name)
        {
            int idx = name.LastIndexOf('.');
            if (idx != -1)
            {
                return name.Substring(0, idx);
            }
            else
            {
                return name;
            }
        }
    }
}
