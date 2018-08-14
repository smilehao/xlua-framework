package com.chivas.xluaframework;


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
import com.unity3d.player.UnityPlayerNativeActivity;


public class MainActivity extends UnityPlayerNativeActivity {
	
	SGNotificationService notificationService;
	private String reyunKey = "623b80b422f9f05d1e8204942eac7a8f";
	
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
		/** 锟斤拷取锟斤拷锟斤拷锟斤拷锟绞憋拷牟锟斤拷锟� */
		public void onServiceConnected(ComponentName name, IBinder service) {
			// TODO Auto-generated method stub
			notificationService = ((SGNotificationService.ServiceBinder) service).getService();
		}

		/** 锟睫凤拷锟斤拷取锟斤拷锟斤拷锟斤拷锟斤拷锟绞憋拷牟锟斤拷锟� */
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

	/* 锟斤拷锟斤拷一锟斤拷锟斤拷示锟皆伙拷锟斤拷姆锟斤拷锟斤拷锟斤拷锟斤拷墙锟斤拷锟経nity锟叫碉拷锟矫此凤拷锟斤拷 */
	public void ShowDialog(final String mTitle, final String mContent,
			final String yesStr,final String noStr, final String gameName, final String yesFunc,
			final String noFunc) {
		/* 锟斤拷UI锟竭筹拷锟斤拷执锟斤拷锟斤拷胤锟斤拷锟� */
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

				// 锟斤拷锟斤拷Builder
				AlertDialog.Builder mBuilder = new AlertDialog.Builder(
						MainActivity.this);
				// 锟斤拷锟斤拷锟皆伙拷锟斤拷
				mBuilder.setTitle(mTitle).setMessage(mContent);

				mBuilder.setPositiveButton(yesStr, yesImpl);

				mBuilder.setNegativeButton(noStr, noImpl);

				// 锟斤拷示锟皆伙拷锟斤拷
				mBuilder.setCancelable(false);
				mBuilder.show();
			}
		});
	}
	
	public void TrackInit()
	{
		//ReYunTrack.initWithKeyAndChannelId(this, reyunKey, "jituo");
	}
	
	public String GetMacAddress() 
	{  
		try {
			// 锟斤拷取wifi锟斤拷锟斤拷
			WifiManager wifiManager = (WifiManager) getSystemService(Context.WIFI_SERVICE);
			// 锟叫讹拷wifi锟角凤拷锟斤拷
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
	
	public void TrackPayStart(String transactionId, String paymentType, String  currencyType, float currencyAmount){
		//ReYunTrack. setPaymentStart (transactionId, paymentType, currencyType, currencyAmount);
	}
	
	public void TrackPaySuccess(String transactionId, String paymentType, String  currencyType, float currencyAmount ){
		//ReYunTrack.setPayment(transactionId, paymentType, currencyType, currencyAmount);
	}
	
	public void TrackRegist(String accountID, String serverID)
	{
		//ReYunTrack.setRegisterWithAccountID(accountID);
	}
	
	public void TrackLogin(String accountID, String serverID, int level)
	{
		//ReYunTrack.setLoginSuccessBusiness(accountID);
	}
	
	public void TrackExit(){
		//ReYunTrack.exitSdk ();
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
