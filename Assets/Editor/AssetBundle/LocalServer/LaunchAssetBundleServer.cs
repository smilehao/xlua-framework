using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AssetBundles
{
	internal class LaunchAssetBundleServer : ScriptableSingleton<LaunchAssetBundleServer>
	{

		[SerializeField]
		int mServerPID = 0;
        
        public static void CheckAndDoRunning()
        {
            WriteAssetBundleServerURL();

            bool needRunning = AssetBundleConfig.IsSimulateMode;
            bool isRunning = IsRunning();
            if (needRunning != isRunning)
            {
                KillRunningAssetBundleServer();
                if (needRunning)
                {
                    Run();
                }
            }
        }
        
		static bool IsRunning ()
		{
            if (instance.mServerPID == 0)
            {
                return false;
            }

            try
            {
                var process = Process.GetProcessById(instance.mServerPID);
                if (process == null)
                {
                    return false;
                }

                return !process.HasExited;
            }
            catch
            {
                return false;
            }
		}
        
		static void KillRunningAssetBundleServer ()
		{
			try
			{
				if (instance.mServerPID == 0)
					return;

				var lastProcess = Process.GetProcessById (instance.mServerPID);
				lastProcess.Kill();
				instance.mServerPID = 0;
                UnityEngine.Debug.Log("Local assetbundle server stop!");
            }
			catch
			{
			}
		}

		static void Run ()
		{
			string args = string.Format("\"{0}\" {1}", AssetBundleConfig.LocalSvrAppWorkPath, Process.GetCurrentProcess().Id);
            ProcessStartInfo startInfo = ExecuteInternalMono.GetProfileStartInfoForMono(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), GetMonoProfileVersion(), AssetBundleConfig.LocalSvrAppPath, args, true);
            startInfo.WorkingDirectory = AssetBundleConfig.LocalSvrAppWorkPath;
			startInfo.UseShellExecute = false;
			Process launchProcess = Process.Start(startInfo);
			if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
			{
                UnityEngine.Debug.LogError ("Unable Start AssetBundleServer process!");
			}
			else
			{
				instance.mServerPID = launchProcess.Id;
                UnityEngine.Debug.Log("Local assetbundle server run!");
            }
        }

        static string GetMonoProfileVersion()
        {
            string path = Path.Combine(Path.Combine(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), "lib"), "mono");

            string[] folders = Directory.GetDirectories(path);
            string[] foldersWithApi = folders.Where(f => f.Contains("-api")).ToArray();
            string profileVersion = "0";

            for (int i = 0; i < foldersWithApi.Length; i++)
            {
                foldersWithApi[i] = foldersWithApi[i].Split(Path.DirectorySeparatorChar).Last();
                foldersWithApi[i] = foldersWithApi[i].Split('-').First();
                
                if (string.Compare(foldersWithApi[i], profileVersion) > 0)
                {
                    profileVersion = foldersWithApi[i];
                }
            }

            return profileVersion;
        }

        public static string GetStreamingAssetBundleServerUrl()
        {
            string assetBundleServerUrl = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
            assetBundleServerUrl = Path.Combine(assetBundleServerUrl, AssetBundleConfig.AssetBundleServerUrlFileName);
            return assetBundleServerUrl;
        }

        public static void WriteAssetBundleServerURL()
        {
            var path = GetStreamingAssetBundleServerUrl();
            GameUtility.SafeWriteAllText(path, GetAssetBundleServerURL());
            AssetDatabase.Refresh();
        }

        public static void ClearAssetBundleServerURL()
        {
            var path = GetStreamingAssetBundleServerUrl();
            GameUtility.SafeDeleteFile(path);
            AssetDatabase.Refresh();
        }

        public static string GetAssetBundleServerURL()
        {
            string ip = PackageUtils.GetLocalServerIP();
            string downloadURL = "http://" + ip + ":7888/";
            downloadURL = downloadURL + PackageUtils.GetCurPlatformChannelRelativePath() + "/";
            return downloadURL;
        }
    }
}