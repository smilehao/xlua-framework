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

        public string channelName
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

        public void Init(string channelName)
        {
            this.channelName = channelName;
            channel = CreateChannel(channelName);
        }
        
        public BaseChannel CreateChannel(string channelName)
        {
            ChannelType platName = (ChannelType)Enum.Parse(typeof(ChannelType), channelName);
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
            Logger.platChannel = channelName;

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

        public string GetProductName()
        {
            if (channel == null)
            {
                return "xluaframework";
            }
            return channel.GetProductName();
        }

        public bool IsGooglePlay()
        {
            if (channel == null)
            {
                return false;
            }
            return channel.IsGooglePlay();
        }

        public override void Dispose()
        {
        }
    }
}
