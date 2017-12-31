--[[
-- added by wsh @ 2017-12-13
-- Lua侧UI特效组件
-- TODO：考虑后续添加通用特效模块作，这里暂时先这么写
--]]

local UIEffect = BaseClass("UIEffect", UIBaseComponent)
local base = UIBaseComponent

-- 创建
local function OnCreate(self, relative_order)
	base.OnCreate(self)
	-- Unity侧原生组件
	self.renderers = {}
	local tmp = self.gameObject:GetComponentsInChildren(typeof(CS.UnityEngine.Renderer), true)
	for i = 0, tmp.Length - 1 do
		table.insert(self.renderers, tmp[i])
	end
	assert(table.count(self.renderers) > 0)
	
	-- order
	self.relative_order = relative_order or 0
	self:SetOrder(self.relative_order)
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
	assert(relative_order < UIManager:GetInstance().MaxOderPerWindow, "Relative order larger then MaxOderPerWindow!")
	self.relative_order = relative_order
	for _,renderer in pairs(self.renderers) do
		renderer.sortingLayerName = SortingLayerNames.UI
		renderer.sortingOrder = self.view.base_order + relative_order
	end
end

-- 销毁
local function OnDestroy(self)
	self.renderers = nil
	base.OnDestroy(self)
end

UIEffect.OnCreate = OnCreate
UIEffect.OnEnable = OnEnable
UIEffect.GetOrder = GetOrder
UIEffect.SetOrder = SetOrder
UIEffect.OnDestroy = OnDestroy

return UIEffect