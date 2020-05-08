--[[
-- added by wsh @ 2017-11-28
-- 消息系统单元测试
--]]

local Messenger = require "Framework.Common.Messenger"

local TestMessengerType1 = "TestMessengerType1"
local TestMessengerType2 = "TestMessengerType2"
local TestMessengerParam1 = "TestMessengeParam1"
local TestMessengerParam2 = 1000
local TestMessengerFunc1Call = 0
local TestMessengerFunc2Call = 0
local TestMessengerFunc3Call = 0

local function TestMessengerFunc1(arg1)
	TestMessengerFunc1Call = TestMessengerFunc1Call + 1
	assert(arg1 == TestMessengerParam1, "arg1 = "..tostring(arg1))
end

local function TestMessengerFunc2(arg1, arg2)
	TestMessengerFunc2Call = TestMessengerFunc2Call + 1
	assert(arg1 == TestMessengerParam1 and arg2 == TestMessengerParam2, "arg1 = "..arg1..", arg2"..arg2)
end

local function TestMessengerFunc3(arg1)
	TestMessengerFunc3Call = TestMessengerFunc3Call + 1
end

-- 基本功能测试
local function TestAddRemoveBroadcast()
	local TestEventCenter = Messenger.New()
	TestMessengerFunc1Call = 0
	TestEventCenter:AddListener(TestMessengerType1, TestMessengerFunc1)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	assert(TestMessengerFunc1Call == 1, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	assert(TestMessengerFunc1Call == 2, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	assert(TestMessengerFunc1Call == 3, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	TestEventCenter:RemoveListener(TestMessengerType1, TestMessengerFunc1)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	assert(TestMessengerFunc1Call == 3, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	TestEventCenter:Delete()
end

-- 参数传递测试
local function TestErrArgs()
	local TestEventCenter = Messenger.New()
	TestEventCenter:AddListener(TestMessengerType1, TestMessengerFunc1)
	ret = pcall(TestEventCenter.Broadcast, TestEventCenter, TestMessengerType1, TestMessengerParam2)
	assert(ret == false, "MessengerTest1 pcall1 error.")
	TestEventCenter:Cleanup(TestMessengerType1, TestMessengerFunc1)
	ret = pcall(TestEventCenter.Broadcast, TestEventCenter, TestMessengerType1, TestMessengerParam2)
	assert(ret == true, "MessengerTest1 pcall2 error.")
	TestEventCenter:Delete()
end

-- 清理系统测试
local function TestCleanup()
	local TestEventCenter = Messenger.New()
	TestMessengerFunc1Call = 0
	TestEventCenter:AddListener(TestMessengerType1, TestMessengerFunc1)
	TestEventCenter:Cleanup(TestMessengerType1, TestMessengerFunc1)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	assert(TestMessengerFunc1Call == 0, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	TestEventCenter:Delete()
end

-- 重复添加监听测试
local function TestReAdd()
	local TestEventCenter = Messenger.New()
	TestEventCenter:AddListener(TestMessengerType1, TestMessengerFunc1)
	TestEventCenter:AddListener(TestMessengerType1, TestMessengerFunc3)
	ret = pcall(TestEventCenter.AddListener, TestEventCenter, TestMessengerType1, TestMessengerFunc1)
	assert(ret == false, "MessengerTest1 pcall2 error.")
	TestEventCenter:RemoveListener(TestMessengerType1, TestMessengerFunc1)
	ret = pcall(TestEventCenter.AddListener, TestEventCenter, TestMessengerType1, TestMessengerFunc1)
	assert(ret == true, "MessengerTest1 pcall2 error.")
	TestEventCenter:Delete()
end

-- 多消息测试
local function TestMix()
	local TestEventCenter = Messenger.New()
	TestMessengerFunc1Call = 0
	TestMessengerFunc2Call = 0
	TestMessengerFunc3Call = 0
	
	TestEventCenter:AddListener(TestMessengerType1, TestMessengerFunc1)
	TestEventCenter:AddListener(TestMessengerType2, TestMessengerFunc2)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	assert(TestMessengerFunc1Call == 1, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	TestEventCenter:Broadcast(TestMessengerType2, TestMessengerParam1, TestMessengerParam2)
	assert(TestMessengerFunc2Call == 1, "TestMessengerFunc2Call == "..TestMessengerFunc2Call)
	
	TestEventCenter:AddListener(TestMessengerType1, TestMessengerFunc3)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	assert(TestMessengerFunc1Call == 2, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	assert(TestMessengerFunc2Call == 1, "TestMessengerFunc2Call == "..TestMessengerFunc2Call)
	assert(TestMessengerFunc3Call == 1, "TestMessengerFunc3Call == "..TestMessengerFunc3Call)
	
	TestEventCenter:RemoveListener(TestMessengerType1, TestMessengerFunc3)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	assert(TestMessengerFunc1Call == 3, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	assert(TestMessengerFunc2Call == 1, "TestMessengerFunc2Call == "..TestMessengerFunc2Call)
	assert(TestMessengerFunc3Call == 1, "TestMessengerFunc3Call == "..TestMessengerFunc3Call)
	
	
	TestMessengerFunc1Call = 0
	TestMessengerFunc2Call = 0
	TestMessengerFunc3Call = 0
	TestEventCenter:Cleanup(TestMessengerType1, TestMessengerFunc1)
	TestEventCenter:Broadcast(TestMessengerType1, TestMessengerParam1)
	TestEventCenter:Broadcast(TestMessengerType2, TestMessengerParam1, TestMessengerParam2)
	assert(TestMessengerFunc1Call == 0, "TestMessengerFunc1Call == "..TestMessengerFunc1Call)
	assert(TestMessengerFunc2Call == 0, "TestMessengerFunc2Call == "..TestMessengerFunc2Call)
	assert(TestMessengerFunc3Call == 0, "TestMessengerFunc3Call == "..TestMessengerFunc3Call)
	TestEventCenter:Delete()
end

-- 参数绑定测试和弱引用测试
local function TestParamsBind()
	local recordSelf = nil
	local class1 = BaseClass("class1")
	class1.callback = function(self, keyStr, valAny, keyStr2, valAny2)
		recordSelf = self
		if self == nil then
			return 
		end
		if keyStr ~= nil then
			self[keyStr] = valAny
		end
		if keyStr2 ~= nil then
			self[keyStr2] = valAny2
		end
	end
	local inst1 = class1.New()
	
	assert(recordSelf == nil)
	assert(inst1.aaa == nil)
	assert(inst1.bbb == nil)
	assert(inst1.ccc == nil)
	assert(inst1.ddd == nil)
	assert(inst1.mmm == nil)
	local TestEventCenter = Messenger.New()
	TestEventCenter:AddListener(TestMessengerType1, inst1.callback, inst1)
	TestEventCenter:Broadcast(TestMessengerType1, "aaa", 111)
	assert(recordSelf == inst1)
	assert(inst1.aaa == 111)
	assert(inst1.bbb == nil)
	assert(inst1.ccc == nil)
	assert(inst1.ddd == nil)
	assert(inst1.mmm == nil)
	TestEventCenter:Broadcast(TestMessengerType1, "bbb", 222)
	assert(recordSelf == inst1)
	assert(inst1.aaa == 111)
	assert(inst1.bbb == 222)
	assert(inst1.ccc == nil)
	assert(inst1.ddd == nil)
	assert(inst1.mmm == nil)
	TestEventCenter:AddListener(TestMessengerType2, inst1.callback, inst1, "mmm", 999)
	TestEventCenter:Broadcast(TestMessengerType2, "ccc", 333)
	assert(recordSelf == inst1)
	assert(inst1.aaa == 111)
	assert(inst1.bbb == 222)
	assert(inst1.ccc == 333)
	assert(inst1.ddd == nil)
	assert(inst1.mmm == 999)
	TestEventCenter:Broadcast(TestMessengerType2, "ddd", 444)
	assert(recordSelf == inst1)
	assert(inst1.aaa == 111)
	assert(inst1.bbb == 222)
	assert(inst1.ccc == 333)
	assert(inst1.ddd == 444)
	assert(inst1.mmm == 999)
	
	recordSelf = nil
	inst1 = nil
	collectgarbage()
	TestEventCenter:Broadcast(TestMessengerType1, "aaa", 333)
	-- 确保TestEventCenter不会持有对象引用
	assert(recordSelf == nil)
	class1 = nil
	collectgarbage()
	-- 这里需要注意，回调函数时释放不掉的，它的生命周期并不和对象绑定
	-- 所以要完全注销消息，必须手动
	assert(TestEventCenter.events[TestMessengerType1] ~= nil)
	assert(TestEventCenter.events[TestMessengerType2] ~= nil)
	TestEventCenter:RemoveListenerByType(TestMessengerType1)
	assert(TestEventCenter.events[TestMessengerType1] == nil)
	assert(TestEventCenter.events[TestMessengerType2] ~= nil)
	TestEventCenter:RemoveListenerByType(TestMessengerType2)
	assert(TestEventCenter.events[TestMessengerType1] == nil)
	assert(TestEventCenter.events[TestMessengerType2] == nil)
end

local function Run()
	TestAddRemoveBroadcast()
	TestErrArgs()
	TestCleanup()
	TestReAdd()
	TestMix()
	TestParamsBind()
	print("MessengerTest Pass!")
end

return {
	Run = Run
}