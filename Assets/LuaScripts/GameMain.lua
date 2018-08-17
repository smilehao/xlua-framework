-- 全局模块
require "Global.Global"
	
-- 定义为全局模块，整个lua程序的入口类
GameMain = {};

-- 全局初始化
local function Initilize()
	local loadingAssetbundlePath = "UI/Prefabs/View/UILoading.prefab"
	ResourcesManager:GetInstance():CoLoadAssetBundleAsync(loadingAssetbundlePath)
end

-- 进入游戏
local function EnterGame()
	-- TODO：服务器信息应该从服务器上拉取，这里读取测试数据
	local ServerData = require "DataCenter.ServerData.ServerData"
	local TestServerData = require "GameTest.DataTest.TestServerData"
	local ClientData = require "DataCenter.ClientData.ClientData"
	ServerData:GetInstance():ParseServerList(TestServerData)
	local selected = ClientData:GetInstance().login_server_id
	if selected == nil or ServerData:GetInstance().servers[selected] == nil then
		ClientData:GetInstance():SetLoginServerID(10001)
	end
	
	SceneManager:GetInstance():SwitchScene(SceneConfig.LoginScene)
	
	--Logger.Log("###################################################")
end

--主入口函数。从这里开始lua逻辑
local function Start()
	print("GameMain start...")
	
	-- 模块启动
	UpdateManager:GetInstance():Startup()
	TimerManager:GetInstance():Startup()
	LogicUpdater:GetInstance():Startup()
	UIManager:GetInstance():Startup()
	
	if Config.Debug then
		-- 单元测试
		local UnitTest = require "UnitTest.UnitTestMain"
		UnitTest.Run()
	end
	
	coroutine.start(function()
		Initilize()
		EnterGame()
	end)
end

-- 场景切换通知
local function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

local function OnApplicationQuit()
	-- 模块注销
	UpdateManager:GetInstance():Dispose()
	TimerManager:GetInstance():Dispose()
	LogicUpdater:GetInstance():Dispose()
end

-- GameMain公共接口，其它的一律为私有接口，只能在本模块访问
GameMain.Start = Start
GameMain.OnLevelWasLoaded = OnLevelWasLoaded
GameMain.OnApplicationQuit = OnApplicationQuit

return GameMain