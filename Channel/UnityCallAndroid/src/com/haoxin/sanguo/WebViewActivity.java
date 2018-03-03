package com.haoxin.sanguo;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.ActivityManager;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.Gravity;
import android.view.ViewGroup.LayoutParams;
import android.webkit.JavascriptInterface;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.FrameLayout;

public class WebViewActivity extends Activity 
{
	@SuppressLint("SetJavaScriptEnabled") @Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		String url = "";
		Intent intent = getIntent();
		if (intent != null)
		{
			url = intent.getStringExtra("url");
		}
		WebView webView = new WebView(this);
		webView.getSettings().setJavaScriptEnabled(true);
		webView.setWebViewClient(new WebViewClient());
		webView.addJavascriptInterface(this, "Android");
		FrameLayout layout = new FrameLayout(this); 
		this.addContentView(layout, new LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT)); 
		layout.setFocusable(true); 
		layout.setFocusableInTouchMode(true);
		layout.addView(webView, new FrameLayout.LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT, Gravity.NO_GRAVITY)); 
		webView.loadUrl(url);
		Log.d("debug", "open webview success");
	}
	
	@JavascriptInterface
	public void doClose()
	{
		finish();
	}
}
