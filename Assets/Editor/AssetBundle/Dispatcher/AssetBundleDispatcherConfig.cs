using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// added by wsh @ 2018.01.08
/// 说明：Assetbundle分发器序列化配置
/// 注意：
/// 1、自定义类和数据结构都没法序列化
/// 2、被序列化的脚本必须和Mono脚本一样类名和脚本名一致
/// </summary>

namespace AssetBundles
{
    public class AssetBundleDispatcherConfig : ScriptableObject
    {
        public string PackagePath = string.Empty;
        public AssetBundleDispatcherFilterType Type = AssetBundleDispatcherFilterType.Root;
        public List<AssetBundleCheckerFilter> CheckerFilters = new List<AssetBundleCheckerFilter>();

        // 序列化用的，AssetBundleCheckerFilter的字段拆成两个数组
        [SerializeField]
        string[] RelativePaths = null;
        [SerializeField]
        string[] ObjectFilters = null;

        public AssetBundleDispatcherConfig()
        {
            Load();
        }

        public void Load()
        {
            CheckerFilters.Clear();
            if (RelativePaths != null && RelativePaths.Length > 0)
            {
                for (int i = 0; i < RelativePaths.Length; i++)
                {
                    CheckerFilters.Add(new AssetBundleCheckerFilter(RelativePaths[i], ObjectFilters[i]));
                }
            }
        }

        public void Apply()
        {
            if (CheckerFilters.Count <= 0)
            {
                RelativePaths = null;
                ObjectFilters = null;
                return;
            }

            RelativePaths = new string[CheckerFilters.Count];
            ObjectFilters = new string[CheckerFilters.Count];
            for (int i = 0; i < CheckerFilters.Count; i++)
            {
                RelativePaths[i] = CheckerFilters[i].RelativePath;
                ObjectFilters[i] = CheckerFilters[i].ObjectFilter;
            }
        }
    }
}
