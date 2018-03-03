--[[
-- added by wsh @ 2017-12-08
-- UI组件基类：所有UI组件从这里继承
-- 说明：
-- 1、采用基于组件的设计方式，容器类负责管理和调度子组件，实现类似于Unity中挂载脚本的功能
-- 2、组件对应Unity原生的各种Component和Script，容器对应Unity原生的GameObject
-- 3、写逻辑时完全不需要关注脚本调度，在cs中脚本函数怎么调度的，这里就怎么调度，只是要注意接口变动，lua侧没有Get、Set访问器
-- 注意：
-- 1、Lua侧组件的名字并不总是和Unity侧组件名字同步，Lua侧组件名字会作为组件系统中组件的标识
-- 2、Lua侧组件名字会在组件创建时提取Unity侧组件名字，随后二者没有任何关联，Unity侧组件名字可以随便改
-- 3、虽然Unity侧组件名字随后可以随意改，但是不建议（有GC），此外Lua侧组件一旦创建，使用时全部以Lua侧名字为准
-- 4、虽然支持Update、LateUpdate、FixedUpdate更新，但是UI组件最好不要使用---不要定义这些函数即可
-- 5、需要定时刷新的界面，最好启用定时器、协程，界面需要刷新的频率一般较低，倒计时之类的只需要每秒钟更新一次即可
--]]

local UIBaseComponent = BaseClass("UIBaseComponent", Updatable)
local base = Updatable

-- 构造函数：除非特殊情况，所有子类不要再写这个函数，初始化工作放OnCreate
-- 多种重载方式：
-- 1、ComponentTypeClass.New(relative_path)
-- 2、ComponentTypeClass.New(child_index)
-- 3、ComponentTypeClass.New(unity_gameObject)
local function __init(self, holder, var_arg)
	assert(not IsNull(holder), "Err : holder nil!")
	assert(not IsNull(holder.transform), "Err : holder tansform nil!")
	assert(not IsNull(var_arg), "Err: var_arg nil!")
	-- 窗口view层脚本
	self.view = nil
	-- 持有者
	self.holder = holder
	-- 脚本绑定的transform
	self.transform = nil
	-- transform对应的gameObject
	self.gameObject = nil
	-- trasnform对应的RectTransform
	self.rectTransform = nil
	-- 名字：Unity中获取Transform的名字是有GC的，而Lua侧组件大量使用了名字，所以这里缓存下
	self.__name = nil
	-- 绑定数据：在某些场景下可以提供诸多便利
	self.__bind_data = nil
	-- 可变类型参数，用于重载
	self.__var_arg = var_arg
	-- 这里一定要等资源异步加载完毕才启用Update
	self:EnableUpdate(false)
end

-- 析构函数：所有组件的子类不要再写这个函数，释放工作全部放到OnDestroy
local function __delete(self)
	self:OnDestroy()
end

-- 创建
local function OnCreate(self)
	assert(not IsNull(self.holder), "Err : holder nil!")
	assert(not IsNull(self.holder.transform), "Err : holder tansform nil!")
	-- 初始化view
	if self._class_type == UILayer then
		self.view = nil
	else
		local now_holder = self.holder
		while not IsNull(now_holder) do	
			if now_holder._class_type == UILayer then
				self.view = self
				break
			elseif not IsNull(now_holder.view) then
				self.view = now_holder.view
				break
			end
			now_holder = now_holder.holder
		end
		assert(not IsNull(self.view))
	end
	
	-- 初始化其它基本信息
	if type(self.__var_arg) == "string" then
		-- 与持有者的相对路径
		self.transform = UIUtil.FindTrans(self.holder.transform, self.__var_arg)
		self.gameObject = self.transform.gameObject
	elseif type(self.__var_arg) == "number" then
		-- 持有者第index个孩子
		self.transform = UIUtil.GetChild(self.holder.transform, self.__var_arg)
		self.gameObject = self.transform.gameObject
	elseif type(self.__var_arg) == "userdata" then
		-- Unity侧GameObject
		self.gameObject = self.__var_arg
		self.transform = gameObject.transform
	else
		error("OnCreate : error params list! "..type(self.__var_arg).." "..tostring(self.__var_arg))
	end
	self.__name = self.gameObject.name
	self.rectTransform = UIUtil.FindComponent(self.transform, typeof(CS.UnityEngine.RectTransform))
	self.__var_arg = nil
end

-- 打开
local function OnEnable(self)
	-- 启用更新函数
	self:EnableUpdate(true)
end

-- 获取名字
local function GetName(self)
	return self.__name
end

-- 设置名字：toUnity指定是否同时设置Unity侧的名字---不建议，实在想不到什么情况下会用，但是调试模式强行设置，好调试
local function SetName(self, name, toUnity)
	if self.holder.OnComponentSetName ~= nil then
		self.holder:OnComponentSetName(self, name)
	end
	self.__name = name
	if toUnity or Config.Debug then
		if IsNull(self.gameObject) then
			Logger.LogError("gameObject null, you maybe have to wait for loading prefab finished!")
			return
		end
		self.gameObject.__name = name
	end
end

-- 设置绑定数据
local function SetBindData(self, data)
	self.__bind_data = data
end

-- 获取绑定数据
local function GetBindData(self)
	return self.__bind_data
end

-- 激活、反激活
local function SetActive(self, active)
	if active then
		self.gameObject:SetActive(active)
		self:OnEnable()
	else
		self:OnDisable()
		self.gameObject:SetActive(active)
	end
end

-- 获取激活状态
local function GetActive(self)
	return self.gameObject.activeSelf
end

-- 等待资源准备完毕：用于协程
local function WaitForCreated(self)
	coroutine.waituntil(function()
		return not IsNull(self.gameObject)
	end)
end

-- 关闭
local function OnDisable(self)
	-- 禁用更新函数
	self:EnableUpdate(false)
end

-- 销毁
local function OnDestroy(self)
	if self.holder.OnComponentDestroy ~= nil then
		self.holder:OnComponentDestroy(self)
	end
	self.holder = nil
	self.transform = nil
	self.gameObject = nil
	self.rectTransform = nil
	self.__name = nil
	self.__bind_data = nil
end

UIBaseComponent.__init = __init
UIBaseComponent.__delete = __delete
UIBaseComponent.OnCreate = OnCreate
UIBaseComponent.OnEnable = OnEnable
UIBaseComponent.GetName = GetName
UIBaseComponent.SetName = SetName
UIBaseComponent.SetBindData = SetBindData
UIBaseComponent.GetBindData = GetBindData
UIBaseComponent.SetActive = SetActive
UIBaseComponent.GetActive = GetActive
UIBaseComponent.WaitForCreated = WaitForCreated
UIBaseComponent.OnDisable = OnDisable
UIBaseComponent.OnDestroy = OnDestroy

return UIBaseComponent