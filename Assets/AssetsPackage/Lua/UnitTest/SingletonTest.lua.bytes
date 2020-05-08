--[[
-- added by wsh @ 2017-12-05
-- Singleton单元测试
--]]

require "Framework.Common.BaseClass"
local Singleton = require "Framework.Common.Singleton"

local function TestSingleton()
	local testSingleton1 = BaseClass("testSingleton1", Singleton)
	local testSingleton2 = BaseClass("testSingleton1", Singleton)
	assert(testSingleton1.Instance == nil)
	assert(testSingleton2.Instance == nil)
	local inst1 = testSingleton1:GetInstance()
	assert(testSingleton1.Instance == inst1)
	assert(testSingleton2.Instance == nil)
	local inst2 = testSingleton2:GetInstance()
	assert(testSingleton1.Instance == inst1)
	assert(testSingleton2.Instance == inst2)
	assert(inst1 ~= inst2)
	inst1.testVar1 = 111
	inst2.testVar1 = 222
	assert(inst1.testVar1 == 111)
	assert(inst2.testVar1 == 222)
	assert(testSingleton1.Instance.testVar1 == 111)
	assert(testSingleton2.Instance.testVar1 == 222)
	assert(testSingleton1:GetInstance().testVar1 == 111)
	assert(testSingleton2:GetInstance().testVar1 == 222)
	inst1:Delete()
	inst1 = nil --这里一定要置空，所以不建议这么用，单例类建议都使用“类名:GetInstance().XXX”方式使用
	assert(testSingleton1.Instance == nil)
	testSingleton2:GetInstance():Delete()
	inst2 = nil --同上
	assert(testSingleton2.Instance == nil)
end

local function TestSingleton2()
	local testSingleton1 = BaseClass("testSingleton1", Singleton)
	local testSingleton2 = BaseClass("testSingleton2", testSingleton1)
	assert(testSingleton1.Instance == nil)
	assert(testSingleton2.Instance == nil)
	local inst1 = testSingleton1:GetInstance()
	assert(testSingleton1.Instance == inst1)
	assert(testSingleton2.Instance == nil)
	local inst2 = testSingleton2:GetInstance()
	assert(testSingleton1.Instance == inst1)
	assert(testSingleton2.Instance == inst2)
	assert(inst1 ~= inst2)
	inst1.testVar1 = 111
	inst2.testVar1 = 222
	assert(inst1.testVar1 == 111)
	assert(inst2.testVar1 == 222)
	assert(testSingleton1.Instance.testVar1 == 111)
	assert(testSingleton2.Instance.testVar1 == 222)
	assert(testSingleton1:GetInstance().testVar1 == 111)
	assert(testSingleton2:GetInstance().testVar1 == 222)
	inst2:Delete()
	inst2 = nil --同上
	assert(testSingleton2.Instance == nil)
	assert(testSingleton1.Instance ~= nil)
	testSingleton1:GetInstance():Delete()
	inst1 = nil --同上
	assert(testSingleton1.Instance == nil)
	inst1 = testSingleton1:GetInstance()
	inst2 = testSingleton2:GetInstance()
	testSingleton2:GetInstance():Delete()
	inst2 = nil --同上
	assert(testSingleton2.Instance == nil)
	assert(testSingleton1.Instance ~= nil)
	assert(testSingleton1.Instance.testVar1 ~= 111)
	testSingleton1:GetInstance():Delete()
	inst1 = nil --同上
end

local function TestSingletonErr()
	local testSingleton1 = BaseClass("testSingleton1", Singleton)
	assert(testSingleton1.Instance == nil)
	
	local inst1 = testSingleton1:GetInstance()
	local inst2 = testSingleton1.New()
end

local function Run()
	TestSingleton()
	TestSingleton2()
	assert(pcall(TestSingletonErr) == false, "TestSingletonErr failed!")
	print("SingletonTest Pass!")
end

return {
	Run = Run
}