--[[
-- added by wsh @ 2017-12-19
-- 协程模块：对Lua协程conroutine进行扩展，使其具有Unity侧协程的特性
-- 注意：
-- 1、主线程使用coroutine.start启动协程，协程启动以后，首次挂起时主线程继续往下执行，这里和Unity侧表现是一致的
-- 2、协程里可以再次使用coroutine.start启动协程，和在Unity侧协程中使用StartCoroutine表现一致
-- 3、协程里启动子级协程并等待其执行完毕，在Unity侧是yield return StartCoroutine，但是在Lua不需要另外启动协程，直接调用函数即可
-- 4、如果lua侧协程不使用本脚本的扩展函数，则无法实现分帧；lua侧启动协程以后不管协程函数调用栈多深，不管使用多少次本脚本扩展函数，都运行在一个协程
-- 5、使用coroutine.waitforframes(1)来等待一帧，千万不要用coroutine.yield，否则整个协程将永远不被唤醒===>***很重要，除非你在其它地方手动唤醒它
-- 6、子级协同在lua侧等同于普通函数调用，和普通函数一样可在退出函数时带任意返回值，而Unity侧协同不能获取子级协同退出时的返回值
-- 7、理论上任何协同都可以用回调方式去做，但是对于异步操作，回调方式也需要每帧去检测异步操作是否完成，消耗相差不多，而使用协同可以简单很多，清晰很多
-- 8、协程所有等待时间的操作，如coroutine.waitforseconds误差由帧率决定，循环等待时有累积误差，所以最好只是用于分帧，或者等待异步操作
-- 9、yieldstart、yieldreturn、yieldbreak实际上是用lua不对称协程实现对称协同，使用方式和Unity侧协同类似，注意点看相关函数头说明
-- TODO：
-- 1、CS侧做可视化调试器，方便单步查看各个协程运行状态
--]]

-- 协程内部使用定时器实现，定时器是weak表，所以这里必须缓存Action，否则会被GC回收
local action_map = {}
-- action缓存池
local action_pool = {}
-- 用于子级协程yieldreturn时寻找父级协程
local yield_map = {}
-- 协程数据缓存池
local yield_pool = {}
-- 协程缓存池
local co_pool = {}

-- 回收协程
local function __RecycleCoroutine(co)
	if not coroutine.status(co) == "suspended" then
		error("Try to recycle coroutine not suspended : "..coroutine.status(co))
	end
	
	table.insert(co_pool, co)
end

-- 可复用协程
local function __Coroutine(func, ...)
	local args = SafePack(...)
	while func do
		local ret = SafePack(func(SafeUnpack(args)))
		__RecycleCoroutine(coroutine.running())
		args = SafePack(coroutine.yield(SafeUnpack(ret)))
		func = args[1]
		table.remove(args, 1)
	end
end

-- 获取协程
local function __GetCoroutine()
	local co = nil
	if table.length(co_pool) > 0 then
		co = table.remove(co_pool)
	else
		co = coroutine.create(__Coroutine)
	end
	return co
end

-- 回收Action
local function __RecycleAction(action)
	action.co = false
	action.timer = false
	action.func = false
	action.args = false
	action.result = false
	table.insert(action_pool, action)
end

-- 获取Action
local function __GetAction(co, timer, func, args, result)
	local action = nil
	if table.length(action_pool) > 0 then
		action = table.remove(action_pool)
	else
		action = {false, false, false, false, false}
	end
	action.co = co and co or false
	action.timer = timer and timer or false
	action.func = func and func or false
	action.args = args and args or false
	action.result = result and result or false
	return action
end

-- 协程运行在保护模式下，不会抛出异常，所以这里要捕获一下异常
-- 但是可能会遇到调用协程时对象已经被销毁的情况，这种情况应该被当做正常情况
-- 所以这里并不继续抛出异常，而只是输出一下错误日志，不要让客户端当掉
-- 注意：Logger中实际上在调试模式会抛出异常
local function __PResume(co, func, ...)
	local resume_ret = nil
	if func ~= nil then
		resume_ret = SafePack(coroutine.resume(co, func, ...))
	else
		resume_ret = SafePack(coroutine.resume(co, ...))
	end
	local flag, msg = resume_ret[1], resume_ret[2]
	if not flag then
		Logger.LogError(msg.."\n"..debug.traceback(co))
	elseif resume_ret.n > 1 then
		table.remove(resume_ret, 1)
	else
		resume_ret = nil
	end
	return flag, resume_ret
end

-- 启动一个协程：等待协程第一次让出控制权时主函数继续往下执行，这点表现和Unity协程一致
-- 等同于Unity侧的StartCoroutine
-- @func：协程函数体
-- @...：传入协程的可变参数
local function start(func, ...)
	local co = __GetCoroutine()
	__PResume(co, func, ...)
	return co
end

-- 启动一个协程并等待
-- 注意：
-- 1、这里会真正启动一个子级协程，比起在协程中直接函数调用开销稍微大点，但是灵活度很高
-- 2、最大好处就是可以在子级协程中使用yieldreturn返回值给父级协程执行一次某个回调，用于交付数据给父级协程
-- 3、如果子级协程没有结束，父级协程会在执行完回调之后等待一帧再次启动子级协程
-- 4、具体运用参考场景管理（ScenceManager）部分控制加载界面进度条的代码，十分清晰简洁
-- 5、如果不需要callback，即不需要子级协程交付数据，别使用yieldstart，直接使用普通函数调用方式即可
-- 6、回调callback函数体一般处理的是父级协程的逻辑，但是跑在子级协程内，这点要注意，直到yieldbreak父级协程都是挂起的
-- @func：子级协程函数体
-- @callback:子级协程在yieldreturn转移控制权给父级协程时，父级协程跑的回调，这个回调会填入子级协程yieldreturn时的参数
-- @...：传递给子级协程的可变参数
local function yieldstart(func, callback, ...)
	local co = coroutine.running() or error ('coroutine.yieldstart must be run in coroutine')
	local map = nil
	if table.length(yield_pool) > 0 then
		map = table.remove(yield_pool)
		map.parent = co
		map.callback = callback
		map.waiting = false
		map.over = false
	else
		map = {parent = co, callback = callback, waiting = false, over = false}
	end
	
	local child = __GetCoroutine()
	yield_map[child] = map
	local flag, resume_ret = __PResume(child, func, ...)
	if not flag then
		table.insert(yield_pool, map)
		yield_map[child] = nil
		return nil
	elseif map.over then
		table.insert(yield_pool, map)
		yield_map[child] = nil
		if resume_ret == nil then
			return nil
		else
			return SafeUnpack(resume_ret)
		end
	else
		map.waiting = true
		local yield_ret = SafePack(coroutine.yield())
		table.insert(yield_pool, map)
		yield_map[child] = nil
		return SafeUnpack(yield_ret)
	end
end

-- 子级协程将控制权转移给父级协程，并交付数据给父级协程回调，配合yieldstart使用
-- 注意：
-- 1、与Unity侧协程yield return不同，对子级协程来说yieldreturn一定会等待一帧再往下执行
local function yieldreturn(...)
	local co = coroutine.running() or error ("coroutine.yieldreturn must be run in coroutine")
	local map = yield_map[co]
	local parent = map.parent
	-- 没有父级协程，啥都不做
	if not map or not parent then
		return
	end
	
	local callback = map.callback
	assert(callback ~= nil, "If you don't need callback, use normal function call instead!!!")
	callback(co, ...)
	
	-- 子级协程等待一帧再继续往下执行
 	return coroutine.waitforframes(1)
end

-- 子级协程在异步回调中交付数据给父级协程回调，配合yieldstart使用
-- 注意：
-- 1、子级协程异步回调并没有运行在子级协程当中，不能使用yieldreturn，实际上不能使用任何协程相关接口，除了start
-- 2、yieldcallback需要传递当前的子级协程，这个可以从异步回调的首个参数获取
-- 3、不会等待一帧，实际上协程中的回调是每帧执行一次的
local function yieldcallback(co, ...)
	assert(co ~= nil and type(co) == "thread")
	local map = yield_map[co]
	-- 没有父级协程，啥都不做
	if not map or not map.parent then
		return
	end
	
	local callback = map.callback
	assert(callback ~= nil, "If you don't need callback, use normal function call instead!!!")
	callback(co, ...)
end

-- 退出子级协程，将控制权转移给父级协程，并交付数据作为yieldstart返回值，配合yieldstart使用
-- 注意：
-- 1、一定要使用return coroutine.yieldbreak退出===>很重要***
-- 2、不使用coroutine.yieldbreak无法唤醒父级协程
-- 3、不使用return，可能无法正确退出子级协程
local function yieldbreak(...)
	local co = coroutine.running() or error ("coroutine.yieldbreak must be run in coroutine")
	local map = yield_map[co]
	-- 没有父级协程
	if not map then
		return ...
	end
	
	map.over = true
	assert(map.parent ~= nil, "What's the fuck!!!")
	if not map.waiting then
		return ...
	else
		__PResume(map.parent, nil, ...)
	end
end

local function __Action(action, abort, ...)
	assert(action.timer)
	if not action.func then
		abort = true
	end
	
	if not abort and action.func then
		if action.args and action.args.n > 0 then
			abort = (action.func(SafeUnpack(action.args)) == action.result)
		else
			abort = (action.func() == action.result)
		end
	end
	
	if abort then
		action.timer:Stop()
		action_map[action.co] = nil
		__PResume(action.co, ...)
		__RecycleAction(action)
	end
end

-- 等待下次FixedUpdate，并在FixedUpdate执行完毕后resume
-- 等同于Unity侧的yield return new WaitForFixedUpdate
local function waitforfixedupdate()
	local co = coroutine.running() or error ("coroutine.waitforfixedupdate must be run in coroutine")
	local timer = TimerManager:GetInstance():GetCoFixedTimer()
	local action = __GetAction(co, timer)
	
	timer:Init(0, __Action, action, true, true)
 	timer:Start()
	action_map[co] = action
 	return coroutine.yield()
end

-- 等待帧数，并在Update执行完毕后resume
local function waitforframes(frames)
	assert(type(frames) == "number" and frames >= 1 and math.floor(frames) == frames)
	local co = coroutine.running() or error ("coroutine.waitforframes must be run in coroutine")
	local timer = TimerManager:GetInstance():GetCoTimer()
	local action = __GetAction(co, timer)
	
	timer:Init(frames, __Action, action, true, true)
 	timer:Start()
	action_map[co] = action
 	return coroutine.yield()
end

-- 等待秒数，并在Update执行完毕后resume
-- 等同于Unity侧的yield return new WaitForSeconds
local function waitforseconds(seconds)
	assert(type(seconds) == "number" and seconds >= 0)
	local co = coroutine.running() or error ("coroutine.waitforsenconds must be run in coroutine")
	local timer = TimerManager:GetInstance():GetCoTimer()
	local action = __GetAction(co, timer)
	
	timer:Init(seconds, __Action, action, true)
 	timer:Start()
	action_map[co] = action
 	return coroutine.yield()
end

local function __AsyncOpCheck(co, async_operation, callback)
	if callback ~= nil then
		callback(co, async_operation.progress)
	end
	return async_operation.isDone
end

-- 等待异步操作完成，并在Update执行完毕resume
-- 等同于Unity侧的yield return AsyncOperation
-- 注意：yield return WWW也是这种情况之一
-- @async_operation：异步句柄---或者任何带有isDone、progress成员属性的异步对象
-- @callback：每帧回调，传入参数为异步操作进度progress
local function waitforasyncop(async_operation, callback)
	assert(async_operation)
	local co = coroutine.running() or error ("coroutine.waitforasyncop must be run in coroutine")
	local timer = TimerManager:GetInstance():GetCoTimer()
	local action = __GetAction(co, timer, __AsyncOpCheck, SafePack(co, async_operation, callback), true)
	
	timer:Init(1, __Action, action, false, true)
 	timer:Start()
	action_map[co] = action
 	return coroutine.yield()
end

-- 等待条件为真，并在Update执行完毕resume
-- 等同于Unity侧的yield return new WaitUntil
local function waituntil(func, ...)
	assert(func)
	local co = coroutine.running() or error ("coroutine.waituntil must be run in coroutine")
	local timer = TimerManager:GetInstance():GetCoTimer()
	local action = __GetAction(co, timer, func, SafePack(...), true)
	
	timer:Init(1, __Action, action, false, true)
 	timer:Start()
	action_map[co] = action
 	return coroutine.yield()
end

-- 等待条件为假，并在Update执行完毕resume
-- 等同于Unity侧的yield return new WaitWhile
local function waitwhile(func, ...)
	assert(func)
	local co = coroutine.running() or error ("coroutine.waitwhile must be run in coroutine")
	local timer = TimerManager:GetInstance():GetCoTimer()
	local action = __GetAction(co, timer, func, SafePack(...), false)
	
	timer:Init(1, __Action, action, false, true)
 	timer:Start()
	action_map[co] = action
 	return coroutine.yield()
end

-- 等待本帧结束，并在进入下一帧之前resume
-- 等同于Unity侧的yield return new WaitForEndOfFrame
local function waitforendofframe()
	local co = coroutine.running() or error ("coroutine.waitforendofframe must be run in coroutine")
	local timer = TimerManager:GetInstance():GetCoLateTimer()
	local action = __GetAction(co, timer)
	
	timer:Init(0, __Action, action, true, true)
 	timer:Start()
	action_map[co] = action
 	return coroutine.yield()
end

-- 终止协程等待操作（所有waitXXX接口）
local function stopwaiting(co, ...)
	local action = action_map[co]
	if action then
		__Action(action, true, ...)
	end
end

coroutine.start = start
coroutine.yieldstart = yieldstart
coroutine.yieldreturn = yieldreturn
coroutine.yieldcallback = yieldcallback
coroutine.yieldbreak = yieldbreak
coroutine.waitforfixedupdate = waitforfixedupdate
coroutine.waitforframes = waitforframes
coroutine.waitforseconds = waitforseconds
coroutine.waitforasyncop = waitforasyncop
coroutine.waituntil = waituntil
coroutine.waitwhile = waitwhile
coroutine.waitforendofframe = waitforendofframe
coroutine.stopwaiting = stopwaiting

-- 调试用：查看内部状态
if Config.Debug then
	return{
		action_map = action_map,
		action_pool = action_pool,
		yield_map = yield_map,
		yield_pool = yield_pool,
		co_pool = co_pool,
	}
end