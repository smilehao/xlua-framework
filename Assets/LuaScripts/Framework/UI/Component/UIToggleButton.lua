--[[
-- added by wsh @ 2017-12-08
-- Lua侧带有选中功能的UIButton，UIButtonGroup需要使用到
-- 使用方式：一般不直接使用，用来作为基类或者接口具体实现自定制的按钮组件，配合UIButtonGroup使用
--]]

local UIToggleButton = BaseClass("UIToggleButton", UIButton)
local base = UIButton

local function OnCreate(self)
	base.OnCreate(self)
	-- 记录选中回调、取消选中回调
	-- 回调原型：void __oncheck(bool)
	self.__oncheck = nil
end

-- 虚拟选中、虚拟取消选中
local function SetCheck(self, check)
	self.unity_uibutton.interactable = (not check)
	if self.__oncheck  ~= nil then
		self.__oncheck(check)
	end
end

-- 设置回调
local function SetOnCheck(self, ...)
	self.__oncheck = BindCallback(...)
end

local function OnDestroy(self)
	self.__oncheck = nil
	base.OnDestroy(self)
end

UIToggleButton.OnCreate = OnCreate
UIToggleButton.SetCheck = SetCheck
UIToggleButton.SetOnCheck = SetOnCheck
UIToggleButton.OnDestroy = OnDestroy

return UIToggleButton