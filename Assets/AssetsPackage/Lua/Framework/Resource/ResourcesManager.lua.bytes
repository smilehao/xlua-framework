--[[
-- added by wsh @ 2017-12-01
-- 资源管理系统：提供资源加载管理
-- 注意：
-- 1、只提供异步接口，即使内部使用的是同步操作，对外来说只有异步
-- 2、两套API：使用回调（任何不带"Co"的接口）、使用协程（任何带"Co"的接口）
-- 3、对于串行执行一连串的异步操作，建议使用协程（用同步形式的代码写异步逻辑），回调方式会使代码难读
-- 4、所有lua层脚本别直接使用cs侧的AssetBundleManager，都来这里获取接口
-- 5、理论上做到逻辑层脚本对AB名字是完全透明的，所有资源只有packagePath的概念，这里对路径进行处理
--]]

local ResourcesManager = BaseClass("ResourcesManager", Singleton)
local AssetBundleManager = CS.AssetBundles.AssetBundleManager.Instance
local AssetBundleUtility = CS.AssetBundles.AssetBundleUtility

-- 是否有加载任务正在进行
local function IsProsessRunning(self)
	return AssetBundleManager.IsProsessRunning
end

-- 设置常驻包
-- 注意：
-- 1、公共包（被2个或者2个其它AB包所依赖的包）底层会自动设置常驻包
-- 2、任何情况下不想被卸载的非公共包（如Lua脚本）需要手动设置常驻包
local function SetAssetBundleResident(self, path, resident)
	local assetbundleName = AssetBundleUtility.AssetBundlePathToAssetBundleName(path)
	resident = resident and true or false
	AssetBundleManager:SetAssetBundleResident(assetbundleName, resident)
end

-- 异步加载AssetBundle：回调形式
local function LoadAssetBundleAsync(self, path, callback, ...)
	assert(path ~= nil and type(path) == "string" and #path > 0, "path err : "..path)
	assert(callback ~= nil and type(callback) == "function", "Need to provide a function as callback")
	local args = SafePack(...)
	coroutine.start(function()
		local assetbundle = self:CoLoadAssetBundleAsync(path, nil)
		callback(SafeUnpack(args))
	end)
end

-- 异步加载AssetBundle：协程形式
local function CoLoadAssetBundleAsync(self, path, progress_callback)
	assert(path ~= nil and type(path) == "string" and #path > 0, "path err : "..path)
	local assetbundleName = AssetBundleUtility.AssetBundlePathToAssetBundleName(path)
	local loader = AssetBundleManager:LoadAssetBundleAsync(assetbundleName)
	coroutine.waitforasyncop(loader, progress_callback)
    loader:Dispose()
end

-- 异步加载Asset：回调形式
local function LoadAsync(self, path, res_type, callback, ...)
	assert(path ~= nil and type(path) == "string" and #path > 0, "path err : "..path)
	assert(callback ~= nil and type(callback) == "function", "Need to provide a function as callback")
	local args = SafePack(nil, ...)
	coroutine.start(function()
		local asset = self:CoLoadAsync(path, res_type, nil)
		args[1] = asset
		callback(SafeUnpack(args))
	end)
end

-- 异步加载Asset：协程形式
local function CoLoadAsync(self, path, res_type, progress_callback)
	assert(path ~= nil and type(path) == "string" and #path > 0, "path err : "..path)
	local loader = AssetBundleManager:LoadAssetAsync(path, res_type)
	coroutine.waitforasyncop(loader, progress_callback)
	local asset = loader.asset
    loader:Dispose()
	if IsNull(asset) then
		Logger.LogError("Asset load err : "..path)
	end
	return asset
end

-- 清理资源：切换场景时调用
local function Cleanup(self)
	AssetBundleManager:ClearAssetsCache()
	AssetBundleManager:UnloadAllUnusedResidentAssetBundles()
	
	-- TODO：Lua脚本要重新加载，暂时吧，后面缓缓策略
	local luaAssetbundleName = CS.XLuaManager.Instance.AssetbundleName
	AssetBundleManager:AddAssetbundleAssetsCache(luaAssetbundleName)
end

ResourcesManager.IsProsessRunning = IsProsessRunning
ResourcesManager.SetAssetBundleResident = SetAssetBundleResident
ResourcesManager.LoadAssetBundleAsync = LoadAssetBundleAsync
ResourcesManager.CoLoadAssetBundleAsync = CoLoadAssetBundleAsync
ResourcesManager.LoadAsync = LoadAsync
ResourcesManager.CoLoadAsync = CoLoadAsync
ResourcesManager.Cleanup = Cleanup

return ResourcesManager
