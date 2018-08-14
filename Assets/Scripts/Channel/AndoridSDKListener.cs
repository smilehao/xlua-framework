using UnityEngine;

namespace GameChannel
{
    public class AndroidSDKListener : MonoSingleton<AndroidSDKListener>
    {
        private void InitCallback(string msg)
        {
            Logger.Log("InitSDKComplete with msg : " + msg);
            ChannelManager.instance.OnInitSDKCompleted(msg);
        }

        private void DownloadGameCallback(string msg)
        {
            Logger.Log("Download game with msg: " + msg);
            int result = -1;
            int.TryParse(msg, out result);
            ChannelManager.instance.OnDownloadGameFinished(result == 0);
        }

        private void DownloadGameProgressValueChangeCallback(string msg)
        {
            Logger.Log("Download game progress : " + msg);
            int progress = 0;
            int.TryParse(msg, out progress);
            ChannelManager.instance.OnDownloadGameProgressValueChange(progress);
        }

        private void InstallApkCallback(string msg)
        {
            Logger.Log("Install apk with msg: " + msg);
            int result = -1;
            int.TryParse(msg, out result);
            ChannelManager.instance.OnInstallGameFinished(result == 0);
        }

        private void LoginCallback(string msg)
        {
            Logger.Log("Login with msg : " + msg);
            ChannelManager.instance.OnLogin(msg);
        }

        private void LogoutCallback(string msg)
        {
            Logger.Log("Logout with msg : " + msg);
            ChannelManager.instance.OnLoginOut(msg);
        }

        private void PayCallback(string msg)
        {
            Logger.Log("SDKPay complete with msg : " + msg);
            ChannelManager.instance.OnSDKPay(msg);
        }
    }
}