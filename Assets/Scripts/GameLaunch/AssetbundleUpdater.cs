using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using XLua;
using GameChannel;
using System;

/// <summary>
/// added by wsh @ 2017.12.29
/// 功能：Assetbundle更新器
/// </summary>

[Hotfix]
[LuaCallCSharp]
public class AssetbundleUpdater : MonoBehaviour
{
    static int MAX_DOWNLOAD_NUM = 5;
    static int UPDATE_SIZE_LIMIT = 5 * 1024 * 1024;
    static string APK_FILE_PATH = "/sgc_{0}_{1}.apk";

    string noticeUrl = null;
    string resVersionPath = null;
    string noticeVersionPath = null;
    string clientResVersion = null;
    string serverResVersion = null;

    bool needDownloadGame = false;
    bool needUpdateGame = false;

    double timeStamp = 0;
    bool isDownloading = false;
    bool hasError = false;
    Manifest localManifest = null;
    Manifest hostManifest = null;
    List<string> needDownloadList = new List<string>();
    List<ResourceWebRequester> downloadingRequest = new List<ResourceWebRequester>();

    int downloadSize = 0;
    int totalDownloadCount = 0;
    int finishedDownloadCount = 0;

    Text statusText;
    Slider slider;

#if UNITY_EDITOR || CLIENT_DEBUG
#if !CLIENT_DEBUG
        [BlackList]
#endif
    // Hotfix测试---用于测试热更模块的热修复
    public void TestHotfix()
    {
        Logger.Log("********** AssetbundleUpdater : Call TestHotfix in cs...");
    }
#endif

    void Awake()
    {
        statusText = transform.Find("ContentRoot/LoadingDesc").GetComponent<Text>();
        slider = transform.Find("ContentRoot/SliderBar").GetComponent<Slider>();
        slider.gameObject.SetActive(false);
    }

    void Start ()
    {
        resVersionPath = AssetBundleUtility.GetPersistentDataPath(BuildUtils.ResVersionFileName);
        noticeVersionPath = AssetBundleUtility.GetPersistentDataPath(BuildUtils.NoticeVersionFileName);
        DateTime startDate = new DateTime(1970, 1, 1);
        timeStamp = (DateTime.Now - startDate).TotalMilliseconds;
        statusText.text = "正在检测资源更新...";
    }

    public void StartCheckUpdate()
    {
        StartCoroutine(CheckUpdateOrDownloadGame());
#if UNITY_EDITOR || CLIENT_DEBUG
        TestHotfix();
#endif
    }

    IEnumerator CheckUpdateOrDownloadGame()
    {
#if UNITY_EDITOR
        // EditorMode总是跳过资源更新
        if (AssetBundleConfig.IsEditorMode)
        {
            yield return StartGame();
            yield break;
        }
#if UNITY_5_5
        // 说明：亲测在Unity5.5版本本地服务器根本无法连接，倒是在手机上正常
        Logger.Log("No support simulate in Unity5.5 in windows...");
        yield return StartGame();
        yield break;
#endif
#endif
        yield return null;
        
        var start = DateTime.Now;
        yield return InitLocalVersion();
        Logger.Log(string.Format("InitLocalVersion use {0}ms", (DateTime.Now - start).Milliseconds));

        start = DateTime.Now;
        yield return InitSDK();
        Logger.Log(string.Format("InitSDK use {0}ms", (DateTime.Now - start).Milliseconds));

        serverResVersion = clientResVersion;
        if (ChannelManager.instance.IsInternalVersion())
        {
            // 内部版本不做大版本更新，不做公告，每次都检测资源更新
            yield return InternalGetUrlList();
            yield return CheckGameUpdate(true);
            yield return StartGame();
        }
        else
        {
            // 外部版本一律使用外网服务器更新
            yield return GetUrlList();
            if (needDownloadGame)
            {
                UINoticeTip.Instance.ShowOneButtonTip("游戏下载", "需要下载新的游戏版本！", "确定", null);
                yield return UINoticeTip.Instance.WaitForResponse();
                yield return DownloadGame();
            }
            else if (needUpdateGame)
            {
                yield return CheckGameUpdate(false);
                yield return StartGame();
            }
            else
            {
                yield return StartGame();
            }
        }
        yield break;
    }
    
    IEnumerator InitLocalVersion()
    {
        var resVersionRequest = AssetBundleManager.Instance.RequestAssetFileAsync(BuildUtils.ResVersionFileName);
        yield return resVersionRequest;
        var streamingResVersion = resVersionRequest.text;
        resVersionRequest.Dispose();
        var persistentResVersion = GameUtility.SafeReadAllText(resVersionPath);

        if (string.IsNullOrEmpty(persistentResVersion))
        {
            clientResVersion = streamingResVersion;
        }
        else
        {
            clientResVersion = BuildUtils.CheckIsNewVersion(streamingResVersion, persistentResVersion) ? persistentResVersion : streamingResVersion;
        }
        
        GameUtility.SafeWriteAllText(resVersionPath, clientResVersion);

        var persistentNoticeVersion = GameUtility.SafeReadAllText(noticeVersionPath);
        if (!string.IsNullOrEmpty(persistentNoticeVersion))
        {
            ChannelManager.instance.noticeVersion = persistentNoticeVersion;
        }
        else
        {
            ChannelManager.instance.noticeVersion = "1.0.0";
        }
        Logger.Log(string.Format("streamingResVersion = {0}, persistentResVersion = {1}, persistentNoticeVersion = {2}", streamingResVersion, persistentResVersion, persistentNoticeVersion));
        yield break;
    }

    IEnumerator InitSDK()
    {
        bool SDKInitComplete = false;
        ChannelManager.instance.InitSDK(() =>
        {
            SDKInitComplete = true;
        });
        yield return new WaitUntil(()=> {
            return SDKInitComplete;
        });
        yield break;
    }

    IEnumerator GetUrlList()
    {
        var args = string.Format("package={0}&app_version={1}&res_version={2}&notice_version={3}", ChannelManager.instance.packageName, ChannelManager.instance.appVersion, clientResVersion, ChannelManager.instance.noticeVersion);

        bool GetUrlListComplete = false;
        WWW www = null;
        SimpleHttp.HttpPost(Setting.START_UP_URL, null, DataUtils.StringToBytes(args), (WWW wwwInfo) => {
            www = wwwInfo;
            GetUrlListComplete = true;
        });
        yield return new WaitUntil(() =>
        {
            return GetUrlListComplete;
        });
        
        if (www == null || !string.IsNullOrEmpty(www.error) || www.bytes == null || www.bytes.Length == 0)
        {
            Logger.LogError("Get url list for args {0} with err : {1}", args, www == null ? "www null" : (!string.IsNullOrEmpty(www.error) ? www.error : "bytes length 0"));
            yield return GetUrlList();
        }

        var urlList = (Dictionary<string, object>)MiniJSON.Json.Deserialize(DataUtils.BytesToString(www.bytes));
        if (urlList == null)
        {
            Logger.LogError("Get url list for args {0} with err : {1}", args, "Deserialize url list null!");
            yield return GetUrlList();
        }

        if (urlList.ContainsKey("serverlist"))
        {
            Setting.SERVER_LIST_URL = urlList["serverlist"].ToString();
        }
        if (urlList.ContainsKey("verifying"))
        {
            Setting.LOGIN_URL = urlList["verifying"].ToString();
        }
        if (urlList.ContainsKey("logserver"))
        {
            Setting.REPORT_ERROR_URL = urlList["logserver"].ToString();
        }
        if (urlList.ContainsKey("res_version") && !string.IsNullOrEmpty(urlList["res_version"].ToString()))
        {
            serverResVersion = urlList["res_version"].ToString();
        }
        if (urlList.ContainsKey("notice_version") && !string.IsNullOrEmpty(urlList["notice_version"].ToString()))
        {
            ChannelManager.instance.noticeVersion = urlList["notice_version"].ToString();
            GameUtility.SafeWriteAllText(noticeVersionPath, ChannelManager.instance.noticeVersion);
        }
        if (urlList.ContainsKey("notice_url") && !string.IsNullOrEmpty(urlList["notice_url"].ToString()))
        {
            noticeUrl = urlList["notice_url"].ToString();
        }
        if (urlList.ContainsKey("app") && !string.IsNullOrEmpty(urlList["app"].ToString()))
        {
            Setting.APP_ADDR = urlList["app"].ToString();
            needDownloadGame = true;
        }
        else if (urlList.ContainsKey("res") && !string.IsNullOrEmpty(urlList["res"].ToString()))
        {
            Setting.SERVER_RESOURCE_ADDR = urlList["res"].ToString();
            needUpdateGame = true;
        }

#if UNITY_CLIENT || LOGGER_ON
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendFormat("SERVER_LIST_URL = {0}\n", Setting.SERVER_LIST_URL);
        sb.AppendFormat("LOGIN_URL = {0}\n", Setting.LOGIN_URL);
        sb.AppendFormat("REPORT_ERROR_URL = {0}\n", Setting.REPORT_ERROR_URL);
        sb.AppendFormat("NOTIFY_URL = {0}\n", Setting.NOTIFY_URL);
        sb.AppendFormat("NOTIFY_URL1 = {0}\n", Setting.NOTIFY_URL1);
        sb.AppendFormat("APP_ADDR = {0}\n", Setting.APP_ADDR);
        sb.AppendFormat("SERVER_RESOURCE_ADDR = {0}\n", Setting.SERVER_RESOURCE_ADDR);
        sb.AppendFormat("noticeVersion = {0}\n", ChannelManager.instance.noticeVersion);
        sb.AppendFormat("serverResVersion = {0}\n", serverResVersion);
        sb.AppendFormat("noticeUrl = {0}\n", noticeUrl);
        Logger.Log(sb.ToString());
#endif
        yield break;
    }

    IEnumerator DownloadGame()
    {
#if UNITY_ANDROID
        if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            UINoticeTip.Instance.ShowOneButtonTip("游戏下载", "当前为非Wifi网络环境，下载需要消耗手机流量，继续下载？", "确定", null);
            yield return UINoticeTip.Instance.WaitForResponse();
        }
        DownloadGameForAndroid();
#elif UNITY_IPHONE
        ChannelManager.instance.StartDownLoadGame(Setting.APP_ADDR);
#endif
        yield break;
    }

#if UNITY_ANDROID
    void DownloadGameForAndroid()
    {
        string bigServerVersion = string.Empty;
        string[] svList = serverResVersion.Split('.');
        if (svList.Length >= 3)
        {
            svList[2] = "0";
            bigServerVersion = string.Join(".", svList);
        }

        slider.normalizedValue = 0;
        slider.gameObject.SetActive(true);
        ChannelManager.instance.StartDownLoadGame(Setting.APP_ADDR, DownloadGameSuccess, DownloadGameFail, (int progress) =>
        {
            slider.normalizedValue = progress;
        }, string.Format(APK_FILE_PATH, ChannelManager.instance.packageName, bigServerVersion));
    }

    void DownloadGameSuccess()
    {
        UINoticeTip.Instance.ShowOneButtonTip("下载完毕", "游戏下载完毕，确认安装？", "安装", () =>
        {
            ChannelManager.instance.InstallGame(DownloadGameSuccess, DownloadGameFail);
        });
    }

    void DownloadGameFail()
    {
        UINoticeTip.Instance.ShowOneButtonTip("下载失败", "游戏下载失败！", "重试", () =>
        {
            DownloadGameForAndroid();
        });
    }
#endif

    
    private bool ShowUpdatePrompt(int downloadSize)
    {
        if (UPDATE_SIZE_LIMIT <= 0 && Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            // wifi不提示更新了
            return false;
        }

        if (downloadSize < UPDATE_SIZE_LIMIT)
        {
            return false;
        }

        return true;
    }

    IEnumerator GetDownloadAssetBundlesSize()
    {
        var request = AssetBundleManager.Instance.DownloadAssetBundleAsync(BuildUtils.AssetBundlesSizeFileName);
        yield return request;
        if (request.error != null)
        {
            UINoticeTip.Instance.ShowOneButtonTip("网络错误", "检测更新失败，请确认网络已经连接！", "重试", null);
            yield return UINoticeTip.Instance.WaitForResponse();
            Logger.LogError("Download host manifest :  " + request.assetbundleName + "\n from url : " + request.url + "\n err : " + request.error);
            request.Dispose();
            yield return GetDownloadAssetBundlesSize();
        }
        var content = request.text.Trim().Replace("\r","");
        request.Dispose();

        downloadSize = 0;
        var lines = content.Split('\n');
        var lookup = new Dictionary<string, int>();
        var separator = new[] { AssetBundleConfig.CommonMapPattren };
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                Logger.LogError("line empty!");
                continue;
            }

            var slices = line.Split(separator, StringSplitOptions.None);
            if (slices.Length < 2)
            {
                Logger.LogError("line split err : " + line);
                continue;
            }

            int size = 0;
            if (!int.TryParse(slices[1], out size))
            {
                Logger.LogError("size TryParse err : " + line);
            }
            lookup.Add(slices[0], size);
        }

        foreach (var assetbundle in needDownloadList)
        {
            int size = 0;
            if (!lookup.TryGetValue(assetbundle, out size))
            {
                Logger.LogError("no assetbundle size info : " + assetbundle);
            }
            downloadSize += size;
        }
        yield break;
    }

    IEnumerator CheckGameUpdate(bool isInternal)
    {
        // 检测资源更新
        Logger.Log("Resource download url : " + Setting.SERVER_RESOURCE_ADDR);
        var start = DateTime.Now;
        yield return CheckIfNeededUpdate(isInternal);
        Logger.Log(string.Format("CheckIfNeededUpdate use {0}ms", (DateTime.Now - start).Milliseconds));
        
        // Unity有个Bug会给空的名字，不记得在哪个版本修复了，这里强行过滤下
        for (int i = needDownloadList.Count - 1; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(needDownloadList[i]))
            {
                needDownloadList.RemoveAt(i);
            }
        }
        if (needDownloadList.Count <= 0)
        {
            Logger.Log("No resources to update...");
            yield return UpdateFinish();
            yield break;
        }
        
        start = DateTime.Now;
        yield return GetDownloadAssetBundlesSize();
        Logger.Log("GetDownloadAssetBundlesSize : {0}, use {1}ms", KBSizeToString(downloadSize), (DateTime.Now - start).Milliseconds);
        if (ShowUpdatePrompt(downloadSize) || isInternal)
        {
            UINoticeTip.Instance.ShowOneButtonTip("更新提示", string.Format("本次更新需要消耗{0}流量！", KBSizeToString(downloadSize)), "确定", null);
            yield return UINoticeTip.Instance.WaitForResponse();
        }

        statusText.text = "正在更新资源...";
        slider.normalizedValue = 0f;
        slider.gameObject.SetActive(true);
        totalDownloadCount = needDownloadList.Count;
        finishedDownloadCount = 0;
        Logger.Log(totalDownloadCount + " resources to update...");

        start = DateTime.Now;
        yield return StartUpdate();
        Logger.Log(string.Format("Update use {0}ms", (DateTime.Now - start).Milliseconds));
        
        slider.normalizedValue = 1.0f;
        start = DateTime.Now;
        yield return UpdateFinish();
        Logger.Log(string.Format("UpdateFinish use {0}ms", (DateTime.Now - start).Milliseconds));

        if (!string.IsNullOrEmpty(noticeUrl))
        {
            var url = noticeUrl + "?v" + timeStamp;
            var request = AssetBundleManager.Instance.DownloadWebResourceAsync(url);
            yield return request;
            if (request.error == null)
            {
                var path = AssetBundleUtility.GetPersistentDataPath(BuildUtils.UpdateNoticeFileName);
                GameUtility.SafeWriteAllText(path, request.text);
            }
            request.Dispose();
        }
        yield break;
    }

    IEnumerator InternalGetUrlList()
    {
        var resUrlRequest = AssetBundleManager.Instance.RequestAssetFileAsync(AssetBundleConfig.AssetBundleServerUrlFileName);
        yield return resUrlRequest;
        Setting.SERVER_RESOURCE_ADDR = resUrlRequest.text;
        resUrlRequest.Dispose();

        var resVersionRequest = AssetBundleManager.Instance.DownloadAssetFileAsync(BuildUtils.ResVersionFileName);
        yield return resVersionRequest;
        serverResVersion = resVersionRequest.text;
        resVersionRequest.Dispose();

        yield break;
    }

    IEnumerator GetHostManifest(string downloadManifestUrl,bool isInternal)
    {
        var request = AssetBundleManager.Instance.DownloadAssetBundleAsync(downloadManifestUrl);
        yield return request;
        if (!string.IsNullOrEmpty(request.error))
        {
            UINoticeTip.Instance.ShowOneButtonTip("网络错误", "检测更新失败，请确认网络已经连接！", "重试", null);
            yield return UINoticeTip.Instance.WaitForResponse();
            Logger.LogError("Download host manifest :  " + request.assetbundleName + "\n from url : " + request.url + "\n err : " + request.error);
            request.Dispose();
            if (isInternal)
            {
                // 内部版本本地服务器有问题直接跳过，不要卡住游戏
                yield break;
            }
            yield return GetHostManifest(downloadManifestUrl, isInternal);
        }

        var assetbundle = request.assetbundle;
        hostManifest.LoadFromAssetbundle(assetbundle);
        hostManifest.SaveBytes(request.bytes);
        assetbundle.Unload(false);
        request.Dispose();
        yield break;
    }

    IEnumerator CheckIfNeededUpdate(bool isInternal)
    {
        localManifest = AssetBundleManager.Instance.curManifest;
        hostManifest = new Manifest();

        string downloadManifestUrl = hostManifest.AssetbundleName;
        if (!isInternal)
        {
            downloadManifestUrl += ("?v" + timeStamp);
        }
        yield return GetHostManifest(downloadManifestUrl, isInternal);

        needDownloadList = localManifest.CompareTo(hostManifest);
        yield break;
    }

    IEnumerator StartUpdate()
    {
        downloadingRequest.Clear();
        isDownloading = true;
        hasError = false;
        yield return new WaitUntil(()=>
        {
            return isDownloading == false;
        });
        if (needDownloadList.Count > 0)
        {
            UINoticeTip.Instance.ShowOneButtonTip("网络错误", "游戏更新失败，请确认网络已经连接！", "重试", null);
            yield return UINoticeTip.Instance.WaitForResponse();
            yield return StartUpdate();
        }
        yield break;
    }

    IEnumerator UpdateFinish()
    {
        statusText.text = "正在准备资源...";

        // 保存服务器资源版本号与Manifest
        GameUtility.SafeWriteAllText(resVersionPath, serverResVersion);
        clientResVersion = serverResVersion;
        hostManifest.SaveToDiskCahce();
        
        // 重启资源管理器
        yield return AssetBundleManager.Instance.Cleanup();
        yield return AssetBundleManager.Instance.Initialize();

        // 重启Lua虚拟机
        string luaAssetbundleName = XLuaManager.Instance.AssetbundleName;
        AssetBundleManager.Instance.SetAssetBundleResident(luaAssetbundleName, true);
        var abloader = AssetBundleManager.Instance.LoadAssetBundleAsync(luaAssetbundleName);
        yield return abloader;
        abloader.Dispose();
        XLuaManager.Instance.Restart();
        XLuaManager.Instance.StartHotfix();
        yield break;
    }

    IEnumerator StartGame()
    {
        statusText.text = "正在准备资源...";
#if UNITY_EDITOR || CLIENT_DEBUG
        AssetBundleManager.Instance.TestHotfix();
#endif
        Logger.clientVerstion = clientResVersion;
        ChannelManager.instance.resVersion = clientResVersion;
        
        XLuaManager.Instance.StartGame();
        CustomDataStruct.Helper.Startup();
        UINoticeTip.Instance.DestroySelf();
        Destroy(gameObject, 0.5f);
        yield break;
    }
	
	void Update () {
        if (!isDownloading)
        {
            return;
        }

        for (int i = downloadingRequest.Count - 1; i >= 0; i--)
        {
            var request = downloadingRequest[i];
            if (request.isDone)
            {
                if (!string.IsNullOrEmpty(request.error))
                {
                    Logger.LogError("Error when downloading file : " + request.assetbundleName + "\n from url : " + request.url + "\n err : " + request.error);
                    hasError = true;
                    needDownloadList.Add(request.assetbundleName);
                }
                else
                {
                    // TODO：是否需要显示下载流量？
                    Logger.Log("Finish downloading file : " + request.assetbundleName + "\n from url : " + request.url);
                    downloadingRequest.RemoveAt(i);
                    finishedDownloadCount++;
                    var filePath = AssetBundleUtility.GetPersistentDataPath(request.assetbundleName);
                    GameUtility.SafeWriteAllBytes(filePath, request.bytes);
                }
                request.Dispose();
            }
        }

        if (!hasError)
        {
            while (downloadingRequest.Count < MAX_DOWNLOAD_NUM && needDownloadList.Count > 0)
            {
                var fileName = needDownloadList[needDownloadList.Count - 1];
                needDownloadList.RemoveAt(needDownloadList.Count - 1);
                var request = AssetBundleManager.Instance.DownloadAssetBundleAsync(fileName);
                downloadingRequest.Add(request);
            }
        }

        if (downloadingRequest.Count == 0)
        {
            isDownloading = false;
        }
        float progressSlice = 1.0f / totalDownloadCount;
        float progressValue = finishedDownloadCount * progressSlice;
        for (int i = 0; i < downloadingRequest.Count; i++)
        {
            progressValue += (progressSlice * downloadingRequest[i].progress);
        }
        slider.normalizedValue = progressValue;
    }

    private string KBSizeToString(int kbSize)
    {
        string sizeStr = string.Empty;
        if (kbSize >= 1024)
        {
            sizeStr = (kbSize / 1024.0f).ToString("0.0") + "M";
        }
        else
        {
            sizeStr = kbSize + "K";
        }

        return sizeStr;
    }
}
