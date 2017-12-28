#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using System;
using System.Collections.Generic;

/// <summary>
/// 说明：LuaFunction扩展
/// 
/// @by wsh 2017-09-07
/// </summary>

namespace XLua
{
    public partial class LuaFunction : LuaBase
    {
        public Delegate Cast(Type delType)
        {
            if (!delType.IsSubclassOf(typeof(Delegate)))
            {
                throw new InvalidOperationException(delType.Name + " is not a delegate type");
            }
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                var L = luaEnv.L;
                var translator = luaEnv.translator;
                push(L);
                Delegate ret = (Delegate)translator.GetObject(L, -1, delType);
                LuaAPI.lua_pop(luaEnv.L, 1);
                return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }
    }
}
