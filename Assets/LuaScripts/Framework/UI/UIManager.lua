--[[
-- added by wsh @ 2017-11-30
-- UI管理系统：提供UI操作、UI层级、UI消息、UI资源加载、UI调度、UI缓存等管理
-- 注意：
-- 1、Window包括：Model、Ctrl、View、和Active状态等构成的一个整体概念
-- 2、所有带Window接口的都是操作整个窗口，如CloseWindow以后：整个窗口将不再活动
-- 3、所有带View接口的都是操作视图层展示，如CloseView以后：View、Model依然活跃，只是看不见，可看做切入了后台
-- 4、如果只是要监听数据，可以创建不带View、Ctrl的后台窗口，配置为nil，比如多窗口需要共享某控制model（配置为后台窗口）
-- 5、可将UIManager看做一个挂载在UIRoot上的不完全UI组件，但是它是Singleton，不使用多重继承，UI组件特性隐式实现
--]]

local Messenger = require "Framework.Common.Messenger"
local UIManager = BaseClass("UIManager", Singleton)

-- UIRoot路径
local UIRootPath = "UIRoot"
-- EventSystem路径
local EventSystemPath = "EventSystem"
-- UICamera路径
local UICameraPath = UIRootPath.."/UICamera"
-- 分辨率
local Resolution = Vector2.New(1024, 960)
-- 窗口最大可使用的相对order_in_layer
local MaxOderPerWindow = 10

-- 构造函数
local function __init(self)
	-- 成员变量
	-- 消息中心
	self.ui_message_center = Messenger.New()
	-- 所有存活的窗体
	self.windows = {}
	-- 所有可用的层级
	self.layers = {}
	-- 保持Model
	self.keep_model = {}
	
	-- 初始化组件
	self.gameObject = CS.UnityEngine.GameObject.Find(UIRootPath)
	self.transform = self.gameObject.transform
	self.camera_go = CS.UnityEngine.GameObject.Find(UICameraPath)
	self.UICamera = self.camera_go:GetComponent(typeof(CS.UnityEngine.Camera))
	self.Resolution = Resolution
	self.MaxOderPerWindow = MaxOderPerWindow
	CS.UnityEngine.Object.DontDestroyOnLoad(self.gameObject)
	local event_system = CS.UnityEngine.GameObject.Find(EventSystemPath)
	CS.UnityEngine.Object.DontDestroyOnLoad(event_system)
	assert(not IsNull(self.transform))
	assert(not IsNull(self.UICamera))
	
	-- 初始化层级
	local layers = table.choose(Config.Debug and getmetatable(UILayers) or UILayers, function(k, v)
		return type(v) == "table" and not IsNull(v.OrderInLayer) and not IsNull(v.Name) and type(v.Name) == "string" and #v.Name > 0
	end)
	table.walksort(layers, function(lkey, rkey)
		return layers[lkey].OrderInLayer < layers[rkey].OrderInLayer
	end, function(index, layer)
		assert(IsNull(self.layers[layer]), "Aready exist layer : "..layer.Name)
		local go = CS.UnityEngine.GameObject(layer.Name)
		local trans = go.transform
		trans:SetParent(self.transform)
		local new_layer = UILayer.New(self, layer.Name)
		new_layer:OnCreate(layer)
		self.layers[layer.Name] = new_layer
	end)
end

-- 注册消息
local function AddListener(self, e_type, e_listener, ...)
	self.ui_message_center:AddListener(e_type, e_listener, ...)
end

-- 发送消息
local function Broadcast(self, e_type, ...)
	self.ui_message_center:Broadcast(e_type, ...)
end

-- 注销消息
local function RemoveListener(self, e_type, e_listener)
	self.ui_message_center:RemoveListener(e_type, e_listener)
end

-- 获取窗口
local function GetWindow(self, ui_name, active, view_active)
	local target = self.windows[ui_name]
	if IsNull(target) then
		return nil
	end
	if not IsNull(active) and target.Active ~= active then
		return nil
	end
	if not IsNull(view_active) and target.View:GetActive() ~= view_active then
		return nil
	end
	return target
end

-- 初始化窗口
local function InitWindow(self, ui_name, window)
	local config = UIConfig[ui_name]
	assert(config, "No window named : "..ui_name..".You should add it to UIConfig first!")
	
	local layer = self.layers[config.Layer.Name]
	assert(layer, "No layer named : "..config.Layer.Name..".You should create it first!")
	
	window.Name = ui_name
	window.Model = self.keep_model[ui_name]
	if not window.Model and not IsNull(config.Model) then
		window.Model = config.Model.New(ui_name)
	end
	if not IsNull(config.Ctrl) then
		window.Ctrl = config.Ctrl.New(window.Model)
	end
	if not IsNull(config.View) then
		window.View = config.View.New(layer, window.Name, window.Model, window.Ctrl)
	end
	window.Active = false
	window.Layer = layer
	window.PrefabPath = config.PrefabPath
	
	self:Broadcast(UIMessageNames.UIFRAME_ON_WINDOW_CREATE, window)
	return window
end

-- 激活窗口
local function ActivateWindow(self, target, ...)
	assert(not IsNull(target))
	assert(target.IsLoading == false, "You can only activate window after prefab locaded!")
	target.Model:Activate(...)
	target.View:SetActive(true)
	self:Broadcast(UIMessageNames.UIFRAME_ON_WINDOW_OPEN, target)
end

-- 反激活窗口
local function Deactivate(self, target)
	target.Model:Deactivate()
	target.View:SetActive(false)
	self:Broadcast(UIMessageNames.UIFRAME_ON_WINDOW_CLOSE, target)
end

-- 打开窗口：私有，必要时准备资源
local function InnerOpenWindow(self, target, ...)
	assert(not IsNull(target))
	assert(not IsNull(target.Model))
	assert(not IsNull(target.Ctrl))
	assert(not IsNull(target.View))
	assert(target.Active == false, "You should close window before open again!")
	
	target.Active = true
	local has_view = target.View ~= UIBaseView
	local has_prefab_res = not IsNull(target.PrefabPath) and #target.PrefabPath > 0
	local has_loaded = not IsNull(target.View.gameObject)
	local need_load = has_view and has_prefab_res and not has_loaded
	if not need_load then
		ActivateWindow(self, target, ...)
	elseif not target.IsLoading then
		target.IsLoading = true
		local params = SafePack(...)
		ResourcesManager:GetInstance():LoadAsync(target.PrefabPath, function(go)
			if IsNull(go) then
				Logger.LogError("Resources load err : Window "..target.Name.." gameObject nil!")
				return
			end
			
			local trans = go.transform
			trans:SetParent(target.Layer.transform)
			trans.name = target.Name
			
			target.IsLoading = false
			target.View:OnCreate()
			if target.Active then
				ActivateWindow(self, target, SafeUnpack(params))
			end
		end)
	end
end

-- 关闭窗口：私有
local function InnerCloseWindow(self, target)
	assert(not IsNull(target))
	assert(not IsNull(target.Model))
	assert(not IsNull(target.Ctrl))
	assert(not IsNull(target.View))
	if target.Active then
		Deactivate(self, target)
		target.Active = false
	end
end

-- 打开窗口：公有
local function OpenWindow(self, ui_name, ...)
	local target = self:GetWindow(ui_name)
	if IsNull(target) then
		local window = UIWindow.New()
		self.windows[ui_name] = window
		target = InitWindow(self, ui_name, window)
	end
	
	-- 先关闭
	InnerCloseWindow(self, target)
	InnerOpenWindow(self, target, ...)
end

-- 关闭窗口：公有
local function CloseWindow(self, ui_name)
	local target = self:GetWindow(ui_name, true)
	if IsNull(target) then
		return
	end
	
	InnerCloseWindow(self, target)
end

-- 关闭层级所有窗口
local function CloseWindowByLayer(self, layer)
	for _,v in pairs(self.windows) do
		if v.Layer:GetName() == layer.Name then
			InnerCloseWindow(self, v)
		end
	end
end

-- 关闭其它层级窗口
local function CloseWindowExceptLayer(self, layer)
	for _,v in pairs(self.windows) do
		if v.Layer:GetName() ~= layer.Name then
			InnerCloseWindow(self, v)
		end
	end
end

-- 关闭所有窗口
local function CloseAllWindows(self)
	for _,v in pairs(self.windows) do
		InnerCloseWindow(self, v)
	end
end

-- 展示窗口
local function OpenView(self, ui_name, ...)
	local target = self:GetWindow(ui_name)
	assert(not IsNull(target), "Try to show a window that does not exist: "..ui_name)
	if not target.View:GetActive() then
		target.View:SetActive(true)
	end
end

-- 隐藏窗口
local function CloseView(self, ui_name)
	local target = self:GetWindow(ui_name)
	assert(not IsNull(target), "Try to hide a window that does not exist: "..ui_name)
	if target.View:GetActive() then
		target.View:SetActive(false)
	end
end

local function InnerDelete(plugin)
	if plugin.__ctype == ClassType.instance then
		plugin:Delete()
	end
end

local function InnerDestroyWindow(self, ui_name, target, include_keep_model)
	self:Broadcast(UIMessageNames.UIFRAME_ON_WINDOW_DESTROY, target)
	if include_keep_model then
		self.keep_model[ui_name] = nil
		InnerDelete(target.Model)
	elseif not self.keep_model[ui_name] then
		InnerDelete(target.Model)
	end
	InnerDelete(target.Ctrl)
	InnerDelete(target.View)
	self.windows[ui_name] = nil
end

-- 销毁窗口
local function DestroyWindow(self, ui_name, include_keep_model)
	local target = self:GetWindow(ui_name)
	if IsNull(target) then
		return
	end
	
	InnerCloseWindow(self, target)
	InnerDestroyWindow(self, ui_name, target, include_keep_model)
end

-- 销毁层级所有窗口
local function DestroyWindowByLayer(self, layer, include_keep_model)
	for k,v in pairs(self.windows) do
		if v.Layer:GetName() == layer.Name then
			InnerCloseWindow(self, v)
			InnerDestroyWindow(self, k, v, include_keep_model)
		end
	end
end

-- 销毁其它层级窗口
local function DestroyWindowExceptLayer(self, layer, include_keep_model)
	for k,v in pairs(self.windows) do
		if v.Layer:GetName() ~= layer.Name then
			InnerCloseWindow(self, v)
			InnerDestroyWindow(self, k, v, include_keep_model)
		end
	end
end

-- 销毁所有窗口
local function DestroyAllWindow(self, ui_name, include_keep_model)
	for k,v in pairs(self.windows) do
		InnerCloseWindow(self, v)
		InnerDestroyWindow(self, k, v, include_keep_model)
	end
end

-- 设置是否保持Model
local function SetKeepModel(self, ui_name, keep)
	local target = self:GetWindow(ui_name)
	assert(not IsNull(target), "Try to keep a model that window does not exist: "..ui_name)
	if keep then
		self.keep_model[target.Name] = target.Model
	else
		self.keep_model[target.Name] = nil
	end
end

-- 获取保持的Model
local function GetKeepModel(self, ui_name)
	return self.keep_model[ui_name]
end

-- 析构函数
local function __delete(self)
	self.ui_message_center = nil
	self.windows = nil
	self.layers = nil
	self.keep_model = nil
end

UIManager.__init = __init
UIManager.AddListener = AddListener
UIManager.Broadcast = Broadcast
UIManager.RemoveListener = RemoveListener
UIManager.GetWindow = GetWindow
UIManager.OpenWindow = OpenWindow
UIManager.CloseWindow = CloseWindow
UIManager.CloseWindowByLayer = CloseWindowByLayer
UIManager.CloseWindowExceptLayer = CloseWindowExceptLayer
UIManager.CloseAllWindows = CloseAllWindows
UIManager.OpenView = OpenView
UIManager.CloseView = CloseView
UIManager.DestroyWindow = DestroyWindow
UIManager.DestroyWindowByLayer = DestroyWindowByLayer
UIManager.DestroyWindowExceptLayer = DestroyWindowExceptLayer
UIManager.DestroyAllWindow = DestroyAllWindow
UIManager.SetKeepModel = SetKeepModel
UIManager.GetKeepModel = GetKeepModel
UIManager.__delete = __delete

return UIManager;