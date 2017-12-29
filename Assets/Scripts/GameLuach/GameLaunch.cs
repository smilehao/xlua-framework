using UnityEngine;
using System.Collections;
using AssetBundles;
using UnityEngine.UI;

public class GameLaunch : MonoBehaviour
{
    const string luachPrefabPath = "UI/Prefabs/View/UILuanch.prefab";

    IEnumerator Start ()
    {
        // 启动资源管理模块
        int frameCount = Time.frameCount;
        yield return AssetBundleManager.Instance.Initialize();
        UnityEngine.Debug.Log("AssetBundleManager Initialized, use frameCount = " + (Time.frameCount -frameCount));
        frameCount = Time.frameCount;

        // 启动xlua热修复模块
        // TODO：根据公共包自动设置常驻包
        //string luaScriptsPath = "Lua";
        //var abloader = AssetBundleManager.Instance.LoadAssetBundleAsync(luaScriptsPath);
        //yield return abloader;
        //abloader.Dispose();
        //XLuaManager.Instance.Startup();
        //XLuaManager.Instance.StartHotfix();

        // 加载UI界面
        var loader = AssetBundleManager.Instance.LoadAssetAsync(luachPrefabPath, typeof(GameObject));
        yield return loader;
        UnityEngine.Debug.Log("Open luanch window, use frameCount :" + (Time.frameCount - frameCount));
        var prefab = loader.asset as GameObject;
        loader.Dispose();
        if (prefab != null)
        {
            GameObject go = GameObject.Instantiate(prefab);
            InitAssetbundleUpdater(go);
        }
        else
        {
            UnityEngine.Debug.LogError("LoadAssetAsync luachPrefabPath err : " + luachPrefabPath);
        }

        yield break;
	}

    void InitAssetbundleUpdater(GameObject go)
    {
        var luanchLayer = GameObject.Find("UIRoot/LuanchLayer");
        go.transform.parent = luanchLayer.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        go.AddComponent<AssetbundleUpdater>();
    }
}
