--[[
-- added by wsh @ 2017-12-01
-- UILoginServerCtrl控制层
--]]

local UILoginServerCtrl = BaseClass("UILoginServerCtrl", UIBaseCtrl)

local function SetSelectedServer(self, svr_id)
	-- 合法性校验
	if svr_id == nil then
		-- TODO：错误弹窗
		Logger.LogError("svr_id nil")
		return
	end
	local servers = ServerData:GetInstance().servers
	if servers[svr_id] == nil then
		-- TODO：错误弹窗
		Logger.LogError("no svr_id : "..tostring(svr_id))
		return
	end
	ClientData:GetInstance():SetLoginServerID(svr_id)
end

local function CloseSelf(self)
	UIManager:GetInstance():CloseWindow(UIWindowNames.UILoginServer)
end

UILoginServerCtrl.SetSelectedServer = SetSelectedServer
UILoginServerCtrl.CloseSelf = CloseSelf

return UILoginServerCtrl