--[[
-- added by wsh @ 2017-01-10
-- String单元测试
--]]

local function TestSplitJoin()
	local aaa = "11aaa\0\0\0aaaaaa"
	local bbb = string.split(aaa, "aaa", 1, true)
	local ccc = string.split(aaa, "aaaa", 1, true)
	assert(#bbb == 4)
	assert(#ccc == 2)
	local ddd = string.join(bbb, "aaa")
	local eee = string.join(ccc, "aaaa")
	assert(aaa == ddd)
	assert(aaa == eee)
end

local function TestContains()
	local aaa = "11aaa\0\0\0aaaaaa"
	assert(string.contains(aaa, "aaa", true) == true)
	assert(string.contains(aaa, "aaab", true) == false)
end

local function TestStartEndsWith()
	local aaa = "11aaa\0\0\0aaaaaa"
	assert(string.startswith(aaa, "11aaa", true) == true)
	assert(string.startswith(aaa, "11aaae", true) == false)
	assert(string.endswith(aaa, "aaaa", true) == true)
	assert(string.endswith(aaa, "aaae", true) == false)
end

local function TestStringByte()
	local aaa = "11aaa\0\0\0aaaaaa"
	assert(#aaa == 14)
	assert(string.len(aaa) == 14)
	assert(string.byte(aaa, 1) == 49)
	assert(string.byte(aaa, 3) == 97)
	assert(string.byte(aaa, 6) == 0)
	assert(string.byte(aaa, 9) == 97)
	
	local bbb = {string.byte(aaa, 1 , #aaa)}
	assert(#bbb == 14)
	assert(table.length(bbb) == 14)
	assert(bbb[1] == 49)
	assert(bbb[3] == 97)
	assert(bbb[6] == 0)
	assert(bbb[9] == 97)
	
	local ccc = ""
	for _,v in ipairs(bbb) do
		ccc = ccc..string.char(v)
	end
	assert(aaa == ccc)
	assert(#ccc == 14)
	assert(string.len(ccc) == 14)
	assert(string.byte(ccc, 1) == 49)
	assert(string.byte(ccc, 3) == 97)
	assert(string.byte(ccc, 6) == 0)
	assert(string.byte(ccc, 9) == 97)
end

local function TestStringPack()
	local aaa = 1234500
	local aaa_pack = string.pack("=I4", aaa)
	assert(#aaa_pack == 4)
	local bbb = string.unpack("=I4", aaa_pack, 1)
	assert(aaa == bbb)
	local ccc = string.unpack("=I2", aaa_pack, 1)
	local ddd = string.unpack("=I2", aaa_pack, 3)
	local eee = ((ddd << 16) + ccc)
	assert(aaa == eee)
	assert((aaa ~ eee) == 0)
end

local function Run()
	TestSplitJoin()
	TestContains()
	TestStartEndsWith()
	TestStringByte()
	TestStringPack()
end

return {
	Run = Run
}