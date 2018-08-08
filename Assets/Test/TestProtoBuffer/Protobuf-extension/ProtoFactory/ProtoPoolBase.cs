using System;
using System.Collections.Generic;
using CustomDataStruct;

/// <summary>
/// 说明：proto网络数据缓存池基类，多线程安全
/// 
/// @by wsh 2017-07-01
/// </summary>

public abstract class ProtoPoolBase<T> : IProtoPool where T : class
{
    private const int POOL_SIZE_LIMIT = 1000;
#if UNITY_EDITOR
    MemoryLeakDetecter detecter = MemoryLeakDetecter.Add(typeof(T).FullName, 500, 1000);
#endif
    object mutex = new object();
    Queue<T> pool = new Queue<T>(32);

    protected abstract void RecycleChildren(T data);
    protected abstract void ClearNetData(T data);

    public object Get()
    {
        lock (mutex)
        {
#if UNITY_EDITOR
            detecter.IncreseInstance();
#endif
            return pool.Count > 0 ? pool.Dequeue() : Activator.CreateInstance<T>();
        }
    }

#if UNITY_EDITOR
    void CheckType(object data)
    {
        if (data.GetType() != typeof(T)) throw new Exception(string.Format("Need type <{0}>", typeof(T)));
    }
#endif

    public void Recycle(object data)
    {
#if UNITY_EDITOR
        CheckType(data);
#endif
        T netData = data as T;
        if (netData != null)
        {
            RecycleChildren(netData);
            ClearData(netData);
            lock (mutex)
            {
                // 说明：断网、或者UNITY_EDITOR暂停，必然导致网络包堆积，缓存池容量会迅速增长
                // 这里对缓存池做一次限容
                if (pool.Count < POOL_SIZE_LIMIT)
                {
                    pool.Enqueue(netData);
                }
#if UNITY_EDITOR
                detecter.DecreseInstance();
                detecter.SetPooledObjectCount(pool.Count);
#endif
            }
        }
    }

    public void ClearData(object data)
    {
#if UNITY_EDITOR
        CheckType(data);
#endif
        T netData = data as T;
        if (netData != null)
        {
            ClearNetData(netData);
        }
    }

    public object DeepCopy<U>(U data)
    {
        // 说明：只有需要发送的协议才需要实现此接口
        throw new NotImplementedException();
    }

    public virtual object DeepCopy(object data)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        lock (mutex)
        {
#if UNITY_EDITOR
            MemoryLeakDetecter.Remove(detecter);
#endif
            pool.Clear();
        }
    }
}
