using UnityEngine;
using System.Collections;
using XLua;

/// <summary>
/// 说明：xlua中的扩展方法
///     
/// @by wsh 2017-12-28
/// </summary>

[LuaCallCSharp]
[ReflectionUse]
public static class UnityEngineObjectExtention
{
    // 说明：lua侧判Object为空全部使用这个函数
    public static bool IsNull(this UnityEngine.Object o)
    {
        return o == null;
    }
}