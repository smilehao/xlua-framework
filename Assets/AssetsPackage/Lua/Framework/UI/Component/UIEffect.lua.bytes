--[[
-- added by wsh @ 2017-12-13
-- Lua侧UI特效组件
--]]

local UIEffect = BaseClass("UIEffect", UIBaseComponent)
local base = UIBaseComponent

-- 创建
local function OnCreate(self, relative_order, effect_config, create_callback)
	base.OnCreate(self)
	
	-- order
	self.relative_order = relative_order or 0
	self.effect = BaseEffect.New(self.transform, effect_config, function()
		if not IsNull(self.effect.gameObject) then
			local trans = self.effect.transform
			local rectTransform = UIUtil.FindComponent(trans, typeof(CS.UnityEngine.RectTransform))
			if not IsNull(rectTransform) then
				-- 初始化RectTransform
				rectTransform.offsetMax = Vector2.zero
				rectTransform.offsetMin = Vector2.zero
				rectTransform.localScale = Vector3.one
				rectTransform.localPosition = Vector3.zero
			end
			
			self:SetOrder(self.relative_order)
			if create_callback ~= nil then
				create_callback()
			end
		end
	end)
	
	if effect_config == nil then
		self:SetOrder(self.relative_order)
	end
end

-- 激活
local function OnEnable(self)
	base.OnEnable(self)
	self:SetOrder(self.relative_order)
end

-- 获取层级内order
local function GetOrder(self)
	return self.relative_order
end

-- 设置层级内order
local function SetOrder(self, relative_order)
	assert(type(relative_order) == "number", "Relative order must be nonnegative number!")
	assert(relative_order >= 0, "Relative order must be nonnegative number!")
	assert(relative_order < UIManager:GetInstance().MaxOrderPerWindow, "Relative order larger then MaxOrderPerWindow!")
	self.relative_order = relative_order
	self.effect:SetSortingOrder(self.view.base_order + relative_order)
	self.effect:SetSortingLayerName(SortingLayerNames.UI)
end

-- 销毁
local function OnDestroy(self)
	self.effect:Delete()
	base.OnDestroy(self)
end

UIEffect.OnCreate = OnCreate
UIEffect.OnEnable = OnEnable
UIEffect.GetOrder = GetOrder
UIEffect.SetOrder = SetOrder
UIEffect.OnDestroy = OnDestroy

return UIEffect