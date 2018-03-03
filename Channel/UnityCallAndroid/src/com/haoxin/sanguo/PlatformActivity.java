package com.haoxin.sanguo;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.text.MessageFormat;

import org.json.JSONObject;

import android.annotation.SuppressLint;
import android.content.Intent;
import android.content.res.Configuration;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.util.Log;

import com.hxsdk.wrapper.common.HXMessageCode;
import com.hxsdk.wrapper.data.*;
import com.hxsdk.wrapper.impl.HXWrapper;
import com.hxsdk.wrapper.listener.HXResultListener;
import com.tencent.mm.sdk.modelmsg.SendMessageToWX;
import com.tencent.mm.sdk.modelmsg.WXImageObject;
import com.tencent.mm.sdk.modelmsg.WXMediaMessage;
import com.tencent.mm.sdk.openapi.IWXAPI;
import com.unity3d.player.UnityPlayer;

public class PlatformActivity extends MainActivity
{
	private Handler m_downloadHandler;
	/* ������ */
    private static final int DOWNLOAD = 1;
    /* ���ؽ��� */
    private static final int DOWNLOAD_FINISH = 2;
    
    private int m_progress;
    private String m_url;
    private String m_savePath;
    private String m_saveName;
    
    private IWXAPI m_wxAPI = null;
    private String m_wxAppID = "";
    
    private static PlatformActivity m_instacne = null;
    
    public static PlatformActivity getInstance()
    {
    	return m_instacne;
    }
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		m_instacne = this;
		
		HXWrapper.getInstance().init(this, new HXResultListener<String>() {
			
			@Override
			public void dispatchResult(int arg0, String arg1, String arg2) {
				if (arg0 == HXMessageCode.LOGOUT_SUCCESS){
					UnityPlayer.UnitySendMessage("PlatformListener", "LogoutCallback", "");
				}
				else if (arg0 == HXMessageCode.PAY_SUCCESS){
					UnityPlayer.UnitySendMessage("PlatformListener", "PayCallback", "ret=0");
				}
			}
		});
	}
	
	public void onDestroy() {
		super.onDestroy();
		HXWrapper.getInstance().onDestroy(this);
	}
	
	@Override
	protected void onResume()
	{
		super.onResume();
		HXWrapper.getInstance().onResume(this);
	}
	
	@Override
	protected void onPause()
	{
		super.onPause();
		HXWrapper.getInstance().onPause(this);
	}
	
	@Override
	protected void onStart()
	{
		super.onStart();
		HXWrapper.getInstance().onStart(this);
	}
	
	@Override
	protected void onStop()
	{
		super.onStop();
		HXWrapper.getInstance().onStop(this);
	}
	
	@Override
	protected void onNewIntent(Intent intent)
	{
		super.onNewIntent(intent);
		HXWrapper.getInstance().onNewIntent(intent);
	}
	
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		super.onActivityResult(requestCode, resultCode, data);
		HXWrapper.getInstance().onActivityResult(this, requestCode, resultCode, data);
	}
	
	@Override
	public void onConfigurationChanged(Configuration arg0)
	{
		super.onConfigurationChanged(arg0);
		HXWrapper.getInstance().onConfigurationChanged(this);
	}
	
	public void HXPay(final String channelUserId, final int moneyAmount, final String productName, final String productId, final String exchangeRate, 
			final String notifyUri, final String appName, final String appUserName, final String appUserId, final String appUserLevel, 
			final String appOrderId, final String serverId, final String payExt1, final String payExt2, final String submitTime)
	{
		runOnUiThread(new Runnable() {
		
		@Override
		public void run() {
			runPay(channelUserId, moneyAmount, productName, productId, exchangeRate, notifyUri, 
					appName, appUserName, appUserId, appUserLevel, appOrderId, serverId, payExt1, payExt2, submitTime);
		}
		});
	}

	private void runPay(String channelUserId, int moneyAmount, String productName, String productId, String exchangeRate, 
				String notifyUri, String appName, String appUserName, String appUserId, String appUserLevel, 
				String appOrderId, String serverId, String payExt1, String payExt2, String submitTime)
	{
		HXPayData data = new HXPayData();

		data.setAppExt1(payExt1);
		data.setAppExt2(payExt2);
		data.setPlatUserId(channelUserId);
		data.setMoneyAmount(moneyAmount);
		data.setProductName(productName);
		data.setProductId(productId);
		data.setExchangeRate(exchangeRate);
		data.setNotifyUri(notifyUri);
		data.setAppName(appName);
		data.setAppUserName(appUserName);
		data.setAppUserId(appUserId);
		data.setAppUserLevel(appUserLevel);
		data.setProductCount(1);
		data.setServerId(serverId);
		data.setTransactionNumCP(appOrderId);
		data.setCpInfo(appOrderId);
		data.setSubmitTime(submitTime);
		
		Log.d("debug", "hjsdk start pay");
		Log.d("debug", "channelUserId:" + channelUserId);
		Log.d("debug", "moneyAmount:" + moneyAmount);
		Log.d("debug", "productId:" + productId);
		Log.d("debug", "productName:" + productName);
		Log.d("debug", "exchangeRate:" + exchangeRate);
		Log.d("debug", "notifyUri:" + notifyUri);
		Log.d("debug", "appName:" + appName);
		Log.d("debug", "appUserName:" + appUserName);
		Log.d("debug", "appUserId:" + appUserId);
		Log.d("debug", "appUserLevel:" + appUserLevel);
		Log.d("debug", "appOrderId:" + appOrderId);
		Log.d("debug", "serverId:" + serverId);
		Log.d("debug", "payExt1:" + payExt1);
		Log.d("debug", "payExt2:" + payExt2);
		Log.d("debug", "submitTime:" + submitTime);
	
		HXWrapper.getInstance().pay(this, data, new HXResultListener<HXOrderData>() {

			@Override
			public void dispatchResult(int arg0, String arg1, HXOrderData arg2) {
				int ret = -1;
				if (arg0 == HXMessageCode.PAY_SUCCESS)
				{
					ret = 0;
				}
				UnityPlayer.UnitySendMessage("PlatformListener", 
											"PayCallback", 
											MessageFormat.format("ret={0}", ret));
			}
		});
	}
	
	public void DownLoadGame(String url, String saveName)
	{
		m_url = url;
		m_saveName = saveName;
		
		runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				runDownLoadGame();
			}
		});
	}
	
	@SuppressLint("HandlerLeak")
	private void runDownLoadGame()
	{
		m_downloadHandler = new Handler()
		{
			public void handleMessage(Message msg)
	        {
	            switch (msg.what)
	            {
	            // ��������
	            case DOWNLOAD:
	                // ���ý�����λ��
	            	UnityPlayer.UnitySendMessage("PlatformListener", "DownLoadGameProgressCallback", String.valueOf(m_progress));
	                break;
	            case DOWNLOAD_FINISH:
	                // ��װ�ļ�
	            	InstallApk();
	                break;
	            default:
	                break;
	            }
	        };
		};
		
		new downloadApkThread().start();
	}
	
	private class downloadApkThread extends Thread
    {
        @Override
        public void run()
        {
            try
            {
                // �ж�SD���Ƿ���ڣ������Ƿ���ж�дȨ��
                if (Environment.getExternalStorageState().equals(Environment.MEDIA_MOUNTED))
                {
                    // ��ô洢����·��
                    String sdpath = Environment.getExternalStorageDirectory() + "/";
                    m_savePath = sdpath + "Download";
                    File file = new File(m_savePath);
                    // �ж��ļ�Ŀ¼�Ƿ����
                    if (!file.exists())
                    {
                        file.mkdir();
                    }
                    
                    File apkFile = new File(m_savePath, m_saveName);
                    
                    if (apkFile.exists())
                    {
                    	m_downloadHandler.sendEmptyMessage(DOWNLOAD_FINISH);
                    	return;
                    }
                    
                    File tmpFile = new File(m_savePath, m_saveName + ".tmp");
                    int loadedLength = (int)tmpFile.length();
                    
                    URL url = new URL(m_url);
                    // ��������
                    HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                    conn.setRequestProperty("range", "bytes=" + loadedLength + "-");
                    conn.connect();
                    // ��ȡ�ļ���С
                    int length = conn.getContentLength();
                    length += loadedLength;
                    // ����������
                    InputStream is = conn.getInputStream();

                    FileOutputStream fos = new FileOutputStream(tmpFile, tmpFile.exists());
                    int count = loadedLength;
                    // ����
                    byte buf[] = new byte[1024];
                    m_progress = 0;
                    m_downloadHandler.sendEmptyMessage(DOWNLOAD);
                    // д�뵽�ļ���
                    while (true)
                    {
                        int numread = is.read(buf);
                        count += numread;
                        // ���������λ��
                        int newProgress = (int) (((float) count / length) * 100);
                        if (newProgress > m_progress)
                        {
                        	m_progress = newProgress;
                        	m_downloadHandler.sendEmptyMessage(DOWNLOAD);
                        }
                        if (numread <= 0)
                        {
                            // �������
                            break;
                        }
                        // д���ļ�
                        fos.write(buf, 0, numread);
                    }
                    fos.close();
                    is.close();
                    
                    tmpFile.renameTo(apkFile);
                    m_downloadHandler.sendEmptyMessage(DOWNLOAD_FINISH);
                }
                else
                {
                	UnityPlayer.UnitySendMessage("PlatformListener", "DownLoadGameCallback", "-1");
                }
            }
            catch (Exception e)
            {
                e.printStackTrace();
                UnityPlayer.UnitySendMessage("PlatformListener", "DownLoadGameCallback", "-1");
            }
        }
    };

	
	public void InstallApk()
	{
		File apkfile = new File(m_savePath, m_saveName);
        if (!apkfile.exists())
        {
            return;
        }
        // ͨ��Intent��װAPK�ļ�
        Intent i = new Intent(Intent.ACTION_VIEW);
        i.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        i.setDataAndType(Uri.parse("file://" + apkfile.toString()), "application/vnd.android.package-archive");
        this.startActivity(i);

        UnityPlayer.UnitySendMessage("PlatformListener", "DownLoadGameCallback", "0");
	}
	
	public void StartWebView(String url)
	{
		Log.d("debug", "start open webview");
		Intent intent = new Intent(this, WebViewActivity.class);
		intent.putExtra("url", url);
		this.startActivity(intent);
	}
	
	public void WXInit(String appID)
	{
		m_wxAppID = appID;
		//m_wxAPI = WXAPIFactory.createWXAPI(this, appID, true);
		//m_wxAPI.registerApp(appID);
	}
	
	public String GetWXAppID()
	{
		return m_wxAppID;
	}
	
	public void WXShareImage(String imageUrl)
	{
		String path = Environment.getExternalStorageDirectory() + "/haoxin/" + this.getPackageName() + "/images";
		File file = new File(path);
		if (!file.exists())
		{
			file.mkdirs();
		}
		
		String imageName = StrToMd5(imageUrl) + ".jpg";
		String imagePath = path + "/" + imageName;
		File imageFile = new File(imagePath);
		
		try
		{
			URL url = new URL(imageUrl);
            // ��������
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.connect();
            // ����������
            InputStream is = conn.getInputStream();

            FileOutputStream fos = new FileOutputStream(imageFile);
            // ����
            byte buf[] = new byte[1024];
            for (int len = is.read(buf); len > 0; len = is.read(buf)) 
            {
                fos.write(buf, 0, len);
            }
            fos.flush();
            is.close();
            fos.close();
		}
		catch (Exception e)
		{
			
		}
		
		WXImageObject imageObj = new WXImageObject();
		imageObj.imageUrl = "http://ylws.gaeamobile.net";
		imageObj.imagePath = imagePath;
		
		WXMediaMessage message = new WXMediaMessage();
		message.mediaObject = imageObj;
		
		SendMessageToWX.Req req = new SendMessageToWX.Req();
		req.message = message;
		req.scene = SendMessageToWX.Req.WXSceneTimeline;
		req.transaction = System.currentTimeMillis() + "";
		m_wxAPI.sendReq(req);
	}
	
	private String StrToMd5(String str) 
	{  
        try 
        {  
            MessageDigest md = MessageDigest.getInstance("MD5");  
            md.update(str.getBytes());  
            byte b[] = md.digest();  
  
            int i = 0;  
  
            StringBuffer buf = new StringBuffer("");  
            for (int offset = 0; offset < b.length; offset++) 
            {  
                i = b[offset];  
                if (i < 0)  
                    i += 256;  
                if (i < 16)  
                    buf.append("0");  
                buf.append(Integer.toHexString(i));  
            }  
            //32λ����  
            return buf.toString();  
        } 
        catch (NoSuchAlgorithmException e) 
        {  
            e.printStackTrace();  
            return null;  
       }  
  
    }

	public void XGRegister(String account)
	{
		//XGPushManager.registerPush(getApplicationContext(), account);
	}
	
	public void HXInit()
	{
		String platformName = HXWrapper.getInstance().getPlatform();
		
		UnityPlayer.UnitySendMessage("PlatformListener", "InitCallback", platformName);
	}
	
	public void HXLogin()
	{
		runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				runLogin();
			}
		});
	}
	
	private void runLogin()
	{
		HXWrapper.getInstance().login(this, new HXResultListener<HXUserData>(){
			@Override
			public void dispatchResult(int nonsupport, String message, HXUserData data) {
				if (data == null)
				{
					return;
				}
				UnityPlayer.UnitySendMessage("PlatformListener", 
											"LoginCallback", 
											MessageFormat.format("platform_id={0}&token={1}&ext={2}", data.getOpenId(), data.getToken(), data.getExt()));
			}
		});
	}
	
	public void HXSwitchAccount()
	{
		runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				runSwitchAccount();
			}
		});
	}
	
	private void runSwitchAccount()
	{
		HXWrapper.getInstance().switchAccount(this, new HXResultListener<HXUserData>(){
			@Override
			public void dispatchResult(int nonsupport, String message, HXUserData data) {
				if (data == null)
				{
					return;
				}
				UnityPlayer.UnitySendMessage("PlatformListener", 
											"LoginCallback", 
											MessageFormat.format("platform_id={0}&token={1}&ext={2}", data.getOpenId(), data.getToken(), data.getExt()));
			}
		});
	}
	
	public void HXSubmitUserConfig(String roleId, String roleName, String roleLevel, int zoneId, String zoneName, int registerTime, int currentTime, String type)
	{
		Log.d("debug", "HJLoginSubmitExtendData start");
		Log.d("debug", "roleId:" + roleId);
		Log.d("debug", "roleName:" + roleName);
		Log.d("debug", "roleLevel:" + roleLevel);
		Log.d("debug", "zoneId:" + zoneId);
		Log.d("debug", "zoneName:" + zoneName);
		Log.d("debug", "registerTime:" + registerTime);
		Log.d("debug", "currentTime:" + currentTime);
		Log.d("debug", "type:" + type);
		
		try
		{
			JSONObject json = new JSONObject();
			json.put("roleId", roleId);
			json.put("roleName", roleName);
			json.put("roleLevel", roleLevel);
			json.put("zoneId", zoneId);
			json.put("zoneName", zoneName);
			json.put("roleCTime", registerTime);
			json.put("roleLevelMTime", currentTime);
			HXWrapper.getInstance().SubmitUserConfig(this, type, json);
		}
		catch (Exception e)
		{
			
		}
	}
	
	public void HXExitGame()
	{
		runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				runExitGame();
			}
		});
	}
	
	private void runExitGame()
	{
		HXWrapper.getInstance().exitGame(this);
	}
}
