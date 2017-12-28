--[[
-- add by wsh @ 2017-11-28
-- 单元测试
-- 修改或者添加核心、公共脚本以后最好写并跑一遍单元测试，确保没问题，降低错误和调试难度
--]]

local ClassTest = require "UnitTest.ClassTest"
local SingletonTest = require "UnitTest.SingletonTest"
local MessengerTest = require "UnitTest.MessengerTest"
local ProtobufTest = require "UnitTest.ProtobufTest"
local LoggerTest = require "UnitTest.LoggerTest"
local LuaUtilTest = require "UnitTest.LuaUtilTest"
local TableUtilTest = require "UnitTest.TableUtilTest"
local CoroutineTest = require "UnitTest.CoroutineTest"

local function Run()
	ClassTest.Run()
	SingletonTest.Run()
	MessengerTest.Run()
	ProtobufTest.Run()
	LoggerTest.Run()
	LuaUtilTest.Run()
	TableUtilTest.Run()
	CoroutineTest.Run()
end

return {
	Run = Run
}