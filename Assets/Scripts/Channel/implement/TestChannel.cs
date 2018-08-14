using System;

namespace GameChannel
{
    public class TestChannel : BaseChannel
    {
        public override void Init()
        {
            AndroidSDKHelper.FuncCall("TestChannelInit");
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
            AndroidSDKHelper.FuncCall("DownloadGame", url, saveName);
        }

        public override void InstallApk()
        {
            AndroidSDKHelper.FuncCall("InstallApk");
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
