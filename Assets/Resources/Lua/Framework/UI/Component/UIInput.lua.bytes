--[[
-- add by wsh @ 2017-12-08
-- Lua侧UIInput
-- 使用方式：
-- self.xxx_input = self:AddComponent(UIInput, var_arg)--添加孩子，各种重载方式查看UIBaseContainer
--]]

local UIInput = BaseClass("UIInput", UIBaseComponent)
local base = UIBaseComponent

-- 创建
local function OnCreate(self)
	base.OnCreate(self)
	-- Unity侧原生组件
	self.unity_uiinput = UIUtil.FindInput(self.transform)
	
	if self.unity_uiinput ~= nil and self.gameObject == nil then
		self.gameObject = self.unity_uiinput.gameObject
		self.transform = self.unity_uiinput.transform
	end
end

-- 获取文本
local function GetText(self)
	if self.unity_uiinput ~= nil then
		return self.unity_uiinput.text
	end
end

-- 设置文本
local function SetText(self, text)
	if self.unity_uiinput ~= nil then
		self.unity_uiinput.text = text
	end
end

-- 销毁
local function OnDestroy(self)
	self.unity_uiinput = nil
	base.OnDestroy(self)
end

UIInput.OnCreate = OnCreate
UIInput.GetText = GetText
UIInput.SetText = SetText
UIInput.OnDestroy = OnDestroy


return UIInput