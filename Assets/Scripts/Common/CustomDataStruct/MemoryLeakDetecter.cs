using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 说明：缓存池的内存泄漏检测工具
/// 
/// 注意：
///     1）inUsingCount溢出表示申请的缓存并没有被回收，结果等同于：缓存池失效，但对内存没有影响
///     2）poolCount溢出表示回收的对象不是从缓存池拿的，而是在其它地方创建的，对内存会有影响
///        由于缓存池持有引用会导致对象无法被GC，对于可能出现这种情况的缓存池，必须设置池容量，否则内存可能无限增长
/// 
/// by wsh @ 2017-07-03
/// </summary>

#if UNITY_EDITOR
namespace CustomDataStruct
{
    public sealed class MemoryLeakDetecter
    {
        internal static List<MemoryLeakDetecter> detecters = new List<MemoryLeakDetecter>();
        int USING_LIMIT = 1000;
        int POOL_LIMIT = 1000;
        int CHECK_PENDING_MS = 1000;//为了防止抖动，只有在这个时间内状态持续才认为内存已经泄露
        string MODULE_NAME = string.Empty;
        int inUsingCount = 0;
        int poolCount;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();
        long curStartTime = 0;
        long curStartTime2 = 0;
        bool hasUsingLeaks = false;

        private MemoryLeakDetecter()
        {
        }

        static public MemoryLeakDetecter Add(string moduleName, int usingLimit = 1000, int poolLimit = 1000, 
            int checkPendingMS = 1000)
        {
            MemoryLeakDetecter detecter = new MemoryLeakDetecter();
            detecter.USING_LIMIT = usingLimit;
            detecter.POOL_LIMIT = poolLimit;
            detecter.CHECK_PENDING_MS = checkPendingMS;
            detecter.MODULE_NAME = Helper.HandleTypeFullName(moduleName);
            detecters.Add(detecter);
            return detecter;
        }

        static public void Remove(MemoryLeakDetecter detecter)
        {
            if (detecter == null) return;
            for (int i = 0; i < detecters.Count; i++)
            {
                if (detecters[i] == detecter)
                {
                    detecters.RemoveAt(i);
                    break;
                }
            }
        }

        public void IncreseInstance()
        {
            inUsingCount++;
        }

        public void DecreseInstance()
        {
            inUsingCount--;
        }

        public void SetPooledObjectCount(int count)
        {
            poolCount = count;
        }

        internal void ClearUsingData()
        {
            inUsingCount = 0;
        }
        
        internal void Clear()
        {
            inUsingCount = 0;
            poolCount = 0;
            if (sw.IsRunning) sw.Stop();
            if (sw2.IsRunning) sw2.Stop();
            curStartTime = 0;
            curStartTime2 = 0;
        }

        internal void DetectMemoryLeaks()
        {
            if (Check(sw, inUsingCount >= USING_LIMIT, ref curStartTime))
            {
                Debug.LogError(string.Format("[{0}]inUsingCount = <{1}>, USING_LIMIT = <{2}>",
                    MODULE_NAME, inUsingCount, USING_LIMIT));
            }
            if (!hasUsingLeaks && Check(sw2, poolCount >= POOL_LIMIT, ref curStartTime2))
            {
                hasUsingLeaks = true;
                Debug.LogError(string.Format("[{0}]poolCount = <{1}>, POOL_LIMIT = <{2}>",
                    MODULE_NAME, poolCount, POOL_LIMIT));
            }
        }

        bool Check(System.Diagnostics.Stopwatch sw, bool pending, ref long startTime)
        {
            bool memoryLeaks = false;
            if (pending)
            {
                if (!sw.IsRunning)
                {
                    sw.Reset();
                    sw.Start();
                    startTime = sw.ElapsedMilliseconds;
                }
                else if (sw.ElapsedMilliseconds - startTime >= CHECK_PENDING_MS)
                {
                    memoryLeaks = true;
                    sw.Stop();
                }
            }
            else
            {
                if (sw.IsRunning) sw.Stop();
            }
            return memoryLeaks;
        }

        internal string ToLogString()
        {
            return string.Format("poolCount = <{0}>, inUsingCount = <{1}> <<===[{2}]",
                    poolCount, inUsingCount, MODULE_NAME);
        }

        public static void Cleanup()
        {
            for (int i = detecters.Count - 1; i >= 0; i--)
            {
                if (detecters[i] == null) detecters.RemoveAt(i);
                else detecters[i].Clear();
            }
        }
    }
}
#endif