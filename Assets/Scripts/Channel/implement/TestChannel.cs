namespace GameChannel
{
    public class TestChannel : BaseChannel
    {
        public override void Init()
        {
            ChannelManager.instance.InitSDKComplete("");
        }

        public override void Login()
        {
        }

        public override void Logout()
        { 
        
        }

        public override void ShowUserCenter(int serverId, string roleId)
        { 
        
        }

        public override void Pay(params object[] paramList)
        { 
        
        }

        public override void SubmitUserConfig(params object[] paramList)
        { 
        
        }

        public override string GetPackageName()
        {
            return "xluaframework";
        }

        public override bool IsInternalChannel()
        {
            return true;
        }

        public override void DownloadGame(params object[] paramList)
        {
        }

        public override string GetBundleID()
        {
            return "com.chivas.framework";
        }
    }
}
