--[[
-- added by wsh @ 2017-11-30
-- UIWindow数据，用以表示一个窗口
-- 注意：
-- 1、窗口名字必须和预设名字一致
--]]

local UIWindow = {
	-- 窗口名字
	Name = "Background",
	-- Layer层级
	Layer = UILayers.NormalLayer,
	-- Model实例
	Model = UIBaseModel,
	-- Ctrl实例
	Ctrl = UIBaseCtrl,
	-- View实例
	View = UIBaseView,
	-- 是否激活
	Active = false,
	-- 预设路径
	PrefabPath = "",
	-- 是否正在加载
	IsLoading = false,
}
	
return DataClass("UIWindow", UIWindow)