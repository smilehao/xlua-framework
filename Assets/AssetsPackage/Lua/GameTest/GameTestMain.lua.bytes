--[[
-- added by wsh @ 2017-01-12
-- 说明：方便做Lua模块测试，在这张表中导出的函数会自动在Unity的OnGui中创建按钮
-- 按钮显示名字---对应的Lua函数功能
--]]

local TestNoticeTip = require "GameTest/ModuleTest/TestNoticeTip"
local TestKeepModel = require "GameTest/ModuleTest/TestKeepModel"
local TestLogWinStack = require "GameTest/ModuleTest/TestLogWinStack"
local CustomTest = require "GameTest/CustomTest/CustomTest"

local function FullGC()
	collectgarbage("collect") 
	print("Mem : "..collectgarbage("count").."KB") 
end

return {
	["01、Full GC"] = FullGC,
	["02、Test Tip"] = TestNoticeTip.Run,
	["03、Keep Model"] = TestKeepModel.Run,
	["04、Log WinStack"] = TestLogWinStack.Run,
	["05、Custom Test"] = CustomTest.Run,
}