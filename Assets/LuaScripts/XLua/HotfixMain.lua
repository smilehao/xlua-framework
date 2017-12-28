--[[
-- add by wsh 2017-12-29
-- 游戏热修复入口
--]]

Hotfix = {}

-- 需要被加载的热修复模块
local modules = {
	"XLua.Hotfix.HotfixTest",
}

local function Start()
	for _,v in ipairs(modules) do
		reimport(v)
	end
end

Hotfix.modules = modules
Hotfix.Start = Start

-- 启动
Hotfix.Start()

return Hotfix