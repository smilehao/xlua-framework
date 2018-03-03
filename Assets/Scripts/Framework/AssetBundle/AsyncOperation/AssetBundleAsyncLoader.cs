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
#if UNITY_EDITOR
                    // 说明：Editor下被依赖的AB中的Asset也要全部加载出来，不然有问题
                    AssetBundleManager.Instance.AddAssetbundleAssetsCache(curFinished);
#endif
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
