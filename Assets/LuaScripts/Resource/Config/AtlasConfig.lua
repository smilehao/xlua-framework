--[[
-- added by wsh @ 2018-01-08
-- 图集配置
--]]

local AtlasConfig = {
	Comm = {
		Name = "Comm",
		AtlasPath = "UI/Atlas/Comm",
	},
	Group = {
		Name = "Group",
		PackagePath = "UI/Atlas/Comm",
	},
	Hyper = {
		Name = "Hyper",
		AtlasPath = "UI/Atlas/Hyper",
	},
	Login = {
		Name = "Login",
		AtlasPath = "UI/Atlas/Login",
	},
	Role = {
		Name = "Role",
		AtlasPath = "UI/Atlas/Role",
	},
}

return ConstClass("AtlasConfig", AtlasConfig)