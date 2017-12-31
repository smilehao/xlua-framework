using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// added by wsh @ 2017.12.22
/// 功能：异步操作抽象基类，继承自IEnumerator接口，支持迭代，主要是为了让异步操作能够适用于协程操作
/// 注意：提供对协程操作的支持，但是异步操作的进行不依赖于协程，可以在Update等函数中查看进度值
/// </summary>

namespace AssetBundles
{
    public abstract class ResourceAsyncOperation : IEnumerator, IDisposable
    {
        public object Current
        {
            get
            {
                return null;
            }
        }

        public bool isDone
        {
            get
            {
                return IsDone();
            }
        }

        public float progress
        {
            get
            {
                return Progress();
            }
        }

        abstract public void Update();

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
        }
        
        abstract public bool IsDone();
        
        abstract public float Progress();

        public virtual void Dispose()
        {
        }
    }

    abstract public class BaseAssetBundleAsyncLoader : ResourceAsyncOperation
    {
        public string assetbundleName
        {
            get;
            protected set;
        }

        public AssetBundle assetbundle
        {
            get;
            protected set;
        }

        public override void Dispose()
        {
            assetbundleName = null;
            assetbundle = null;
        }
    }

    abstract public class BaseAssetAsyncLoader : ResourceAsyncOperation
    {
        public UnityEngine.Object asset
        {
            get;
            protected set;
        }

        public override void Dispose()
        {
            asset = null;
        }
    }
}
