using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomDataStruct
{
    static public class Helper
    {
#pragma warning disable 0414
        static CustomDataStructHelper helper =
            (new GameObject("CustomDataStructHelper")).AddComponent<CustomDataStructHelper>();
#pragma warning restore 0414

        static public void Startup()
        { 
        }

        static public void Cleanup()
        {
#if UNITY_EDITOR
            Debug.Log("CustomDataStruct Cleanup!");
#endif
            BetterDelegate.Cleanup();
            BetterStringBuilder.Cleanup();
            ValueObject.Cleanup();
            ObjPoolBase.Cleanup();
#if UNITY_EDITOR
            MemoryLeakDetecter.Cleanup();
#endif
        }

#if UNITY_EDITOR
        static public void ClearDetecterUsingData()
        {
            List<MemoryLeakDetecter> deteters = MemoryLeakDetecter.detecters;
            for (int i = 0; i < deteters.Count; i++)
            {
                deteters[i].ClearUsingData();
            }
        }
#endif

        static public string HandleTypeFullName(string name)
        {
            string[] list = name.Split(',');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                string cur = list[i];
                if (!cur.Contains("Assembly") &&
                    !cur.Contains("mscorlib") &&
                    !cur.Contains("Version") &&
                    !cur.Contains("Culture")
                    )
                {
                    if (cur.Contains("PublicKeyToken"))
                    {
                        int startIndex = cur.IndexOf(']');
                        if (startIndex >= 0)
                        {
                            sb.Append(cur.Substring(startIndex));
                        }
                    }
                    else
                    {
                        sb.Append(cur);
                    }
                }
            }
            return sb.ToString();
        }
    }

    sealed class CustomDataStructHelper : MonoBehaviour
    {
#if UNITY_EDITOR
        const float LOG_INTERVAL = 1.0f;
        public bool debug = true;
        public bool log = false;
        float nextLogTime = 0f;
#endif
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
            nextLogTime = Time.realtimeSinceStartup + LOG_INTERVAL;
#endif
        }

#if UNITY_EDITOR
        void Update()
        {
            if (debug)
            {
                List<MemoryLeakDetecter> deteters = MemoryLeakDetecter.detecters;
                for (int i = 0; i < deteters.Count; i++)
                {
                    deteters[i].DetectMemoryLeaks();
                }
            }

            log = debug ? log : debug;
            if (log && nextLogTime < Time.realtimeSinceStartup)
            {
                StringBuilder sb = new StringBuilder();
                nextLogTime = Time.realtimeSinceStartup + LOG_INTERVAL;
                List<MemoryLeakDetecter> deteters = MemoryLeakDetecter.detecters;
                for (int i = 0; i < deteters.Count; i++)
                {
                    sb.AppendLine(deteters[i].ToLogString());
                }
                Debug.Log(sb.ToString());
            }
        }
#endif

        public void OnDisable()
        {
            Helper.Cleanup();
        }
    }
}