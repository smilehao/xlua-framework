using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 说明：类包装的委托；无GC，多线程安全
///     1）针对委托无GC使用方式（参考TestDelegateGC）太过麻烦而提供的一个简单使用的解决方案
///     2）委托参数全部使用泛型，避免装箱而产生GC
///     3）委托和委托所带的参数被封装到一块，提供更加便捷的使用方式
/// 
/// by wsh @ 2017-06-19
/// </summary>

namespace CustomDataStruct
{
    static public class BetterDelegate
    {
        static private List<IDelegateBase> instanceList = new List<IDelegateBase>();
        
        static private void Add(IDelegateBase delagateAction)
        {
            lock (instanceList) instanceList.Add(delagateAction);
        }

        static private void Remove(IDelegateBase delagateAction)
        {
            lock (instanceList) instanceList.Remove(delagateAction);
        }

        static public void Cleanup()
        {
            lock (instanceList)
            {
                for (int i = 0; i < instanceList.Count; i++)
                {
                    if (instanceList[i] != null)
                    {
                        instanceList[i].Cleanup();
                    }
                }
                instanceList.Clear();
            }
        }

        static public IDelegateAction GetAction(Delegate callback)
        {
            DelegateAction delegateAction = DelegateAction.Get();
            if (delegateAction != null)
            {
                delegateAction.mCallback = callback;
            }
            return delegateAction;
        }

        static public IDelegateAction GetAction<T>(Delegate callback, T arg1)
        {
            DelegateAction<T> delegateAction = DelegateAction<T>.Get();
            if (delegateAction != null)
            {
                delegateAction.mCallback = callback;
                delegateAction.mArg1 = arg1;
            }
            return delegateAction;
        }

        static public IDelegateAction GetAction<T, U>(Delegate callback, T arg1, U arg2)
        {
            DelegateAction<T, U> delegateAction = DelegateAction<T, U>.Get();
            if (delegateAction != null)
            {
                delegateAction.mCallback = callback;
                delegateAction.mArg1 = arg1;
                delegateAction.mArg2 = arg2;
            }
            return delegateAction;
        }

        static public IDelegateAction GetAction<T, U, V>(Delegate callback, T arg1, U arg2, V arg3)
        {
            DelegateAction<T, U, V> delegateAction = DelegateAction<T, U, V>.Get();
            if (delegateAction != null)
            {
                delegateAction.mCallback = callback;
                delegateAction.mArg1 = arg1;
                delegateAction.mArg2 = arg2;
                delegateAction.mArg3 = arg3;
            }
            return delegateAction;
        }

        static void LogError(Type delType, params Type[] p)
        {
            string delName = Helper.HandleTypeFullName(delType.FullName);
            string[] argNames = new string[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                argNames[i] = Helper.HandleTypeFullName(p[i].FullName);
            }
            LogError(delName, argNames);
        }

        static void LogError(string delName, string[] argNames)
        {
            StringBuilder formatString = new StringBuilder();
            formatString.AppendFormat("{0} signature not match!", delName);
            if (argNames != null && argNames.Length > 0)
            {
                formatString.AppendFormat("{0} params are : ", argNames.Length);
                formatString.Append(string.Join(", ", argNames));
            }
            Debug.LogError(formatString.ToString());
        }
        
        private class DelegateAction : IDelegateBase
        {
            private const int POOL_SIZE_LIMIT = 100;
#if UNITY_EDITOR
            static MemoryLeakDetecter detecter = MemoryLeakDetecter.Add(typeof(Queue<DelegateAction>).FullName, 100, 100);
#endif
            static Queue<DelegateAction> pool = new Queue<DelegateAction>(32);
            public Delegate mCallback;

            public static DelegateAction Get()
            {
                lock (pool)
                {
#if UNITY_EDITOR
                    detecter.IncreseInstance();
#endif
                    if (pool.Count > 0)
                    {
                        return pool.Dequeue();
                    }
                }
                DelegateAction instance = new DelegateAction();
                BetterDelegate.Add(instance);
                return instance;
            }

            public void Invoke()
            {
                DelegateCallback action = mCallback as DelegateCallback;
                if (action != null)
                {
                    action.Invoke();
                }
                else
                {
                    LogError(mCallback.GetType());
                }
                Release();
            }

            public void Invoke<J>(J outArg1)
            {
                DelegateCallback<J> action = mCallback as DelegateCallback<J>;
                if (action != null)
                {
                    action.Invoke(outArg1);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J));
                }
                Release();
            }

            public void Invoke<J, K>(J outArg1, K outArg2)
            {
                DelegateCallback<J, K> action = mCallback as DelegateCallback<J, K>;
                if (action != null)
                {
                    action.Invoke(outArg1, outArg2);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(K));
                }
                Release();
            }

            public void Invoke<J, K, M>(J outArg1, K outArg2, M outArg3)
            {
                DelegateCallback<J, K, M> action = mCallback as DelegateCallback<J, K, M>;
                if (action != null)
                {
                    action.Invoke(outArg1, outArg2, outArg3);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(K), typeof(M));
                }
                Release();
            }

            public void Release()
            {
                lock (pool)
                {
                    if (pool.Count < POOL_SIZE_LIMIT)
                    {
                        pool.Enqueue(this);
                    }
#if UNITY_EDITOR
                    detecter.DecreseInstance();
                    detecter.SetPooledObjectCount(pool.Count);
#endif
                }
            }

            public void Dispose()
            {
                Release();
            }

            public void Cleanup()
            {
                lock (pool)
                {
                    pool.Clear();
                }
#if UNITY_EDITOR
                MemoryLeakDetecter.Remove(detecter);
#endif
            }
        }

        private class DelegateAction<T> : IDelegateBase
        {
            private const int POOL_SIZE_LIMIT = 100;
#if UNITY_EDITOR
            static MemoryLeakDetecter detecter = MemoryLeakDetecter.Add(typeof(DelegateAction<T>).FullName, 100, 100);
#endif
            static Queue<DelegateAction<T>> pool = new Queue<DelegateAction<T>>(32);
            public Delegate mCallback;
            public T mArg1;

            public static DelegateAction<T> Get()
            {
                lock (pool)
                {
#if UNITY_EDITOR
                    detecter.IncreseInstance();
#endif
                    if (pool.Count > 0)
                    {
                        return pool.Dequeue();
                    }
                }
                DelegateAction<T> instance = new DelegateAction<T>();
                BetterDelegate.Add(instance);
                return instance;
            }

            public void Invoke()
            {
                DelegateCallback<T> action = mCallback as DelegateCallback<T>;
                if (action != null)
                {
                    action.Invoke(mArg1);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(T));
                }
                Release();
            }

            public void Invoke<J>(J outArg1)
            {
                DelegateCallback<J, T> action = mCallback as DelegateCallback<J, T>;
                if (action != null)
                {
                    action.Invoke(outArg1, mArg1);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(T));
                }
                Release();
            }

            public void Invoke<J, K>(J outArg1, K outArg2)
            {
                DelegateCallback<J, K, T> action = mCallback as DelegateCallback<J, K, T>;
                if (action != null)
                {
                    action.Invoke(outArg1, outArg2, mArg1);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(K), typeof(T));
                }
                Release();
            }

            public void Invoke<J, K, M>(J outArg1, K outArg2, M outArg3)
            {
                DelegateCallback<J, K, M, T> action = mCallback as DelegateCallback<J, K, M, T>;
                if (action != null)
                {
                    action.Invoke(outArg1, outArg2, outArg3, mArg1);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(K), typeof(M), typeof(T));
                }
                Release();
            }

            public void Release()
            {
                lock (pool)
                {
                    if (pool.Count < POOL_SIZE_LIMIT)
                    {
                        pool.Enqueue(this);
                    }
#if UNITY_EDITOR
                    detecter.DecreseInstance();
                    detecter.SetPooledObjectCount(pool.Count);
#endif
                }
            }

            public void Dispose()
            {
                Release();
            }

            public void Cleanup()
            {
                lock (pool)
                {
                    pool.Clear();
                }
#if UNITY_EDITOR
                MemoryLeakDetecter.Remove(detecter);
#endif
            }
        }

        private class DelegateAction<T, U> : IDelegateBase
        {
            private const int POOL_SIZE_LIMIT = 100;
#if UNITY_EDITOR
            static MemoryLeakDetecter detecter = MemoryLeakDetecter.Add(typeof(DelegateAction<T, U>).FullName, 100, 100);
#endif
            static Queue<DelegateAction<T, U>> pool = new Queue<DelegateAction<T, U>>(32);
            public Delegate mCallback;
            public T mArg1;
            public U mArg2;

            public static DelegateAction<T, U> Get()
            {
                lock (pool)
                {
#if UNITY_EDITOR
                    detecter.IncreseInstance();
#endif
                    if (pool.Count > 0)
                    {
                        return pool.Dequeue();
                    }
                }
                DelegateAction<T, U> instance = new DelegateAction<T, U>();
                BetterDelegate.Add(instance);
                return instance;
            }

            public void Invoke()
            {
                DelegateCallback<T, U> action = mCallback as DelegateCallback<T, U>;
                if (action != null)
                {
                    action.Invoke(mArg1, mArg2);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(T), typeof(U));
                }
                Release();
            }

            public void Invoke<J>(J outArg1)
            {
                DelegateCallback<J, T, U> action = mCallback as DelegateCallback<J, T, U>;
                if (action != null)
                {
                    action.Invoke(outArg1, mArg1, mArg2);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(T), typeof(U));
                }
                Release();
            }

            public void Invoke<J, K>(J outArg1, K outArg2)
            {
                DelegateCallback<J, K, T, U> action = mCallback as DelegateCallback<J, K, T, U>;
                if (action != null)
                {
                    action.Invoke(outArg1, outArg2, mArg1, mArg2);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(K), typeof(T), typeof(U));
                }
                Release();
            }

            public void Invoke<J, K, M>(J outArg1, K outArg2, M outArg3)
            {
                DelegateCallback<J, K, M, T, U> action = mCallback as DelegateCallback<J, K, M, T, U>;
                if (action != null)
                {
                    action.Invoke(outArg1, outArg2, outArg3, mArg1, mArg2);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(K), typeof(M), typeof(T), typeof(U));
                }
                Release();
            }

            public void Release()
            {
                lock (pool)
                {
                    if (pool.Count < POOL_SIZE_LIMIT)
                    {
                        pool.Enqueue(this);
                    }
#if UNITY_EDITOR
                    detecter.DecreseInstance();
                    detecter.SetPooledObjectCount(pool.Count);
#endif
                }
            }

            public void Dispose()
            {
                Release();
            }

            public void Cleanup()
            {
                lock (pool)
                {
                    pool.Clear();
                }
#if UNITY_EDITOR
                MemoryLeakDetecter.Remove(detecter);
#endif
            }
        }

        private class DelegateAction<T, U, V> : IDelegateBase
        {
            private const int POOL_SIZE_LIMIT = 100;
#if UNITY_EDITOR
            static MemoryLeakDetecter detecter = MemoryLeakDetecter.Add(typeof(DelegateAction<T, U, V>).FullName, 100, 100);
#endif
            static Queue<DelegateAction<T, U, V>> pool = new Queue<DelegateAction<T, U, V>>(32);
            public Delegate mCallback;
            public T mArg1;
            public U mArg2;
            public V mArg3;

            public static DelegateAction<T, U, V> Get()
            {
                lock (pool)
                {
#if UNITY_EDITOR
                    detecter.IncreseInstance();
#endif
                    if (pool.Count > 0)
                    {
                        return pool.Dequeue();
                    }
                }
                DelegateAction<T, U, V> instance = new DelegateAction<T, U, V>();
                BetterDelegate.Add(instance);
                return instance;
            }

            public void Invoke()
            {
                DelegateCallback<T, U, V> action = mCallback as DelegateCallback<T, U, V>;
                if (action != null)
                {
                    action.Invoke(mArg1, mArg2, mArg3);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(T), typeof(U), typeof(V));
                }
                Release();
            }

            public void Invoke<J>(J outArg1)
            {
                DelegateCallback<J, T, U, V> action = mCallback as DelegateCallback<J, T, U, V>;
                if (action != null)
                {
                    action.Invoke(outArg1, mArg1, mArg2, mArg3);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(T), typeof(U), typeof(V));
                }
                Release();
            }

            public void Invoke<J, K>(J outArg1, K outArg2)
            {
                DelegateCallback<J, K, T, U, V> action = mCallback as DelegateCallback<J, K, T, U, V>;
                if (action != null)
                {
                    action.Invoke(outArg1, outArg2, mArg1, mArg2, mArg3);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(K), typeof(T), typeof(U), typeof(V));
                }
                Release();
            }

            public void Invoke<J, K, M>(J outArg1, K outArg2, M outArg3)
            {
                DelegateCallback<J, K, M, T, U, V> action = mCallback as DelegateCallback<J, K, M, T, U, V>;
                if (action != null)
                {
                    action.Invoke(outArg1, outArg2, outArg3, mArg1, mArg2, mArg3);
                }
                else
                {
                    LogError(mCallback.GetType(), typeof(J), typeof(K), typeof(M), typeof(T), typeof(U), typeof(V));
                }
                Release();
            }

            public void Release()
            {
                lock (pool)
                {
                    if (pool.Count < POOL_SIZE_LIMIT)
                    {
                        pool.Enqueue(this);
                    }
#if UNITY_EDITOR
                    detecter.DecreseInstance();
                    detecter.SetPooledObjectCount(pool.Count);
#endif
                }
            }

            public void Dispose()
            {
                Release();
            }

            public void Cleanup()
            {
                lock (pool)
                {
                    pool.Clear();
                }
#if UNITY_EDITOR
                MemoryLeakDetecter.Remove(detecter);
#endif
            }
        }
    }

    public interface IDelegateAction : IDisposable
    {
        void Invoke();

        void Invoke<J>(J arg1);

        void Invoke<J, K>(J arg1, K arg2);

        void Invoke<J, K, M>(J arg1, K arg2, M arg3);
    }

    internal interface IDelegateBase : IDelegateAction
    {
        void Release();

        void Cleanup();
    }
}
