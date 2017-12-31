--[[
-- added by wsh @ 2017-12-01
-- LuaUtil单元测试
--]]

require "Common.LuaUtil"

local function TestBind()
	local class = BaseClass("class")
	class.callback = function(self, a, b, c, d)
		self.var = self.var + 1
		self.a = a
		self.b = b
		self.c = c
		self.d = d
	end
	
	local inst = class.New()
	inst.var = 111
	inst.a = nil
	inst.b = nil
	inst.c = nil
	inst.d = nil
	
	local bindFunc = Bind(inst, inst.callback, "aaa", 1234)
	assert(inst.var == 111)
	assert(inst.a == nil)
	assert(inst.b == nil)
	assert(inst.c == nil)
	assert(inst.d == nil)

	bindFunc()
	assert(inst.var == 112)
	assert(inst.a == "aaa")
	assert(inst.b == 1234)
	assert(inst.c == nil)
	assert(inst.d == nil)

	bindFunc("rrr")
	assert(inst.var == 113)
	assert(inst.a == "aaa")
	assert(inst.b == 1234)
	assert(inst.c == "rrr")
	assert(inst.d == nil)
	
	bindFunc("kkk", 999)
	assert(inst.var == 114)
	assert(inst.a == "aaa")
	assert(inst.b == 1234)
	assert(inst.c == "kkk")
	assert(inst.d == 999)
	
	local inst2 = class.New()
	inst2.var = 999
	inst2.a = nil
	inst2.b = nil
	inst2.c = nil
	inst2.d = nil
	
	local bindFunc2 = Bind(inst2, inst2.callback, "bbb", 4321)
	assert(inst2.var == 999)
	assert(inst2.a == nil)
	assert(inst2.b == nil)
	assert(inst2.c == nil)
	assert(inst2.d == nil)
	
	bindFunc2()
	assert(inst2.var == 1000)
	assert(inst2.a == "bbb")
	assert(inst2.b == 4321)
	assert(inst2.c == nil)
	assert(inst2.d == nil)

	bindFunc2("qqq")
	bindFunc2("vvv", 765)
	assert(inst.var == 114)
	assert(inst.a == "aaa")
	assert(inst.b == 1234)
	assert(inst.c == "kkk")
	assert(inst.d == 999)
	assert(inst2.var == 1002)
	assert(inst2.a == "bbb")
	assert(inst2.b == 4321)
	assert(inst2.c == "vvv")
	assert(inst2.d == 765)
	
	local bindFunc3 = Bind(inst, inst.callback)
	bindFunc3()
	assert(inst.var == 115)
	assert(inst.a == nil)
	assert(inst.b == nil)
	assert(inst.c == nil)
	assert(inst.d == nil)
	bindFunc3(4532, "4532")
	assert(inst.var == 116)
	assert(inst.a == 4532)
	assert(inst.b == "4532")
	assert(inst.c == nil)
	assert(inst.d == nil)
	
	local bindFunc4 = Bind(inst, inst.callback, nil, "ttt")
	bindFunc4()
	assert(inst.var == 117)
	assert(inst.a == nil)
	assert(inst.b == "ttt")
	assert(inst.c == nil)
	assert(inst.d == nil)
	bindFunc4(nil, "4532")
	assert(inst.var == 118)
	assert(inst.a == nil)
	assert(inst.b == "ttt")
	assert(inst.c == nil)
	assert(inst.d == "4532")
end

local function Run()
	TestBind()
	print("LuaUtilTest Pass!")
end

return {
	Run = Run
}