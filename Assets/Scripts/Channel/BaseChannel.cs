namespace GameChannel
{
    public abstract class BaseChannel
    {
        public abstract void Init();

        public abstract void Login();

        public abstract void Logout();

        public abstract void ShowUserCenter(int serverId, string roleId);

        public abstract void Pay(params object[] paramList);

        public abstract void SubmitUserConfig(params object[] paramList);

        public abstract void DownloadGame(params object[] paramList);

        public abstract string GetBundleID();
        
        public abstract string GetPackageName();
        
        public virtual bool IsInternalChannel()
        {
            return false;
        }
        
        public virtual void DataTrackInit()
        { 
        }
        
        public virtual bool IsGooglePlay()
        {
            return false;
        }
    }
}
