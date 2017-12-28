local string_find = string.find
local string_sub = string.sub
local ipairs = ipairs
_G['errlog'] = print

game_config = game_config or {}

local loaded_config = {}

local reload_unsupported_config = {
	lua_arena_robot = true,
}

local lua_config_path = "sconfig" 
package.path = lua_config_path .. "/?.lua;" .. package.path

local function load_all_config()
	local conf_root = lua_config_path
	local files = os.listdir("sconfig\\*.*")

	for _,file in ipairs(files) do 
		if string_sub(file.name, 1, 1) ~='.' and file.type ~= 'dir' then
			if string_find(file.name, '.lua$') then
				local idx = string_find(file.name, '.lua') - 1
				local file_name = string_sub(file.name, 1, idx)
				local file_path = conf_root ..'.'..file_name..'.lua'
				 
				print('loading '..file_path,file_name)
				local one_conf = require(file_name)
				loaded_config[file_name] = one_conf
				game_config[file_name] = true
				--print('loaded',one_conf) 
			end
		end
	end
end

local function unload_all_config()
	for mod_name,_ in pairs(game_config) do
		package.loaded[mod_name] = nil
	end
end

local function record_all_game_config()
	for mod_name,_ in pairs(loaded_config) do
		game_config[mod_name] = true 
	end
end

unload_all_config()

load_all_config()

record_all_game_config()

_G['Main'] = { global_constants = require('checker.constant_value') }

local config_check = require('checker.config_checker')

local ret,result,msg = pcall(config_check,loaded_config)
if not ret then
	print(result)
	os.exit(-1)
end

if not result then
	print(msg)
	os.exit(-2)
end

os.exit(0)