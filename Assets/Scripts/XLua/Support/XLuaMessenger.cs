using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XLua;

/// <summary>
/// 说明：解决XLua侧无法使用Messenger泛型方法发送消息的问题
/// 
/// 注意：
///     1）频繁运行的消息会有严重GC和性能问题，对于这类消息，最好不要在Lua侧使用消息系统
///     2）在Lua侧添加的消息一定也要在Lua侧移除
/// 
/// @by wsh 2017-09-07
/// </summary>

public static class XLuaMessenger
{
    // 说明：
    // 1）有可能在Lua侧使用的消息最好在此处映射
    // 2）有要求高效率运作的消息最好在此处映射
    // 3）如果不做映射，则使用反射机制，灵活，但是效率低，还要注意IOS代码裁剪
	// 4）在Lua侧以某种方式添加监听的消息也一定要以对应的方式在Lua侧移除

    public static Dictionary<string, Type> MessageNameTypeMap = new Dictionary<string, Type>() {
        // UIArena测试模块
        //{ MessageName.MN_ARENA_PERSONAL_PANEL, typeof(Callback<ArenaPanelData>) },//导出测试
        //{ MessageName.MN_RESET_RIVAL, typeof(Callback<List<ArenaRivalData>>) },
        //{ MessageName.MN_ARENA_UPDATE, typeof(Callback<ArenaPanelData>) },//缓存委托测试
        //{ MessageName.MN_ARENA_BOX, typeof(Callback<int>) },//反射测试
        //{ MessageName.MN_ARENA_CLEARDATA, typeof(Callback) },
    };

    public static Delegate CreateDelegate(string eventType, LuaFunction func)
    {
        if (!MessageNameTypeMap.ContainsKey(eventType))
        {
            Logger.LogError(string.Format("You should register eventType : {0} first!", eventType));
            return null;
        }
        return func.Cast(MessageNameTypeMap[eventType]);
    }

    public static void AddListener(string eventType, Delegate handler)
    {
        Messenger.OnListenerAdding(eventType, handler);
        Messenger.eventTable[eventType] = Delegate.Combine(Messenger.eventTable[eventType], handler);
    }

    public static void RemoveListener(string eventType, Delegate handler)
    {
        //OnListenerRemoving(eventType, handler);
        if (Messenger.eventTable.ContainsKey(eventType))
        {
            Messenger.eventTable[eventType] = Delegate.Remove(Messenger.eventTable[eventType], handler);
        }
        Messenger.OnListenerRemoved(eventType);
    }

    public static void Broadcast(string eventType, object arg1)
    {
        Messenger.OnBroadcasting(eventType);

        Delegate d;
        if (Messenger.eventTable.TryGetValue(eventType, out d))
        {
            try
            {
                Type[] paramArr = d.GetType().GetGenericArguments();
                object param1 = arg1;
                if (paramArr.Length >= 1)
                {
                    param1 = CastType(paramArr[0], arg1) ?? arg1;
                }
                d.DynamicInvoke(param1);
            }
            catch (System.Exception ex)
            {
                Logger.LogError(string.Format("{0}:{1}", ex.Message, string.Format("arg1 = {0}, typeof(arg1) = {1}", arg1, arg1.GetType())));
                throw Messenger.CreateBroadcastSignatureException(eventType);
            }
        }
    }

    public static void Broadcast(string eventType, object arg1, object arg2)
    {
        Messenger.OnBroadcasting(eventType);

        Delegate d;
        if (Messenger.eventTable.TryGetValue(eventType, out d))
        {
            try
            {
                Type[] paramArr = d.GetType().GetGenericArguments();
                object param1 = arg1;
                object param2 = arg2;
                if (paramArr.Length >= 2)
                {
                    param1 = CastType(paramArr[0], arg1) ?? arg1;
                    param2 = CastType(paramArr[1], arg2) ?? arg2;
                }
                d.DynamicInvoke(param1, param2);
            }
            catch (System.Exception ex)
            {
                Logger.LogError(string.Format("{0}:{1}", ex.Message, string.Format("arg1 = {0}, typeof(arg1) = {1}, arg2 = {2}, typeof(arg2) = {3}", arg1, arg1.GetType(), arg2, arg2.GetType())));
                throw Messenger.CreateBroadcastSignatureException(eventType);
            }
        }
    }

    public static void Broadcast(string eventType, object arg1, object arg2, object arg3)
    {
        Messenger.OnBroadcasting(eventType);

        Delegate d;
        if (Messenger.eventTable.TryGetValue(eventType, out d))
        {
            try
            {
                Type[] paramArr = d.GetType().GetGenericArguments();
                object param1 = arg1;
                object param2 = arg2;
                object param3 = arg3;
                if (paramArr.Length >= 3)
                {
                    param1 = CastType(paramArr[0], arg1) ?? arg1;
                    param2 = CastType(paramArr[1], arg2) ?? arg2;
                    param3 = CastType(paramArr[2], arg3) ?? arg3;
                }
                d.DynamicInvoke(param1, param2, param3);
            }
            catch (System.Exception ex)
            {
                Logger.LogError(string.Format("{0}:{1}", ex.Message, string.Format("arg1 = {0}, typeof(arg1) = {1}, arg2 = {2}, typeof(arg2) = {3}, arg3 = {2}, typeof(arg3) = {3}", arg1, arg1.GetType(), arg2, arg2.GetType(), arg3, arg3.GetType())));
                throw Messenger.CreateBroadcastSignatureException(eventType);
            }
        }
    }

    public static object CastType(Type type, object valueObj)
    {
        try
        {
            if (type == typeof(TimeSpan)) return (TimeSpan)valueObj;
            if (type == typeof(Guid)) return (Guid)valueObj;

            TypeCode code = Type.GetTypeCode(type);
            if (code == TypeCode.Object)
            {
                return valueObj;
            }
            else
            {
                return Convert.ChangeType(valueObj, code);
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError(string.Format("{0} : {1}", ex.Message, string.Format("Unknow cast type : {0}, valueObj type : {1}", type, valueObj.GetType())));
            return valueObj;
        }
    }
}

#if UNITY_EDITOR
public static class XLuaMessengerExporter
{
#if UNITY_EDITOR
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() {
        // XLuaMessenger
        typeof(XLuaMessenger),
        typeof(MessageName),

        // UIArena回调消息中的参数类型
        //typeof(ArenaPanelData),
        //typeof(List<ArenaRivalData>),
    };

    [CSharpCallLua]
    public static List<Type> CSharpCallLua1 = new List<Type>()
    {
    };

    // 由映射表自动导出
    [CSharpCallLua]
    public static List<Type> CSharpCallLua2 = Enumerable.Where(XLuaMessenger.MessageNameTypeMap.Values, type => typeof(Delegate).IsAssignableFrom(type)).ToList();
#endif
}
#endif
