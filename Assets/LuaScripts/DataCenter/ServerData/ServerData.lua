--[[
-- added by wsh @ 2017-12-05
-- 服务器数据
--]]

local ServerItemData = {
	-- 服务器ID
	server_id = 0,
	-- 区域ID
	area_id = 0,
	-- 服务器状态：0-良好、1-普通、2-爆满、3-未开服
	state = 0,
	-- 是否推荐
	recommend = true,
}

local ServerData = BaseClass("ServerData", Singleton)
local ServerItem = DataClass("ServerItem", ServerItemData)

local function __init(self)
	-- 所有服务器列表
	self.servers = {}
end

-- 解析网络数据
local function ParseServerList(self, servers)
	self.servers = {}
	for _,v in pairs(servers) do
		local item = ServerItem.New()
		item.server_id = v.server_id
		item.area_id = v.area_id
		item.state = v.state
		item.recommend = v.recommend
		self.servers[item.server_id] = item
	end
	DataManager:GetInstance():Broadcast(DataMessageNames.ON_SERVER_LIST_CHG, self)
end

ServerData.ParseServerList = ParseServerList

return ServerData
