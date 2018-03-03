--[[
-- added by wsh @ 2017-01-09
-- 网络接收包定义
--]]

local ReceiveMsgDefine = {
	RequestSeq = 0,
	Packages = {},
	
	__init = function(self, request_seq, packages)
		self.RequestSeq = request_seq
		self.Packages = packages
	end,
	
	__tostring = function(self)
		local str = "RequestSeq = "..tostring(self.RequestSeq)..", "
		for _,pakcage in ipairs(self.Packages) do
			str = str..tostring(pakcage).."\n"
		end
		return str
	end,
}

return DataClass("ReceiveMsgDefine", ReceiveMsgDefine)