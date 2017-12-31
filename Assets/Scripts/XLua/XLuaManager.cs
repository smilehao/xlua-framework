using AssetBundles;
using UnityEngine;
using XLua;

/// <summary>
/// 说明：xLua管理类
/// 注意：
/// 1、整个Lua虚拟机执行的脚本分成3个模块：热修复、公共模块、逻辑模块
/// 2、热修复模块：脚本全部放Lua/XLua目录下，随着游戏的启动而启动，资源热更完毕后重修加载
///     A）资源热更完成前的热修复脚本不要引用XLua目录以外的其它脚本，这个时候其它模块还没载入，而引用会造成其它模块被载入，而其它两个模块暂时不支持重载入
///     B）
/// @by wsh 2017-12-28
/// </summary>

public class XLuaManager : MonoSingleton<XLuaManager>
{
    const string luaAssetbundleAssetName = "lua";
    const string commonMainScriptName = "Common.Main";
    const string gameMainScriptName = "GameMain";
    const string hotfixMainScriptName = "XLua.HotfixMain";
    LuaEnv luaEnv = null;
    LuaUpdater luaUpdater = null;

    protected override void Init()
    {
        base.Init();
        string path = AssetBundleUtility.RelativeAssetPathToAbsoluteAssetPath(luaAssetbundleAssetName);
        AssetbundleName = AssetBundleUtility.AssetBundleAssetPathToAssetBundleName(path);
        InitLuaEnv();
    }

    void InitLuaEnv()
    {
        luaEnv = new LuaEnv();
        if (luaEnv != null)
        {
            luaEnv.AddLoader(CustomLoader);
        }
        else
        {
            Logger.LogError("InitLuaEnv null!!!");
        }
    }

    // 这里必须要等待资源管理模块加载Lua AB包以后才能初始化
    public void OnInit()
    {
        if (luaEnv != null)
        {
            LoadScript(commonMainScriptName);
            luaUpdater = gameObject.GetComponent<LuaUpdater>();
            if (luaUpdater == null)
            {
                luaUpdater = gameObject.AddComponent<LuaUpdater>();
            }
            luaUpdater.OnInit(luaEnv);
        }
    }

    public string AssetbundleName
    {
        get;
        protected set;
    }

    // 重启虚拟机：热更资源以后被加载的lua脚本可能已经过时，需要重新加载
    // 最简单和安全的方式是另外创建一个虚拟器，所有东西一概重启
    public void Restart()
    {
        Dispose();
        InitLuaEnv();
        OnInit();
    }

    public void StartHotfix(bool restart = false)
    {
        if (luaEnv == null)
        {
            return;
        }

        if (restart)
        {
            ReloadScript(hotfixMainScriptName);
        }
        else
        {
            LoadScript(hotfixMainScriptName);
        }
    }

    public void StartGame()
    {
        if (luaEnv != null)
        {
            LoadScript(gameMainScriptName);
        }
    }
    
    public void ReloadScript(string scriptName)
    {
        try
        {
            luaEnv.DoString(string.Format("package.loaded['{0}'] = nil", scriptName));
        }
        catch (System.Exception ex)
        {
            string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
            Logger.LogError(msg, null);
        }
        LoadScript(scriptName);
    }

    void LoadScript(string scriptName)
    {
        try
        {
            luaEnv.DoString(string.Format("require('{0}')", scriptName));
        }
        catch (System.Exception ex)
        {
            string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
            Logger.LogError(msg, null);
        }
    }

    public static byte[] CustomLoader(ref string filepath)
    {
#if UNITY_EDITOR
        string scriptPath = Application.dataPath +  "/LuaScripts/" + filepath.Replace(".", "/") + ".lua";
        return GameUtility.SafeReadAllBytes(scriptPath);
#else
        string scriptPath = "Lua/" + filepath.Replace(".", "/") + ".lua.bytes";
        Logger.Log("Load lua script : " + scriptPath);
        string assetbundleName = null;
        string assetName = null;
        bool status = AssetBundleManager.Instance.MapAssetPath(scriptPath, out assetbundleName, out assetName);
        if (!status)
        {
            Logger.LogError("MapAssetPath failed : " + scriptPath);
            return null;
        }
        var asset = AssetBundleManager.Instance.GetAssetCache(assetName) as TextAsset;
        if (asset != null)
        {
            Logger.Log("Load lua script : " + scriptPath);
            return asset.bytes;
        }
        Logger.LogError("Load lua script failed : " + scriptPath);
        return null;
#endif
    }

    private void Update()
    {
        if (luaEnv != null)
        {
            luaEnv.Tick();

            if (Time.frameCount % 100 == 0)
            {
                luaEnv.FullGc();
            }
        }
    }

    public override void Dispose()
    {
        if (luaUpdater != null)
        {
            luaUpdater.OnDispose();
        }
        if (luaEnv != null)
        {
            luaEnv.Dispose();
            luaEnv = null;
        }
    }
}
