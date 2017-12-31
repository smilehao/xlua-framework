--[[
-- added by wsh @ 2017-12-11
-- UILogin模块UILoginView窗口中服务器列表的可复用Item
--]]

local UIServerWrapItem = BaseClass("UIServerWrapItem", UIWrapComponent)
local base = UIWrapComponent

-- 创建
local function OnCreate(self)
	base.OnCreate(self)
	-- 组件初始化
	self.server_name_text = self:AddComponent(UIText, "SvrName")
	-- TODO：后续图集管理做好才能切换图片
	self.server_state_img = nil
	self.server_choose_cmp = self:AddComponent(UIBaseComponent, "SvrChoose")
end

-- 组件被复用时回调该函数，执行组件的刷新
local function OnRefresh(self, real_index, check)
	local server = self.view.server_list[real_index + 1]
	self.server_name_text:SetText(LangUtil.GetServerName(server.server_id))
	self.server_choose_cmp:SetActive(check)
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