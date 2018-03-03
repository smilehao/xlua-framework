package com.haoxin.sanguo;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;

import com.tencent.mm.sdk.constants.ConstantsAPI;
import com.tencent.mm.sdk.modelbase.BaseReq;
import com.tencent.mm.sdk.modelbase.BaseResp;
import com.tencent.mm.sdk.openapi.IWXAPI;
import com.tencent.mm.sdk.openapi.IWXAPIEventHandler;
import com.tencent.mm.sdk.openapi.WXAPIFactory;
import com.unity3d.player.UnityPlayer;

public class WXBaseActivity extends Activity implements IWXAPIEventHandler {

	private IWXAPI m_wxAPI;
	
	@Override
    public void onCreate(Bundle savedInstanceState) 
	{
        super.onCreate(savedInstanceState);
        
        m_wxAPI = WXAPIFactory.createWXAPI(this, PlatformActivity.getInstance().GetWXAppID(), false);
		m_wxAPI.handleIntent(getIntent(), this);
	}
	
	@Override
	public void onReq(BaseReq req) {
		switch (req.getType()) {
		case ConstantsAPI.COMMAND_GETMESSAGE_FROM_WX:
			break;
		case ConstantsAPI.COMMAND_SHOWMESSAGE_FROM_WX:
			break;
		default:
			break;
		}
	}

	@Override
	public void onResp(BaseResp resp) {
		// TODO Auto-generated method stub
		int result = -1;
		if (resp.errCode == BaseResp.ErrCode.ERR_OK)
		{
			result = 0;
		}
		
		UnityPlayer.UnitySendMessage("PlatformListener", "WXShareCallback", String.valueOf(result));
		finish();
	}

}
