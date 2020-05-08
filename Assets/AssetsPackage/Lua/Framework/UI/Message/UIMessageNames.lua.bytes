--[[
-- added by wsh @ 2017-11-30
-- UI消息定义，手动添加
-- 定义格式：模块名_ON_事件描述 = "驼峰式消息名"
-- 注意：
-- 1、这类型的消息只在UI模块中流通，View层只关注这里的消息，由Model层发送
-- 2、如果窗口足够简单，那每次数据变化时发送OnRefresh消息就可以了，View层进行整体刷新，以避免消息臃肿
--]]

local UIMessageNames = {
	-- 框架消息
	UIFRAME_ON_WINDOW_CREATE = "UIFrameOnWindowCreeate",
	UIFRAME_ON_WINDOW_OPEN = "UIFrameOnWindowOpen",
	UIFRAME_ON_WINDOW_CLOSE = "UIFrameOnWindowClose",
	UIFRAME_ON_WINDOW_DESTROY = "UIFrameOnWindowDestroy",
	
	-- 模块消息添加到下面
	-- UILogin模块
	UILOGIN_ON_SELECTED_SVR_CHG = "UILoginOnSelectedSvrChg",
}

return ConstClass("UIMessageNames", UIMessageNames)