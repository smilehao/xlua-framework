--[[
-- added by wsh @ 2017-12-04
-- UILoginServerView视图层
--]]

local UIServerWrapItem = require "UI.UILogin.Component.UIServerWrapItem"
local UILoginServerView = BaseClass("UILoginServerView", UIBaseView)
local base = UIBaseView

-- 各个组件路径
local back_btn_path = "ContentRoot/BackBtnRoot/Parent/BackBtn"
local confirm_btn_path = "ContentRoot/ConfirmBtnRoot/Parent/ConfirmBtn"
local recommend_btn_path = "ContentRoot/RecommendBtn"
local area_scroll_content_path = "ContentRoot/AreaScrollView/AreaScrollRect/AreaScrollContent"
local svr_scroll_content_path = "ContentRoot/SvrScrollView/SvrScrollRect/SvrScrollContent"
local area_btn_text_path = "Text"
local recommend_btn_virtual_index = -1

local function OnCreate(self)
	base.OnCreate(self)
	
	-- 1、按钮初始化
	self.back_btn = self:AddComponent(UIButton, back_btn_path)
	self.confirm_btn = self:AddComponent(UIButton, confirm_btn_path)
	self.recommend_btn = self:AddComponent(UIToggleButton, recommend_btn_path)
	
	-- 2、区域列表初始化
	-- A）不继承UIWrapComponent去实现子类，而是直接挂载组件
	-- B）添加按钮组，area_wrapgroup下所以按钮以UIToggleButton组件实例添加到按钮组
	-- C）再添加外部按钮（推荐按钮）---设置虚拟索引为-1
	self.area_wrapgroup = self:AddComponent(UIWrapGroup, area_scroll_content_path, UIWrapComponent)
	self.area_wrapgroup:Walk(function(component)
		component:AddComponent(UIText, area_btn_text_path)
	end)
	self.area_wrapgroup:AddButtonGroup(UIToggleButton)
	self.area_wrapgroup:AddButton(UIToggleButton, self.recommend_btn, recommend_btn_virtual_index)
	
	-- 3、服务器列表初始化
	-- A）继承UIWrapComponent去实现子类
	-- B）添加按钮组，area_wrapgroup下所以按钮以UIToggleButton组件实例添加到按钮组
	self.svr_wrapgroup = self:AddComponent(UIWrapGroup, svr_scroll_content_path, UIServerWrapItem)
	self.server_list = nil
	self.selected_server_id = nil
	self.svr_wrapgroup:AddButtonGroup(UIToggleButton)
	
	-- 4、按钮点击回调
	self.back_btn:SetOnClick(function()
		self.ctrl:CloseSelf()
	end)
	self.confirm_btn:SetOnClick(function()
		self.ctrl:SetSelectedServer(self.selected_server_id)
		self.ctrl:CloseSelf()
	end)
	
	-- 5、区域列表回调：
	-- A）area_wrapgroup使用挂载组件的方式，必须注册刷新和按钮点击回调
	-- B）设置默认选中推荐按钮
	self.area_wrapgroup:SetOnRefresh(self.OnAreaWrapgroupRefresh, self)
	self.area_wrapgroup:SetOnClick(self.OnAreaBtngroupClick, self)
	self.area_wrapgroup:SetOriginal(recommend_btn_virtual_index)
end

local function OnEnable(self)
	base.OnEnable(self)
	
	-- 获取model层当前选择server
	self.selected_server_id = self.model.selected_server_id
	
	-- 各组件刷新，重置wrapgroup长度，wrapgroup、btngroup复位
	self.area_wrapgroup:SetLength(table.count(self.model.area_ids))
	self.area_wrapgroup:ResetToBeginning()
end

-- 区域列表Item复用刷新
local function OnAreaWrapgroupRefresh(self, wrap_component, real_index, check)
	-- 刷新按钮下的文字
	local text = wrap_component:GetComponent(area_btn_text_path, UIText)
	local area_id = self.model.area_ids[real_index + 1]
	local btn_name = LangUtil.GetServerAreaName(area_id)
	text:SetText(btn_name)
end

-- 区域按钮组点击刷新
local function OnAreaBtngroupClick(self, wrap_component, toggle_btn, virtual_index, check)
	if not check then
		return
	end
	
	if virtual_index == recommend_btn_virtual_index then
		self.server_list = self.model.recommend_servers
	else
		local area_id = self.model.area_ids[virtual_index + 1]
		self.server_list = self.model.area_servers[area_id]
	end
	
	-- 区域列表回调：UIWrapGroup建立专门脚本UIServerItem刷新示例
	local selected_server_index = self:ServerID2ServerIndex(self.selected_server_id)
	self.svr_wrapgroup:SetLength(table.count(self.server_list))
	self.svr_wrapgroup:SetOriginal(selected_server_index)
	self.svr_wrapgroup:ResetToBeginning()
end

-- server_id转换到server_index
local function ServerID2ServerIndex(self, server_id)
	local choose_pairs = table.choose(self.server_list, function(i, v)
		return v.server_id == server_id
	end)
	if table.count(choose_pairs) == 0 then
		return nil
	else
		local keys = table.keys(choose_pairs)
		assert(table.count(keys) == 1)
		return keys[1] - 1
	end
end

-- 设置选择server
local function SetSelectedServer(self, server_index)
	self.selected_server_id = self.server_list[server_index + 1].server_id
end

UILoginServerView.OnCreate = OnCreate
UILoginServerView.OnEnable = OnEnable
UILoginServerView.OnAreaWrapgroupRefresh = OnAreaWrapgroupRefresh
UILoginServerView.OnAreaBtngroupClick = OnAreaBtngroupClick
UILoginServerView.ServerID2ServerIndex = ServerID2ServerIndex
UILoginServerView.SetSelectedServer = SetSelectedServer

return UILoginServerView