using UnityEngine;
using System.Collections.Generic;

public class LoggerHelper : MonoBehaviour
{
    public enum LOG_TYPE
    {
        LOG = 0,
        LOG_ERR,
    }

    struct log_info
    {
        public LOG_TYPE type;
        public string msg;

        public log_info(LOG_TYPE type, string msg)
        {
            this.type = type;
            this.msg = msg;
        }
    }

    private static LoggerHelper _instance = null;
    private List<log_info> logList = new List<log_info>(100);
    private List<log_info> tmpLogList = new List<log_info>(100);

    public static LoggerHelper instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(LoggerHelper)) as LoggerHelper;
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(LoggerHelper).Name);
                    _instance = go.AddComponent<LoggerHelper>();
                    GameObject parent = GameObject.Find("Boot");
                    if (parent != null)
                    {
                        go.transform.parent = parent.transform;
                    }
                }
            }

            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Startup()
    {
    }

    private void Update()
    {
        lock (logList)
        {
            if (logList.Count > 0)
            {
                tmpLogList.Clear();
                for (int i = 0; i < logList.Count; i++)
                {
                    tmpLogList.Add(logList[i]);
                }
                logList.Clear();
            }
        }

        if (tmpLogList.Count > 0)
        {
            for (int i = 0; i < tmpLogList.Count; i++)
            {
                var logInfo = tmpLogList[i];
                switch (logInfo.type)
                {
                    case LOG_TYPE.LOG:
                        {
                            Logger.Log(logInfo.msg, null);
                            break;
                        }
                    case LOG_TYPE.LOG_ERR:
                        {
                            Logger.LogError(logInfo.msg, null);
                            break;
                        }
                }
            }
            tmpLogList.Clear();
        }
    }

    public void DestroySelf()
    {
        //Dispose
        _instance = null;
        Destroy(gameObject);

        lock (logList)
        {
            logList.Clear();
        }
        tmpLogList.Clear();
    }

    public void LogToMainThread(LOG_TYPE type, string msg)
    {
        lock (logList)
        {
            logList.Add(new log_info(type, msg));
        }
    }
}
