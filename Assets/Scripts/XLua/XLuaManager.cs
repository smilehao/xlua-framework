using UnityEngine;
using XLua;

/// <summary>
/// 说明：xLua管理类
/// 
/// @by wsh 2017-08-30
/// </summary>

public class XLuaManager : MonoSingleton<XLuaManager>
{
    const string gameMainScriptName = "GameMain";
    const string commonGameTools = "Common.Main";
    LuaEnv luaEnv = null;
    LuaUpdater luaUpdater = null;

    protected override void Init()
    {
        base.Init();

        luaEnv = new LuaEnv();
        if (luaEnv != null)
        {
            luaEnv.AddLoader(CustomLoader);
            LoadScript(commonGameTools);
            luaUpdater = gameObject.AddComponent<LuaUpdater>();
            luaUpdater.OnInit(luaEnv);
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
            Debug.LogError(msg, null);
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
            Debug.LogError(msg, null);
        }
    }

    public static byte[] CustomLoader(ref string filepath)
    {
#if UNITY_EDITOR
        string scriptPath = Application.dataPath +  "/LuaScripts/" + filepath.Replace(".", "/") + ".lua";
        return GameUtility.SafeReadAllBytes(scriptPath);
#else
        // TODO：此处从项目资源管理器加载lua脚本
        //TextAsset textAsset = (TextAsset)ResourceMgr.instance.SyncLoad(ResourceMgr.RESTYPE.XLUA_SCRIPT, filepath).resObject;

        string scriptPath = "Lua/" + filepath.Replace(".", "/") + ".lua";
        TextAsset textAsset = Resources.Load<TextAsset>(scriptPath);
        if (textAsset != null)
        {
            return textAsset.bytes;
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
