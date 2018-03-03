--[[
-- added by wsh @ 2018-01-11
-- UINoticeTip模块窗口配置，要使用还需要导出到UI.Config.UIConfig.lua
--]]

-- 窗口配置
local UINoticeTip = {
	Name = UIWindowNames.UINoticeTip,
	Layer = UILayers.TipLayer,
	Model = require "UI.UINoticeTip.Model.UINoticeTipModel",
	Ctrl = nil,
	View = require "UI.UINoticeTip.View.UINoticeTipView",
	PrefabPath = "UI/Prefabs/Common/UINoticeTip.prefab",
}

return {
	-- 配置
	UINoticeTip = UINoticeTip,
}