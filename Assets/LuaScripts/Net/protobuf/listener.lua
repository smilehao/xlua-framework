--
--------------------------------------------------------------------------------
--  FILE:  listener.lua
--  DESCRIPTION:  protoc-gen-lua
--      Google's Protocol Buffers project, ported to lua.
--      https://code.google.com/p/protoc-gen-lua/
--
--      Copyright (c) 2010 , 林卓毅 (Zhuoyi Lin) netsnail@gmail.com
--      All rights reserved.
--
--      Use, modification and distribution are subject to the "New BSD License"
--      as listed at <url: http://www.opensource.org/licenses/bsd-license.php >.
--
--  COMPANY:  NetEase
--  CREATED:  2010年08月02日 17时35分25秒 CST
--------------------------------------------------------------------------------
--
-- added by wsh @ 2017-12-26
-- 注意：代码已经被重构，不能再简简单单做升级

local setmetatable = setmetatable
local _null_listener = {
    Modified = function()
    end
}

local function NullMessageListener()
    return _null_listener
end

local _listener_meta = {
    Modified = function(self)
        if self.dirty then
            return
        end
        if self._parent_message then
            self._parent_message:_Modified()
        end
    end
}
_listener_meta.__index = _listener_meta

local function Listener(parent_message)
    local o = {}
    o.__mode = "v"
    o._parent_message = parent_message
    o.dirty = false
    return setmetatable(o, _listener_meta)
end

return {
	NullMessageListener = NullMessageListener,
	Listener = Listener,
}
