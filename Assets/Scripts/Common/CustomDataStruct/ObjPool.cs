using System;
using System.Collections.Generic;

/// <summary>
/// 说明：内部缓存池，主要是用于泛型缓存，多线程安全
/// 
/// 注意：
///     1）效率很低，比普通的Queue、Stack、BetterList等做缓存池要慢一倍，所以如果类型确定，最好自己做缓存
/// 
/// by wsh @ 2017-06-29
/// </summary>

namespace CustomDataStruct
{
    internal interface IRelease
    {
        void Release();
    }

    internal abstract class ObjPoolBase : IDisposable
    {
        protected static List<ObjPoolBase> instanceList = new List<ObjPoolBase>();
        protected static void AddInstance(ObjPoolBase instance) { instanceList.Add(instance); }
        public abstract void Release(object obj);
        public abstract void Dispose();
        public static void Cleanup()
        {
            lock (instanceList)
            {
                for (int i = 0; i < instanceList.Count; i++)
                {
                    instanceList[i].Dispose();
                }
                instanceList.Clear();
            }
        }
    }

    internal sealed class ObjPool<T> : ObjPoolBase where T : class, IRelease
    {
#if UNITY_EDITOR
        MemoryLeakDetecter detecter = MemoryLeakDetecter.Add(typeof(T).FullName, 1000, 1000);
#endif
        private const int POOL_SIZE_LIMIT = 1000;
        public static ObjPool<T> instance;
        private Queue<T> pool = new Queue<T>(32);

        public static T Get()
        {
            lock (instanceList)
            {
                if (instance == null) AddInstance(instance = new ObjPool<T>());
            }
            lock (instance.pool)
            {
#if UNITY_EDITOR
                instance.detecter.IncreseInstance();
#endif
                return instance.pool.Count > 0 ? instance.pool.Dequeue() : Activator.CreateInstance<T>();
            }
        }

        public override void Release(object obj)
        {
            lock (instance.pool)
            {
                if (pool.Count < POOL_SIZE_LIMIT)
                {
                    T target = obj as T;
                    instance.pool.Enqueue(target);
                }
#if UNITY_EDITOR
                instance.detecter.DecreseInstance();
                instance.detecter.SetPooledObjectCount(instance.pool.Count);
#endif
            }
        }

        public override void Dispose()
        {
#if UNITY_EDITOR
            MemoryLeakDetecter.Remove(detecter);
#endif
        }
    }
}
