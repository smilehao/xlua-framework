--[[
-- added by wsh @ 2017-12-08
-- Lua侧UIButton
-- 注意：
-- 1、按钮一般会带有其他的组件，如带一个UIText、或者一个UIImange标识说明按钮功能，所以这里是一个容器类
-- 2、UIButton组件必须挂载在根节点，其下某个子节点有个Unity侧原生Button即可，如果有多个，需要指派相对路径
-- 使用方式：
-- self.xxx_btn = self:AddComponent(UIButton, var_arg)--添加孩子，各种重载方式查看UIBaseContainer
--]]

local UIButton = BaseClass("UIButton", UIBaseContainer)
local base = UIBaseContainer

-- 创建
local function OnCreate(self, relative_path)
	base.OnCreate(self)
	-- Unity侧原生组件
	self.unity_uibutton = UIUtil.FindButton(self.transform, relative_path)
	-- 记录点击回调
	self.__onclick = nil
	
	if IsNull(self.unity_uibutton) and IsNull(self.gameObject) then
		self.gameObject = self.unity_uibutton.gameObject
		self.transform = self.unity_uibutton.transform
	end
end

-- 虚拟点击
local function Click(self)
	if self.__onclick  ~= nil then
		self.__onclick()
	end
end

-- 设置回调
local function SetOnClick(self, ...)
	self.__onclick = BindCallback(...)
	self.unity_uibutton.onClick:AddListener(self.__onclick)
end

-- 资源释放
local function OnDestroy(self)
	if self.__onclick ~= nil then
		self.unity_uibutton.onClick:RemoveListener(self.__onclick)
	end
	self.unity_uibutton = nil
	self.__onclick = nil
	base.OnDestroy(self)
end

UIButton.OnCreate = OnCreate
UIButton.SetOnClick = SetOnClick
UIButton.Click = Click
UIButton.OnDestroy = OnDestroy

return UIButton