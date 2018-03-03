package com.haoxin.sanguo;

/**
 * Created by Administrator on 2015/2/12.
 */

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.TimerTask;
import java.util.concurrent.ScheduledThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

import org.apache.http.util.EncodingUtils;

import com.haoxin.sgc.R;

import android.app.ActivityManager;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.graphics.BitmapFactory;
import android.os.Handler;
import android.os.IBinder;
import android.os.Binder;
import android.os.Message;
import android.os.Parcel;
import android.os.Parcelable;
import android.support.v4.app.NotificationCompat;

class GameNotification implements Parcelable {

	public int id;
	public int hour;
	public int min;
	public String title;
	public String msg;

	public GameNotification(int id, int hour, int min, String title, String msg) {
		this.id = id;
		this.hour = hour;
		this.min = min;
		this.title = title;
		this.msg = msg;
	}

	public int describeContents() {
		return 0;
	}

	public void writeToParcel(Parcel out, int flags) {
		out.writeInt(id);
		out.writeInt(hour);
		out.writeInt(min);
		out.writeString(title);
		out.writeString(msg);
	}

	public static final Parcelable.Creator<GameNotification> CREATOR = new Parcelable.Creator<GameNotification>() {
		public GameNotification createFromParcel(Parcel in) {
			return new GameNotification(in);
		}

		public GameNotification[] newArray(int size) {
			return new GameNotification[size];
		}
	};

	private GameNotification(Parcel in) {
		id = in.readInt();
		hour = in.readInt();
		min = in.readInt();
		title = in.readString();
		msg = in.readString();
	}
}

public class SGNotificationService extends Service {

	ArrayList<GameNotification> notificationList = null;
	//淇濆瓨鎺ㄩ�佹暟鎹殑鏂囦欢鍚�
	static final String NOTIFI_FILE_NAME = "NotifiDataFile";

	// 瀹氭椂鍣�
	ScheduledThreadPoolExecutor mTimer = new ScheduledThreadPoolExecutor(1);

	@Override
	public void onDestroy() {
		super.onDestroy();
	}

	long mLastCheckTimeMS = 0;

	public SGNotificationService() {
		notificationList = new ArrayList<GameNotification>();

		mTimer.scheduleAtFixedRate(new GameNotificationChecker(), 1, 1,
				TimeUnit.SECONDS);

		Calendar calendar = Calendar.getInstance();
		Date currDate = calendar.getTime();
		mLastCheckTimeMS = currDate.getTime();

	}

	public void NotificationListAdd(int id, int hour, int min, String title, String msg)
	{
		GameNotification notifi = new GameNotification(id, hour, min, title, msg);
		int i = 0;
		for(	; i < notificationList.size(); i++)
		{
			if(id == notificationList.get(i).id)
			{
				notificationList.set(i, notifi);
				break;
			}
		}
		if(i >= notificationList.size())
		{
			notificationList.add(notifi);
		}
		SaveNotifiDataFile();//灏嗘帹閫佹暟鎹繚瀛樺埌鏂囦欢涓�
	}

	@Override
	public IBinder onBind(Intent intent) {
		//return null;
		return new ServiceBinder();
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {
		//Toast.makeText(this, "create a service", 13).show();
		if (null != intent) {
			ArrayList<GameNotification> notificationList = intent
					.getParcelableArrayListExtra("notify_condition");
			if (null != notificationList) {
				// passGameNotifition(notificationList);
			}
		}

		Date dt = new Date();
		mLastCheckTimeMS = dt.getTime();

		return START_STICKY;
	}

	class GameNotificationChecker extends TimerTask {
		@Override
		public void run() {
			mHandler.sendEmptyMessage(0);
		}
	}

	private void SendNotification(int nID, String title, String msg) {
		NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(
				this).setSmallIcon(R.drawable.ic_launcher, 30)
				.setContentTitle(title).setContentText(msg);

		mBuilder.setLargeIcon(BitmapFactory.decodeResource(getResources(),
				R.drawable.ic_launcher));
		mBuilder.setAutoCancel(true);
		mBuilder.setOnlyAlertOnce(true);
		mBuilder.setDefaults(Notification.DEFAULT_VIBRATE
				| Notification.DEFAULT_LIGHTS);

		Intent resultIntent = new Intent(this, MainActivity.class);
		resultIntent.setAction(Intent.ACTION_MAIN);
		resultIntent.addCategory(Intent.CATEGORY_LAUNCHER);

		PendingIntent pendingIntent = PendingIntent.getActivity(this, nID,
				resultIntent, 0);

		mBuilder.setContentIntent(pendingIntent);

		NotificationManager mNotificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

		mNotificationManager.notify(nID, mBuilder.build());
	}

	void CheckIfNeedNotification() {
		Calendar calendar = Calendar.getInstance();
		Date currDate = calendar.getTime();

		calendar.set(calendar.get(Calendar.YEAR), calendar.get(Calendar.MONTH),
				calendar.get(Calendar.DATE));

		long currTimeMS = currDate.getTime();
		if(notificationList.size() <= 0)
		{
			LoadNotifiDataFile();//浠庢枃浠朵腑璇诲彇鍑烘帹閫佹暟鎹�
		}
		for (GameNotification notification : notificationList) {
			calendar.set(Calendar.HOUR_OF_DAY, notification.hour);
			calendar.set(Calendar.MINUTE, notification.min);
			calendar.set(Calendar.SECOND, 0);
			calendar.set(Calendar.MILLISECOND, 0);

			long tempTimeMS = calendar.getTime().getTime();

			if (currTimeMS >= tempTimeMS && mLastCheckTimeMS < tempTimeMS) {
				if (isActivityInForeground()) {
					continue;
				}
				SendNotification(notification.id, notification.title, notification.msg);
			}
		}
		mLastCheckTimeMS = currTimeMS;
	}

	public Handler mHandler = new Handler() {
		@Override
		public void handleMessage(Message msg) {
			// 杩欓噷瑕佹鏌ユ椂闂�
			CheckIfNeedNotification();
			super.handleMessage(msg);
		}
	};

	public boolean isActivityInForeground() {
		String selfPackageName = getApplicationContext().getPackageName();
		ActivityManager am = (ActivityManager) this
				.getSystemService(Context.ACTIVITY_SERVICE);

		List<ActivityManager.RunningAppProcessInfo> processes = am
				.getRunningAppProcesses();
		for (ActivityManager.RunningAppProcessInfo process : processes) {
			if (process.importance == ActivityManager.RunningAppProcessInfo.IMPORTANCE_FOREGROUND) {
				// Log.d("sanguo",selfPackageName + "  -> " +
				// process.processName);
				if (process.processName.equals(selfPackageName)) {
					return true;
				}
			}
		}
		return false;
	}

	class ServiceBinder extends Binder
	{
		public SGNotificationService getService()
		{
			return SGNotificationService.this;
		}
	}

	//鍐欐暟鎹�
	public void writeFile(String fileName,String writestr) //throws IOException
	{
		try
		{
			FileOutputStream fout = openFileOutput(fileName, MODE_PRIVATE);
			byte[] bytes = writestr.getBytes();
			fout.write(bytes);
			fout.close();
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}

	//璇绘暟鎹�
	public String readFile(String fileName) //throws IOException
	{
		String res = "";
		try
		{
			FileInputStream fin = openFileInput(fileName);
			int length = fin.available();
			byte [] buffer = new byte[length];
			fin.read(buffer);
			res = EncodingUtils.getString(buffer, "UTF-8");
			fin.close();
		}
		catch(Exception e)
		{
			//e.printStackTrace();
		}
		return res;
	}

	//灏嗘帹閫佹暟鎹繚瀛樺埌鏂囦欢涓�
	public void SaveNotifiDataFile()
	{
		String strNotifiData = "";
		for (GameNotification notification : notificationList)
		{
			strNotifiData += notification.id;
			strNotifiData += "@";
			strNotifiData += notification.hour;
			strNotifiData += "@";
			strNotifiData += notification.min;
			strNotifiData += "@";
			strNotifiData += notification.title;
			strNotifiData += "@";
			strNotifiData += notification.msg;
			strNotifiData += "@";
		}

		writeFile(NOTIFI_FILE_NAME, strNotifiData);
	}

	//浠庢枃浠朵腑璇诲彇鍑烘帹閫佹暟鎹�
	public void LoadNotifiDataFile()
	{
		String strNotifiData = readFile(NOTIFI_FILE_NAME);
		String[] strSplit = strNotifiData.split("@");
		GameNotification notification = null;
		int id = 0;
		int hour = 0;
		int min = 0;
		String title = "";
		String msg = "";
		try
		{
			for(int i = 0; i + 4 < strSplit.length; i += 5)
			{
				id = Integer.parseInt(strSplit[i]);
				hour = Integer.parseInt(strSplit[i + 1]);
				min = Integer.parseInt(strSplit[i + 2]);
				title = strSplit[i + 3];
				msg = strSplit[i + 4];
				notification = new GameNotification(id, hour, min, title, msg);
				notificationList.add(notification);
			}
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}
}
