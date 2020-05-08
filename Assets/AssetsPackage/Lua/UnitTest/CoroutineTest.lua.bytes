--[[
-- added by wsh @ 2017-12-19
-- 协程单元测试
--]]

-- 启动协程测试
local function TestStart(finished_flag)
	finished_flag["TestStart1"] = false
	finished_flag["TestStart2"] = false
	
	local aaa = math.random(1, 30)
	local aaa_record = aaa
	coroutine.start(function(var)
		aaa = aaa + var
		finished_flag["TestStart1"] = true
	end, 11)
	-- 说明：以上协程相当于同步调用函数，所以会立即执行函数体并返回，aaa的值马上会变
	assert(aaa == aaa_record + 11, aaa, aaa_record + 11)
	
	local bbb = math.random(1, 30)
	local bbb_record = bbb
	coroutine.start(function(var)
		coroutine.waitforframes(math.random(1, 3))
		bbb = bbb + var
		assert(bbb == bbb_record + 11)
		finished_flag["TestStart2"] = true
	end, 11)
	-- 说明：协程函数体一进去就放弃了控制权，所以bbb的值不会马上被修改，这里一定不变
	assert(bbb == bbb_record)
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
local function TestYield(finished_flag)
	finished_flag["TestYield"] = false
	coroutine.start(function()
		-- 等一帧，这里可能从Unity侧Start函数过来的
		coroutine.waitforframes(1)
		local frame_count = Time.frameCount
		coroutine.waitforframes(15)
		assert(Time.frameCount == frame_count + 15, tostring(Time.frameCount).."\t"..tostring(frame_count + 15))
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
		finished_flag["TestYield"] = true
	end)
end

-- yieldcallback测试
local function TestYieldCallback(finished_flag)
	finished_flag["TestYieldCallback"] = false
	-- 模拟一个异步回调
	local async_op = {isDone = false, progress = 0}
	-- 用于同步
	local cur = Time.frameCount
	local wait_frame_count = 50
	local is_done_frame_count = 0
	local until_func = function()
		return Time.frameCount == cur + wait_frame_count
	end
	-- 启动一个协程驱动异步回调
	coroutine.start(function()
		coroutine.waituntil(until_func)
		while async_op.progress < 1.0 do
			async_op.progress = async_op.progress + 0.01 * math.random()
			coroutine.waitforframes(1)
		end
		async_op.isDone = true
		is_done_frame_count = Time.frameCount
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
			-- 说明：这里可能会晚一帧，取决于lua定时器管理系统对各个定时器的调用顺序，而目前是随机的
			assert(Time.frameCount == is_done_frame_count or Time.frameCount == is_done_frame_count + 1)
			-- yieldbreak测试
			return coroutine.yieldbreak()
		end
		-- 启动子级协程并等待
		local ret1, ret2 = coroutine.yieldstart(func, callback)
		finished_flag["TestYieldCallback"] = true
	end)
end

-- waituntil测试
local function TestWaitUntil(finished_flag)
	finished_flag["TestWaitUntil"] = false
	coroutine.start(function()
		local cur = Time.frameCount
		local until_func = function()
			return Time.frameCount == cur + 10
		end
		
		coroutine.waituntil(until_func)
		local frame_count = Time.frameCount
		assert(frame_count == cur + 10)
		finished_flag["TestWaitUntil"] = true
	end)
end

-- waitwhile测试
local function TestWaitWhile(finished_flag)
	finished_flag["TestWaitWhile"] = false
	coroutine.start(function()
		local cur = Time.frameCount
		local while_func = function()
			return Time.frameCount < cur + 10
		end
		
		coroutine.waitwhile(while_func)
		local frame_count = Time.frameCount
		assert(frame_count == cur + 10)
		finished_flag["TestWaitWhile"] = true
	end)
end

-- stopwaiting测试
local function TestStopWaiting(finished_flag)
	finished_flag["TestStopWaiting1"] = false
	finished_flag["TestStopWaiting2"] = false
	local abort_err = "Abort Err!!!"
	local co1 = coroutine.start(function()
		local err = coroutine.waitforseconds(3)
		assert(not err)
		finished_flag["TestStopWaiting1"] = true
	end)
	local co2 = coroutine.start(function()
		local err = coroutine.waitforseconds(3)
		assert(err == abort_err)
		finished_flag["TestStopWaiting2"] = true
	end)
	coroutine.stopwaiting(co2, abort_err)
end

local function InnerRun()
	local finished_flag = {}
	coroutine.waitforframes(math.random(1, 3))
	TestStart(finished_flag)
	coroutine.waitforframes(math.random(1, 3))
	--TestError()
	coroutine.waitforframes(math.random(1, 3))
	TestYield(finished_flag)
	coroutine.waitforframes(math.random(1, 3))
	TestYieldCallback(finished_flag)
	coroutine.waitforframes(math.random(1, 3))
	TestWaitUntil(finished_flag)
	coroutine.waitforframes(math.random(1, 3))
	TestWaitWhile(finished_flag)
	coroutine.waitforframes(math.random(1, 3))
	TestStopWaiting(finished_flag)
	
	-- 等待协程运行完毕：检测是否有协程被挂起的BUG
	local wait_start = Time.frameCount
	coroutine.waituntil(function()
		local finished = true
		for _,v in pairs(finished_flag) do
			if not v then
				finished = false
				break
			end
		end
		if finished then
			return true
		elseif Time.frameCount - wait_start > 1000 then
			error("Something coroutine has hang up!!!"..table.dump(finished_flag))
			return true
		end
		return false
	end)	
	print("CorountineTest Pass!")
end

local function Run()
	coroutine.start(InnerRun)
end

return {
	Run = Run
}