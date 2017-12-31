-- added by wsh @ 2018-01-01
-- xlua热修复测试，主要测试下资源热更以后xlua重启后热修复是否生效

local util = require 'XLua.Common.util'

xlua.private_accessible(CS.AssetbundleUpdater)
xlua.private_accessible(CS.AssetBundles.AssetBundleManager)

util.hotfix(CS.AssetbundleUpdater, "TestHotfix", function(self)
	print("********** AssetbundleUpdater : Call TestHotfix in lua...")
end)

util.hotfix_ex(CS.AssetBundles.AssetBundleManager, "TestHotfix", function(self)
	print("********** AssetBundleManager : Call TestHotfix in cs...")
	CS.AssetBundles.AssetBundleManager.instance:TestHotfix()
end)
