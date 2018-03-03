--[[
-- added by wsh @ 2017-11-30
-- UI模型层基类：该界面相关数据，同时负责消息定制
-- 注意：
-- 1、数据大体分为两类：游戏逻辑数据、界面控制数据
-- 2、游戏逻辑数据：从游戏数据中心取数据，这里不做为数据源，只做中转和必要处理（如排序）---游戏中心数据改动以后在这里监听变化
-- 3、界面控制数据：一般会影响到多个界面展示的控制数据，登陆界面显示当前服务器，当受到选服界面操作的影响
-- 4、界面Model层对View层是只读不写的，一定不要在View层写Model
-- 5、界面Model层不依赖Ctrl层和View层，意思是说拿掉这这两层代码，Model依旧能完好运行
-- 6、界面Model层数据只影响UI，不影响游戏逻辑，游戏逻辑不能从Model层取数据，意思是没有界面，游戏依旧能跑
--]]

local UIBaseModel = BaseClass("UIBaseModel")

-- 如非必要，别重写构造函数，使用OnCreate初始化
local function __init(self, ui_name)
	-- 回调管理，使其最长保持和Model等同的生命周期
	self.__ui_callback = {}
	self.__data_callback = {}
	self.__ui_name = ui_name
	self:OnCreate()
end

-- 如非必要，别重写析构函数，使用OnDestroy销毁资源
local function __delete(self)
	self:OnDestroy()
	for k,v in pairs(self.__ui_callback) do
		self:RemoveUIListener(k, v)
	end
	for k,v in pairs(self.__data_callback) do
		self:RemoveDataListener(k, v)
	end
	self.__ui_callback = nil
	self.__data_callback = nil
	self.__ui_name = nil
end

-- 创建：变量定义，初始化，消息注册
-- 注意：窗口生命周期内保持的成员变量放这
local function OnCreate(self)
end

-- 打开：刷新数据模型
-- 注意：窗口关闭时可以清理的成员变量放这
local function OnEnable(self, ...)
end

-- 关闭
-- 注意：必须清理OnEnable中声明的变量
local function OnDisable(self)
end

-- 销毁
-- 注意：必须清理OnCreate中声明的变量
local function OnDestroy(self)
end

-- 注册消息
local function OnAddListener(self)
end

-- 注销消息
local function OnRemoveListener(self)
end

-- 激活：给UIManager用，别重写
local function Activate(self, ...)
	self:OnAddListener()
	self:OnEnable(...)
end

-- 反激活：给UIManager用，别重写
local function Deactivate(self)
	self:OnRemoveListener()
	self:OnDisable()
end

local function AddCallback(keeper, msg_name, callback)
	assert(callback ~= nil)
	keeper[msg_name] = callback
end

local function GetCallback(keeper, msg_name)
	return keeper[msg_name]
end

local function RemoveCallback(keeper, msg_name, callback)
	assert(callback ~= nil)
	keeper[msg_name] = nil
end

-- 注册UI数据监听事件，别重写
local function AddUIListener(self, msg_name, callback)
	local bindFunc = Bind(self, callback)
	AddCallback(self.__ui_callback, msg_name, bindFunc)
	UIManager:GetInstance():AddListener(msg_name, bindFunc)
end

-- 发送UI数据变动事件，别重写
local function UIBroadcast(self, msg_name, ...)
	UIManager:GetInstance():Broadcast(msg_name, ...)
end

-- 注销UI数据监听事件，别重写
local function RemoveUIListener(self, msg_name, callback)
	local bindFunc = GetCallback(self.__ui_callback, msg_name)
	RemoveCallback(self.__ui_callback, msg_name, bindFunc)
	UIManager:GetInstance():RemoveListener(msg_name, bindFunc)
end

-- 注册游戏数据监听事件，别重写
local function AddDataListener(self, msg_name, callback)
	local bindFunc = Bind(self, callback)
	AddCallback(self.__data_callback, msg_name, bindFunc)
	DataManager:GetInstance():AddListener(msg_name, bindFunc)
end

-- 注销游戏数据监听事件，别重写
local function RemoveDataListener(self, msg_name, callback)
	local bindFunc = GetCallback(self.__data_callback, msg_name)
	RemoveCallback(self.__data_callback, msg_name, bindFunc)
	DataManager:GetInstance():RemoveListener(msg_name, bindFunc)
end

UIBaseModel.__init = __init
UIBaseModel.__delete = __delete
UIBaseModel.OnCreate = OnCreate
UIBaseModel.OnEnable = OnEnable
UIBaseModel.OnDisable = OnDisable
UIBaseModel.OnDestroy = OnDestroy
UIBaseModel.OnAddListener = OnAddListener
UIBaseModel.OnRemoveListener = OnRemoveListener
UIBaseModel.Activate = Activate
UIBaseModel.Deactivate = Deactivate
UIBaseModel.AddUIListener = AddUIListener
UIBaseModel.UIBroadcast = UIBroadcast
UIBaseModel.RemoveUIListener = RemoveUIListener
UIBaseModel.AddDataListener = AddDataListener
UIBaseModel.RemoveDataListener = RemoveDataListener

return UIBaseModel