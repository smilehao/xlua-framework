using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// add by wsh @ 2017.12.22
/// 功能：资源异步请求，本地、远程通杀
/// 注意：
/// 1、Unity5.3官方建议用UnityWebRequest取代WWW：https://unity3d.com/cn/learn/tutorials/topics/best-practices/assetbundle-fundamentals?playlist=30089
/// 2、这里还是采用WWW，因为UnityWebRequest在Unity5.3.5中有Bug，以后升级Unity版本再考虑重构：http://blog.csdn.net/st75033562/article/details/52411197
/// </summary>

namespace AssetBundles
{
    public class ResourceWebRequester : ResourceAsyncOperation
    {
        static Queue<ResourceWebRequester> pool = new Queue<ResourceWebRequester>();
        protected WWW www = null;
        protected bool isOver = false;

        public static ResourceWebRequester Get()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                return new ResourceWebRequester();
            }
        }

        public static void Recycle(ResourceWebRequester creater)
        {
            pool.Enqueue(creater);
        }

        public void Init(string name, string url, bool noCache = false)
        {
            assetbundleName = name;
            this.url = url;
            this.noCache = noCache;
            www = null;
            isOver = false;
        }

        public bool noCache
        {
            get;
            protected set;
        }

        public string assetbundleName
        {
            get;
            protected set;
        }

        public string url
        {
            get;
            protected set;
        }

        public AssetBundle assetbundle
        {
            get
            {
                return www.assetBundle;
            }
        }

        public byte[] bytes
        {
            get
            {
                return www.bytes;
            }
        }

        public string text
        {
            get
            {
                return www.text;
            }
        }

        public string error
        {
            get
            {
                // 注意：不能直接判空
                // 详见：https://docs.unity3d.com/530/Documentation/ScriptReference/WWW-error.html
                return string.IsNullOrEmpty(www.error) ? null : www.error;
            }
        }

        public override bool IsDone()
        {
            return isOver;
        }

        public void Start()
        {
            www = new WWW(url);
            if (www == null)
            {
                UnityEngine.Debug.LogError("New www failed!!!");
                isOver = true;
            }
            else
            {
                UnityEngine.Debug.Log("Downloading : " + url);
            }
        }
        
        public override float Progress()
        {
            if (isDone)
            {
                return 1.0f;
            }

            return www != null ? www.progress : 0f;
        }

        public override void Update()
        {
            if (isDone)
            {
                return;
            }
            
            isOver = www != null && (www.isDone || !string.IsNullOrEmpty(www.error));
            if (!isOver)
            {
                return;
            }

            if (www != null && !string.IsNullOrEmpty(www.error))
            {
                UnityEngine.Debug.LogError(www.error);
            }
        }

        public override void Dispose()
        {
            if (www != null)
            {
                www.Dispose();
                www = null;
            }
            Recycle(this);
        }
    }
}
