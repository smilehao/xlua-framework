--[[
-- added by wsh @ 2017-11-28
-- Logger单元测试
--]]

local Logger = require "Framework.Logger.Logger"

local function Run()
	Logger.Log("LoggerTest: log")
	--Logger.LogError("LoggerTest: log error")
	print("LoggerTest Pass!")
end

return {
	Run = Run
}