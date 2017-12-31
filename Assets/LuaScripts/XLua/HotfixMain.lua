--[[
-- added by wsh 2017-12-29
-- 游戏热修复入口
--]]

HotfixMain = {}

-- 需要被加载的热修复模块
local modules = {
	"XLua.Hotfix.HotfixTest",
}

local function Start()
	print("HotfixMain start...")
	for _,v in ipairs(modules) do
		local hotfix_module = reimport(v)
		hotfix_module.Register()
	end
end

local function Stop()
	print("HotfixMain stop...")
	for _,v in ipairs(modules) do
		local hotfix_module = require(v)
		hotfix_module.Unregister()
	end
end

HotfixMain.modules = modules
HotfixMain.Start = Start
HotfixMain.Stop = Stop

return HotfixMain