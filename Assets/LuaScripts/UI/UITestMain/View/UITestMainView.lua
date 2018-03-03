--[[
-- added by wsh @ 2018-02-26
-- UITestMain视图层
--]]

local UITestMainView = BaseClass("UITestMainView", UIBaseView)
local base = UIBaseView

-- 各个组件路径
local fighting_btn_path = "ContentRoot/BtnGrid/FightingBtn"
local logout_btn_path = "ContentRoot/BtnGrid/LogoutBtn"

local function OnCreate(self)
	base.OnCreate(self)
	-- 初始化各个组件
	self.fighting_btn = self:AddComponent(UIButton, fighting_btn_path)
	self.logout_btn = self:AddComponent(UIButton, logout_btn_path)
	
	self.fighting_btn:SetOnClick(function()
		self.ctrl:StartFighting()
	end)
	
	self.logout_btn:SetOnClick(function()
		self.ctrl:Logout()
	end)
end

local function OnEnable(self)
	base.OnEnable(self)
end

local function OnDestroy(self)
	base.OnDestroy(self)
end

UITestMainView.OnCreate = OnCreate
UITestMainView.OnEnable = OnEnable
UITestMainView.OnDestroy = OnDestroy

return UITestMainView