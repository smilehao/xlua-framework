--[[
-- add by wsh @ 2017-12-01
-- Class单元测试
--]]

require "Framework.Common.DataClass"
require "Framework.Common.ConstClass"

local function TestBaseClassOverride()
	local var1 = 1
	
	local class1 = BaseClass("class1")
	class1.__init = function(self)
		self.var1 = 1
	end
	class1.func1 = function(self)
		var1 = var1 + 1
		self.var1 = self.var1 + 1
		assert(var1 == 2)
		assert(self.var1 == 2)
	end
	
	-- no override
	local class2 = BaseClass("class2", class1)
	
	local class3 = BaseClass("class3", class2)
	-- override
	class3.func1 = function(self)
		class2.func1(self)
		var1 = var1 + 1
		self.var1 = self.var1 + 1
		assert(var1 == 3)
		assert(self.var1 == 3)
	end
	
	-- no override
	local class4 = BaseClass("class4", class3)
	
	
	local class5 = BaseClass("class5", class4)
	-- override
	class5.func1 = function(self)
		class4.func1(self)
		var1 = var1 + 1
		self.var1 = self.var1 + 1
		assert(var1 == 4)
		assert(self.var1 == 4)
	end
	
	local inst1 = class5.New()
	inst1:func1()
	
	var1 = 1
	local inst2 = class4.New()
	inst2:func1()
end

local function TestDataClass()
	local Data = DataClass("Data",{
		number = 0,
		bool = false,
		string = "",
		table = {},
		func = function() end,
	})
	
	local inst1 = Data.New()
	assert(inst1.__cname == "Data")
	assert(inst1.number == 0)
	assert(inst1.bool == false)
	assert(inst1.string == "")
	assert(#inst1.table == 0)
	assert(inst1.func() == nil)
	inst1.number = 12
	inst1.bool = true
	inst1.string = "string"
	inst1.table["name"] = "table"
	inst1.func = function() return 12 end
	assert(inst1.number == 12)
	assert(inst1.bool == true)
	assert(inst1.string == "string")
	assert(inst1.table["name"] == "table")
	assert(inst1.func() == 12)
	
	local inst2 = Data.New()
	assert(inst2.__cname == "Data")
	assert(inst2.number == 0)
	assert(inst2.bool == false)
	assert(inst2.string == "")
	assert(#inst2.table == 0)
	assert(inst2.func() == nil)
	inst2.number = 122
	inst2.bool = false
	inst2.string = "string222"
	inst2.table["name"] = "table2222"
	inst2.func = function() return 122 end
	assert(inst1.number == 12)
	assert(inst1.bool == true)
	assert(inst1.string == "string")
	assert(inst1.table["name"] == "table")
	assert(inst1.func() == 12)
	assert(inst2.number == 122)
	assert(inst2.bool == false)
	assert(inst2.string == "string222")
	assert(inst2.table["name"] == "table2222")
	assert(inst2.func() == 122)
end

local function TestDataClassReadErr()
	local Data = DataClass("Data")
	Data.aaa = nil
	
	local inst1 = Data.New()
	local bbb = inst1.bbb
end

local function TestDataClassWriteErr()
	local Data = DataClass("Data")
	Data.aaa = nil
	
	local inst1 = Data.New()
	inst1.bbb = 11
end

local function TestConstClass()
	local Const = ConstClass("Const", {
		aaa = 1
	})
	
	assert(Const.__cname == "Const")
	assert(Const.aaa == 1)
end

local function TestConstClassWriteErr1()
	local Const = ConstClass("Const", {
		aaa = 1
	})
	
	Const.aaa = 2
end

local function TestConstClassWriteErr2()
	local Const = ConstClass("Const", {
		aaa = 1
	})
	
	Const.bbb = 2
end

local function TestConstClassReadErr()
	local Const = ConstClass("Const", {
		aaa = 1
	})
	
	local bbb= Const.bbb
end

local function Run()
	TestBaseClassOverride()
	TestDataClass()
	assert(pcall(TestDataClassReadErr) == false, "TestDataClassReadErr failed!")
	assert(pcall(TestDataClassWriteErr) == false, "TestDataClassWriteErr failed!")
	TestConstClass()
	assert(pcall(TestConstClassWriteErr1) == false, "TestConstClassWriteErr1 failed!")
	assert(pcall(TestConstClassWriteErr2) == false, "TestConstClassWriteErr2 failed!")
	assert(pcall(TestConstClassReadErr) == false, "TestConstClassReadErr failed!")
	print("ClassTest Pass!")
end

return {
	Run = Run
}