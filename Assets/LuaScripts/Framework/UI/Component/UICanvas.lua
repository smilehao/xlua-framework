--[[
-- added by wsh @ 2017-12-13
-- Lua侧UICanvas：
-- 注意：
-- 1、为了调整UI层级，所以这里的overrideSorting设置为true
-- 2、如果只是类似NGUI的Panel那样划分drawcall管理，直接在预设上添加Canvas，并设置overrideSorting为false
-- 3、这里的order是相对于window.view中base_order的差量，窗口内的order最多为10个---UIManager中配置
-- 4、旧窗口内所有canvas的real_order都应该在新窗口之下，即保证旧窗口内包括UI特效在内的所有组件，不会跑到新窗口之上
-- 5、UI逻辑代码禁止手动直接设置Unity侧Cavans组件的orderInLayer，全部使用本脚本接口调整层级，避免层级混乱
--]]

local UICanvas = BaseClass("UICanvas", UIBaseComponent)
local base = UIBaseComponent

-- 创建
local function OnCreate(self, relative_order)
	base.OnCreate(self)
	-- Unity侧原生组件
	self.unity_canvas = nil
	self.unity_graphic_raycaster = nil
	
	-- canvas
	self.unity_canvas = UIUtil.FindComponent(self.transform, typeof(CS.UnityEngine.Canvas))
	if IsNull(self.unity_canvas) then
		self.unity_canvas = self.gameObject:AddComponent(typeof(CS.UnityEngine.Canvas))
	end
	self.unity_canvas.overrideSorting = true
	self.unity_canvas.sortingLayerName = SortingLayerNames.UI
	
	-- raycaster
	self.unity_graphic_raycaster = UIUtil.FindComponent(self.transform, typeof(CS.UnityEngine.UI.GraphicRaycaster))
	if IsNull(self.unity_graphic_raycaster) then
		self.unity_graphic_raycaster = self.gameObject:AddComponent(typeof(CS.UnityEngine.UI.GraphicRaycaster))
	end
	
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
	assert(relative_order < UIManager:GetInstance().MaxOrderPerWindow, "Relative order larger then MaxOrderPerWindow!")
	self.relative_order = relative_order
	self.unity_canvas.sortingOrder = self.view.base_order + relative_order
end

-- 销毁
local function OnDestroy(self)
	self.unity_canvas = nil
	self.unity_graphic_raycaster = nil
	base.OnDestroy(self)
end


UICanvas.OnCreate = OnCreate
UICanvas.OnEnable = OnEnable
UICanvas.GetOrder = GetOrder
UICanvas.SetOrder = SetOrder
UICanvas.OnDestroy = OnDestroy

return UICanvas