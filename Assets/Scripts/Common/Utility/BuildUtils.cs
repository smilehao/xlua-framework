using UnityEngine;

/// <summary>
/// added by wsh @ 2018.01.04
/// 功能：构建相关配置和通用函数
/// </summary>

public class BuildUtils
{
    public const string PackageNameFileName = "package_name.bytes";
    public const string AppVersionFileName = "app_version.bytes";
    public const string ResVersionFileName = "res_version.bytes";
    public const string NoticeVersionFileName = "notice_version.bytes";
    public const string AssetBundlesSizeFileName = "assetbundls_size.bytes";
    public const string UpdateNoticeFileName = "update_notice.bytes";

    public static bool CheckIsNewVersion(string sourceVersion, string targetVersion)
    {
        string[] sVerList = sourceVersion.Split('.');
        string[] tVerList = targetVersion.Split('.');

        if (sVerList.Length >= 3 && tVerList.Length >= 3)
        {
            try
            {
                int sV0 = int.Parse(sVerList[0]);
                int sV1 = int.Parse(sVerList[1]);
                int sV2 = int.Parse(sVerList[2]);
                int tV0 = int.Parse(tVerList[0]);
                int tV1 = int.Parse(tVerList[1]);
                int tV2 = int.Parse(tVerList[2]);

                if (tV0 > sV0)
                {
                    return true;
                }
                else if (tV0 < sV0)
                {
                    return false;
                }

                if (tV1 > sV1)
                {
                    return true;
                }
                else if (tV1 < sV1)
                {
                    return false;
                }

                if (tV2 > sV2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogError(string.Format("parse version error. clientversion: {0} serverversion: {1}\n {2}\n{3}", sourceVersion, targetVersion, ex.Message, ex.StackTrace));
                return false;
            }
        }

        return false;
    }
}
