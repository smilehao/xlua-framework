--[[
-- add by wsh @ 2017-12-01
-- UILogin控制层
--]]

local UILoginCtrl = BaseClass("UILoginCtrl", UIBaseCtrl)

function LoginServer(self, name, password)
	-- 合法性检验
	if string.len(name) > 20 or string.len(name) < 1 then
		-- TODO：错误弹窗
		Logger.LogError("name length err!")
	    return;
	end
	if string.len(password) > 20 or string.len(password) < 1 then
		-- TODO：错误弹窗
		Logger.LogError("password length err!")
	    return;
	end
	-- 检测是否有汉字
	for i=1, string.len(name) do
		local curByte = string.byte(name, i)
	    if curByte > 127 then
			-- TODO：错误弹窗
			Logger.LogError("name err : only ascii can be used!")
	        return;
	    end;
	end
	
	ClientData:GetInstance():SetAccountInfo(name, password)
	-- TODO
	SceneManager:GetInstance():SwitchScene(SceneConfig.HomeScene)
end

function ChooseServer(self)
	UIManager:GetInstance():OpenWindow(UIWindowNames.UILoginServer)
end

UILoginCtrl.LoginServer = LoginServer
UILoginCtrl.ChooseServer = ChooseServer

return UILoginCtrl