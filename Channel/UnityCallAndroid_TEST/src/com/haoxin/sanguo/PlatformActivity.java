package com.haoxin.sanguo;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.text.MessageFormat;

import android.R.integer;
import android.annotation.SuppressLint;
import android.content.Intent;
import android.content.res.Configuration;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.util.Log;

import com.haoxin.sdk.HaoXinLoginCallback;
import com.haoxin.sdk.HaoXinLogoutCallback;
import com.haoxin.sdk.HaoXinManager;
import com.haoxin.sdk.LoginCallBackData;
import com.iapppay.interfaces.callback.IPayResultCallback;
import com.iapppay.sdk.main.IAppPay;
import com.iapppay.sdk.main.IAppPayOrderUtils;
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
    
    private String m_wxAppID = "";
    private String iappayId = "3014541103";
    private String iappayPrivateKey = "MIICXAIBAAKBgQCAl/hNh28f2fFxflWisi7isfPl6QMY1AwNMIeNAXKJD+B2jM3TzUZcs2v/h8Vft2XTVVFtu1nBYIGLGvkkmAXtnr1mpHK72LsaVKcwNOxv4IzMpOM6nt+Q17KfNvkfG5EIKGOu6U0Rb0e4qe9/m0ftvmpur+/BHhlVDSXRngBtjQIDAQABAoGAA9H/+3WplH2qEaAaNTIr+Gom/86TW/p4vS+S51qCp5XEKmF2f/NaQsjFzZqf/374VHX1bFgji34tew97FV461tBGH35H73axAiiatcLxhvCtiB3Clkvp7r+4CWW0ra0aGBHuri9UkGz9jLGRgZt+qAw7SICLK4S6ijn5yTocT1UCQQDnN1CIN364QwtZiXpFc71sOWYs++yYwZf2wtm8LIiVpfkNqOTSxlL4FUKkdsVeWQORTRJfknYGRnyw4ZBYSYfnAkEAjmCk2WGIx4YD/LYNjLBUjDGiyAQ5UKN5HygptGEddHYD7L4xTUG/Ln1RMLQiou4vnVvZxdKzX5s7sdx8t4JgawJAcbN94r6Hjk1J84nrmuPDrsi7OjvYqXXqKOrA8AxmTlEEHHeFrzDf9CdgUkgl3rOfUYC5HE6Fw6g+AS2rMf9W2QJANiPn68Wb9osujvVHd71BmUHyrW51wQU4tLFYEoBva+7IlUjJhKBAq9P5gLSvCxfZwVPBHul9ThjAfWTjhACu2wJBALcRaPJhBMaaceq3vqpkIOAAoO5mPsmoRsK5UAuDcJtfR+J0EGRxBXKhzeBaELV4tWLETsyxFlQANjaqa85gBls=";
    private String iappayPublicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCo075tP/Ph3cieQ9qRQHHjPXAgIh0I009SSLZKDmSG4YHUbIBBZ8YoaxLoVZCJCar01uSD47g19MvUeAje3UK/98Mtkd/+cZblEOXxiBfrNcYcwl7FQ57jN2B/bnrehmj/b7e1WwfKI8+gV7Ucv4hJIj+2/p0bZQSRcdM1VN1rIQIDAQAB";
    private String haoxinAppId = "801713";
    private String haoxinAppKey = "6885ZCUP5CHUMDJ5IS5UU6HHSJSNDMF4";
    
    private static PlatformActivity m_instacne = null;
    
    public static PlatformActivity getInstance()
    {
    	return m_instacne;
    }
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		HaoXinManager.init(this, haoxinAppId, haoxinAppKey, true);
		HaoXinManager.setLogoutCallback(new HaoXinLogoutCallback() {
			@Override
			public void run() {
				UnityPlayer.UnitySendMessage("PlatformListener", "LogoutCallback", "");
			}
		});
		
		m_instacne = this;
	}
	
	public void onDestroy() {
		super.onDestroy();
	}
	
	@Override
	protected void onResume()
	{
		super.onResume();
	}
	
	@Override
	protected void onPause()
	{
		super.onPause();
	}
	
	@Override
	protected void onStart()
	{
		super.onStart();
	}
	
	@Override
	protected void onStop()
	{
		super.onStop();
	}
	
	@Override
	protected void onNewIntent(Intent intent)
	{
		super.onNewIntent(intent);
	}
	
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		super.onActivityResult(requestCode, resultCode, data);
	}
	
	@Override
	public void onConfigurationChanged(Configuration arg0)
	{
		super.onConfigurationChanged(arg0);
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
	    IAppPayOrderUtils orderUtils = new IAppPayOrderUtils();
			orderUtils.setAppid(iappayId);
	        orderUtils.setWaresid(getIappPayProductId(Integer.parseInt(productId)));
	        orderUtils.setCporderid(appOrderId);
	        orderUtils.setAppuserid(appUserId);
	        orderUtils.setPrice(Float.valueOf(moneyAmount) * 0.01f);
	        orderUtils.setWaresname(productName);
	        orderUtils.setNotifyurl(notifyUri);
	        String param = orderUtils.getTransdata(iappayPrivateKey);
	        IAppPay.startPay (this, param, iPayResultCallback);
	}
	
	private int getIappPayProductId(int cpProductId){
		switch(cpProductId){
		case 101:
			return 1;
		case 103:
			return 2;
		case 155:
			return 3;
		case 154:
			return 4;
		case 153:
			return 5;
		case 152:
			return 6;
		case 151:
			return 7;
		case 150:
			return 8;
		case 201:
			return 9;
		case 202:
			return 10;
		case 203:
			return 11;
		case 251:
			return 12;
		case 252:
			return 13;
		case 253:
			return 14;
		case 254:
			return 15;
		case 255:
			return 16;
		case 256:
			return 17;
		case 257:
			return 18;
		case 258:
			return 19;
		case 259:
			return 20;
		case 102:
			return 21;
		}
		return 0;
	}
	
    /**
     * 支付结果回调
     */
    IPayResultCallback iPayResultCallback = new IPayResultCallback() {

        @Override
        public void onPayResult(int resultCode, String signvalue, String resultInfo) {
            switch (resultCode) {
                case IAppPay.PAY_SUCCESS:
                    //调用 IAppPayOrderUtils 的验签方法进行支付结果验证
                    boolean payState = IAppPayOrderUtils.checkPayResult(signvalue, iappayPublicKey);
                    if(payState){
                    	UnityPlayer.UnitySendMessage("PlatformListener", "PayCallback", "ret=0");
                    }
                    break;
                case IAppPay.PAY_ING:
                    break ;
                case IAppPay.PAY_ERROR:
                	UnityPlayer.UnitySendMessage("PlatformListener", "PayCallback", "ret=-1");
                	break;
                default:
                    break;
            }
            Log.d("HaoXinSdk", "requestCode:" + resultCode + ",signvalue:" + signvalue + ",resultInfo:" + resultInfo);
        }
    };
	
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
		/*String path = Environment.getExternalStorageDirectory() + "/haoxin/" + this.getPackageName() + "/images";
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
		m_wxAPI.sendReq(req);*/
	}

	public void XGRegister(String account)
	{
		//XGPushManager.registerPush(getApplicationContext(), account);
	}
	
	public void HXInit()
	{
		IAppPay.init (this, IAppPay.LANDSCAPE, iappayId);
		UnityPlayer.UnitySendMessage("PlatformListener", "InitCallback", "JTRXWS");
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
		HaoXinManager.login(this, new HaoXinLoginCallback(){

			@Override
			public void run(int arg0, String arg1, LoginCallBackData arg2) {
				if (arg0 == HaoXinLoginCallback.SUCCEED){
					UnityPlayer.UnitySendMessage("PlatformListener", 
							"LoginCallback", 
							MessageFormat.format("platform_id={0}&token={1}&username={2}", arg2.userId, arg2.sessionId, arg2.username));
				}
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
		HaoXinManager.logout(this);
	}
	
	public void HXSubmitUserConfig(String roleId, String roleName, String roleLevel, int zoneId, String zoneName, int registerTime, int currentTime, String type)
	{
		/*Log.d("debug", "HJLoginSubmitExtendData start");
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
			
		}*/
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
		//HXWrapper.getInstance().exitGame(this);
	}
}
