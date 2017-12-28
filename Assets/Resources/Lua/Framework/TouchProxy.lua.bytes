--[[
	触摸操作代理 by jinwen
	
	基本流程：在TouchProxy注册需要监听的 ==>> c#中InputTouch判断，调用Lua相应函数 ==>> TouchProxy进行处理
	（操作判断在c#的InputTouch中进行）

	通用事件方法：
		TouchProxy.RegisterEvent(self, TouchType.xxx, callBack);
		TouchProxy.UnregisterEvent(self, TouchType.xxx);

	UI事件方法：
		-- 长按循环触发事件注册
		TouchProxy.RegisterUIPress(self.voice, 1, M.Test, self);
		TouchProxy.UnregisterPress(self.voice);

		function M:Test()
    		info("Bingo!!!!!!!!")
		end
	
	参数：eventData（对应InputTouch中的Finger类）

	Finger属性：

	PickedGameObject  			-- 选中的3D对象
	CurrentSelectedGameObject	-- EventSystem当前选中的对象（继承Selectable的组件，如Button，Toggle等）
	IsOverUI					-- 是否在UI上
	Position					-- 操作位置
	StartPosition				-- 操作开始的位置
	EndPosition					-- 操作结束的位置
	DeltaPosition				-- 变化位置
	HoldTime					-- 动作保持的时间
--]]

local Object = require "Framework.Object";
TouchProxy = Class("TouchProxy", Object);
local M = TouchProxy;

TouchType = {
	Down = 0;
	Up = 1;
	Click = 2,
	DragBegin = 3,
	Drag = 4,
	DragEnd = 5,
	Press = 6
}

local touchCS = nil;
local mainCamera = nil;

local downEventMaps = {};
local upEventMaps = {};
local clickEventMaps = {};
local pressEventMaps = {};
local dragBeginEventMaps = {};
local dragEventMaps = {};
local dragEndEventMaps = {};

local uiPressMaps = {};
local uiClickMaps = {};

function M:Ctor()
	self.lastClickPosition = Vector2.zero;
end

--c#调用
function M.SetTouch(touch)
	touchCS = touch;
	if mainCamera and uguiCamera then
		touchCS.SetCamera(mainCamera);
	end
end

--Lua调用
function M.SetCamera(camera)
	mainCamera = camera;
	if touchCS then
		touchCS:SetCamera(camera);
	end
end

function M.GetLastClickPosition()
	return TouchProxy.lastClickPosition;
end

-------------------------------------------------------------------------------------------------------
----------------------------------------------  通用事件  ---------------------------------------------
-------------------------------------------------------------------------------------------------------
--[[
	注册监听事件
	listener 	--监听者
	event		--监听的事件类型
	callBack 	--触发函数
--]]
function M.RegisterEvent(listener, event, callBack)
	local key = tostring(listener);
	local list = {};
	list.callBack = callBack;
	list.listener = listener;

	if event == TouchType.Down then
		downEventMaps[key] = list;
	elseif event == TouchType.Up then
		upEventMaps[key] = list;
	elseif event == TouchType.Click then
		clickEventMaps[key] = list;
	elseif event == TouchType.Press then
		pressEventMaps[key] = list;
	elseif event == TouchType.DragBegin then
		dragBeginEventMaps[key] = list;
	elseif event == TouchType.Drag then
		dragEventMaps[key] = list;
	elseif event == TouchType.DragEnd then
		dragEndEventMaps[key] = list;
	end	
end

--[[
	注销监听事件
	listener 	--监听者
	event		--监听的事件类型
--]]
function M.UnregisterEvent(listener, event)
	local key = tostring(listener);
	if event == TouchType.Down then
		if downEventMaps[key] then
			downEventMaps[key] = nil;
		end
	elseif event == TouchType.Up then
		if upEventMaps[key] then
			upEventMaps[key] = nil;
		end
	elseif event == TouchType.Click then
		if clickEventMaps[key] then
			clickEventMaps[key] = nil;
		end
	elseif event == TouchType.Press then
		if pressEventMaps[key] then
			pressEventMaps[key] = nil;
		end
	elseif event == TouchType.DragBegin then
		if dragBeginEventMaps[key] then
			dragBeginEventMaps[key] = nil;
		end
	elseif event == TouchType.Drag then
		if dragEventMaps[key] then
			downEventMaps[key] = nil;
		end
	elseif event == TouchType.DragEnd then
		if dragEndEventMaps[key] then
			dragEndEventMaps[key] = nil;
		end	
	end	
end


-------------------------------------------------------------------------------------------------------
----------------------------------------------  UI事件  -----------------------------------------------
-------------------------------------------------------------------------------------------------------
--[[
	UI注册长按（封装了一个计时器，可以设置循环，如：长按按钮，某个数值按照间隔时间一直增长）
	
	如果后面四个参数全都没有，就是普通的回调监听，如果有，就是使用了计时器

	@param target 		--监听目标,Transform或者GameObject类型
	@param callBack     --回调方法
	@param data         --回传参数
	@param delay		--触发时间
	@param loop     	--是否循环
	@param useFrame     --是否使用帧数
--]]
function M.RegisterUIPress(target, callBack, data, delay, loop, useFrame)
	if not target or not target.gameObject then
		if error then error("Register ui press timer need a gameObject as target") end
		return
	end

	local key = target.gameObject:GetInstanceID();
	if uiPressMaps[key] then
		local timer = uiPressMaps[key].timer;
		if timer then
			timer:Stop();
			timer = nil;
		end
	else
		uiPressMaps[key] = {};
	end

	if not delay and not loop and not useFrame then
		local pressTarget = {};
		pressTarget.callBack = callBack;
		pressTarget.data = data;
		uiPressMaps[key].pressTarget = pressTarget;
	else
		uiPressMaps[key].timer = require "Framework.Timer".New(delay, callBack, data, loop, useFrame);
	end
end

--[[
	UI注销长按
--]]
function M.UnregisterUIPress(target)
	if not target or not target.gameObject then 
		if error then error("Unregister ui press timer need a gameObject as target, Please your RegisterPressTimer function.") end
		return
	end

	local key = target.gameObject:GetInstanceID();
	if uiPressMaps[key] then
		local timer = uiPressMaps[key].timer;
		if timer then
			timer:Stop();
			timer = nil;
		end
		
		uiPressMaps[key] = nil;
	end
end

--[[
	UI注册点击
	@param target 		--监听目标,Transform或者GameObject类型
	@param callBack     --回调方法
	@param data         --回传参数
--]]
function M.RegisterUIClick(target, callBack, data)
	if not target or not target.gameObject then 
		if error then error("Unregister ui click need a gameObject as target, Please your RegisterPressTimer function.") end
		return
	end

	local key = target.gameObject:GetInstanceID();
	if uiClickMaps[key] then
		uiClickMaps[key].callBack = callBack;
		uiClickMaps[key].data = data;
	else
		local clickTarget = {};
		clickTarget.callBack = callBack;
		clickTarget.data = data;
		uiClickMaps[key] = clickTarget;
	end
end

--[[
	UI注销点击
--]]
function M.UnregisterUIClick(target)
	if not target or not target.gameObject then 
		if error then error("Unregister ui click need a gameObject as target, Please your RegisterPressTimer function.") end
		return
	end

	local key = target.gameObject:GetInstanceID();
	if uiClickMaps[key] then
		uiClickMaps[key] = nil;
	end
end

-------------------------------------------------------------------------------------------------------
----------------------------------------------  触发事件  ---------------------------------------------
-------------------------------------------------------------------------------------------------------

function M.OnTouchDown(eventData)
	TouchProxy.lastClickPosition = eventData.Position;
	
	for k,v in pairs(downEventMaps) do
		v.callBack(eventData, v.listener);
	end
end

function M.OnTouchUp(eventData)
	for k,v in pairs(upEventMaps) do
		v.callBack(eventData, v.listener);
	end
end

function M.OnTouchClick(eventData)
	for k,v in pairs(clickEventMaps) do
		v.callBack(eventData, v.listener);
	end

	-- UI点击
	if eventData.CurrentSelectedGameObject then
		local key = eventData.CurrentSelectedGameObject:GetInstanceID();
		if uiClickMaps and uiClickMaps[key] then
			local clickTarget = uiClickMaps[key];
			if clickTarget.callBack then
				clickTarget.callBack(clickTarget.data);
			end
		end
	end

	-- if info then info("屏幕点击position:" .. eventData.Position.x .. "|" .. eventData.Position.y) end
	UIManager:GetInstance():CheckOutClick(eventData.Position);
	UIManager:GetInstance():CheckComponentClick(eventData.Position);
end

function M.OnTouchPress(eventData)
	for k,v in pairs(pressEventMaps) do
		v.callBack(eventData, v.listener);
	end

	-- UI长按
	if eventData.CurrentSelectedGameObject then
		local key = eventData.CurrentSelectedGameObject:GetInstanceID();
		if uiPressMaps and uiPressMaps[key] then
			local timer = uiPressMaps[key].timer;
			if timer then
				timer:Start();
			else
				local pressTarget = uiPressMaps[key].pressTarget;
				if pressTarget.callBack then
					pressTarget.callBack(pressTarget.data);
				end
			end
		end
	end
end

function M.OnTouchPressEnd(eventData)
	if eventData.CurrentSelectedGameObject then
		local key = eventData.CurrentSelectedGameObject:GetInstanceID();
		if uiPressMaps and uiPressMaps[key] and uiPressMaps[key].timer then
			uiPressMaps[key].timer:Stop();
		end
	end
end

function M.OnTouchDrag(eventData)
	for k,v in pairs(dragEventMaps) do
		v.callBack(eventData, v.listener);
	end
end

function M.OnTouchDragBegin(eventData)
	for k,v in pairs(dragBeginEventMaps) do
		v.callBack(eventData, v.listener);
	end
end

function M.OnTouchDragEnd(eventData)
	for k,v in pairs(dragEndEventMaps) do
		v.callBack(eventData, v.listener);
	end
end

return M;