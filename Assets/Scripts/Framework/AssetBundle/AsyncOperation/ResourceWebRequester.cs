using System.Collections.Generic;
using UnityEngine;
using XLua;

/// <summary>
/// added by wsh @ 2017.12.22
/// 功能：资源异步请求，本地、远程通杀
/// 注意：
/// 1、Unity5.3官方建议用UnityWebRequest取代WWW：https://unity3d.com/cn/learn/tutorials/topics/best-practices/assetbundle-fundamentals?playlist=30089
/// 2、这里还是采用WWW，因为UnityWebRequest的Bug无数：
///     1）Unity5.3.5：http://blog.csdn.net/st75033562/article/details/52411197
///     2）Unity5.5：https://bitbucket.org/Unity-Technologies/assetbundledemo/pull-requests/25/feature-unitywebrequest/diff#comment-None
///     3）还有各个版本发行说明中关于UnityWebRequest的修复，如Unity5.4.1（5.4全系列版本基本都有修复这个API的Bug）：https://unity3d.com/cn/unity/whats-new/unity-5.4.1
///     4）此外对于LZMA压缩，采用UnityWebRequest好处在于节省内存，性能上并不比WWW优越：https://docs.unity3d.com/530/Documentation/Manual/AssetBundleCompression.html
/// 3、LoadFromFile(Async)在Unity5.4以上支持streamingAsset目录加载资源，5.3.7和5.4.3以后支持LAMZ压缩，但是没法加载非Assetbundle资源
/// 4、另外，虽然LoadFromFile(Async)是加载ab最快的API，但是会延缓Asset加载的时间（读磁盘），如果ab尽量预加载，不考虑内存敏感问题，这个API意义就不大
/// </summary>

namespace AssetBundles
{
    [Hotfix]
    [LuaCallCSharp]
    public class ResourceWebRequester : ResourceAsyncOperation
    {
        static Queue<ResourceWebRequester> pool = new Queue<ResourceWebRequester>();
        static int sequence = 0;
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
                return new ResourceWebRequester(++sequence);
            }
        }

        public static void Recycle(ResourceWebRequester creater)
        {
            pool.Enqueue(creater);
        }

        public ResourceWebRequester(int sequence)
        {
            Sequence = sequence;
        }

        public void Init(string name, string url, bool noCache = false)
        {
            assetbundleName = name;
            this.url = url;
            this.noCache = noCache;
            www = null;
            isOver = false;
        }

        public int Sequence
        {
            get;
            protected set;
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
                Logger.LogError("New www failed!!!");
                isOver = true;
            }
            else
            {
                //Logger.Log("Downloading : " + url);
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
                Logger.LogError(www.error);
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
