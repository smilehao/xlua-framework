--[[
-- added by wsh @ 2017-12-01
-- 数据结构类：不可写、读不存在的域：避免访问错误，访问控制仅在调试模式下生效
-- 注意：
-- 1、必须初始化
-- 2、字符串全部初始化为空串：""
-- 3、table全部初始化为空表：{}
-- 4、函数（比如回调）初始化为空函数：function() end
-- 5、初始化一定不要使用nil--为nil的项lua会认为你是要删除这个项，全部设置为false
-- 6、调试模式下需要getmetatable再执行遍历
--]]

-- 访问限制
local DatakIndex = function(mt, key)
	local value = nil
	if mt[key] ~= nil then 
		value = mt[key]
	end
	if value == nil then
		error(mt.__cname.." read err: no key named : "..key.."\n"..table.dump(mt), 3)
	end
	return value
end

local DataNewindex = function(mt, key, value)
	if mt[key] == nil then
		error(mt.__cname.." write err: No key named : "..key.."\n"..table.dump(mt), 3)
	end
	if value then
		rawset(mt, key, value)
	else
		rawset(mt, key, false)
	end
end

function DataClass(classname, data_tb, super)
	assert(type(classname) == "string" and #classname > 0)
    local cls
    if super then
        cls = DeepClone(super)
    else
        cls = {}
    end
	
	if data_tb then
		for i,v in pairs(data_tb) do
			cls[i] = v
		end
	end
	
    cls.__cname = classname
    function cls.New()
		local data = DeepCopy(cls)
		data.New = nil
		if Config.Debug then
			-- 访问限制
			data.__index = function(tb, key)
				return DatakIndex(data, key)
			end
			data.__newindex = function(tb, key, value)
				DataNewindex(data, key, value)
			end
			return setmetatable({}, data)
		else
			return data
		end
    end
	
    return cls
end