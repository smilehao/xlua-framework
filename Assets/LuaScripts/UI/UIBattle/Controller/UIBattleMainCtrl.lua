--[[
-- added by wsh @ 2018-02-26
-- UIBattleMain控制层
--]]

local UIBattleMainCtrl = BaseClass("UIBattleMainCtrl", UIBaseCtrl)

local function Back(self)
	SceneManager:GetInstance():SwitchScene(SceneConfig.HomeScene)
end

UIBattleMainCtrl.Back = Back

return UIBattleMainCtrl