--[[
-- added by wsh @ 2017-12-05
-- 本地化工具类
--]]

local LangUtil = {}

local function GetData(path, lang)
	-- TODO：根据语言设置自动切换各语言源表
	return require(path)
end

local function GetServerName(server_id)
	local data = GetData("Config.Data.ServerLang")
	if data[server_id] == nil then
		return "["..server_id.."]"
	end
	return data[server_id].name
end

local function GetServerAreaName(area_id)
	local data = GetData("Config.Data.ServerAreaLang")
	if data[area_id].name == nil then
		return "["..area_id.."]"
	end
	return data[area_id].name
end

LangUtil.GetServerName = GetServerName
LangUtil.GetServerAreaName = GetServerAreaName

return ConstClass("LangUtil", LangUtil)