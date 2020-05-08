--[[
-- added by wsh @ 2017-12-18
-- UILoading模型层
--]]

local UILoadingModel = BaseClass("UILoadingModel", UIBaseModel)
local base = UIBaseModel

-- 打开
local function OnEnable(self)
	base.OnEnable(self)
	-- 进度
	self.value = 0
end

-- 关闭
local function OnDisable(self)
	base.OnDisable(self)
	self.value = 0
end

UILoadingModel.OnEnable = OnEnable
UILoadingModel.OnDisable = OnDisable

return UILoadingModel