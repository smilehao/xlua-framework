--[[
-- added by wsh @ 2017-12-15
-- 场景基类，各场景类从这里继承：提供统一的场景加载和初始化步骤，负责资源预加载
--]]

local BaseScene = BaseClass("BaseScene")

-- 构造函数，别重写，初始化放OnInit
local function __init(self, scene_config)
	-- 场景配置
	self.scene_config = scene_config
	-- 预加载资源：资源路径、资源类型
	self.preload_resources = {}
	-- 预加载GameObject：资源路径、实例化个数
	self.preload_prefab = {}
	self:OnCreate()
end

-- 析构函数，别重写，资源释放放OnDispose
local function __delete(self)
	self:OnDestroy()
end

-- 创建：初始化一些需要全局保存的状态
local function OnCreate(self)
end

-- 添加预加载资源
-- 注意：只有prefab类型才需要填inst_count，用于指定初始实例化个数
local function AddPreloadResource(self, path, res_type, inst_count)
	assert(res_type ~= nil)
	assert(type(path) == "string" and #path > 0)
	if res_type == typeof(CS.UnityEngine.GameObject) then
		self.preload_prefab[path] = inst_count
	else
		self.preload_resources[path] = res_type
	end
end

-- 加载前的初始化
local function OnEnter(self)
end

-- 场景加载结束：后续资源准备（预加载等）
-- 注意：这里使用协程，子类别重写了，需要加载的资源添加到列表就可以了
local function CoOnPrepare(self)
	local res_count = table.count(self.preload_resources)
	local prefab_count = table.count(self.preload_prefab)
	local total_count = res_count + prefab_count
	if total_count <= 0 then
		return coroutine.yieldbreak()
	end
	
	-- 进度条切片，已加载数目
	-- 注意：这里的进度被归一化，所有预加载资源占场景加载百分比由SceneManager决定
	local progress_slice = 1.0 / total_count
	local finish_count = 0
	local prefab_type = typeof(CS.UnityEngine.GameObject)
	
	local function ProgressCallback(co, progress)
		assert(progress <= 1.0, "What's the fuck!!!")
		return coroutine.yieldcallback(co, (finish_count + progress) * progress_slice)
	end

	for res_path,res_type in pairs(self.preload_resources) do
		ResourcesManager:GetInstance():CoLoadAsync(res_path, res_type, ProgressCallback)
		finish_count = finish_count + 1
		coroutine.yieldreturn(finish_count * progress_slice)
	end
	for res_path,inst_count in pairs(self.preload_prefab) do
		GameObjectPool:GetInstance():CoPreLoadGameObjectAsync(res_path, inst_count, ProgressCallback)
		finish_count = finish_count + 1
		coroutine.yieldreturn(finish_count * progress_slice)
	end
	return coroutine.yieldbreak()
end

-- 场景加载完毕
local function OnComplete(self)
end

-- 离开场景：清理场景资源
local function OnLeave(self)
end

-- 销毁：释放全局保存的状态
local function OnDestroy(self)
	self.scene_config = nil
	self.preload_resources = nil
end

BaseScene.__init = __init
BaseScene.__delete = __delete
BaseScene.OnCreate = OnCreate
BaseScene.AddPreloadResource = AddPreloadResource
BaseScene.OnEnter = OnEnter
BaseScene.CoOnPrepare = CoOnPrepare
BaseScene.OnComplete = OnComplete
BaseScene.OnLeave = OnLeave
BaseScene.OnDestroy = OnDestroy

return BaseScene