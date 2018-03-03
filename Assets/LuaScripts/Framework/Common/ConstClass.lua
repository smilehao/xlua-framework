--[[
-- added by wsh @ 2017-12-01
-- 静态类：只读：避免访问错误，访问控制仅在调试模式下生效
-- 注意：
-- 1、调试模式下需要getmetatable再执行遍历
--]]

function ConstClass(classname, const_tb, super)
	assert(type(classname) == "string" and #classname > 0)
    local cls
    if super then
        cls = DeepCopy(super)
    else
        cls = {}
    end
	
	if const_tb then
		for i,v in pairs(const_tb) do
			cls[i] = v
		end
	end
	
    cls.__cname = classname
	cls.__tostring = function(self)
		return table.dump(self, true, 2)
	end
	
	if Config.Debug then
		-- 访问限制
		cls.__index = function(tb, key)
			local value = cls[key]
			if value == nil then
				error(tb.__cname.." read err: no key named : "..key.."\n"..table.dump(tb), 2)
			end
			return value
		end
		cls.__newindex = function(tb, key, value)
			if cls[key] == nil then
				error(tb.__cname.." write err: No key named : "..key.."\n"..table.dump(tb), 2)
			else
				error(tb.__cname.."(const) can not be writed : "..key, 2)
			end
		end
		return setmetatable({}, cls)
	else
		return cls
	end
end