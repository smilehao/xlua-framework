using System;

/// <summary>
/// 说明：内部测试渠道
/// 
/// 注意：
/// 1、Unity版本5.3.4f1，使用andorid sdk没有任何问题
/// 2、Unity版本5.3.4f1以上，Unity2017版本以下，需要改AndroidManifest中的android:targetSdkVersion值为26
/// 3、Unity2017以上版本，构建报错，目前还没找到解决方案
/// 
/// @by wsh 2018-08-16
/// </summary>

namespace GameChannel
{
    public class TestChannel : BaseChannel
    {
        public override void Init()
        {
#if UNITY_2017_1_OR_NEWER
            ChannelManager.instance.OnInitSDKCompleted("No use android sdk !!!");
#else
            AndroidSDKHelper.FuncCall("TestChannelInit");
#endif
        }

        public override string GetBundleID()
        {
            return "com.chivas.framework";
        }

        public override string GetProductName()
        {
            return "xluaframework";
        }

        public override bool IsInternalChannel()
        {
            return true;
        }

        public override void DownloadGame(params object[] paramList)
        {
            string url = paramList[0] as string;
            string saveName = paramList[1] as string;
#if UNITY_2017_1_OR_NEWER
            Logger.LogError("No support download game !!!");
#else
            AndroidSDKHelper.FuncCall("DownloadGame", url, saveName);
#endif
        }

        public override void InstallApk()
        {
#if UNITY_2017_1_OR_NEWER
            Logger.LogError("No support install game !!!");
#else
            AndroidSDKHelper.FuncCall("InstallApk");
#endif
        }

        public override void Login()
        {
        }

        public override void Logout()
        { 
        
        }
        
        public override void Pay(params object[] paramList)
        { 
        
        }
    }
}
