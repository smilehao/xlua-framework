--[[
-- added by wsh @ 2017-12-06
-- UI通用组件：标签组
-- 说明：用于管理互斥显示的组件---有且只有一个显示，其它隐藏
-- 使用方式：
-- self.xxx_tabgroup = self:AddComponent(UITabGroup, group_path, original_name)--group_path下所有孩子加入组，默认激活original_name对应的孩子
-- self.xxx_tabgroup:AddComponent(UIComponentTypeClass, var_arg)--添加孩子，各种重载方式查看UIBaseContainer
-- self.xxx_tabgroup:SetOriginal(original_name)--以上，均属于初始化
-- self.xxx_tabgroup:ResetToBeginning()
-- self.xxx_tabgroup:Activate(cmp_name)
--]]

local UITabGroup = BaseClass("UITabGroup", UIBaseContainer)
local base = UIBaseContainer

-- 自动添加当前挂载节点下的所有孩子
local function OnCreate(self, original_name)
	base.OnCreate(self)
	
	-- 当前选中Tab
	self.current = nil
	-- 初始选中Tab的名字
	self.original_name = nil
	
	local child_count = self.transform.childCount
	for i = 0, child_count - 1 do
		self:AddComponent(UIBaseComponent, i)
	end
	self.original_name = original_name
end

-- 设置初始Tab名字
local function SetOriginal(self, original_name)
	self.original_name = original_name
end

-- 复位
local function ResetToBeginning(self)
	if self.original_name ~= nil then
		self:Activate(self.original_name)
	end
end

-- 添加Tab：必须保证一个名字只对应一个组件
local function AddComponent(self, component_target, var_arg, ...)
	local cmp = base.AddComponent(self, component_target, var_arg, ...)
	-- 调试模式下强行检测，避免手误
	if Config.Debug then
		local lookup_table = {}
		self:Walk(function(componet)
			local cmp_name = componet:GetName()
			assert(lookup_table[cmp_name] == nil, "Aready exists component named : "..cmp_name)
			lookup_table[cmp_name] = true
		end)
	end
	return cmp
end

-- 子组件销毁
local function OnComponentDestroy(self, component)
	base.OnComponentDestroy(self, component)
	if self.current == component then
		self.current = nil
	end
end

-- 激活子Tab
local function Activate(self, cmp_name)
	assert(cmp_name == nil or type(cmp_name) == "string")
	if self.current ~= nil then
		self.current:SetActive(false)
	end
	
	self.current = nil
	local cmp = self:GetComponent(cmp_name)
	if cmp ~= nil and not IsNull(cmp.gameObject) then
		self.current = cmp
		cmp:SetActive(true)
	end
	return self.current ~= nil
end

-- 获取当前激活Tab
local function GetCurrent(self)
	return self.current
end

-- 销毁
local function OnDestroy(self)
	self.current = nil
	self.original_name = nil
	base.OnDestroy(self)
end

UITabGroup.OnCreate = OnCreate
UITabGroup.SetOriginal = SetOriginal
UITabGroup.ResetToBeginning = ResetToBeginning
UITabGroup.AddComponent = AddComponent
UITabGroup.OnComponentDestroy = OnComponentDestroy
UITabGroup.Activate = Activate
UITabGroup.GetCurrent = GetCurrent
UITabGroup.OnDestroy = OnDestroy

return UITabGroup