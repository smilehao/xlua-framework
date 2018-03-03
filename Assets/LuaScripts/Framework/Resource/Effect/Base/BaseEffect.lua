--[[
-- added by wsh @ 2018-01-08
-- 特效基类：提供特效组件的基础功能
--]]

local BaseEffect = BaseClass("BaseEffect")

-- 获取渲染器
local function GetRenderers(self)
	local tmp = self.gameObject:GetComponentsInChildren(typeof(CS.UnityEngine.Renderer), true)
	for i = 0, tmp.Length - 1 do
		table.insert(self.renderers, tmp[i])
	end
	assert(table.count(self.renderers) > 0)
end

-- 初始化特效：资源已经被加载出来
local function InitEffect(self, go)
	if IsNull(go) then
		return
	end
	
	self.gameObject = go
	self.transform = go.transform
	if not IsNull(self.parent_trans) then
		self.transform:SetParent(self.parent_trans)
	end
	self.transform.localPosition = Vector3.zero
	self.transform.localEulerAngles = Vector3.zero
	self.transform.localScale = Vector3.one
	
	GetRenderers(self)
	
	-- 配置
	if not self.config.IsLoop and self.config.LiveTime > 0 then
		self.timer = TimerManager:GetInstance():GetTimer(self.config.LiveTime, self.timer_action , self)
		self.timer:Start()
	end
end

-- 计时器到
local function TimerAction(self)
	if self ~= nil then
		self:Delete()
	end
end

-- 构造函数：无特殊情况不要重写，子类销毁逻辑放OnCreate
local function __init(self, parent_trans, effect_config, create_callback)
	self.config = effect_config
	-- Unity侧原生组件
	self.parent_trans = parent_trans
	self.gameObject = nil
	self.transform = nil
	self.renderers = {}
	self.sortingLayerName = nil
	self.sortingOrder = nil
	self.timer = nil
	self.timer_action = TimerAction
	-- 回调：用于组合方式
	self.create_callback = create_callback
	
	if effect_config == nil then
		self.gameObject = parent_trans.gameObject
		self.transform = parent_trans
		GetRenderers(self)
	else
		-- 资源加载
		GameObjectPool:GetInstance():GetGameObjectAsync(effect_config.EffectPath, function(go, self)
			if self ~= nil then
				InitEffect(self, go)
				self:OnCreate()
				if self.create_callback then
					self.create_callback(go)
				end
			end
		end, self)
	end
end

-- 析构函数：无特殊情况不要重写，子类销毁逻辑放OnDestroy
local function __delete(self)
	self:OnDestroy()
	-- 回收资源
	if effect_config and not IsNull(self.gameObject) then
		GameObjectPool:GetInstance():RecycleGameObject(self.config.EffectPath, self.gameObject)
	end
	-- 释放引用
	self.config = nil
	self.gameObject = nil
	self.transform = nil
	self.renderers = nil
	self.sortingLayerName = nil
	self.sortingOrder = nil
	self.timer = nil
	self.timer_action = nil
end

-- 创建：子类继承
local function OnCreate(self)
end

-- 获取sortingLayerName
local function GetSortingLayerName(self)
	return self.sortingLayerName
end

-- 设置sortingLayerName
local function SetSortingLayerName(self, sorting_layer_name)
	assert(sorting_layer_name ~= nil and type(sorting_layer_name) == "string")
	self.sortingLayerName = sorting_layer_name
	for _,renderer in pairs(self.renderers) do
		renderer.sortingLayerName = sorting_layer_name
	end
end

-- 获取sortingOrder
local function GetSortingOrder(self)
	return self.sortingOrder
end

-- 设置sortingOrder
local function SetSortingOrder(self, sorting_order)
	assert(sorting_order ~= nil and type(sorting_order) == "number")
	self.sortingOrder = sorting_order
	for _,renderer in pairs(self.renderers) do
		renderer.sortingOrder = sorting_order
	end
end

-- 销毁：子类继承
local function OnDestroy(self)
end

BaseEffect.__init = __init
BaseEffect.__delete = __delete
BaseEffect.OnCreate = OnCreate
BaseEffect.GetSortingLayerName = GetSortingLayerName
BaseEffect.SetSortingLayerName = SetSortingLayerName
BaseEffect.GetSortingOrder = GetSortingOrder
BaseEffect.SetSortingOrder = SetSortingOrder
BaseEffect.OnDestroy = OnDestroy

return BaseEffect