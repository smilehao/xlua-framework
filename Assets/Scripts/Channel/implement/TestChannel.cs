using System;

namespace GameChannel
{
    public class TestChannel : BaseChannel
    {
        public override void Init()
        {
            // TODO：
            //AndroidSDKHelper.FuncCall("Init");
            ChannelManager.instance.OnInitSDKCompleted("");
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
            // TODO：
            //AndroidSDKHelper.FuncCall("DownLoadGame", url, saveName);
            ChannelManager.instance.OnDownloadGameFinished(true);
        }

        public override void InstallApk()
        {
            // TODO：
            //AndroidSDKHelper.FuncCall("InstallApk");
            ChannelManager.instance.OnInstallGameFinished(true);
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
