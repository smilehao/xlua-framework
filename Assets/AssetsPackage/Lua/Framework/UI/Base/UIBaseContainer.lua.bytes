--[[
-- added by wsh @ 2017-12-08
-- UI容器基类：当一个UI组件持有其它UI组件时，它就是一个容器类，它要负责调度其它UI组件的相关函数
-- 注意：
-- 1、window.view是窗口最上层的容器类
-- 2、AddComponent用来添加组件，一般在window.view的OnCreate中使用，RemoveComponent相反
-- 3、GetComponent用来获取组件，GetComponents用来获取一个类别的组件
-- 4、很重要：子组件必须保证名字互斥，即一个不同的名字要保证对应于Unity中一个不同的Transform
--]]

local UIBaseContainer = BaseClass("UIBaseContainer", UIBaseComponent)
-- 基类，用来调用基类方法
local base = UIBaseComponent

-- 创建
local function OnCreate(self)
	base.OnCreate(self)
	self.components = {}
	self.length = 0
end

-- 打开
local function OnEnable(self)
	base.OnEnable(self)
	self:Walk(function(component)
		component:OnEnable()
	end)
end

-- 遍历：注意，这里是无序的
local function Walk(self, callback, component_class)
	for _,components in pairs(self.components) do
		for cmp_class,component in pairs(components) do
			if component_class == nil then
				callback(component)
			elseif cmp_class == component_class then
				callback(component)
			end
		end
	end
end

-- 如果必要，创建新的记录，对应Unity下一个Transform下所有挂载脚本的记录表
local function AddNewRecordIfNeeded(self, name)
	if self.components[name] == nil then
		self.components[name] = {}
	end
end

-- 记录Component
local function RecordComponent(self, name, component_class, component)
	-- 同一个Transform不能挂两个同类型的组件
	assert(self.components[name][component_class] == nil, "Aready exist component_class : ", component_class.__cname)
	self.components[name][component_class] = component
end

-- 子组件改名回调
local function OnComponentSetName(self, component, new_name)
	AddNewRecordIfNeeded(self, new_name)
	-- 该名字对应Unity的Transform下挂载的所有脚本都要改名
	local old_name = component:GetName()
	local components = self.components[old_name]
	for k,v in pairs(components) do
		v:SetName(new_name)
		RecordComponent(self, new_name, k, v)
	end
	self.components[old_name] = nil
end

-- 子组件销毁
local function OnComponentDestroy(self, component)
	self.length = self.length - 1
end

-- 添加组件
-- 多种重载方式
-- 1、直接添加Lua侧组件：inst:AddComponent(ComponentTypeClass, luaComponentInst)
-- 2、指定Lua侧组件类型和必要参数，新建组件并添加，多种重载方式：
--    A）inst:AddComponent(ComponentTypeClass, relative_path)
--    B）inst:AddComponent(ComponentTypeClass, child_index)
--    C）inst:AddComponent(ComponentTypeClass, unity_gameObject)
local function AddComponent(self, component_target, var_arg, ...)
	assert(component_target.__ctype == ClassType.class)
	local component_inst = nil
	local component_class = nil
	if type(var_arg) == "table" and var_arg.__ctype == ClassType.instance then
		component_inst = var_arg
		component_class = var_arg._class_type
	else
		component_inst = component_target.New(self, var_arg)
		component_class = component_target
		component_inst:OnCreate(...)
	end
	
	local name = component_inst:GetName()
	AddNewRecordIfNeeded(self, name)
	RecordComponent(self, name, component_class, component_inst)
	self.length = self.length + 1
	return component_inst
end

-- 获取组件
local function GetComponent(self, name, component_class)
	local components = self.components[name]
	if components == nil then
		return nil
	end
	
	if component_class == nil then
		-- 必须只有一个组件才能不指定类型，这一点由外部代码保证
		assert(table.count(components) == 1, "Must specify component_class while there are more then one component!")
		for _,component in pairs(components) do
			return component
		end
	else
		return components[component_class]
	end
end

-- 获取一系列组件：2种重载方式
-- 1、获取一个类别的组件
-- 2、获取某个name（Transform）下的所有组件
local function GetComponents(self, component_target)
	local components = {}
	if type(component_target) == "table" then
		self:Walk(function(component)
			table.insert(component)
		end, component_target)
	elseif type(component_target) == "string" then
		components = self.components[component_target]
	else
		error("GetComponents params err!")
	end
	return components
end

-- 获取组件个数
local function GetComponentsCount(self)
	return self.length
end

-- 移除组件
local function RemoveComponent(self, name, component_class)
	local component = self:GetComponent(name, component_class)
	if component ~= nil then
		local cmp_class = component._class_type
		component:Delete()
		self.components[name][cmp_class] = nil
	end
end

-- 移除一系列组件：2种重载方式
-- 1、移除一个类别的组件
-- 2、移除某个name（Transform）下的所有组件
local function RemoveComponents(self, component_target)
	local components = self:GetComponents(component_target)
	for _,component in pairs(components) do
		local cmp_name = component:GetName()
		local cmp_class = component._class_type
		component:Delete()
		self.components[cmp_name][cmp_class] = nil
	end
	return components
end

-- 关闭
local function OnDisable(self)
	base.OnDisable(self)
	self:Walk(function(component)
		component:OnDisable()
	end)
end

-- 销毁
local function OnDestroy(self)
	self:Walk(function(component)
		-- 说明：现在一个组件可以被多个容器持有，但是holder只有一个，所以由holder去释放
		if component.holder == self then
			component:Delete()
		end
	end)
	self.components = nil
	base.OnDestroy(self)
end

UIBaseContainer.OnCreate = OnCreate
UIBaseContainer.OnEnable = OnEnable
UIBaseContainer.Walk = Walk
UIBaseContainer.OnComponentSetName = OnComponentSetName
UIBaseContainer.OnComponentDestroy = OnComponentDestroy
UIBaseContainer.AddComponent = AddComponent
UIBaseContainer.GetComponent = GetComponent
UIBaseContainer.GetComponents = GetComponents
UIBaseContainer.GetComponentsCount = GetComponentsCount
UIBaseContainer.RemoveComponent = RemoveComponent
UIBaseContainer.RemoveComponents = RemoveComponents
UIBaseContainer.OnDisable = OnDisable
UIBaseContainer.OnDestroy = OnDestroy

return UIBaseContainer