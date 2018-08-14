namespace GameChannel
{
    public abstract class BaseChannel
    {
        public abstract void Init();
        
        public virtual void DataTrackInit()
        {
        }

        public virtual string GetCompanyName()
        {
            return "chivas";
        }

        public abstract string GetBundleID();

        public abstract string GetProductName();

        public virtual bool IsInternalChannel()
        {
            return false;
        }

        public virtual bool IsGooglePlay()
        {
            return false;
        }

        public abstract void DownloadGame(params object[] paramList);

        public abstract void InstallApk();

        public abstract void Login();

        public abstract void Logout();
        
        public abstract void Pay(params object[] paramList);
        
    }
}
