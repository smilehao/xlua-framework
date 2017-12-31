--[[
-- added by wsh @ 2017-12-04
-- UILayers配置
--]]

local UILayers = {
	-- 场景UI，如：点击建筑查看建筑信息---一般置于场景之上，界面UI之下
	SceneLayer = {
		Name = "SceneLayer",
		PlaneDistance = 1000,
		OrderInLayer = 0,
	},
	-- 背景UI，如：主界面---一般情况下用户不能主动关闭，永远处于其它UI的最底层
	BackgroudLayer = {
		Name = "BackgroudLayer",
		PlaneDistance = 900,
		OrderInLayer = 1000,
	},
	-- 普通UI，一级、二级、三级等窗口---一般由用户点击打开的多级窗口
	NormalLayer = {
		Name = "NormalLayer",
		PlaneDistance = 800,
		OrderInLayer = 2000,
	},
	-- 信息UI---如：跑马灯、广播等---一般永远置于用户打开窗口顶层
	InfoLayer = {
		Name = "InfoLayer",
		PlaneDistance = 700,
		OrderInLayer = 3000,
	},
	-- 提示UI，如：错误弹窗，网络连接弹窗等
	TipLayer = {
		Name = "TipLayer",
		PlaneDistance = 600,
		OrderInLayer = 4000,
	},
	-- 顶层UI，如：场景加载
	TopLayer = {
		Name = "TopLayer",
		PlaneDistance = 500,
		OrderInLayer = 5000,
	},
}
	
return ConstClass("UILayers", UILayers)