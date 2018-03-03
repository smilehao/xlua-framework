using UnityEngine;
using System.Collections;
using XLua;
using System.Collections.Generic;

public class LuaTest : MonoBehaviour
{
#if CLIENT_DEBUG
    bool init = false;
    List<string> nameList = new List<string>();
    Dictionary<string, LuaFunction> funcDic = new Dictionary<string, LuaFunction>();

    void Init()
    {
        init = true;
        XLuaManager.Instance.SafeDoString("GameTestMain = require \"GameTest/GameTestMain\"");
        var luaenv = XLuaManager.Instance.GetLuaEnv();
        var luaTable = luaenv.Global.Get<LuaTable>("GameTestMain");
        luaTable.ForEach((string key, LuaFunction func) =>
        {
            nameList.Add(key);
            funcDic.Add(key, func);
        });
        nameList.Sort();
    }

    void OnGUI()
    {
        if (!init && XLuaManager.Instance.HasGameStart)
        {
            Init();
        }

        int rawMaxCount = Screen.width / 100;
        int count = nameList.Count;
        int columnCount = (count + rawMaxCount - 1) / rawMaxCount;
        GUILayout.BeginVertical();
        GUILayout.Space(60);
        for (int i = 0; i < columnCount; i++)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < rawMaxCount; j++)
            {
                int index = i * rawMaxCount + j;
                if (index >= count)
                {
                    break;
                }

                var name = nameList[index];
                var func = funcDic[name];
                if (GUILayout.Button(name, GUILayout.MinWidth(100), GUILayout.MinHeight(50), GUILayout.MaxWidth(100), GUILayout.MaxHeight(50)))
                {
                    func.Call();
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
#endif
}
