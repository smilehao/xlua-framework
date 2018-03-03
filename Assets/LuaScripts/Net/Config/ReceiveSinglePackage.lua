--[[
-- added by wsh @ 2017-01-09
-- 网络接收包定义
--]]

local ReceiveSinglePackage = {
	MsgID = 0,
	MsgProto = {},
	
	__init = function(self, msg_id, msg_proto)
		self.MsgID = msg_id
		self.MsgProto = msg_proto
	end,
	
	__tostring = function(self)
		local full_name = getmetatable(self.MsgProto)._descriptor.full_name
		local str = "MsgID = "..tostring(self.MsgID)..", ".."result = "..tostring(self.MsgProto.result).."\n"
		str = str..full_name..":{\n"..tostring(self.MsgProto).."}"
		return str
	end,
}

return DataClass("ReceiveSinglePackage", ReceiveSinglePackage)