--[[
-- added by wsh @ 2017-12-01
-- UILoginServerModel模型层
--]]

local UILoginServerModel = BaseClass("UILoginServerModel", UIBaseModel)
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
	-- 推荐服务器列表
	self.recommend_servers = nil
	-- 区域id列表
	self.area_ids = nil
	-- 所有区域下的服务器列表
	self.area_servers = nil
	-- 当前选择的登陆服务器
	self.selected_server_id = 0
	
	self:OnRefresh()
end

-- 获取推荐服务器列表
local function FetchRecommendList(servers)
	local recommend_servers = {}
	for _,v in pairs(servers) do
		if v.recommend then 
			table.insert(recommend_servers, v)
		end
	end
	table.sort(recommend_servers, function(ltb, rtb)
		return ltb.server_id < rtb.server_id
	end
	)
	return recommend_servers
end

-- 按区域划分服务器列表
local function FetchAreaList(servers)
	local area_ids_record = {}
	local area_ids = {}
	local area_servers = {}
	for _,v in pairs(servers) do
		local key = v.area_id
		local area = area_servers[key]
		if area == nil then
			area = {}
		end
		table.insert(area, v)
		area_servers[key] = area
		if area_ids_record[v.area_id] == nil then
			area_ids_record[v.area_id] = v.area_id
			table.insert(area_ids, v.area_id)
		end
	end
	table.sort(area_ids)
	for _,v in pairs(area_servers) do
		table.sort(v, function(ltb, rtb)
			return ltb.server_id < rtb.server_id
		end)
	end
	return area_ids, area_servers
end

local function OnRefresh(self)
	local server_data = ServerData:GetInstance()
	self.recommend_servers = FetchRecommendList(server_data.servers)
	self.area_ids, self.area_servers = FetchAreaList(server_data.servers)
	self.selected_server_id = ClientData:GetInstance().login_server_id
end

local function OnServerListChg(self)
	self:OnRefresh()
end

local function OnAddListener(self)
	base.OnAddListener(self)
	self:AddDataListener(DataMessageNames.ON_SERVER_LIST_CHG, OnServerListChg)
end

local function OnRemoveListener(self)
	base.OnRemoveListener(self)
	self:RemoveDataListener(DataMessageNames.ON_SERVER_LIST_CHG, OnServerListChg)
end

-- 关闭
local function OnDisable(self)
	base.OnDisable(self)
	-- 清理成员变量
	self.recommend_servers = nil
	self.area_ids = nil
	self.area_servers = nil
	self.selected_server_id = 0
end

-- 销毁
local function OnDistroy(self)
	base.OnDistroy(self)
	-- 清理成员变量
end

UILoginServerModel.OnCreate = OnCreate
UILoginServerModel.OnEnable = OnEnable
UILoginServerModel.OnRefresh = OnRefresh
UILoginServerModel.OnAddListener = OnAddListener
UILoginServerModel.OnRemoveListener = OnRemoveListener
UILoginServerModel.OnDisable = OnDisable
UILoginServerModel.OnDistroy = OnDistroy

return UILoginServerModel