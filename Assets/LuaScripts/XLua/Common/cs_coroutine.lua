
local util = require "XLua.Common.util"
local gameobject = CS.UnityEngine.GameObject("CoroutineRunner")
CS.UnityEngine.Object.DontDestroyOnLoad(gameobject)
local cs_coroutine_runner = gameobject:AddComponent(typeof(CS.CoroutineRunner))

local function async_yield_return(to_yield, cb)
    cs_coroutine_runner:YieldAndCallback(to_yield, cb)
end

return {
    yield_return = util.async_to_sync(async_yield_return)
}
