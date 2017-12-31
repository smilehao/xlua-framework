--[[
-- added by wsh @ 2017-11-19
-- 主页场景
--]]

local BattleScene = BaseClass("BattleScene", BaseScene)
local base = BaseScene

-- 创建：准备预加载资源
local function OnCreate(self)
	base.OnCreate(self)
end

-- 准备工作
local function OnComplete(self)
	base.OnComplete(self)
end

-- 离开场景
local function OnLeave(self)
	base.OnLeave(self)
end

BattleScene.OnCreate = OnCreate
BattleScene.OnComplete = OnComplete
BattleScene.OnLeave = OnLeave

return BattleScene;