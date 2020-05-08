--[[
-- added by wsh @ 2017-12-08
-- Lua侧UILayer
--]]

local UILayer = BaseClass("UILayer", UIBaseComponent)
local base = UIBaseComponent

-- 创建
local function OnCreate(self, layer)
	base.OnCreate(self)
	-- Unity侧原生组件
	self.unity_canvas = nil
	self.unity_canvas_scaler = nil
	self.unity_graphic_raycaster = nil
	
	-- ui layer
	self.gameObject.layer = 5
	
	-- canvas
	self.unity_canvas = UIUtil.FindComponent(self.transform, typeof(CS.UnityEngine.Canvas))
	if IsNull(self.unity_canvas) then
		self.unity_canvas = self.gameObject:AddComponent(typeof(CS.UnityEngine.Canvas))
		-- 说明：很坑爹，这里添加UI组件以后transform会Unity被替换掉，必须重新获取
		self.transform = self.unity_canvas.transform
		self.gameObject = self.unity_canvas.gameObject
	end
	self.unity_canvas.renderMode = CS.UnityEngine.RenderMode.ScreenSpaceCamera
	self.unity_canvas.worldCamera = UIManager:GetInstance().UICamera
	self.unity_canvas.planeDistance = layer.PlaneDistance
	self.unity_canvas.sortingLayerName = SortingLayerNames.UI
	self.unity_canvas.sortingOrder = layer.OrderInLayer
	
	-- scaler
	self.unity_canvas_scaler = UIUtil.FindComponent(self.transform, typeof(CS.UnityEngine.UI.CanvasScaler))
	if IsNull(self.unity_canvas_scaler) then
		self.unity_canvas_scaler = self.gameObject:AddComponent(typeof(CS.UnityEngine.UI.CanvasScaler))
	end
	self.unity_canvas_scaler.uiScaleMode = CS.UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize
	self.unity_canvas_scaler.screenMatchMode = CS.UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
	self.unity_canvas_scaler.referenceResolution = UIManager:GetInstance().Resolution
	
	-- raycaster
	self.unity_graphic_raycaster = UIUtil.FindComponent(self.transform, typeof(CS.UnityEngine.UI.GraphicRaycaster))
	if IsNull(self.unity_graphic_raycaster) then
		self.unity_graphic_raycaster = self.gameObject:AddComponent(typeof(CS.UnityEngine.UI.GraphicRaycaster))
	end
	
	-- window order
	self.top_window_order = layer.OrderInLayer
	self.min_window_order = layer.OrderInLayer
end

-- pop window order
local function PopWindowOrder(self)
	local cur = self.top_window_order
	self.top_window_order = self.top_window_order + UIManager:GetInstance().MaxOrderPerWindow
	return cur
end

-- push window order
local function PushWindowOrder(self)
	assert(self.top_window_order > self.min_window_order)
	self.top_window_order = self.top_window_order - UIManager:GetInstance().MaxOrderPerWindow
end

-- 销毁
local function OnDestroy(self)
	self.unity_canvas = nil
	self.unity_canvas_scaler = nil
	self.unity_graphic_raycaster = nil
	base.OnDestroy(self)
end


UILayer.OnCreate = OnCreate
UILayer.PopWindowOrder = PopWindowOrder
UILayer.PushWindowOrder = PushWindowOrder
UILayer.OnDestroy = OnDestroy

return UILayer