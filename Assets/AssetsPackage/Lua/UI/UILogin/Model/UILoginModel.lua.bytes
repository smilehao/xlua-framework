--[[
-- added by wsh @ 2017-12-01
-- UILogin模型层
-- 注意：
-- 1、成员变量预先在OnCreate、OnEnable函数声明，提高代码可读性
-- 2、OnCreate内放窗口生命周期内保持的成员变量，窗口销毁时才会清理
-- 3、OnEnable内放窗口打开时才需要的成员变量，窗口关闭后及时清理
-- 4、OnEnable函数每次在窗口打开时调用，可传递参数用来初始化Model
--]]

local UILoginModel = BaseClass("UILoginModel", UIBaseModel)
local base = UIBaseModel

-- 创建
local function OnCreate(self)
	base.OnCreate(self)
	-- 窗口生命周期内保持的成员变量放这
end

-- 打开
local function OnEnable(self)
	base.OnEnable(self)
	-- 窗口关闭时可以清理的成员变量放这
	-- 账号
	self.account = nil
	-- 密码
	self.password = nil
	-- 客户端app版本号
	self.client_app_ver = nil
	-- 客户端资源版本号
	self.client_res_ver = nil
	-- 区域名
	self.area_name = nil
	-- 服务器名
	self.server_name = nil
	
	self:OnRefresh()
end

local function SetServerInfo(self, select_svr_id)
	local server_data = ServerData:GetInstance()
	local select_svr = server_data.servers[select_svr_id]
	if select_svr ~= nil then
		self.area_name = LangUtil.GetServerAreaName(select_svr.area_id)
		self.server_name = LangUtil.GetServerName(select_svr_id)
	end
end

-- 刷新全部数据
local function OnRefresh(self)
	local client_data = ClientData:GetInstance()
	self.account = client_data.account
	self.password = client_data.password
	self.client_app_ver = client_data.app_version
	self.client_res_ver = client_data.res_version
	SetServerInfo(self, client_data.login_server_id)
end

local function OnSelectedSvrChg(self, id)
	SetServerInfo(self, id)
	self:UIBroadcast(UIMessageNames.UILOGIN_ON_SELECTED_SVR_CHG)
end

-- 监听选服变动
local function OnAddListener(self)
	base.OnAddListener(self)
	self:AddDataListener(DataMessageNames.ON_LOGIN_SERVER_ID_CHG, OnSelectedSvrChg)
end

local function OnRemoveListener(self)
	base.OnRemoveListener(self)
	self:RemoveDataListener(DataMessageNames.ON_LOGIN_SERVER_ID_CHG, OnSelectedSvrChg)
end

-- 关闭
local function OnDisable(self)
	base.OnDisable(self)
	-- 清理成员变量
	self.account = nil
	self.password = nil
	self.client_app_ver = nil
	self.client_res_ver = nil
	self.area_name = nil
	self.server_name = nil
end

-- 销毁
local function OnDistroy(self)
	base.OnDistroy(self)
	-- 清理成员变量
end

UILoginModel.OnCreate = OnCreate
UILoginModel.OnEnable = OnEnable
UILoginModel.OnRefresh = OnRefresh
UILoginModel.OnAddListener = OnAddListener
UILoginModel.OnRemoveListener = OnRemoveListener
UILoginModel.OnDisable = OnDisable
UILoginModel.OnDistroy = OnDistroy

return UILoginModel