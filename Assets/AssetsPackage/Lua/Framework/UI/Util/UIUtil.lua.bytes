--[[
-- added by wsh @ 2017-12-03
-- UI工具类
--]]

local UIUtil = {}

local function GetChild(trans, index)
	return trans:GetChild(index)
end

-- 注意：根节点不能是隐藏状态，否则路径将找不到
local function FindComponent(trans, ctype, path)
	assert(trans ~= nil)
	assert(ctype ~= nil)
	
	local targetTrans = trans
	if path ~= nil and type(path) == "string" and #path > 0 then
		targetTrans = trans:Find(path)
	end
	if targetTrans == nil then
		return nil
	end
	local cmp = targetTrans:GetComponent(ctype)
	if cmp ~= nil then
		return cmp
	end
	return targetTrans:GetComponentInChildren(ctype)
end

local function FindTrans(trans, path)
	return trans:Find(path)
end

local function FindText(trans, path)
	return FindComponent(trans, typeof(CS.UnityEngine.UI.Text), path)
end

local function FindImage(trans, path)
	return FindComponent(trans, typeof(CS.UnityEngine.UI.Image), path)
end

local function FindButton(trans, path)
	return FindComponent(trans, typeof(CS.UnityEngine.UI.Button), path)
end

local function FindInput(trans, path)
	return FindComponent(trans, typeof(CS.UnityEngine.UI.InputField), path)
end

local function FindSlider(trans, path)
	return FindComponent(trans, typeof(CS.UnityEngine.UI.Slider), path)
end

local function FindScrollRect(trans, path)
	return FindComponent(trans, typeof(CS.UnityEngine.UI.ScrollRect), path)
end

-- 获取直属画布
local function GetCanvas(ui_component)
	-- 初始化直属画布
	local canvas = nil
	if ui_component._class_type == UILayer then
		canvas = ui_component
	else
		local now_holder = ui_component.holder
		while now_holder ~= nil do	
			local var = ui_component:GetComponents(UICanvas)
			if table.count(var) > 0 then
				assert(table.count(var) == 1)
				canvas = var[1]
				break
			end
			now_holder = now_holder.holder
		end
	end
	assert(canvas ~= nil)
	return canvas
end

UIUtil.GetChild = GetChild
UIUtil.FindComponent = FindComponent
UIUtil.FindTrans = FindTrans
UIUtil.FindText = FindText
UIUtil.FindImage = FindImage
UIUtil.FindButton = FindButton
UIUtil.FindInput = FindInput
UIUtil.FindSlider = FindSlider
UIUtil.FindScrollRect = FindScrollRect

return ConstClass("UIUtil", UIUtil)