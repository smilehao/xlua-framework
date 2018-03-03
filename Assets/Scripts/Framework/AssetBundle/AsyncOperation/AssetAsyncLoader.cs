using System.Collections.Generic;
using XLua;

/// <summary>
/// added by wsh @ 2017.12.22
/// 功能：Asset异步加载器，自动追踪依赖的ab加载进度
/// 说明：一定要所有ab都加载完毕以后再加载asset，所以这里分成两个加载步骤
/// </summary>

namespace AssetBundles
{
    [Hotfix]
    [LuaCallCSharp]
    public class AssetAsyncLoader : BaseAssetAsyncLoader
    {
        static Queue<AssetAsyncLoader> pool = new Queue<AssetAsyncLoader>();
        static int sequence = 0;
        protected bool isOver = false;
        protected BaseAssetBundleAsyncLoader assetbundleLoader = null;

        public static AssetAsyncLoader Get()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                return new AssetAsyncLoader(++sequence);
            }
        }

        public static void Recycle(AssetAsyncLoader creater)
        {
            pool.Enqueue(creater);
        }

        public AssetAsyncLoader(int sequence)
        {
            Sequence = sequence;
        }

        public void Init(string assetName, UnityEngine.Object asset)
        {
            AssetName = assetName;
            this.asset = asset;
            assetbundleLoader = null;
            isOver = true;
        }

        public int Sequence
        {
            get;
            protected set;
        }

        public void Init(string assetName, BaseAssetBundleAsyncLoader loader)
        {
            AssetName = assetName;
            this.asset = null;
            isOver = false;
            assetbundleLoader = loader;
        }

        public string AssetName
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

            return assetbundleLoader.progress;
        }

        public override void Update()
        {
            if (isDone)
            {
                return;
            }

            isOver = assetbundleLoader.isDone;
            if (!isOver)
            {
                return;
            }

            asset = AssetBundleManager.Instance.GetAssetCache(AssetName);
            assetbundleLoader.Dispose();
        }

        public override void Dispose()
        {
            isOver = true;
            AssetName = null;
            asset = null;
            Recycle(this);
        }
    }
}
