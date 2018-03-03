--[[
-- added by wsh @ 2018-01-11
-- UILNoticeTip视图层
--]]

local UILNoticeTip = BaseClass("UILNoticeTip", UIBaseView)
local base = UIBaseView

local function OnCreate(self)
	base.OnCreate(self)
	
	self.cs_obj = CS.UINoticeTip.Instance
	self.cs_obj.UIGameObject = self.gameObject
end

local function OnEnable(self)
	base.OnEnable(self)
	
	self.model.cs_func(self.cs_obj, SafeUnpack(self.model.args))
end

local function OnDestroy(self)
	self.cs_obj:DestroySelf()
	base.OnDestroy(self)
end

UILNoticeTip.OnCreate = OnCreate
UILNoticeTip.OnEnable = OnEnable
UILNoticeTip.OnDestroy = OnDestroy

return UILNoticeTip