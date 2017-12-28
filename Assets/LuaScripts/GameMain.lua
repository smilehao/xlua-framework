--[[
-- add by wsh 2017-11-29
-- 游戏逻辑入口
--]]

-- 游戏逻辑全局模块
require "Global.Global"
	
-- 全局模块
GameMain = {}

--主入口函数
local function Start()
	print("GameMain start...")
	if Config.Debug then
		-- 单元测试
		--local UnitTest = require "UnitTest.UnitTestMain"
		--UnitTest.Run()
	end
	
	-- 模块启动
	UpdateManager:GetInstance():Startup()
	TimerManager:GetInstance():Startup()
	
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
end

GameMain.Start = Start
GameMain.OnLevelWasLoaded = OnLevelWasLoaded
GameMain.OnApplicationQuit = OnApplicationQuit

-- 启动
GameMain.Start()

return GameMain