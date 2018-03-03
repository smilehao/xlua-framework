package com.haoxin.sanguo;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;

/**
 * Created by Administrator on 2015/2/14.
 */
public class BootReceiver extends BroadcastReceiver {
	public BootReceiver() {
		Log.d("sanguo", "BootReceiver start ... ");
	}

	@Override
	public void onReceive(Context context, Intent intent) {
		Intent serviceIntent = new Intent(context,
				SGNotificationService.class);
		context.startService(serviceIntent);
	}
}
