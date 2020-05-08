--[[
-- added by wsh @ 2017-12-18
-- UILoading视图层
--]]

local UILoadingView = BaseClass("UILoadingView", UIBaseView)
local base = UIBaseView

-- 各个组件路径
local loading_text_path = "ContentRoot/LoadingDesc"
local loading_slider_path = "ContentRoot/SliderBar"

local function OnCreate(self)
	base.OnCreate(self)
	-- 初始化各个组件
	self.loading_text = self:AddComponent(UIText, loading_text_path)
	self.loading_slider = self:AddComponent(UISlider, loading_slider_path)
	self.loading_slider:SetValue(0.0)
	
	-- 定时器
	-- 这里一定要对回调函数持有引用，否则随时可能被GC，引起定时器失效
	-- 或者使用成员函数，它的生命周期是和对象绑定在一块的
	local circulator = table.circulator({"loading", "loading.", "loading..", "loading..."})
	self.timer_action = function(self)
		self.loading_text:SetText(circulator())
	end
	self.timer = TimerManager:GetInstance():GetTimer(1, self.timer_action , self)
	self.timer:Start()
end

local function OnEnable(self)
	base.OnEnable(self)
end

local function Update(self)
	self.loading_slider:SetValue(self.model.value)
end

local function OnDestroy(self)
	self.timer:Stop()
	self.loading_text = nil
	self.loading_slider = nil
	self.timer_action = nil
	self.timer = nil
	base.OnDestroy(self)
end

UILoadingView.OnCreate = OnCreate
UILoadingView.OnEnable = OnEnable
UILoadingView.Update = Update
UILoadingView.OnDestroy = OnDestroy

return UILoadingView