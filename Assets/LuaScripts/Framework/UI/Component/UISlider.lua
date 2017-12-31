--[[
-- added by wsh @ 2017-12-18
-- Lua侧UISlider
-- 使用方式：
-- self.xxx_text = self:AddComponent(UISlider, var_arg)--添加孩子，各种重载方式查看UIBaseContainer
--]]

local UISlider = BaseClass("UISlider", UIBaseComponent)
local base = UIBaseComponent

-- 创建
local function OnCreate(self)
	base.OnCreate(self)
	-- Unity侧原生组件
	self.unity_uislider = UIUtil.FindSlider(self.transform)
	
	if not IsNull(self.unity_uislider) and IsNull(self.gameObject) then
		self.gameObject = self.unity_uislider.gameObject
		self.transform = self.unity_uislider.transform
	end
end

-- 获取进度
local function GetValue(self)
	if not IsNull(self.unity_uislider) then
		return self.unity_uislider.normalizedValue
	end
end

-- 设置进度
local function SetValue(self, value)
	if not IsNull(self.unity_uislider) then
		self.unity_uislider.normalizedValue = value
	end
end

-- 销毁
local function OnDestroy(self)
	self.unity_uislider = nil
	base.OnDestroy(self)
end

UISlider.OnCreate = OnCreate
UISlider.GetValue = GetValue
UISlider.SetValue = SetValue
UISlider.OnDestroy = OnDestroy

return UISlider