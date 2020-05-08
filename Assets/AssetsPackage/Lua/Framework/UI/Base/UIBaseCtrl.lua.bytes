--[[
-- added by wsh @ 2017-11-30
-- UI控制层基类：发送网络请求（网络数据）、操作游戏逻辑、修改模型数据（本地数据）
-- 注意：
-- 1、UI控制层用于衔接Model层和View层，主要是用来修改数据，或者进行游戏逻辑控制
-- 2、修改数据：界面操作相关数据直接写Model层、游戏逻辑相关数据写数据中心
-- 3、游戏控制：发送网络请求、调用游戏控制逻辑函数
-- 4、Ctrl层是无状态的，不能保存变量--调试模式下强制
--]]

local UIBaseCtrl = BaseClass("UIBaseCtrl")

local function __init(self, model)
	assert(model ~= nil)
	-- 强制不能直接写变量
	if Config.Debug then
		self.model = setmetatable({}, {
			__index = model,
			__newindex = function(tb, key, value)
				if type(value) ~= "function" then
					error("You can't save data here!", 2)
				end
			end
		})
	else
		self.model = model
	end
end

local function __delete(self)
	self.model = nil
end

UIBaseCtrl.__init = __init
UIBaseCtrl.__delete = __delete

return UIBaseCtrl