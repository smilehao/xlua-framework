--[[
-- added by wsh @ 2017-12-11
-- UILogin模块UILoginView窗口中服务器列表的可复用Item
--]]

local UIServerWrapItem = BaseClass("UIServerWrapItem", UIWrapComponent)
local base = UIWrapComponent

local function GetServerStateSpriteName(state)
	-- 服务器状态：0-良好、1-普通、2-爆满、3-未开服
	local sprite_name = "login2_10.png"
	if state == 1 then
		sprite_name = "login2_05.png"
	elseif state == 2 then
		sprite_name = "login2_11.png"
	elseif state == 3 then
		sprite_name = "login2_06.png"
	end
	return sprite_name
end

-- 创建
local function OnCreate(self)
	base.OnCreate(self)
	-- 组件初始化
	self.server_name_text = self:AddComponent(UIText, "SvrName")
	self.server_choose_cmp = self:AddComponent(UIBaseComponent, "SvrChoose")
	self.server_state_img = self:AddComponent(UIImage, "SvrState", AtlasConfig.Login, GetServerStateSpriteName())
end

-- 组件被复用时回调该函数，执行组件的刷新
local function OnRefresh(self, real_index, check)
	local server = self.view.server_list[real_index + 1]
	self.server_name_text:SetText(LangUtil.GetServerName(server.server_id))
	self.server_choose_cmp:SetActive(check)
	self.server_state_img:SetSpriteName(GetServerStateSpriteName(server.state))
end

-- 组件添加了按钮组，则按钮被点击时回调该函数
local function OnClick(self, toggle_btn, real_index, check)
	self.server_choose_cmp:SetActive(check)
	if check then
		self.view:SetSelectedServer(real_index)
	end
end

UIServerWrapItem.OnCreate = OnCreate
UIServerWrapItem.OnRefresh = OnRefresh
UIServerWrapItem.OnClick = OnClick

return UIServerWrapItem