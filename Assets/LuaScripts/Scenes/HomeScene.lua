--[[
-- added by wsh @ 2017-11-19
-- 主页场景
--]]

local HomeScene = BaseClass("HomeScene", BaseScene)
local base = BaseScene

-- 创建：准备预加载资源
local function OnCreate(self)
	base.OnCreate(self)
	-- TODO
	self:AddPreloadResource(UIConfig[UIWindowNames.UITestMain].PrefabPath, typeof(CS.UnityEngine.GameObject), 1)
end

-- 准备工作
local function OnComplete(self)
	base.OnComplete(self)
	UIManager:GetInstance():OpenWindow(UIWindowNames.UITestMain)
end

-- 离开场景
local function OnLeave(self)
	UIManager:GetInstance():CloseWindow(UIWindowNames.UITestMain)
	base.OnLeave(self)
end

HomeScene.OnCreate = OnCreate
HomeScene.OnComplete = OnComplete
HomeScene.OnLeave = OnLeave

return HomeScene;