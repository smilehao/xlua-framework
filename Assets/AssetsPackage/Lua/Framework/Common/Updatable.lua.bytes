--[[
-- added by wsh @ 2017-12-18
-- 可更新脚本，等效于带有Unity侧Update、LateUpdate、FixedUpdate函数
-- 注意：
-- 1、虽然支持Update、LateUpdate、FixedUpdate更新，但能不用就不用---不要定义这些函数即可，用太多可能对性能有影响
-- 2、使用Time获取时间相关信息，如：Time.deltaTime，Time.fixedDeltaTime，Time.frameCount等
--]]

local Updatable = BaseClass("Updatable")

-- 添加更新函数
local function AddUpdate(self)
	if self.Update ~= nil then
		self.__update_handle = BindCallback(self, self.Update)
		UpdateManager:GetInstance():AddUpdate(self.__update_handle)
	end
	if self.LateUpdate ~= nil then
		self.__lateupdate_handle = BindCallback(self, self.LateUpdate)
		UpdateManager:GetInstance():AddLateUpdate(self.__lateupdate_handle)
	end
	if self.FixedUpdate ~= nil then
		self.__fixedupdate_handle = BindCallback(self, self.FixedUpdate)
		UpdateManager:GetInstance():AddFixedUpdate(self.__fixedupdate_handle)
	end
end

-- 注销更新函数
local function RemoveUpdate(self)
	if self.__update_handle ~= nil then
		UpdateManager:GetInstance():RemoveUpdate(self.__update_handle)
		self.__update_handle = nil
	end
	if self.__lateupdate_handle ~= nil then
		UpdateManager:GetInstance():RemoveLateUpdate(self.__lateupdate_handle)
		self.__lateupdate_handle = nil
	end
	if self.__fixedupdate_handle ~= nil then
		UpdateManager:GetInstance():RemoveFixedUpdate(self.__fixedupdate_handle)
		self.__fixedupdate_handle = nil
	end
end

-- 构造函数
local function __init(self)
	self:EnableUpdate(true)
end

-- 析构函数
local function __delete(self)
	self:EnableUpdate(false)
end

-- 是否启用更新
local function EnableUpdate(self, enable)
	RemoveUpdate(self)
	if enable then
		AddUpdate(self)
	end
end

Updatable.__init = __init
Updatable.__delete = __delete
Updatable.EnableUpdate = EnableUpdate

return Updatable