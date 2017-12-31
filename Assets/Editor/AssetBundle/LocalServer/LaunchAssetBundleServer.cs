using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AssetBundles
{
	internal class LaunchAssetBundleServer : ScriptableSingleton<LaunchAssetBundleServer>
	{

		[SerializeField]
		int mServerPID = 0;
        
        public static void CheckAndDoRunning()
        {
            bool needRunning = AssetBundleConfig.IsSimulateMode;
            bool isRunning = IsRunning();
            if (needRunning != isRunning)
            {
                if (needRunning)
                {
                    Run();
                }
                else
                {
                    KillRunningAssetBundleServer();
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
			KillRunningAssetBundleServer();
			AssetBundleUtility.WriteAssetBundleServerURL();

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
    }
}