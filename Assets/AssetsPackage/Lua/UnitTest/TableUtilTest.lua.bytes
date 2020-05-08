--[[
-- added by wsh @ 2017-12-11
-- TableUtil单元测试
--]]

require "Common.TableUtil"

local function TestCount()
	local array = {}
	assert(table.count(array) == 0)
	array["a"] = 1
	array["b"] = 1
	array["c"] = "1"
	array[1] = {}
	array[2] = "1"
	array[{}] = 9
	assert(table.count(array) == 6)
end

local function TestLength()
	local array = {"a", "b", "c", "c", "d", "e", "c", "f"}
	assert(table.length(array) == 8)
	table.setlen(array, 100)
	assert(table.length(array) == 100)
end

local function TestKeys()
	local hashtable = {a = 1, b = 2, c = 3}
	local keys = table.keys(hashtable)
	table.sort(keys)
	assert(keys[1] == "a")
	assert(keys[2] == "b")
	assert(keys[3] == "c")
end

local function TestValues()
	local hashtable = {a = 1, b = 2, c = 3}
	local values = table.values(hashtable)
	table.sort(values)
	assert(values[1] == 1)
	assert(values[2] == 2)
	assert(values[3] == 3)
end

local function TestMerge()
	local dest = {a = 1, b = 2, c = 19}
	local src  = {c = 3, d = 4}
	table.merge(dest, src)
	assert(dest["a"] == 1)
	assert(dest["b"] == 2)
	assert(dest["c"] == 3)
	assert(dest["d"] == 4)
	assert(table.count(dest) == 4)
end

local function TestInsertTo()
	local dest = {1, 2, 3}
	local src  = {4, 5, 6}
	table.insertto(dest, src)
	assert(dest[1] == 1)
	assert(dest[2] == 2)
	assert(dest[3] == 3)
	assert(dest[4] == 4)
	assert(dest[5] == 5)
	assert(dest[6] == 6)

	dest = {1, 2, 3}
	table.insertto(dest, src, 5)
	assert(dest[1] == 1)
	assert(dest[2] == 2)
	assert(dest[3] == 3)
	assert(dest[4] == nil)
	assert(dest[5] == 4)
	assert(dest[6] == 5)
	assert(dest[7] == 6)
	assert(table.count(dest) == 6)
	assert(table.length(dest) == 7)
end

local function TestIndexOf()
	local array = {"a", "b", "c"}
	assert(table.indexof(array, "b") == 2) 
end

local function TestKeyOf()
	local hashtable = {name = "hello", comp = "world"}
	assert(table.keyof(hashtable, "world") == "comp")
end

local function TestRemoveByValue()
	local array = {"a", "b", "c", "c", "d", "e", "c", "f"}
	local move_count = table.removebyvalue(array, "c", true)
	assert(move_count == 3)
	assert(array[1] == "a")
	assert(array[2] == "b")
	assert(array[3] == "d")
	assert(array[4] == "e")
	assert(array[5] == "f")
end

local function TestMap()
	local tb = {name = "hello", comp = "world"}
	table.map(tb, function(k, v)
		-- 在每一个值前后添加括号
		return "[" .. v .. "]"
	end)
	assert(tb["name"] == "[hello]")
	assert(tb["comp"] == "[world]")
	assert(table.count(tb) == 2)
end

local function TestWalk()
	local record = {}
	local tb = {name = "hello", comp = "world"}
	table.walk(tb, function(k, v)
		record[k] = v
	end)
	assert(record["name"] == "hello")
	assert(record["comp"] == "world")
	assert(table.count(record) == 2)
end

local function TestFilter()
	local tb = {name = 1, comp = 2, a = 3, b = 2, c = 5, d = 2, e = 11}
	local fiter = table.filter(tb, function(k, v)
		return v == 2 
	end)
	assert(table.count(fiter) == 4)
	assert(fiter["name"] == 1)
	assert(fiter["a"] == 3)
	assert(fiter["c"] == 5)
	assert(fiter["e"] == 11)
end

local function TestChoose()
	local tb = {name = 1, comp = 2, a = 3, b = 2, c = 5, d = 2, e = 11}
	local choose = table.choose(tb, function(k, v)
		return v == 2 
	end)
	assert(table.count(choose) == 3)
	assert(choose["comp"] == 2)
	assert(choose["b"] == 2)
	assert(choose["d"] == 2)
end

local function Run()
	TestCount()
	TestLength()
	TestKeys()
	TestValues()
	TestMerge()
	TestInsertTo()
	TestIndexOf()
	TestKeyOf()
	TestRemoveByValue()
	TestMap()
	TestWalk()
	TestFilter()
	TestChoose()
	print("TableUtilTest Pass!")
end

return {
	Run = Run
}