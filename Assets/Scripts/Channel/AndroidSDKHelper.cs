using System;
using UnityEngine;

public class AndroidSDKHelper
{
    public static void FuncCall(string methodName, params object[] param)
    {
        #if UNITY_ANDROID
        try
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            if (jo != null)
            {
                jo.Call(methodName, param);
            }
        }
        catch (Exception ex)
        {
            Logger.Log("call sdk get exception methodName:" + methodName + " message: " + ex.Message);
        }
        #endif
    }
}
