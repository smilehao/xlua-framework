--[[
-- added by wsh @ 2017-12-06
-- UI按钮组通用组件：负责管理和调度组下的所有按钮
-- 注意：
-- 1、添加的组件必须是UIToggleButton、或者其子类、或者实现了它的接口的任何其它类
-- 使用方式：
-- self.xxx_btngroup = self:AddComponent(UIButtonGroup, group_path, UIToggleButtonClassType, original_name)--group_path下所有孩子生成UIToggleButtonClassType实例并加入组，默认激活original_name对应的孩子
-- self.xxx_btngroup:AddComponent(UIToggleButtonClassType, var_arg)--添加按钮，各种重载方式查看UIBaseContainer
-- self.xxx_btngroup:SetOriginal(original_name)
-- self.xxx_btngroup:SetOnClick(function(cmp, check)
-- 		-- do something
-- end)--以上，均属于初始化
-- self.xxx_btngroup:ResetToBeginning()
-- self.xxx_btngroup:ClickOn(cmp_name)
-- self.xxx_btngroup:ClickOnBindData(cmp_binddata)
--]]

local UIButtonGroup = BaseClass("UIButtonGroup", UIBaseContainer)
local base = UIBaseContainer

-- 创建
local function OnCreate(self, togglebtn_class, original_name, ...)
	assert(togglebtn_class == nil or (type(togglebtn_class) == "table" and togglebtn_class.__ctype == ClassType.class), "togglebtn_class err : "..tostring(togglebtn_class))
	assert(original_name == nil or (type(original_name) == "string" and #original_name > 0))
	base.OnCreate(self)
	
	-- 当前选中Button
	self.current = nil
	-- 初始选中Button的名字
	self.original_name = nil
	-- 点击、取消点击回调
	-- 用于托管组下所有按钮的点击事件，给外部代码提供统一的回调入口
	-- 回调原型：void __onclick(togglebtn_cmp, bool check)
	self.__onclick = nil
	
	-- 自动添加当前挂载节点下的所有孩子
	if togglebtn_class ~= nil then
		local child_count = self.transform.childCount
		for i = 0, child_count - 1 do
			self:AddComponent(togglebtn_class, i, ...)
		end
	end
	self.original_name = original_name
end

-- 设置初始选中Button名字
local function SetOriginal(self, original_name)
	assert(original_name == nil or type(original_name) == "string")
	self.original_name = original_name
end

-- 复位
local function ResetToBeginning(self)
	self:ClickOn(self.original_name)
end

-- 选中、取消选中统一回调
local function OnCheckCallback(self, cmp, check)
	if self.__onclick ~= nil then
		self.__onclick(cmp, check)
	end
end

-- 添加ToggleButton：必须保证一个名字只对应一个组件
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
	-- 注册回调
	cmp:SetOnCheck(self, OnCheckCallback, cmp)
	cmp:SetOnClick(function()
		self:ClickOn(cmp)
	end)
	-- 初始化Check状态
	cmp:SetCheck(false)
	return cmp
end

-- 子组件销毁
local function OnComponentDestroy(self, component)
	base.OnComponentDestroy(self, component)
	if self.current == component then
		self.current = nil
	end
end

-- 选中按钮：cmp_target为nil或者按钮组找不到按钮时，等效于全部按钮复位，2种重载
-- 1、传按钮名字
-- 2、传按钮对象
local function ClickOn(self, cmp_target)
	assert(cmp_name == nil or type(cmp_name) == "string")
	if self.current ~= nil then
		self.current:SetCheck(false)
	end
	
	self.current = nil
	if cmp_target == nil then
		return false
	end
	
	local cmp = nil
	if type(cmp_target) == "string" then
		cmp = self:GetComponent(cmp_target)
	elseif type(cmp_target) == "table" then
		cmp = cmp_target
	else
		error("ClickOn cmp_target type err!")
	end
	if cmp ~= nil and not IsNull(cmp.gameObject) then
		self.current = cmp
		cmp:SetCheck(true)
	end
	return self.current ~= nil
end

-- 选中按钮：使用前需要自己预先设置各个按钮的bind_data
-- 注意：
-- 1、bind_data用一个key去标识一个按钮，这个key必须自行保证各个按钮不一致，否则点中第一个符合条件的按钮
-- 2、bind_data举例：比如选服界面，有多个区，每个区若干服务器，点击区按钮切换服务器列表，则区按钮bind_data应该设置为区ID
local function ClickOnBindData(self, data)
	self:Walk(function(component)
		if component:GetBindData() == data then
			self:ClickOn(component:GetName())
			return true
		end
	end)
	return false
end

-- 按钮点击回调
local function SetOnClick(self, ...)
	self.__onclick = BindCallback(...)
end

-- 获取当前选中按钮
local function GetCurrent(self)
	return self.current
end

-- 销毁
local function OnDestroy(self)
	self.current = nil
	self.original_name = nil
	self.__onclick = nil
	base.OnDestroy(self)
end

UIButtonGroup.OnCreate = OnCreate
UIButtonGroup.SetOriginal = SetOriginal
UIButtonGroup.ResetToBeginning = ResetToBeginning
UIButtonGroup.AddComponent = AddComponent
UIButtonGroup.OnComponentDestroy = OnComponentDestroy
UIButtonGroup.ClickOn = ClickOn
UIButtonGroup.ClickOnBindData = ClickOnBindData
UIButtonGroup.SetOnClick = SetOnClick
UIButtonGroup.GetCurrent = GetCurrent
UIButtonGroup.OnDestroy = OnDestroy

return UIButtonGroup