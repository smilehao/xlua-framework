using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// add by wsh @ 2017-12-21
/// 功能：assetbundle管理类，为外部提供统一的资源加载界面、协调Assetbundle各个子系统的运行
/// 说明：移植自官方项目：https://bitbucket.org/Unity-Technologies/assetbundledemo
/// 注意：
/// 1、抛弃Resources目录的使用，官方建议：https://unity3d.com/cn/learn/tutorials/temas/best-practices/resources-folder?playlist=30089
/// 2、提供Editor和Simulate模式，前者不适用Assetbundle，直接加载资源，快速开发；后者使用Assetbundle，用本地服务器模拟资源更新
/// 3、场景不进行打包，场景资源打包为预设
/// 4、只提供异步接口，所有加载按异步进行
/// 5、采用LZMA压缩方式，性能瓶颈在Assetbundle加载上，ab加载异步，asset加载同步，ab加载后导出全部asset并卸载ab
/// 6、所有公共ab包（被多个ab包依赖）常驻内存，非公共包加载asset以后立刻卸载，被依赖的公共ab包会随着资源预加载自动加载并常驻内存
/// 7、随意卸载公共ab包可能导致内存资源重复，最好在切换场景时再手动清理不需要的公共ab包
/// 8、常驻包（公共ab包）引用计数不为0时手动清理无效，正在等待加载的所有ab包不能强行终止---一旦发起创建就一定要等操作结束，异步过程进行中清理无效
/// 9、切换场景时最好预加载所有可能使用到的资源，所有加载器用完以后记得Dispose回收，清理GC时注意先释放所有Asset缓存
/// 10、逻辑层所有Asset路径带文件类型后缀，且是AssetBundleConfig.ResourcesFolderName下的相对路径
/// TODO：
/// 1、配置常驻包（公共ab包）列表，自动加载和卸载常驻包===>区分场景常驻包和全局公共包，切换场景时自动卸载场景公共包
/// 使用说明：
/// 1、由Asset路径获取AssetName、AssetBundleName：ParseAssetPathToNames
/// 2、设置常驻(公共)ab包：SetAssetBundleResident(assebundleName, true)
/// 2、(预)加载资源：var loader = LoadAssetBundleAsync(assetbundleName)，协程等待加载完毕后Dispose：loader.Dispose()
/// 3、加载Asset资源：var loader = LoadAssetAsync(assetPath, TextAsset)，协程等待加载完毕后Dispose：loader.Dispose()，第二个参数只在编辑器模式下生效
/// 4、离开场景清理所有Asset缓存：ClearAssetCache()
/// 5、离开场景清理必要的(公共)ab包：TryUnloadAssetBundle()，注意：这里只是尝试卸载，所有引用计数不为0的包（还正在加载）不会被清理
/// </summary>

namespace AssetBundles
{
    public class AssetBundleManager : MonoSingleton<AssetBundleManager>
    {
        // 最大同时进行的ab创建数量
        const int MAX_ASSETBUNDLE_CREATE_NUM = 5;
        // manifest：提供依赖关系查找以及hash值比对
        Manifest manifest = null;
        // 资源路径相关的映射表
        AssetsPathMapping assetsPathMapping = null;
        // 常驻ab包：需要手动添加公共ab包进来，常驻包不会自动卸载（即使引用计数为0），引用计数为0时可以手动卸载
        HashSet<string> assetbundleResident = new HashSet<string>();
        // ab缓存包：所有目前已经加载的ab包，包括临时ab包与公共ab包
        Dictionary<string, AssetBundle> assetbundleCaching = new Dictionary<string, AssetBundle>();
        // ab缓存包引用计数：卸载ab包时只有引用计数为0时才会真正执行卸载
        Dictionary<string, int> assetbundleRefCount = new Dictionary<string, int>(); 
        // asset缓存：给非公共ab包的asset提供逻辑层的复用
        Dictionary<string, UnityEngine.Object> assetCaching = new Dictionary<string, UnityEngine.Object>();
        // 加载数据请求：正在prosessing或者等待prosessing的资源请求
        Dictionary<string, ResourceWebRequester> webRequesting = new Dictionary<string, ResourceWebRequester>();
        // 等待处理的资源请求
        Queue<ResourceWebRequester> webRequesterQueue = new Queue<ResourceWebRequester>();
        // 正在处理的资源请求
        List<ResourceWebRequester> prosessingWebRequester = new List<ResourceWebRequester>();
        // 逻辑层正在等待的ab加载异步句柄
        List<AssetBundleAsyncLoader> prosessingAssetBundleAsyncLoader = new List<AssetBundleAsyncLoader>();
        // 逻辑层正在等待的asset加载异步句柄
        List<AssetAsyncLoader> prosessingAssetAsyncLoader = new List<AssetAsyncLoader>();

        public IEnumerator Initialize()
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                yield break;
            }
#endif

            manifest = new Manifest();
            assetsPathMapping = new AssetsPathMapping();
            // 说明：同时请求资源可以提高加载速度
            var manifestRequest = RequestAssetAsync(manifest.ManifestFileName);
            var pathMapRequest = RequestAssetAsync(AssetBundleConfig.AssetsPathMapFileName);

            yield return manifestRequest;
            var assetbundle = manifestRequest.assetbundle;
            manifest.LoadFromAssetbundle(assetbundle);
            assetbundle.Unload(false);
            manifestRequest.Dispose();

            yield return pathMapRequest;
            var mapContent = pathMapRequest.text;
            pathMapRequest.Dispose();
            if (mapContent != null)
            {
                assetsPathMapping.Initialize(mapContent);
            }
            yield break;
        }

        public IEnumerator Cleanup()
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                yield break;
            }
#endif

            // 等待所有请求完成：不等待可能会有问题
            yield return new WaitUntil(() =>
            {
                return prosessingWebRequester.Count == 0;
            });
            yield return new WaitUntil(() =>
            {
                return prosessingAssetBundleAsyncLoader.Count == 0;
            });
            yield return new WaitUntil(() =>
            {
                return prosessingAssetAsyncLoader.Count == 0;
            });

            ClearAssetCache();
            foreach (var assetbunle in assetbundleCaching.Values)
            {
                if (assetbunle != null)
                {
                    assetbunle.Unload(false);
                }
            }
            assetbundleCaching.Clear();
            assetbundleRefCount.Clear();
            assetbundleResident.Clear();
            yield break;
        }

        public Manifest curManifest
        {
            get
            {
                return manifest;
            }
        }

        public string DownloadUrl
        {
            get;
            set;
        }
        
        public void SetAssetBundleResident(string assetbundleName, bool resident)
        {
            bool exist = assetbundleResident.Contains(assetbundleName);
            if (resident && !exist)
            {
                assetbundleResident.Add(assetbundleName);
            }
            else if(!resident && exist)
            {
                assetbundleResident.Remove(assetbundleName);
            }
        }

        public bool IsAssetBundleResident(string assebundleName)
        {
            return assetbundleResident.Contains(assebundleName);
        }

        public bool IsAssetBundleLoaded(string assetbundleName)
        {
            return assetbundleCaching.ContainsKey(assetbundleName);
        }

        public AssetBundle GetAssetBundleCache(string assetbundleName)
        {
            AssetBundle target = null;
            assetbundleCaching.TryGetValue(assetbundleName, out target);
            return target;
        }

        protected void RemoveAssetBundleCache(string assetbundleName)
        {
            assetbundleCaching.Remove(assetbundleName);
        }

        protected void AddAssetBundleCache(string assetbundleName, AssetBundle assetbundle)
        {
            assetbundleCaching[assetbundleName] = assetbundle;
        }

        public bool IsAssetLoaded(string assetName)
        {
            return assetCaching.ContainsKey(assetName);
        }

        public UnityEngine.Object GetAssetCache(string assetName)
        {
            UnityEngine.Object target = null;
            assetCaching.TryGetValue(assetName, out target);
            return target;
        }

        public void AddAssetCache(string assetName, UnityEngine.Object asset)
        {
            assetCaching[assetName] = asset;
        }

        public void AddAssetbundleAssetsCache(string assetbundleName)
        {
            if (!IsAssetBundleLoaded(assetbundleName))
            {
                UnityEngine.Debug.LogError("Try to add assets cache from unloaded assetbundle : " + assetbundleName);
                return;
            }
            var curAssetbundle = GetAssetBundleCache(assetbundleName);
            var allAssetNames = assetsPathMapping.GetAllAssetNames(assetbundleName);
            for (int i = 0; i < allAssetNames.Count; i++)
            {
                var assetName = allAssetNames[i];
                if (IsAssetLoaded(assetName))
                {
                    continue;
                }
                var asset = curAssetbundle == null ? null : curAssetbundle.LoadAsset(assetName);
                AddAssetCache(assetName, asset);
            }
        }

        public void ClearAssetCache()
        {
            assetCaching.Clear();
        }
        
        public ResourceWebRequester GetAssetBundleAsyncCreater(string assetbundleName)
        {
            ResourceWebRequester creater = null;
            webRequesting.TryGetValue(assetbundleName, out creater);
            return creater;
        }

        protected int GetReferenceCount(string assetbundleName)
        {
            int count = 0;
            assetbundleRefCount.TryGetValue(assetbundleName, out count);
            return count;
        }

        protected int IncreaseReferenceCount(string assetbundleName)
        {
            int count = 0;
            assetbundleRefCount.TryGetValue(assetbundleName, out count);
            count++;
            assetbundleRefCount[assetbundleName] = count;
            return count;
        }

        protected int DecreaseReferenceCount(string assetbundleName)
        {
            int count = 0;
            assetbundleRefCount.TryGetValue(assetbundleName, out count);
            count--;
            assetbundleRefCount[assetbundleName] = count;
            return count;
        }

        protected void CreateAssetBundleAsync(string assetbundleName)
        {
            if (IsAssetBundleLoaded(assetbundleName) || webRequesting.ContainsKey(assetbundleName))
            {
                return;
            }

            var creater = ResourceWebRequester.Get();
            var url = AssetBundleUtility.GetPlatformFileUrl(assetbundleName);
            creater.Init(assetbundleName, url);
            webRequesting.Add(assetbundleName, creater);
            webRequesterQueue.Enqueue(creater);
            // 创建器持有的引用：创建器对每个ab来说是全局唯一的
            IncreaseReferenceCount(assetbundleName);
        }

        public BaseAssetBundleAsyncLoader LoadAssetBundleAsync(string assetbundleName)
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                return new EditorAssetBundleAsyncLoader(assetbundleName);
            }
#endif

            var loader = AssetBundleAsyncLoader.Get();
            prosessingAssetBundleAsyncLoader.Add(loader);
            if (manifest != null)
            {
                string[] dependancies = manifest.GetAllDependencies(assetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    IncreaseReferenceCount(dependance);
                    CreateAssetBundleAsync(dependance);
                }
                loader.Init(assetbundleName, dependancies);
            }
            else
            {
                loader.Init(assetbundleName, null);
            }
            CreateAssetBundleAsync(assetbundleName);
            // 加载器持有的引用：同一个ab能同时存在多个加载器，等待ab创建器完成
            IncreaseReferenceCount(assetbundleName);
            return loader;
        }

        public void ReleaseAssetBundleAsyncLoader(AssetBundleAsyncLoader loader)
        {
            if (manifest != null)
            {
                string[] dependancies = manifest.GetAllDependencies(loader.assetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    UnloadAssetBundle(dependance);
                }
            }
            UnloadAssetBundle(loader.assetbundleName);
        }

        protected void UnloadAssetBundle(string assetbundleName, bool unloadResident = false, bool unloadAllLoadedObjects = false)
        {
            int count = GetReferenceCount(assetbundleName);
            if (count <= 0)
            {
                return;
            }

            count = DecreaseReferenceCount(assetbundleName);
            if (count > 0)
            {
                return;
            }

            var assetbundle = GetAssetBundleCache(assetbundleName);
            var isResident = IsAssetBundleResident(assetbundleName);
            if (assetbundle != null)
            {
                if (!isResident || isResident && unloadResident)
                {
                    assetbundle.Unload(unloadAllLoadedObjects);
                    RemoveAssetBundleCache(assetbundleName);
                }
            }
        }

        public void TryUnloadAssetBundle(string assetbundleName, bool unloadAllLoadedObjects = false)
        {
            int count = GetReferenceCount(assetbundleName);
            if (count > 0)
            {
                return;
            }

            UnloadAssetBundle(assetbundleName, true, unloadAllLoadedObjects);
        }

        public ResourceWebRequester DownloadAssetAsync(string filePath)
        {
            if (string.IsNullOrEmpty(DownloadUrl))
            {
                UnityEngine.Debug.LogError("You should set download url first!!!");
                return null;
            }

            var creater = ResourceWebRequester.Get();
            var url = DownloadUrl + filePath;
            creater.Init(filePath, url, true);
            webRequesting.Add(filePath, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }

        public bool MapAssetPath(string assetPath, out string assetbundleName, out string assetName)
        {
            return assetsPathMapping.MapAssetPath(assetPath, out assetbundleName, out assetName);
        }

        public ResourceWebRequester RequestAssetAsync(string filePath)
        {
            var creater = ResourceWebRequester.Get();
            var url = AssetBundleUtility.GetPlatformFileUrl(filePath);
            creater.Init(filePath, url, true);
            webRequesting.Add(filePath, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }

        public BaseAssetAsyncLoader LoadAssetAsync(string assetPath, System.Type assetType)
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                string path = string.Format("Assets/{0}/{1}", AssetBundleConfig.AssetsFolderName, assetPath);
                UnityEngine.Object target = AssetDatabase.LoadAssetAtPath(path, assetType);
                return new EditorAssetAsyncLoader(target);
            }
#endif

            string assetbundleName = null;
            string assetName = null;
            bool status = MapAssetPath(assetPath, out assetbundleName, out assetName);
            if (!status)
            {
                UnityEngine.Debug.LogError("No assetbundle at asset path :" + assetPath);
                return null;
            }

            var loader = AssetAsyncLoader.Get();
            prosessingAssetAsyncLoader.Add(loader);
            if (IsAssetLoaded(assetName))
            {
                loader.Init(assetName, GetAssetCache(assetName));
                return loader;
            }
            else
            {
                var assetbundleLoader = LoadAssetBundleAsync(assetbundleName);
                loader.Init(assetName, assetbundleLoader);
                return loader;
            }
        }
        
        void Update()
        {
            OnProsessingWebRequester();
            OnProsessingAssetBundleAsyncLoader();
            OnProsessingAssetAsyncLoader();
        }

        void OnProsessingWebRequester()
        {
            for (int i = prosessingWebRequester.Count - 1; i >= 0; i--)
            {
                var creater = prosessingWebRequester[i];
                creater.Update();
                if (creater.IsDone())
                {
                    UnityEngine.Debug.Log(creater.assetbundleName);
                    prosessingWebRequester.RemoveAt(i);
                    webRequesting.Remove(creater.assetbundleName);
                    if (creater.noCache)
                    {
                        return;
                    }
                    // 说明：有错误也缓存下来，只不过资源为空
                    // 1、避免再次错误加载
                    // 2、如果不存下来加载器将无法判断什么时候结束
                    AddAssetBundleCache(creater.assetbundleName, creater.assetbundle);
                    //AddAssetbundleAssetsCache(creater.assetbundleName);
                    UnloadAssetBundle(creater.assetbundleName);
                    creater.Dispose();
                }
            }
            int slotCount = prosessingWebRequester.Count;
            while (slotCount < MAX_ASSETBUNDLE_CREATE_NUM && webRequesterQueue.Count > 0)
            {
                var creater = webRequesterQueue.Dequeue();
                creater.Start();
                prosessingWebRequester.Add(creater);
                slotCount++;
            }
        }
        
        void OnProsessingAssetBundleAsyncLoader()
        {
            for (int i = prosessingAssetBundleAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingAssetBundleAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    ReleaseAssetBundleAsyncLoader(loader);
                    prosessingAssetBundleAsyncLoader.RemoveAt(i);
                }
            }
        }

        void OnProsessingAssetAsyncLoader()
        {
            for (int i = prosessingAssetAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingAssetAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    prosessingAssetAsyncLoader.RemoveAt(i);
                }
            }
        }
    }
}