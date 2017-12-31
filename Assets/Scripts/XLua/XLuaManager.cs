using AssetBundles;
using UnityEngine;
using XLua;

/// <summary>
/// 说明：xLua管理类
/// 
/// @by wsh 2017-12-28
/// </summary>

public class XLuaManager : MonoSingleton<XLuaManager>
{
    const string commonMainScriptName = "Common.Main";
    const string gameMainScriptName = "GameMain";
    const string hotfixMainScriptName = "XLua.HotfixMain";
    LuaEnv luaEnv = null;
    LuaUpdater luaUpdater = null;

    protected override void Init()
    {
        base.Init();
        luaEnv = new LuaEnv();

        if (luaEnv != null)
        {
            luaEnv.AddLoader(CustomLoader);
            LoadScript(commonMainScriptName);
            luaUpdater = gameObject.AddComponent<LuaUpdater>();
            luaUpdater.OnInit(luaEnv);
        }
    }

    public void StartHotfix(bool restart = false)
    {
        if (luaEnv != null)
        {
            if (restart)
            {
                ReloadScript(hotfixMainScriptName);
            }
            else
            {
                LoadScript(hotfixMainScriptName);
            }
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
            if (luaEnv != null)
            {
                luaEnv.DoString(string.Format("package.loaded['{0}'] = nil", scriptName));
            }
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
            if (luaEnv != null)
            {
                luaEnv.DoString(string.Format("require('{0}')", scriptName));
            }
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
            return asset.bytes;
        }
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
