using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaUpdater : MonoBehaviour
{
    Action<float, float> luaUpdate = null;
    Action luaLateUpdate = null;
    Action<float> luaFixedUpdate = null;

    public void OnInit(LuaEnv luaEnv)
    {
        luaUpdate = luaEnv.Global.Get<Action<float, float>>("Update");
        luaLateUpdate = luaEnv.Global.Get<Action>("LateUpdate");
        luaFixedUpdate = luaEnv.Global.Get<Action<float>>("FixedUpdate");
    }
    
    void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }

    void LateUpdate()
    {
        if (luaLateUpdate != null)
        {
            luaLateUpdate();
        }
    }

    void FixedUpdate()
    {
        if (luaFixedUpdate != null)
        {
            luaFixedUpdate(Time.fixedDeltaTime);
        }
    }

    public void OnDispose()
    {
        luaUpdate = null;
        luaLateUpdate = null;
        luaFixedUpdate = null;
    }
    
    void OnDestroy()
    {
        OnDispose();
    }
}

public static class LuaUpdaterExport
{
    [ReflectionUse]
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        typeof(Action),
        typeof(Action<float>),
        typeof(Action<float, float>),
    };
}
