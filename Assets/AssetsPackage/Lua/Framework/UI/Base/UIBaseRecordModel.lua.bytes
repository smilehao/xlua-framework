--[[
-- added by wsh @ 2017-12-15
-- UI带有子窗口记忆功能的模型层基类：窗口被重新打开时会自动打开之前没有关闭的子级窗口
--]]

local UIBaseRecordModel = BaseClass("UIBaseRecordModel", UIBaseModel)
local base = UIBaseModel

-- 创建
local function OnCreate(self)
	base.OnCreate(self)
	-- 子级窗口栈
	self.__window_stack = {}
	-- 是否启用记录
	self.__enable_record = false
	-- 保持Model
	UIManager:GetInstance():SetKeepModel(self.__ui_name, true)
end

-- 打开
local function OnEnable(self, ...)
	base.OnEnable(self, ...)
	-- 重新打开上次保留的窗口
	table.walk(self.__window_stack, function(index, ui_name)
		UIManager:GetInstance():OpenWindow(ui_name)
	end)
	self.__enable_record = true
end

-- 获取栈
local function GetWindowStack(self)
	return self.__window_stack
end

-- 清空栈
local function ClearWindowStack(self)
	self.__window_stack = {}
end

-- 子级窗口入栈
local function OnWindowOpen(self, window)
	if not self.__enable_record or window.Layer:GetName() ~= UILayers.NormalLayer.Name then
		return
	end
	
	table.insert(self.__window_stack, window.Name)
	-- 保持Model
	UIManager:GetInstance():SetKeepModel(window.Name, true)
end

-- 出栈
local function OnWindowClose(self, window)
	if not self.__enable_record or window.Layer:GetName() ~= UILayers.NormalLayer.Name then
		return
	end
	
	local index = nil
	for i,v in pairs(self.__window_stack) do
		if v == window.Name then
			index = i
			break
		end
	end
	if index then
		local length = table.length(self.__window_stack)
		for i = 0, length - index do
			local ui_name = table.remove(self.__window_stack, length - i)
			-- 取消保持Model
			UIManager:GetInstance():SetKeepModel(ui_name, false)
		end
	end
end

-- 注册消息
local function OnAddListener(self)
	base.OnAddListener(self)
	self:AddUIListener(UIMessageNames.UIFRAME_ON_WINDOW_OPEN, OnWindowOpen)
	self:AddUIListener(UIMessageNames.UIFRAME_ON_WINDOW_CLOSE, OnWindowClose)
end

-- 注销消息
local function OnRemoveListener(self)
	base.OnRemoveListener(self)
	self:RemoveUIListener(UIMessageNames.UIFRAME_ON_WINDOW_OPEN, OnWindowOpen)
	self:RemoveUIListener(UIMessageNames.UIFRAME_ON_WINDOW_CLOSE, OnWindowClose)
end

-- 关闭
local function OnDisable(self)
	base.OnDisable(self)
	self.__enable_record = false
end

-- 销毁
local function OnDestroy(self)
	self.__window_stack = nil
	base.OnDestroy(self)
end

UIBaseRecordModel.OnCreate = OnCreate
UIBaseRecordModel.OnEnable = OnEnable
UIBaseRecordModel.GetWindowStack = GetWindowStack
UIBaseRecordModel.ClearWindowStack = ClearWindowStack
UIBaseRecordModel.OnAddListener = OnAddListener
UIBaseRecordModel.OnRemoveListener = OnRemoveListener
UIBaseRecordModel.OnDisable = OnDisable
UIBaseRecordModel.OnDestroy = OnDestroy

return UIBaseRecordModel