using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XLua;

/// <summary>
/// 说明：用于导出在CS侧频繁使用，但是默认无法导出（如泛型函数）的函数
///     1、对不支持的泛型方法使用改写、扩展方法、具体化等方式导出
/// 
/// 注意：
///     1、功能不完善，需要持续改进，如果必要考虑模块划分
///     2、XLuaHelper、XLuaMessenger从本质上来说是使用代理和桥接为LuaCallCSharp提供更好的支持
///     3、设计模式：CSharp业务模块->CSharp代理->XLua桥接->Lua代理->Lua业务模块。具体参考Lua消息系统的实现
///     4、理论上，可以使用上述模式实现CSharp到Lua代码的无缝衔接
///     
/// TODO：
///     1、阅读XLua源代码，看它的自定义代码生成器是否能完成以上需求，改写这部分程序架构以提供自动化支持
///     
/// @by wsh 2017-09-09
/// </summary>

public static class XLuaHelper
{
    // 说明：扩展NGUITools.AddMissingComponent方法
    public static Component AddMissingComponent(this GameObject go, Type cmpType)
    {
        Component comp = go.GetComponent(cmpType);
        if (comp == null)
        {
            comp = go.AddComponent(cmpType);
        }
        return comp;
    }

    // 说明：扩展CreateInstance方法
    public static Array CreateArrayInstance(Type itemType, int itemCount)
    {
        return Array.CreateInstance(itemType, itemCount);
    }
    
    public static IList CreateListInstance(Type itemType)
    {
        return (IList)Activator.CreateInstance(MakeGenericListType(itemType));
    }

    public static IDictionary CreateDictionaryInstance(Type keyType, Type valueType)
    {
        return (IDictionary)Activator.CreateInstance(MakeGenericDictionaryType(keyType, valueType));
    }

    // 说明：创建委托
    // 注意：重载函数的定义顺序很重要：从更具体类型（Type）到不具体类型（object）,xlua生成导出代码和lua侧函数调用匹配时都是从上到下的，如果不具体类型（object）写在上面，则永远也匹配不到更具体类型（Type）的重载函数，很坑爹
    public static Delegate CreateActionDelegate(Type type, string methodName, params Type[] paramTypes)
    {
        return InnerCreateDelegate(MakeGenericActionType, null, type, methodName, paramTypes);
    }

    public static Delegate CreateActionDelegate(object target, string methodName, params Type[] paramTypes)
    {
        return InnerCreateDelegate(MakeGenericActionType, target, null, methodName, paramTypes);
    }

    public static Delegate CreateCallbackDelegate(Type type, string methodName, params Type[] paramTypes)
    {
        return InnerCreateDelegate(MakeGenericCallbackType, null, type, methodName, paramTypes);
    }

    public static Delegate CreateCallbackDelegate(object target, string methodName, params Type[] paramTypes)
    {
        return InnerCreateDelegate(MakeGenericCallbackType, target, null, methodName, paramTypes);
    }

    delegate Type MakeGenericDelegateType(params Type[] paramTypes);
    static Delegate InnerCreateDelegate(MakeGenericDelegateType del, object target, Type type, string methodName, params Type[] paramTypes)
    {
        if (target != null)
        {
            type = target.GetType();
        }

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        MethodInfo methodInfo = (paramTypes == null || paramTypes.Length == 0) ? type.GetMethod(methodName, bindingFlags) : type.GetMethod(methodName, bindingFlags, null, paramTypes, null);
        Type delegateType = del(paramTypes);
        return Delegate.CreateDelegate(delegateType, target, methodInfo);
    }

    // 说明：构建List类型
    public static Type MakeGenericListType(Type itemType)
    {
        return typeof(List<>).MakeGenericType(itemType);
    }

    // 说明：构建Dictionary类型
    public static Type MakeGenericDictionaryType(Type keyType, Type valueType)
    {
        return typeof(Dictionary<,>).MakeGenericType(new Type[] { keyType, valueType });
    }

    // 说明：构建Action类型

    public static Type MakeGenericActionType(params Type[] paramTypes)
    {
        if (paramTypes == null || paramTypes.Length == 0)
        {
            return typeof(Action);
        }
        else if (paramTypes.Length == 1)
        {
            return typeof(Action<>).MakeGenericType(paramTypes);
        }
        else if (paramTypes.Length == 2)
        {
            return typeof(Action<,>).MakeGenericType(paramTypes);
        }
        else
        {
            return typeof(Action<,,,>).MakeGenericType(paramTypes);
        }
    }

    // 说明：构建Callback类型
    public static Type MakeGenericCallbackType(params Type[] paramTypes)
    {
        if (paramTypes == null || paramTypes.Length == 0)
        {
            return typeof(Callback);
        }
        else if (paramTypes.Length == 1)
        {
            return typeof(Callback<>).MakeGenericType(paramTypes);
        }
        else if (paramTypes.Length == 2)
        {
            return typeof(Callback<,>).MakeGenericType(paramTypes);
        }
        else
        {
            return typeof(Callback<,,>).MakeGenericType(paramTypes);
        }
    }
}

#if UNITY_EDITOR
public static class XLuaHelperExporter
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() {
        // XLuaHelper
        typeof(XLuaHelper),
        typeof(Component),
        typeof(Array),
        typeof(IList),
        typeof(IDictionary),
        typeof(Activator),
        typeof(Type),
        typeof(BindingFlags),
    };

    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
    };
}
#endif
