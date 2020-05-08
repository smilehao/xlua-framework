-- added by wsh @ 2018-01-01
-- xlua热修复测试，主要测试下资源热更以后xlua重启后热修复是否生效
-- 注意：
-- 1、现在的做法热修复模块一定要提供Register、Unregister两个接口，因为现在热修复模块要支持动态加载和卸载
-- 2、注册使用xlua.hotfix或者util.hotfix_ex
-- 3、注销一律使用xlua.hotfix

local util = require "XLua.Common.util"
local AssetbundleUpdater = CS.AssetbundleUpdater
local AssetBundleManager = CS.AssetBundles.AssetBundleManager

xlua.private_accessible(AssetbundleUpdater)
xlua.private_accessible(AssetBundleManager)

local function AssetbundleUpdaterTestHotfix(self)
	print("********** AssetbundleUpdater : Call TestHotfix in lua...<<<")
end

local function AssetBundleManagerTestHotfix(self)
	print("********** AssetBundleManager : Call TestHotfix in lua...<<<")
	AssetBundleManager.Instance:TestHotfix()
end

local function Register()
	xlua.hotfix(AssetbundleUpdater, "TestHotfix", AssetbundleUpdaterTestHotfix)
	util.hotfix_ex(AssetBundleManager, "TestHotfix", AssetBundleManagerTestHotfix)
end

local function Unregister()
	xlua.hotfix(AssetbundleUpdater, "TestHotfix", nil)
	xlua.hotfix(AssetBundleManager, "TestHotfix", nil)
end

return {
	Register = Register,
	Unregister = Unregister,
}