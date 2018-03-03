using UnityEngine;
using System.Collections;
using XLua;
using System.Collections.Generic;
using System;

/// <summary>
/// 说明：xlua中的扩展方法
///     
/// @by wsh 2017-12-28
/// </summary>

public static class UnityEngineObjectExtention
{
    // 说明：lua侧判Object为空全部使用这个函数
    public static bool IsNull(this UnityEngine.Object o)
    {
        return o == null;
    }
}

#if UNITY_EDITOR
public static class UnityEngineObjectExtentionExporter
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(UnityEngineObjectExtention),
        typeof(UnityEngine.Object),
    };
}
#endif