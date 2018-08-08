//#define FOR_GC_TEST
using System.IO;
using UnityEngine;
using XLua;

/// <summary>
/// Xlua集成proto-gen-lua插件测试：启用宏测试GC
/// added by wsh @ 2018-08-09
/// </summary>

public class XluaProtoGenLuaTest : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        DoXluaProtoGenLuaTest();

#if FOR_GC_TEST
        Reporter reporter = GameObject.FindObjectOfType<Reporter>();
        if (reporter != null)
        {
            reporter.gameObject.SetActive(false);
        }
#endif
    }

    public TextAsset XluaProtoGenLuaTestLuaScript;

    private void DoXluaProtoGenLuaTest()
    {
#if !FOR_GC_TEST
        Logger.Log(" =========================XluaProtoGenLuaTest=========================");
#endif

        LuaEnv luaEnv = new LuaEnv();
        luaEnv.AddLoader(CustomLoader);
        luaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadPb);
        luaEnv.DoString(XluaProtoGenLuaTestLuaScript.text);
        luaEnv.Dispose();
    }

    public static byte[] CustomLoader(ref string filepath)
    {
        string scriptPath = string.Empty;
        filepath = filepath.Replace(".", "/") + ".lua";
        scriptPath = Path.Combine(Application.dataPath, XLuaManager.luaScriptsFolder);
        scriptPath = Path.Combine(scriptPath, filepath);
        //Logger.Log("Load lua script : " + scriptPath);
        return GameUtility.SafeReadAllBytes(scriptPath);
    }

#if !FOR_GC_TEST
    #region 给个提示
    void OnGUI()
    {
        GUI.Label(new Rect(100, 100, 300, 300), "画圆圈看输出！画圆圈看输出！画圆圈看输出！\n重要的事情说三遍！！！");
    }
    #endregion
#endif
}
