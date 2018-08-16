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

                    // 正常情况下只需要等待所有ab加载，而不需要加载依赖包的asset：https://unity3d.com/cn/learn/tutorials/topics/best-practices/assetbundle-fundamentals （见“3.4.2. AssetBundle dependencies”章节）
                    // 但是Unity中很多版本字体加载有Bug，被依赖的字体必须手动提前加载出来（目前只发现Unity5.3.4、Unity5.5没问题---包括Unity5.6、Unity2017等都有问题）
                    // Bug报告：https://issuetracker.unity3d.com/issues/custom-font-material-is-missing-after-loading-ui-text-prefab-from-an-asset-bundle
                    // 这个Bug官方不执行修复，所以这里强制把依赖的字体Assets加载出来
                    // 注意：
                    // 1、理论上如果仅仅存在2级依赖关系，则导出所有Assets并不会有什么问题，但是没有实际意义
                    // 2、如果存在多级依赖，则Assets的导出必须等待ab全部加载完毕，会增加代码处理的复杂度
                    // 3、所以这里暂时只把字体导出，字体不依赖其它ab，不会有问题
                    //AssetBundleManager.Instance.AddAssetbundleAssetsCache(curFinished, ".TTF");

                    // added @ 2018.08.17
                    // 同上述Bug报告用户回复的情况，这里我也测试出一个问题，窗口A使用了两个图集B和C，有一个图集的资源会加载不出来
                    // 具体情况：选服界面，服务器列表Item的背景，是Group图集，而其它是Comm图集，结果发现Group图集的资源丢失
                    // 对于上述情况，试过所有AB加载出来时，立刻实例化窗口预设也不行，Group图集还是会丢失，说明Group图集的AB还就必须被缓存下来，坑爹
                    // 鉴于上述所列的几个注意点，这里根本没法保证被依赖的AB包不再去依赖其它AB包
                    // 所以如果用类似处理字体的情况去处理肯定是存在问题的
                    // 所以目前唯一想到的好点的办法，就是所有被依赖的AB包当作常驻包来处理
                    // 这样处理存在的问题点：
                    // 1）只被一个AB包依赖的AB包也被当作了常驻包，增加了内存负担
                    // 2）一般一个界面会使用到至少两个图集，一个界面专属的图集，还有1到N个公共图集，公共图集是常驻包，没问题，但是专属图集这里也被处理成了常驻包，导致界面关闭后应该要被卸载却没有被卸载
                    // 综上，对于出现此Bug的Unity版本，被依赖的AB包必须被缓存，那么关键是什么时候执行卸载，考虑过的处理方向：
                    // 1）维护Asset对AB包的引用计数，只有所有Asset都不再在逻辑层使用时，才卸载AB，而卸载AB的同时会自动卸载它的依赖包（如果引用计数为0）
                    //    缺陷：A）实现成本太高，必须在逻辑层去维护每一个Asset对AB包的引用情况，比如，每次打开一个窗口ui_a.prefab，需要对ui_a.assetbundle加引用1，而关闭窗口时去引用1，不是不能做，就是有点繁琐
                    //          B）内存浪费，目前采用LZMA压缩，解压后内存体积大，Asset存留时AB也必须存留，导致至少一倍的内存浪费
                    // 2）考虑使用LZ4压缩格式
                    //    TODO：A）使用LZ4时AB包可以不使用引用计数，因为创建的AB很小，可以设置所有AB常驻，那么加载Asset时肯定是可以找到依赖的，这方案有待测试
                    //          B）IOS切后台会导致缓存被清理，所以切回来的时候可能图集等还是要重新从AB包加载，使用LZ4应该可以避免这种情况，待测
                    // 坑爹，一个bug搞得我可能要推翻整个LZMA压缩方案的使用，目前先简单处理，在处理AB依赖包时，将被依赖的AB包也处理成常驻包

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
