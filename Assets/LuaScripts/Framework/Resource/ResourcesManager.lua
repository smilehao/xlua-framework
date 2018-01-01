--[[
-- added by wsh @ 2017-12-01
-- 资源管理系统：提供资源加载管理
-- 注意：
-- 1、只提供异步接口，即使内部使用的是同步操作，对外来说只有异步
-- 2、两套API：使用回调（任何不带"Co"的接口）、使用协程（任何带"Co"的接口）
-- 3、对于串行执行一连串的异步操作，建议使用协程（用同步形式的代码写异步逻辑），回调方式会使代码难读
--]]

local ResourcesManager = BaseClass("ResourcesManager", Singleton);
local __callback = function(co, progress)
	print(progress)
	return coroutine.yieldcallback(co, progress)
end

-- 异步加载：回调形式
local function LoadAsync(self, path, callback)
	assert(path ~= nil and type(path) == "string" and #path > 0, "path err!")
	assert(callback ~= nil and type(callback) == "function", "Need to provide a function as callback")
	coroutine.start(function()
		local asset = self:CoLoadAsync(path)
		-- TODO：判空，支持ab加载、考虑是否做资源层缓存
		local inst = CS.UnityEngine.GameObject.Instantiate(asset)
		callback(inst)
	end)
end

-- 异步加载：协程形式
local function CoLoadAsync(self, path, callback)
	assert(path ~= nil and type(path) == "string" and #path > 0, "path err!")
	-- TODO：资源类型
	local loader = CS.AssetBundles.AssetBundleManager.Instance:LoadAssetAsync(path, typeof(CS.UnityEngine.GameObject))
	coroutine.waitforasyncop(loader, callback or __callback)
	local asset = loader.asset
    loader:Dispose()
	return asset
end

ResourcesManager.LoadAsync = LoadAsync
ResourcesManager.CoLoadAsync = CoLoadAsync

return ResourcesManager
