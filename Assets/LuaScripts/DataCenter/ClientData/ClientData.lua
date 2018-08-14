--[[
-- added by wsh @ 2017-12-05
-- 客户端数据
--]]

local ClientData = BaseClass("ClientData", Singleton)

local function __init(self)
	self.app_version = CS.GameChannel.ChannelManager.instance.appVersion
	self.res_version = CS.GameChannel.ChannelManager.instance.resVersion
	self.account = CS.UnityEngine.PlayerPrefs.GetString("account")
	self.password = CS.UnityEngine.PlayerPrefs.GetString("password")
	self.login_server_id = CS.UnityEngine.PlayerPrefs.GetInt("login_server_id")
end

local function SetAccountInfo(self, account, password)
	self.account = account
	self.password = password
	CS.UnityEngine.PlayerPrefs.SetString("account", account)
	CS.UnityEngine.PlayerPrefs.SetString("password", password)
	DataManager:GetInstance():Broadcast(DataMessageNames.ON_ACCOUNT_INFO_CHG, account, password)
end

local function SetLoginServerID(self, id)
	self.login_server_id = id
	CS.UnityEngine.PlayerPrefs.SetInt("login_server_id", id)
	DataManager:GetInstance():Broadcast(DataMessageNames.ON_LOGIN_SERVER_ID_CHG, id)
end

ClientData.__init = __init
ClientData.SetAccountInfo = SetAccountInfo
ClientData.SetLoginServerID = SetLoginServerID

return ClientData