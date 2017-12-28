-- added by wsh @ 2017-09-07 for Messenger-System-Proxy
-- lua侧消息系统，基于CS.XLuaMessenger导出类，可以看做是对CS.Messenger的扩展，使其支持Lua

local unpack = unpack or table.unpack
local util = require "XLua.Common.util"
local helper = require 'XLua.Common.helper'
local cache = {}

local GetKey = function(...)
	local params = {...}
	local key = ''
	for _,v in ipairs(params) do
		key = key..'\t'..tostring(v)
	end
	return key
end

local GetCache = function(key)
	return cache[key]
end

local SetCache = function(key, value)
    assert(GetCache(key) == nil, 'already contains key '..key)
	cache[key] = value
end

local ClearCache = function(key)
	cache[key] = nil
end

local add_listener_with_delegate = function(messengerName, cs_del_obj)
	CS.XLuaMessenger.AddListener(messengerName, cs_del_obj)
end

local add_listener_with_func = function(messengerName, cs_obj, func)
	local key = GetKey(cs_obj, func)
	local obj_bind_callback = GetCache(key)
	if obj_bind_callback == nil then
		obj_bind_callback = util.bind(func, cs_obj)
		SetCache(key, obj_bind_callback)
		
		local lua_callback = CS.XLuaMessenger.CreateDelegate(messengerName, obj_bind_callback)
		CS.XLuaMessenger.AddListener(messengerName, lua_callback)
	end
end

local add_listener_with_reflection = function(messengerName, cs_obj, method_name, ...)
	local cs_del_obj = helper.new_callback(cs_obj, method_name, ...)
	CS.XLuaMessenger.AddListener(messengerName, cs_del_obj)
end

local add_listener = function(messengerName, ...)
	local params = {...}
	assert(#params >= 1, 'error params count!')
	if #params == 1 then
		add_listener_with_delegate(messengerName, unpack(params))
	elseif #params == 2 and type(params[2]) == 'function' then
		add_listener_with_func(messengerName, unpack(params))
	else
		add_listener_with_reflection(messengerName, unpack(params))
	end
end

local broadcast = function(messengerName, ...)
	CS.XLuaMessenger.Broadcast(messengerName, ...)
end

local remove_listener_with_delegate = function(messengerName, cs_del_obj)
	CS.XLuaMessenger.RemoveListener(messengerName, cs_del_obj)
end

local remove_listener_with_func = function(messengerName, cs_obj, func)
	local key = GetKey(cs_obj, func)
	local obj_bind_callback = GetCache(key)
	if obj_bind_callback ~= nil then
		ClearCache(key)
		
		local lua_callback = CS.XLuaMessenger.CreateDelegate(messengerName, obj_bind_callback)
		CS.XLuaMessenger.RemoveListener(messengerName, lua_callback)
	end
end

local remove_listener_with_reflection = function(messengerName, cs_obj, method_name, ...)
	local cs_del_obj = helper.new_callback(cs_obj, method_name, ...)
	CS.XLuaMessenger.RemoveListener(messengerName, cs_del_obj)
end

local remove_listener = function(messengerName, ...)
	local params = {...}
	assert(#params >= 1, 'error params count!')
	if #params == 1 then
		remove_listener_with_delegate(messengerName, unpack(params))
	elseif #params == 2 and type(params[2]) == 'function' then
		remove_listener_with_func(messengerName, unpack(params))
	else
		remove_listener_with_reflection(messengerName, unpack(params))
	end
end

return {
	add_listener = add_listener,
	broadcast = broadcast,
	remove_listener = remove_listener,
}
