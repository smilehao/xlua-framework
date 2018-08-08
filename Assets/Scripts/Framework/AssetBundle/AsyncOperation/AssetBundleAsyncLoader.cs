using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

/// <summary>
/// added by wsh @ 2017.12.22
/// 功能：Assetbundle加载器，给逻辑层使用（预加载），支持协程操作
/// 注意：
/// 1、加载器AssetBundleManager只负责调度，创建，不负责回收，逻辑层代码使用完一定要记得回收，否则会产生GC
/// 2、尝试加载并缓存所有的asset
/// </summary>

namespace AssetBundles
{
    [Hotfix]
    [LuaCallCSharp]
    public class AssetBundleAsyncLoader : BaseAssetBundleAsyncLoader
    {
        static Queue<AssetBundleAsyncLoader> pool = new Queue<AssetBundleAsyncLoader>();
        static int sequence = 0;
        protected List<string> waitingList = new List<string>();
        protected int waitingCount = 0;
        protected bool isOver = false;

        public static AssetBundleAsyncLoader Get()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                return new AssetBundleAsyncLoader(++sequence);
            }
        }

        public static void Recycle(AssetBundleAsyncLoader loader)
        {
            pool.Enqueue(loader);
        }

        public AssetBundleAsyncLoader(int sequence)
        {
            Sequence = sequence;
        }

        public void Init(string name, string[] dependances)
        {
            assetbundleName = name;
            isOver = false;
            waitingList.Clear();
            // 说明：只添加没有被加载过的
            assetbundle = AssetBundleManager.Instance.GetAssetBundleCache(assetbundleName);
            if (assetbundle == null)
            {
                waitingList.Add(assetbundleName);
            }
            if (dependances != null && dependances.Length > 0)
            {
                for (int i = 0; i < dependances.Length; i++)
                {
                    var ab = dependances[i];
                    if (!AssetBundleManager.Instance.IsAssetBundleLoaded(ab))
                    {
                        waitingList.Add(dependances[i]);
                    }
                }
            }
            waitingCount = waitingList.Count;
        }

        public int Sequence
        {
            get;
            protected set;
        }

        public override bool IsDone()
        {
            return isOver;
        }

        public override float Progress()
        {
            if (isDone)
            {
                return 1.0f;
            }

            float progressSlice = 1.0f / waitingCount;
            float progressValue = (waitingCount - waitingList.Count) * progressSlice;
            for (int i = waitingList.Count - 1; i >= 0; i--)
            {
                var cur = waitingList[i];
                var creater = AssetBundleManager.Instance.GetAssetBundleAsyncCreater(cur);
                progressValue += (creater != null ? creater.progress : 1.0f) * progressSlice;
            }
            return progressSlice;
        }
        
        public override void Update()
        {
            if (isDone)
            {
                return;
            }

            for (int i = waitingList.Count - 1; i >= 0; i--)
            {
                if (AssetBundleManager.Instance.IsAssetBundleLoaded(waitingList[i]))
                {
                    var curFinished = waitingList[i];
                    if (curFinished == assetbundleName)
                    {
                        assetbundle = AssetBundleManager.Instance.GetAssetBundleCache(assetbundleName);
                    }
                    // 正常情况下只需要等待所有ab加载，而不需要加载asset：https://unity3d.com/cn/learn/tutorials/temas/best-practices/assetbundle-usage-patterns?playlist=30089
                    // 但是Unity中很多版本字体加载有Bug，被依赖的字体必须手动提前加载出来（目前只发现Unity5.3.4、Unity5.5没问题---包括Unity5.6、Unity2017等都有问题）
                    // Bug报告：https://issuetracker.unity3d.com/issues/custom-font-material-is-missing-after-loading-ui-text-prefab-from-an-asset-bundle
                    // 这个Bug官方不执行修复，所以这里强制把依赖的字体Assets加载出来
                    // 注意：
                    // 1、理论上如果仅仅存在2级依赖关系，则导出所有Assets并不会有什么问题，但是没有实际意义
                    // 2、如果存在多级依赖，则Assets的导出必须等待ab全部加载完毕，会增加代码处理的复杂度
                    // 3、所以这里暂时只把字体导出，字体不依赖其它ab，不会有问题
                    AssetBundleManager.Instance.AddAssetbundleAssetsCache(curFinished, ".TTF");
                    waitingList.RemoveAt(i);
                }
            }
            // 说明：即使等待队列一开始就是0，也必须让AssetBundleManager跑一次update，它要善后
            isOver = waitingList.Count == 0;
            if (isOver)
            {
                AssetBundleManager.Instance.AddAssetbundleAssetsCache(assetbundleName);
            }
        }

        public override void Dispose()
        {
            waitingList.Clear();
            waitingCount = 0;
            assetbundleName = null;
            assetbundle = null;
            Recycle(this);
        }
    }
}
