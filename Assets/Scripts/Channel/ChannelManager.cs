using System;
using XLua;

namespace GameChannel
{
    [Hotfix]
    [LuaCallCSharp]
    public class ChannelManager : Singleton<ChannelManager>
    {
        private BaseChannel channel = null;

        private Action initDelFun = null;
        public Action downLoadGameSucceed = null;
        public Action downLoadGameFail = null;
        public Action<int> downLoadGameProgress = null;

        public string packageName
        {
            get;
            protected set;
        }
        
        public string noticeVersion
        {
            get;
            set;
        }

        public string resVersion
        {
            get;
            set;
        }

        public string appVersion
        {
            get;
            set;
        }

        public string svnVersion
        {
            get;
            set;
        }

        public void Init(string packageName)
        {
            this.packageName = packageName;
            channel = CreateChannel(packageName);
        }
        
        public BaseChannel CreateChannel(string packageName)
        {
            ChannelType platName = (ChannelType)Enum.Parse(typeof(ChannelType), packageName);
            switch ((platName))
            {
                case ChannelType.Test:
                    return new TestChannel();
                default:
                    return new TestChannel();
            }
        }

        public void InitSDK(Action delFun)
        {
            initDelFun = delFun;

            channel.Init();
            channel.DataTrackInit();
        }

        public void InitSDKComplete(string msg)
        {
            Logger.platChannel = packageName;

            if (initDelFun != null)
            {
                initDelFun.Invoke();
                initDelFun = null;
            }
        }
        
        public void StartDownLoadGame(string url, Action succeed = null, Action fail = null, Action<int> progress = null, string saveName = null)
        {
            downLoadGameSucceed = succeed;
            downLoadGameFail = fail;
            downLoadGameProgress = progress;
            channel.DownloadGame(url, saveName);
        }

        public void DownLoadGameEnd(bool succeed)
        {
            if (succeed)
            {
                if (downLoadGameSucceed != null)
                {
                    downLoadGameSucceed.Invoke();
                }
            }
            else
            {
                if (downLoadGameFail != null)
                {
                    downLoadGameFail.Invoke();
                }
            }

            downLoadGameSucceed = null;
            downLoadGameFail = null;
            downLoadGameProgress = null;
        }

        public void DownLoadGameProgress(int progress)
        {
            if (downLoadGameProgress != null)
            {
                downLoadGameProgress.Invoke(progress);
            }
        }

        public void InstallGame(Action succeed, Action fail)
        {
            downLoadGameSucceed = succeed;
            downLoadGameFail = fail;
            AndroidSDKHelper.FuncCall("InstallApk");
        }

        public bool IsInternalVersion()
        {
            if (channel == null)
            {
                return true;
            }
            return channel.IsInternalChannel();
        }
        
        public override void Dispose()
        {
        }
    }
}
