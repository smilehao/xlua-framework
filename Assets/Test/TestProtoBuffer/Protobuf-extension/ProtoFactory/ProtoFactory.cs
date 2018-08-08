using battle;
using System;
using System.Collections.Generic;

/// <summary>
/// 说明：proto网络数据缓存池工厂类
/// 
/// @by wsh 2017-07-01
/// </summary>

public sealed class ProtoFactory
{
    static Dictionary<Type, IProtoPool> poolMap = new Dictionary<Type, IProtoPool>();

    public static void AddProtoPool(Type protoType, IProtoPool protoPool)
    {
#if UNITY_EDITOR
        if (poolMap.ContainsKey(protoType)) throw new Exception(string.Format("poolMap already contains key <{0}>!", protoType));
        if (protoPool == null) throw new ArgumentNullException("protoPool");
#endif
        if (!poolMap.ContainsKey(protoType)) poolMap.Add(protoType, protoPool);
    }

    public static void RemoveProtoPool(Type protoType)
    {
#if UNITY_EDITOR
        if (!poolMap.ContainsKey(protoType)) throw new Exception(string.Format("poolMap do not contains key <{0}>!", protoType));
#endif
        if (poolMap.ContainsKey(protoType)) poolMap.Remove(protoType);
    }

    public static void ClearProtoPool()
    {
        Dispose();
        poolMap.Clear();
    }

    /// <summary>
    /// 获取协议数据对象，如果没缓存，则返回null
    /// 注意：这个是给PB用的
    /// </summary>
    /// <param name="protoType"></param>
    /// <returns></returns>
    public static object Get(Type protoType)
    {
        object protoData = null;
        IProtoPool pool = null;
        if (poolMap.TryGetValue(protoType, out pool))
        {
            protoData = pool.Get();
        }
        return protoData;
    }

    /// <summary>
    /// 获取协议数据对象，如果没缓存，则创建
    /// 注意：游戏逻辑代码中，最好使用这个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Get<T>() where T : class
    {
        T protoData = Get(typeof(T)) as T;
        if (protoData == null)
        {
            protoData = Activator.CreateInstance<T>();
        }
        return protoData;
    }

    // 说明：除了指定的协议数据，其它数据暂时不走缓存
    public static void Recycle(object protoData)
    {
        // TODO：这里对嵌套协议体的回收有问题，如果上层协议P没定义缓存，而它包含了一个被缓存的子协议体C
        // 则在反序列化P时，Protobuf-net会将C从缓存池中取出，而回收时，C没法被回收，表现为缓存失效
        // 对于所有的byte[]缓存获取也存在同样的问题
        // 如果这样不频繁接受这样的上层协议体（对于发送不影响，每次都是new出来），则绝大时间内不影响缓存池的使用和GC释放
        // 现在暂时不做处理，但需要注意******
        if (protoData == null) return;
        Type protoType = protoData.GetType();
        IProtoPool pool;
        if (poolMap.TryGetValue(protoType, out pool))
        {
            pool.Recycle(protoData);
        }
    }

    // 说明：数据深拷贝
    public static object DeepCopy(object protoData)
    {
        if (protoData == null) return null;
        Type protoType = protoData.GetType();
        IProtoPool pool;
        if (poolMap.TryGetValue(protoType, out pool))
        {
            protoData = pool.DeepCopy(protoData);
        }
        return protoData;
    }

    public static void Dispose()
    {
        var item = poolMap.GetEnumerator();
        while (item.MoveNext())
        {
            item.Current.Value.Dispose();
        }
    }
}
