//#define FOR_GC_TEST
using AssetBundles;
using System.IO;
using UnityEngine;
using XLua;

/// <summary>
/// Xlua集成proto-gen-lua插件测试：启用宏测试GC
/// 
/// 注意：
/// 1）如果不是从LaunchScene登陆进入有戏点击CustomTest按钮进行的测试，则AB模拟模式必须选择EditorMode
/// 
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
        XLuaManager.Instance.Startup();
        XLuaManager.Instance.SafeDoString(XluaProtoGenLuaTestLuaScript.text);
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
