package com.haoxin.sanguo;


import java.text.MessageFormat;

import android.app.AlertDialog;
import android.content.ComponentName;
import android.content.Context;
import android.content.DialogInterface;
import android.content.DialogInterface.OnClickListener;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.Intent;
import android.content.ServiceConnection;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.os.IBinder;
import android.os.PowerManager;
import android.os.PowerManager.WakeLock;
import android.os.Vibrator;
import android.telephony.TelephonyManager;
import android.util.Log;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;


public class MainActivity extends UnityPlayerActivity {
	
	SGNotificationService notificationService;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
	}
	
	public void onDestroy() {
		super.onDestroy();
		unbindService(conn);
	}
	
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

	private WakeLock wakeLock = null;

	private void ShowWAKE_LOCK() {
		if (null == wakeLock) {
			PowerManager pm = (PowerManager) this
					.getSystemService(Context.POWER_SERVICE);
			wakeLock = pm.newWakeLock(PowerManager.SCREEN_BRIGHT_WAKE_LOCK,
					"PostLocationService");
			if (null != wakeLock) {
				wakeLock.acquire();
			}
		}
	}

	public void Unsleep() {
		ShowWAKE_LOCK();
	}

	public void Vibrate(int ms) {
		Vibrator vibrator = (Vibrator) getSystemService(VIBRATOR_SERVICE);
		vibrator.vibrate(ms);
	}

	private ServiceConnection conn = new ServiceConnection()
	{
		/** ��ȡ�������ʱ�Ĳ��� */
		public void onServiceConnected(ComponentName name, IBinder service) {
			// TODO Auto-generated method stub
			notificationService = ((SGNotificationService.ServiceBinder) service).getService();
		}

		/** �޷���ȡ���������ʱ�Ĳ��� */
		public void onServiceDisconnected(ComponentName name)
		{
			// TODO Auto-generated method stub
			notificationService = null;
		}
	};

	public void InstallNotification() {
		Intent intent = new Intent(this, SGNotificationService.class);
		startService(intent);
		bindService(intent, conn, Context.BIND_AUTO_CREATE);
	}

	public void NotificationListAdd(int id, int hour, int min, final String title, final String msg) {
		if(notificationService != null)
		{
			notificationService.NotificationListAdd(id, hour, min, title, msg);
		}
	}

	/* ����һ����ʾ�Ի���ķ��������ǽ���Unity�е��ô˷��� */
	public void ShowDialog(final String mTitle, final String mContent,
			final String yesStr,final String noStr, final String gameName, final String yesFunc,
			final String noFunc) {
		/* ��UI�߳���ִ����ط��� */
		runOnUiThread(new Runnable() {
			@Override
			public void run() {
				OnClickListener noImpl = null;
				OnClickListener yesImpl = null;
				if (gameName!=null&&yesFunc != null) {
					yesImpl = new OnClickListener() {
						@Override
						public void onClick(DialogInterface dialog, int which) {
							UnityPlayer.UnitySendMessage(gameName,yesFunc, "");
						}
					};
				}

				if (gameName!=null&&noFunc != null) {
					noImpl = new OnClickListener() {
						@Override
						public void onClick(DialogInterface dialog, int which) {
							UnityPlayer.UnitySendMessage(gameName,noFunc, "");
						}
					};
				}

				// ����Builder
				AlertDialog.Builder mBuilder = new AlertDialog.Builder(
						MainActivity.this);
				// �����Ի���
				mBuilder.setTitle(mTitle).setMessage(mContent);

				mBuilder.setPositiveButton(yesStr, yesImpl);

				mBuilder.setNegativeButton(noStr, noImpl);

				// ��ʾ�Ի���
				mBuilder.setCancelable(false);
				mBuilder.show();
			}
		});
	}
	
	public void GATAInit()
	{
		//GAEAAgent.initContext(this, GATACountry.GATA_CHINA);
		
		String deviceID = this.GetDeviceID();
		//Log.d("sanguo", "getDeviceID " + deviceID);
		String macAddr = this.GetMacAddress();
		
		String versionName = "";
		try 
		{
			PackageManager mgr = this.getPackageManager();
			PackageInfo info = mgr.getPackageInfo(this.getPackageName(), 0);
			if (info != null)
			{
				versionName = info.versionName;
			}
		} 
		catch (Exception e) 
		{
			
		}
		
		String msg = MessageFormat.format("imei={0}&macAddr={1}&appVersion={2}", deviceID, macAddr, versionName);
		UnityPlayer.UnitySendMessage("PlatformListener", "GATAInitCallback", msg);
	}
	
	public String GetMacAddress() 
	{  
		try {
			// ��ȡwifi����
			WifiManager wifiManager = (WifiManager) getSystemService(Context.WIFI_SERVICE);
			// �ж�wifi�Ƿ���
			if (!wifiManager.isWifiEnabled()) {
				return "";
			}
			WifiInfo wifiInfo = wifiManager.getConnectionInfo();
			String macAddress = wifiInfo.getMacAddress();
			return macAddress;
		} catch (Exception e) {
			e.printStackTrace();
			return "";
		}
    }  
	
	private String GetDeviceID()
	{
		String did = "";
				
		try
		{
			TelephonyManager phonyMgr = (TelephonyManager) getSystemService(TELEPHONY_SERVICE);
			if (phonyMgr != null)
			{
				//Log.d("sanguo", "now getDeviceID");
				
				did = phonyMgr.getDeviceId();
			}
		}
		catch(Exception ex)
		{
			Log.d("sanguo", "getDeviceID ex " + ex.toString());
		}
		
		if (did == null)
		{
			did = "";
		}
				
		return did;
	}
	
	public void GaeaLogin(String loginUid, String loginType)
	{
	}
	
	public void GATARegist(String accountID, String serverID)
	{
	}
	
	public void GATALogin(String accountID, String serverID, int level)
	{
	}
	
	public void GATALogout(String accoundID)
	{
	}
	
	public void GATASetLevel(String accountID, int level)
	{
	}
	
	public void GATAInitCoin(long totalCoin, String coinType)
	{
	}
	
	public void GATAAddCoin(String reason, String coinType, long addCoin, long totalCoin)
	{
	}
	
	public void GATALoseCoin(String reason, String coinType, long loseCoin, long totalCoin)
	{
	}
	
	public void GATASetEvent(String identifier)
	{
	}
	
}
