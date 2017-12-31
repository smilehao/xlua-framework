--[[
-- added by wsh @ 2017-12-19
-- 协程单元测试
--]]

-- 启动协程测试
local function TestStart()
	local aaa = 15
	coroutine.start(function(var)
		aaa = aaa + var
	end, 11)
	assert(aaa == 15 + 11)
end

-- 异常测试
local function TestError()
	local Config = require "Global.Config"
	if not Config.Debug then
		return
	end
	
	local Coroutine = require "Framework.Updater.Coroutine"
	print("Before co_pool count : ", table.count(Coroutine.co_pool))
	local aaa = 15
	for i = 1, 100 do
		coroutine.start(function(var)
			aaa = aaa + nil
			print("never to there")
		end, 11)
	end
	print("After co_pool count : ", table.count(Coroutine.co_pool))
end

-- 对称协同测试
local function TestYield()
	coroutine.start(function()
		-- 等一帧，这里可能从Unity侧Start函数过来的
		coroutine.waitforframes(1)
		local frame_count = Time.frameCount
		coroutine.waitforframes(15)
		assert(Time.frameCount == frame_count + 15)
		frame_count = Time.frameCount
		-- 用于统计回调次数
		local cb_count = 0
		-- 父级协程回调
		local callback = function(co, ...)
			local param = {...}
			cb_count = cb_count + 1
			if cb_count == 1 then
				assert(#param == 1 and param[1] == frame_count + 5)
				assert(Time.frameCount == frame_count + 5)
				return
			elseif cb_count == 2 then
				assert(#param == 1 and param[1] == "666")
				assert(Time.frameCount == frame_count + 6)
				coroutine.waitforframes(12)
				assert(Time.frameCount == frame_count + 18)
				return
			end
			error("Never should be there!")
		end
		-- 子级协程函数体
		local func = function(inner_frame_count)
			assert(inner_frame_count == frame_count)
			coroutine.waitforframes(5)
			-- yieldreturn测试
			coroutine.yieldreturn(frame_count + 5)
			-- yieldreturn一定会等待一帧
			assert(Time.frameCount == frame_count + 6)
			coroutine.yieldreturn("666")
			assert(Time.frameCount == frame_count + 19)
			-- yieldbreak测试
			return coroutine.yieldbreak(666, "finished")
		end
		-- 启动子级协程并等待
		local ret1, ret2 = coroutine.yieldstart(func, callback, frame_count)
		assert(ret1 == 666 and ret2 == "finished")
		-- yieldbreak不会等待一帧
		assert(Time.frameCount == frame_count + 19)
	end)
end

-- yieldcallback测试
local function TestYieldCallback()
	-- 模拟一个异步回调
	local async_op = {isDone = false, progress = 0}
	-- 用于同步
	local cur = Time.frameCount
	local until_func = function()
		return Time.frameCount == cur + 20
	end
	-- 启动一个协程驱动异步回调
	coroutine.start(function()
		coroutine.waituntil(until_func)
		while async_op.progress < 1.0 do
			async_op.progress = async_op.progress + 0.01 * math.random()
			coroutine.waitforframes(1)
		end
		async_op.isDone = true
	end)
	-- 启动对称协程
	coroutine.start(function()
		-- 用于统计回调次数
		local cb_count = 0
		-- 父级协程回调
		local callback = function(co, progress)
			assert(progress == async_op.progress)
			cb_count = cb_count + 1
		end
		-- 子级协程函数体
		local func = function()
			-- 同步时间
			coroutine.waituntil(until_func)
			-- yieldcallback测试
			coroutine.waitforasyncop(async_op, function(co, progress)
				coroutine.yieldcallback(co, progress)
			end)
			assert(Time.frameCount == cur + 20 + cb_count + 1)
			-- yieldbreak测试
			return coroutine.yieldbreak()
		end
		-- 启动子级协程并等待
		local ret1, ret2 = coroutine.yieldstart(func, callback)
	end)
end

-- waituntil测试
local function TestWaitUntil()
	coroutine.start(function()
		local cur = Time.frameCount
		local until_func = function()
			return Time.frameCount == cur + 10
		end
		
		coroutine.waituntil(until_func)
		local frame_count = Time.frameCount
		assert(frame_count == cur + 10)
	end)
end

-- waitwhile测试
local function TestWaitWhile()
	coroutine.start(function()
		local cur = Time.frameCount
		local while_func = function()
			return Time.frameCount < cur + 10
		end
		
		coroutine.waitwhile(while_func)
		local frame_count = Time.frameCount
		assert(frame_count == cur + 10)
	end)
end

local function Run()
	TestStart()
	--TestError()
	TestYield()
	TestYieldCallback()
	TestWaitUntil()
	TestWaitWhile()
	print("CorountineTest Pass!")
end

return {
	Run = Run
}