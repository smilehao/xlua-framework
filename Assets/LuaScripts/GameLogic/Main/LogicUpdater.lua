--[[
-- added by wsh @ 2017-01-09
-- 游戏逻辑Updater，游戏逻辑模块可能需要严格的驱动顺序
--]]

local LogicUpdater = BaseClass("LogicUpdater", UpdatableSingleton)
local traceback = debug.traceback

local function Update(self)
	local delta_time = Time.deltaTime
	local hallConnector = HallConnector:GetInstance()
	local status,err = pcall(hallConnector.Update, hallConnector)
	if not status then
		Logger.LogError("hallConnector update err : "..err.."\n"..traceback())
	end
end

local function LateUpdate(self)
end

local function FixedUpdate(self)
end

local function Dispose(self)
end

LogicUpdater.Update = Update
LogicUpdater.LateUpdate = LateUpdate
LogicUpdater.FixedUpdate = FixedUpdate
LogicUpdater.Dispose = Dispose

return LogicUpdater