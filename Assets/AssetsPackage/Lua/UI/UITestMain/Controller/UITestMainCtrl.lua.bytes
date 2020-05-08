--[[
-- added by wsh @ 2018-02-26
-- UITestMain控制层
--]]

local UITestMainCtrl = BaseClass("UITestMainCtrl", UIBaseCtrl)

local function StartFighting(self)
	SceneManager:GetInstance():SwitchScene(SceneConfig.BattleScene)
end

local function Logout(self)
	SceneManager:GetInstance():SwitchScene(SceneConfig.LoginScene)
end

UITestMainCtrl.StartFighting = StartFighting
UITestMainCtrl.Logout = Logout

return UITestMainCtrl