using UnityEngine;
using System.Collections;

public class Setting {
	public static int FPS = 60;
	public static string[] CONFIGS = {"base.xml"};

    private static string resourceUrl = string.Empty;
    private static string loginUrl = string.Empty;
    private static string reportErrorUrl = "http://123.59.60.234/logserver/index.php";
    private static string serverListUrl = string.Empty;
    private static string payUrl = string.Empty;
    private static string reportLoginUrl = string.Empty;
    private static string apkUrl = string.Empty;

    public static string START_UP_URL
    {
        get
        {
            return "http://192.168.1.246:8082/startup?";
        }
    }

    public static string SERVER_RESOURCE_ADDR
    {
        set
        {
            resourceUrl = value;
        }
        get
        {
            return resourceUrl;
        }
    }

    public static string APK_ADDR
    {
        set
        {
            apkUrl = value;
        }
        get
        {
            return apkUrl;
        }
    }

    public static string LOGIN_URL
    {
        set
        {
            loginUrl = value;
        }
        get
        {
            return loginUrl;
        }
    }

    public static string REPORT_ERROR_URL
    {
        set
        {
            reportErrorUrl = value;
        }
        get
        {
            return reportErrorUrl;
        }
    }

    public static string SERVER_LIST_URL
    {
        set
        {
            serverListUrl = value;
        }
        get
        {
            return serverListUrl;
        } 
    }

    public static string PAY_URL
    {
        set
        {
            payUrl = value;
        }
        get
        {
            return payUrl;
        }
    }

    public static string REPORT_LOGIN_URL
    {
        set
        {
            reportLoginUrl = value;
        }
        get
        {
            return reportLoginUrl;
        }
    }

    public static bool PerformaceDebug()
    {
        // 真机慎用，卡到想死
        return false;
    }
}
