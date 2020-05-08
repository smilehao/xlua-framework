--[[
-- added by wsh @ 2017-11-30
-- 1、加载全局模块，所有全局性的东西都在这里加载，好集中管理
-- 2、模块定义时一律用local再return，模块是否是全局模块由本脚本决定，在本脚本加载的一律为全局模块
-- 3、对必要模块执行初始化
-- 注意：
-- 1、全局的模块和被全局模块持有的引用无法GC，除非显式设置为nil
-- 2、除了单例类、通用的工具类、逻辑上的静态类以外，所有逻辑模块不要暴露到全局命名空间
-- 3、Unity侧导出所有接口到CS命名空间，访问cs侧函数一律使用CS.xxx，命名空间再cs代码中给了，这里不需要处理
-- 4、这里的全局模块是相对与游戏框架或者逻辑而言，lua语言层次的全局模块放Common.Main中导出
--]]

-- 加载全局模块
require "Framework.Common.BaseClass"
require "Framework.Common.DataClass"
require "Framework.Common.ConstClass"

-- 创建全局模块
Config = require "Global.Config"
Singleton = require "Framework.Common.Singleton"
Updatable = require "Framework.Common.Updatable"
UpdatableSingleton = require "Framework.Common.UpdatableSingleton"
SortingLayerNames = require "Global.SortingLayerNames"
Logger = require "Framework.Logger.Logger"
require "Framework.Updater.Coroutine"

-- game data
DataMessageNames = require "DataCenter.Config.DataMessageNames"
DataManager = require "DataCenter.DataManager"
ClientData = require "DataCenter.ClientData.ClientData"
ServerData = require "DataCenter.ServerData.ServerData"
UserData = require "DataCenter.UserData.UserData"

-- game config
LangUtil = require "Config.LangUtil"

-- ui base
UIUtil = require "Framework.UI.Util.UIUtil"
UIBaseModel = require "Framework.UI.Base.UIBaseModel"
UIBaseCtrl = require "Framework.UI.Base.UIBaseCtrl"
UIBaseComponent = require "Framework.UI.Base.UIBaseComponent"
UIBaseContainer = require "Framework.UI.Base.UIBaseContainer"
UIBaseView = require "Framework.UI.Base.UIBaseView"

-- ui component
UILayer = require "Framework.UI.Component.UILayer"
UICanvas = require "Framework.UI.Component.UICanvas"
UIText = require "Framework.UI.Component.UIText"
UIImage = require "Framework.UI.Component.UIImage"
UISlider = require "Framework.UI.Component.UISlider"
UIInput = require "Framework.UI.Component.UIInput"
UIButton = require "Framework.UI.Component.UIButton"
UIToggleButton = require "Framework.UI.Component.UIToggleButton"
UIWrapComponent = require "Framework.UI.Component.UIWrapComponent"
UITabGroup = require "Framework.UI.Component.UITabGroup"
UIButtonGroup = require "Framework.UI.Component.UIButtonGroup"
UIWrapGroup = require "Framework.UI.Component.UIWrapGroup"
UIEffect = require "Framework.UI.Component.UIEffect"

-- ui window
UILayers = require "Framework.UI.UILayers"
UIWindow = require "Framework.UI.UIWindow"
UIManager = require "Framework.UI.UIManager"
UIMessageNames = require "Framework.UI.Message.UIMessageNames"
UIWindowNames = require "UI.Config.UIWindowNames"
UIConfig = require "UI.Config.UIConfig"

-- res
ResourcesManager = require "Framework.Resource.ResourcesManager"
GameObjectPool = require "Framework.Resource.GameObjectPool"

-- update & time
Timer = require "Framework.Updater.Timer"
TimerManager = require "Framework.Updater.TimerManager"
UpdateManager = require "Framework.Updater.UpdateManager"
LogicUpdater = require "GameLogic.Main.LogicUpdater"

-- scenes
BaseScene = require "Framework.Scene.Base.BaseScene"
SceneManager = require "Framework.Scene.SceneManager"
SceneConfig = require "Scenes.Config.SceneConfig"

-- atlas
AtlasConfig = require "Resource.Config.AtlasConfig"
AtlasManager = require "Framework.Resource.AtlasManager"

-- effect
EffectConfig = require "Resource.Config.EffectConfig"
BaseEffect = require "Framework.Resource.Effect.Base.BaseEffect"
EffectManager = require "Framework.Resource.Effect.EffectManager"

-- net
HallConnector = require "Net.Connector.HallConnector"

-- 单例类初始化
UIManager:GetInstance()
DataManager:GetInstance()
ResourcesManager:GetInstance()
UpdateManager:GetInstance()
SceneManager:GetInstance()
AtlasManager:GetInstance()
LogicUpdater:GetInstance()
HallConnector:GetInstance()