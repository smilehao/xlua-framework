local errlog = errlog
local table_insert =table.insert
local table_sort = table.sort
local tonumber = tonumber
local math_floor = math.floor
local EQUIP_MAINTYPE = 10--装备主类型
local WUJIANG_ID_LIMIT = 2000---武将ID最大值
local EMBRYO_WEAPON_PART = 1---武器胚子部位
local XINWU = 1--武将信物的类型ID
local EQUIP_STAGE = 4--武器进阶材料的类型ID
local EQUIP = 10 --武器的类型ID
local WUJIANG_STAGE = 3--武将进阶材料的类型ID
local ZHENFA = 8--阵法合成材料的类型ID
local month_days = {
	[1] = 31,
	[2] = 0,
	[3] = 31,
	[4] = 30,
	[5] = 31,
	[6] = 30,
	[7] = 31,
	[8] = 31,
	[9] = 30,
	[10] = 31,
	[11] = 30,
	[12] = 31,
}
local function get_month_days(year,month)
	if month ~= 2 then
		return month_days[month]
	end

	if (year % 4 == 0 and year % 100 ~= 0) or year % 400 == 0 then
		return 29
	else
		return 28
	end
end
--把武将的进阶配置表给转一下以供程序方便使用
local function translate_wujiang_stage(all_config)
	local lua_wujiang_stage = all_config.lua_wujiang_stage
	if not lua_wujiang_stage then
		errlog('could not find lua_wujiang_stage')
		return
	end

	for id,cfg in pairs(lua_wujiang_stage) do
		--还得检查一下每一种材料道具是否有重复
		local check_duplicate = {}
		for i = 1, 4 do 
			local item_id = cfg['item_id'..i]
			local count = cfg['item_count'..i]
			if item_id ~= 0 then
				if check_duplicate[item_id] then
					errlog('lua_wujiang_stage check failed',item_id,count)
					return
				end
				check_duplicate[item_id] = true
			end
		end
	end

	return true
end

local function translate_goods(lua_shop_goods)
	local new_config = {}
	local shop_goods_config_ex = {}
	local new_config_original = {}

	for id,shop_goods_cfg in pairs(lua_shop_goods) do
		local maintype = shop_goods_cfg.maintype
		local maintype_config = new_config[maintype]
		if not maintype_config then
			maintype_config = {}
			new_config[maintype] = maintype_config
		end

		maintype_config[id] = shop_goods_cfg

		local temp_config = shop_goods_config_ex[maintype]
		if not temp_config then
			temp_config = {}
			shop_goods_config_ex[maintype] = temp_config
		end
		table_insert(temp_config,shop_goods_cfg)

		table_insert(new_config_original,shop_goods_cfg)
	end

	return new_config,shop_goods_config_ex,new_config_original
end

local function translate_shop_goods(all_config)
	local lua_shop_goods = all_config.lua_shop_goods
	if not lua_shop_goods then
		errlog('translate_shop_goods get lua_shop_goods nil')
		return
	end

	local new_config ,shop_goods_config_ex,new_config_original = translate_goods(lua_shop_goods)
	if not new_config or not shop_goods_config_ex then
		errlog('translate_shop_goods get new_config nil')
		return
	end

	all_config.lua_shop_goods = new_config
	all_config.lua_shop_goods_ex = shop_goods_config_ex
	all_config.lua_shop_goods_original = new_config_original
	
	return true
end

local function translate_blackmarket_shop_goods(all_config)
	local lua_blackmarket_shop_goods = all_config.lua_blackmarket_shop_goods
	if not lua_blackmarket_shop_goods then
		errlog('translate_blackmarket_shop_goods get lua_blackmarket_shop_goods nil')
		return
	end

	local new_config,blackmarket_shop_goods_config_ex,new_config_original = translate_goods(lua_blackmarket_shop_goods)
	if not new_config or not blackmarket_shop_goods_config_ex then
		errlog('translate_blackmarket_shop_goods get new_config nil')
		return
	end

	all_config.lua_blackmarket_shop_goods = new_config
	all_config.lua_blackmarket_shop_goods_ex = blackmarket_shop_goods_config_ex
	all_config.lua_blackmarket_shop_goods_original = new_config_original
	
	return true
end

local function translate_qshop_goods(all_config)
	local lua_qshop_goods = all_config.lua_qshop_goods
	if not lua_qshop_goods then
		errlog('translate_qshop_goods get lua_qshop_goods nil')
		return
	end

	local new_config,qshop_goods_config_ex,new_config_original = translate_goods(lua_qshop_goods)
	if not new_config or not qshop_goods_config_ex then
		errlog('translate_shop_goods get new_config nil')
		return
	end

	all_config.lua_qshop_goods = new_config
	all_config.lua_qshop_goods_ex = qshop_goods_config_ex
	all_config.lua_qshop_goods_original = new_config_original
	
	return true
end

local function translate_turntable_box_item(all_config)
	local lua_turntable_box_item = all_config.lua_turntable_box_item
	local lua_turntable_box_maintype = all_config.lua_turntable_box_maintype
	local new_config = {}

	for id,box_item_cfg in pairs(lua_turntable_box_item) do
		local maintype = box_item_cfg.maintype
		local turntable_box_maintype_cfg = lua_turntable_box_maintype[maintype]
		if not turntable_box_maintype_cfg then
			errlog(uid,'can not find this type in lua_turntable_box_maintype',maintype)
			return false
		end
		local maintype_config = new_config[maintype]
		if not maintype_config then
			new_config[maintype] = {box_item_list={},prob =turntable_box_maintype_cfg.prob}
			maintype_config = new_config[maintype]
		end
		local box_item_list = maintype_config.box_item_list
		box_item_list[id] = box_item_cfg
	end
	
	all_config.lua_turntable_box_item_ex = new_config
	return true 
end
--老板娘商店
local function translate_landlady_item(all_config)
	local lua_landlady_item = all_config.lua_landlady_item
	local new_config = {}

	for id,landlady_item_cfg in pairs(lua_landlady_item) do
		local type = landlady_item_cfg.type
		local type_config = new_config[type]
		if not type_config then
			new_config[type] = {}
			type_config = new_config[type]
		end
		type_config[id] = landlady_item_cfg
	end
	
	all_config.lua_landlady_item_ex = new_config
	return true 
end

local function translate_landlady_box(all_config)
	local lua_landlady_box = all_config.lua_landlady_box
	local new_config = {}

	for id,landlady_box_cfg in pairs(lua_landlady_box) do
		local fixed_item_list = {}
		local fixed_type_list = {}
		for i=1,5 do
			local item_id = landlady_box_cfg['fixed_item_id'..i]
			local count = landlady_box_cfg['fixed_item_count'..i]
			if item_id > 0 and count > 0 then 
				fixed_item_list[i] = {item_id=item_id,count=count}
			else
				break
			end
		end
		for j=1,3 do
			local fixed_type = landlady_box_cfg['fixed_type'..j]
			if fixed_type > 0 then 
				fixed_type_list[j] = fixed_type
			else
				break
			end
		end
		if #fixed_item_list + #fixed_type_list ~= 5 then 
			errlog('#fixed_item_list + #fixed_type_list ~= 5',#fixed_item_list,#fixed_type_list,id)
			return false
		end
		new_config[id]={fixed_type_list=fixed_type_list,fixed_item_list=fixed_item_list}
	end
	
	all_config.lua_landlady_box_ex = new_config
	return true 
end

--特殊宝箱
local function translate_special_gift_item(all_config)
	local lua_special_gift_item = all_config.lua_special_gift_item
	local new_config = {}

	for id,gift_item_cfg in pairs(lua_special_gift_item) do
		local type = gift_item_cfg.type
		local type_config = new_config[type]
		if not type_config then
			new_config[type] = {}
			type_config = new_config[type]
		end
		type_config[id] = gift_item_cfg
	end
	
	all_config.lua_special_gift_item_ex = new_config
	return true 
end

-- nslg_city city连通关系
local function translate_nslg_city_connect(all_config)
	local lua_nslg_city = all_config.lua_nslg_city
	if not lua_nslg_city then
		errlog("translate_nslg_city_connect get lua_nslg_city nil")
		return false
	end
	
	-- errlog(tostring_r(lua_nslg_city))
	local new_config = {}
	for id, tmp_config in pairs(lua_nslg_city) do
		-- errlog("id = ", id)
		local sub_cfg = {}
		for i = 1, 6 do
			local conn_city = tmp_config['conn_city' .. i]
			if conn_city > 0 then
				sub_cfg[conn_city] = true
			end
		end		
		new_config[id] = sub_cfg
	end
	
	for id,sub_cfg in pairs(new_config) do
		for sub_id,_ in pairs(sub_cfg) do
			if not new_config[sub_id][id] then
				errlog('the path is not connected',sub_id,id)
				return false
			end
		end
	end
	
	all_config.lua_nslg_city_connect = new_config
	-- errlog("lua_nslg_city_connect = ", tostring_r(new_config))
	return true
end

-- nslg_city 所有公共城市
local function translate_nslg_public_city(all_config)
	local lua_nslg_city = all_config.lua_nslg_city
	
	if not lua_nslg_city then
		errlog("translate_nslg_public_city get lua_nslg_city nil")
		return false
	end
	local new_config = {}
	for id, temp_config in pairs(lua_nslg_city) do
		if temp_config.type == 1 then
			table_insert(new_config, id)
		end
	end
	
	all_config.lua_nslg_public_city = new_config

	return true
end

local function translate_lottery_wujiang(all_config,lua_lottery_wujiang)
	local lua_role = all_config.lua_role
	if not lua_role then
		errlog('translate_lottery_wujiang get lua_role nil')
		return
	end

	local new_config = {}
	for id,tmp_config in pairs(lua_lottery_wujiang) do
		local tmp_role = lua_role[id]
		if not tmp_role then
			--这里应该是配置错误了,并没有这个武将
			errlog('translate_lottery_wujiang get lua_role nil',id)
			return
		end

		local born_star = tmp_role.born_star
		local sub_config = new_config[born_star]
		if not sub_config then
			sub_config = {}
			new_config[born_star] = sub_config
		end
		sub_config[id] = tmp_config
	end

	return new_config
end

local function translate_lottery_general_wujiang(all_config)
	local lua_lottery_general_wujiang = all_config.lua_lottery_general_wujiang
	if not lua_lottery_general_wujiang then
		errlog('translate_lottery_general_wujiang get lua_lottery_general_wujiang nil')
		return
	end
	local new_config = translate_lottery_wujiang(all_config,lua_lottery_general_wujiang)
	if not new_config then
		errlog('translate_lottery_wujiang get nil')
		return
	end
	all_config.lua_lottery_general_wujiang = new_config
	return true
end

local function translate_lottery_senior_wujiang(all_config)
	local lua_lottery_senior_wujiang = all_config.lua_lottery_senior_wujiang
	if not lua_lottery_senior_wujiang then
		errlog('translate_lottery_senior_wujiang get lua_lottery_senior_wujiang nil')
		return
	end
	local new_config = translate_lottery_wujiang(all_config,lua_lottery_senior_wujiang)
	if not new_config then
		errlog('translate_lottery_wujiang get nil')
		return
	end
	--为屏蔽特殊武将..关羽和马超
	local target_star = 3
	local shiel_wujiang_list = Main.global_constants.CONSTANT_SHIEL_WUJIANG_LIST 
	local spec_wujiang_list = {}
	for id,wujiang in pairs(new_config[target_star]) do
		if not shiel_wujiang_list[id] then 
			spec_wujiang_list[id] = wujiang
		end
	end
	new_config['spec_3star'] = spec_wujiang_list
	
	all_config.lua_lottery_senior_wujiang = new_config
	return true
end

local function _add_lottery_item_to_type(item_id,cfg_obj,new_config,cfg_type,lua_item,lua_lottery_prob)
	local temp_config = new_config[cfg_type]
	if not temp_config then
		temp_config = {}
		new_config[cfg_type] = temp_config
	end

	--道具重复
	if temp_config[item_id] then
		errlog('translate_lottery_item failed with duplicate item_id',item_id)
		return
	end

	--这个道具是否存在
	if not lua_item[item_id] then
		errlog('item not exist ',item_id)
		return
	end

	--这个类型是否存在
	if not lua_lottery_prob[cfg_type] then
		errlog('item  type not exist ',item_id,cfg_type)
		return
	end

	temp_config[item_id] = cfg_obj

	return true
end

local function translate_lottery_item(all_config,lua_lottery_item,lua_lottery_prob)
	local lua_item = all_config.lua_item
	if not lua_item then
		errlog('translate_lottery_item failed lua_item nil')
		return 
	end

	local new_config = {}
	for item_id,cfg_obj in pairs(lua_lottery_item) do
		if cfg_obj.type1 ~= 0 and (not _add_lottery_item_to_type(item_id,cfg_obj,new_config,
			cfg_obj.type1,lua_item,lua_lottery_prob)) then 
			return 
		end

		if cfg_obj.type2 ~= 0 and (not _add_lottery_item_to_type(item_id,cfg_obj,new_config,
			cfg_obj.type2,lua_item,lua_lottery_prob)) then 
			return 
		end
	end

	return new_config
end

function translate_lottery_senior_prob_item(all_config)
	local lua_lottery_senior_prob = all_config.lua_lottery_senior_prob
	local new_config = {}
	for id,lottery_prob in pairs(lua_lottery_senior_prob) do
		if id <= 100 then
			new_config[id]=lottery_prob
		end
	end
	all_config.lua_lottery_senior_prob_ex = new_config
	return true
end

function translate_lottery_general_prob_item(all_config)
	local lua_lottery_general_prob = all_config.lua_lottery_general_prob
	local new_config = {}
	for id,lottery_prob in pairs(lua_lottery_general_prob) do
		if id <= 100 then
			new_config[id]=lottery_prob
		end
	end
	all_config.lua_lottery_general_prob_ex = new_config
	return true
end

local function translate_lottery_general_item(all_config)
	local lua_lottery_general_item = all_config.lua_lottery_general_item
	if not lua_lottery_general_item then
		errlog('get lua_lottery_general_item')
		return false
	end
	local lua_lottery_general_prob = all_config.lua_lottery_general_prob
	if not lua_lottery_general_prob then
		errlog('get lua_lottery_general_prob failed')
		return false
	end
	local new_config = translate_lottery_item(all_config,lua_lottery_general_item,lua_lottery_general_prob)
	if not new_config then
		errlog('get translate_lottery_item failed')
		return false
	end

	all_config.lua_lottery_general_item = new_config
	return true
end

local function translate_lottery_senior_item(all_config)
	local lua_lottery_senior_item = all_config.lua_lottery_senior_item
	if not lua_lottery_senior_item then
		errlog('get lua_lottery_senior_item')
		return false
	end
	local lua_lottery_senior_prob = all_config.lua_lottery_senior_prob
	if not lua_lottery_senior_prob then
		errlog('get lua_lottery_senior_prob failed')
		return false
	end

	local new_config = translate_lottery_item(all_config,lua_lottery_senior_item,lua_lottery_senior_prob)
	if not new_config then
		errlog('get translate_lottery_item failed')
		return false
	end

	all_config.lua_lottery_senior_item = new_config
	return true
end

local function translate_achieve(all_config)
	local lua_achieve = all_config.lua_achieve
	local lua_item = all_config.lua_item

	if not lua_achieve then
		errlog('get translate_achieve failed')
		return
	end
	if not lua_item then
		errlog('get translate_achieve failed')
		return 
	end

	local new_config = {}
	local new_sorted_config = {}
	local new_series_config = {}
	for id,cfg in pairs(lua_achieve) do
		--根据功能类型来划分
		local cfg_type = cfg.type
		local temp_config = new_config[cfg_type]
		if not temp_config then
			temp_config = {}
			new_config[cfg_type] = temp_config
		end
		temp_config[id] = cfg

		local temp_sorted_config = new_sorted_config[cfg_type]
		if not temp_sorted_config then
			temp_sorted_config = {}
			new_sorted_config[cfg_type] = temp_sorted_config
		end
		table_insert(temp_sorted_config,cfg)

		--根据系列来划分
		local series = cfg.series
		local temp_series_config = new_series_config[series]
		if not temp_series_config then
			temp_series_config = {}
			new_series_config[series] = temp_series_config
		end

		local award_list = {}
		cfg.award_list = award_list

		--转道具
		for i = 1, 3 do
			local award_id = cfg['award_id' .. i]
			local count = cfg['award_count' .. i]

			if award_id ~= 0 and count ~= 0 then
				if not lua_item[award_id] then
					errlog('achieve award item get not exist in lua_item',id,award_id)
					return false
				end

				if award_list[award_id] then
					errlog('achieve award got a duplicating award',id,award_id)
					return false
				end

				award_list[award_id] = count
			end
		end

		table_insert(temp_series_config,cfg)
	end

	local function _sort(a,b) return a.id < b.id end
	for _,series_config in pairs(new_series_config) do
		--对每一个系列进行排序
		table_sort(series_config,_sort)
	end

	for _,sorted_config in pairs(new_sorted_config) do
		table_sort(sorted_config,_sort)
	end

	all_config.lua_achieve_ex = new_config
	all_config.lua_achieve_series = new_series_config
	all_config.lua_achieve_sorted = new_sorted_config
	return true
end

--处理掉落列表
local function translate_copy_drop(all_config)
	local lua_copy = all_config.lua_copy
	if not lua_copy then
		errlog('get lua_copy failed')
		return 
	end
	local lua_copy_drop = all_config.lua_copy_drop
	if not lua_copy_drop then
		errlog('get lua_copy_drop failed')
		return 
	end
	for id,copy_cfg in pairs(lua_copy) do
		local real_drop_list = {}
		local real_boss_drop = {}
		local first_drop_list = {}
		local extra_drop_list = {}
		local copy_drop_cfg = lua_copy_drop[id]
		if not copy_drop_cfg then
			errlog('get lua_copy_drop get id failed',id)
			return  
		end
		for i = 1,16 do
			local item_id = copy_drop_cfg['item_id'..i]
			local prob = copy_drop_cfg['prob'..i]
			if item_id and item_id ~= 0 then
				table_insert(real_drop_list,{item_id = item_id,prob = prob})
			end
		end
		for i = 1,2 do
			local boss_idx = copy_drop_cfg['boss' .. i]
			local item_id = copy_drop_cfg['item_id'..boss_idx]
			if item_id and item_id ~= 0 then
				real_boss_drop[item_id] = true
			end
		end

		for i = 1,5 do
			local item_id = copy_drop_cfg['first_drop_id' .. i]
			local count = copy_drop_cfg['first_drop_count' .. i]
			if item_id ~= 0 and count ~= 0 then
				local temp_count = first_drop_list[item_id] or 0
				first_drop_list[item_id] = temp_count + count
			end
		end

		for i = 1,3 do
			local item_id = copy_drop_cfg['extra_drop_id' .. i]
			local count = copy_drop_cfg['extra_drop_count' .. i]
			if item_id ~= 0 and count ~= 0 then
				local temp_count = extra_drop_list[item_id] or 0
				extra_drop_list[item_id] = temp_count + count
			end
		end

		--合并一下
		copy_cfg.real_drop_list = real_drop_list
		copy_cfg.real_boss_drop = real_boss_drop
		copy_cfg.first_drop_list = first_drop_list
		copy_cfg.extra_drop_list = extra_drop_list

		copy_cfg.first_drop_money = copy_drop_cfg.first_drop_money
		copy_cfg.drop_money = copy_drop_cfg.drop_money
		copy_cfg.drop_user_exp = copy_drop_cfg.drop_user_exp
		copy_cfg.drop_wj_exp = copy_drop_cfg.drop_wj_exp

		copy_cfg.spec_drop = copy_drop_cfg.spec_drop
	end

	return true
end

--副本章节维护
local function translate_copy_section(all_config)
	local lua_copy_section = all_config.lua_copy_section
	local lua_copy = all_config.lua_copy

	local CONSTANT_NCOPY_SECTION_COPY_NUM = Main.global_constants.CONSTANT_NCOPY_SECTION_COPY_NUM
	local CONSTANT_ECOPY_SECTION_COPY_NUM = Main.global_constants.CONSTANT_ECOPY_SECTION_COPY_NUM

	local COPY_TYPE_NORMAL = Main.global_constants.CONSTANT_COPY_TYPE_NORMAL
	local COPY_TYPE_ELITE = Main.global_constants.CONSTANT_COPY_TYPE_ELITE
	local COPY_TYPE_MYSTERY = Main.global_constants.CONSTANT_COPY_TYPE_MYSTERY

	for id,cfg in pairs(lua_copy_section) do
		--检查有没有不存在的副本id
		local normal_copy_list = {}
		local elite_copy_list = {}

		for i = 1, CONSTANT_NCOPY_SECTION_COPY_NUM do
			local copy_id = cfg['copyid' .. i]
			local copy_cfg = lua_copy[copy_id]
			if not copy_cfg then
				errlog('copy_id is not existing',copy_id,id)
				return 
			end

			--检查一下副本是不是普通副本
			if copy_cfg.copy_type ~= COPY_TYPE_NORMAL then
				errlog('copy type error',copy_id,copy_cfg.copy_type)
				return
			end

			if copy_cfg.copy_section ~= id then
				errlog('section unmatch',copy_cfg.copy_section,id)
				return
			end
		
			if normal_copy_list[copy_id] then
				errlog('copy_id is duplicated',copy_id,id)
				return
			end

			normal_copy_list[copy_id] = i
		end

		cfg.normal_copy_list = normal_copy_list
	
		for i = 1, CONSTANT_ECOPY_SECTION_COPY_NUM do
			local copy_id = cfg['e_copyid' .. i]
			local copy_cfg = lua_copy[copy_id]
			if not copy_cfg then
				errlog('copy_id is not existing',copy_id,id)
				return 
			end

			--检查一下副本类型
			if copy_cfg.copy_type ~= COPY_TYPE_ELITE then
				errlog('copy type error',copy_id,copy_cfg.copy_type)
				return
			end

			if copy_cfg.copy_section ~= id then
				errlog('section unmatch',copy_cfg.copy_section,id)
				return
			end
		
			if elite_copy_list[copy_id] then
				errlog('copy_id is duplicated',copy_id,id)
				return
			end

			elite_copy_list[copy_id] = i
		end

		cfg.elite_copy_list = elite_copy_list
	end

	return true
end

--系统开放等级配置
local function translate_sys_open(all_config)
	local lua_sysopen = all_config.lua_sysopen
	if not lua_sysopen then
		errlog('get lua_sysopen failed')
		return
	end

	local new_config = {}
	for id,sysopen_cfg in pairs(lua_sysopen) do
		new_config[sysopen_cfg.field_name] = sysopen_cfg.open_level
	end

	all_config.lua_sysopen = new_config

	return true
end

--转换技能属性
local function translate_skill_attr(all_config)
	local lua_skill_attr = all_config.lua_skill_attr
	if not lua_skill_attr then
		errlog('get lua_skill_attr failed')
		return
	end

	local lua_skill = all_config.lua_skill
	if not lua_skill then
		errlog('get lua_skill failed')
		return
	end

	for skill_id,skill_attr_cfg in pairs(lua_skill_attr) do
		local skill_cfg = lua_skill[skill_id]
		if not skill_cfg then
			errlog('get lua_skill failed',skill_id)
			return
		end
		skill_attr_cfg.x = tonumber(skill_cfg.x)
		skill_attr_cfg.ax = tonumber(skill_cfg.ax)
		skill_attr_cfg.y = tonumber(skill_cfg.y)
		skill_attr_cfg.ay = tonumber(skill_cfg.ay)
		skill_attr_cfg.z = tonumber(skill_cfg.z)
		skill_attr_cfg.az = tonumber(skill_cfg.az)
	end

	return true
end

local function translate_world_boss_rank_award(all_config)
	local lua_world_boss_rank_award = all_config.lua_world_boss_rank_award
	local new_config = {}
	for id,cfg in ipairs(lua_world_boss_rank_award) do
		local award_cfg = {}
		for i = 1, 6 do
			local item_id = cfg['award' .. i ..'_id']
			local count = cfg['award' .. i ..'_count']
			if item_id ~= 0 and count ~= 0 then
				local temp_count = award_cfg[item_id] or 0
				award_cfg[item_id] = temp_count + count
			end
		end

		new_config[id] = award_cfg
	end

	all_config.lua_world_boss_rank_award = new_config

	return true
end

local function translate_raid_robots(all_config)
	local lua_raid_robots = all_config.lua_raid_robots
	for id,cfg in pairs(lua_raid_robots) do
		local new_wujiang_cfg_list = {}
		for i=1,5 do
			local wujiang_id = cfg['wujiang_id' .. i]
			local wujiang_stage = cfg['wujiang_stage' .. i]
			local wujiang_star = cfg['wujiang_star' .. i]
			local o = {
				id = wujiang_id,
				stage = wujiang_stage,
				star = wujiang_star,
				pos = i
			}
			table_insert(new_wujiang_cfg_list,o)
		end
		cfg.wujiang_cfg_list = new_wujiang_cfg_list
	end

	return true
end

local function translate_raid_baoxiang_maintype(all_config)
	local lua_raid_baoxiang_goods = all_config.lua_raid_baoxiang_goods
	local lua_raid_baoxiang_maintype = all_config.lua_raid_baoxiang_maintype
	
	local total_prob = 0
	for id,maintype_cfg in pairs(lua_raid_baoxiang_maintype) do
		maintype_cfg.goods_list = {}
		total_prob = total_prob + maintype_cfg.prob
	end

	for id,goods_cfg in pairs(lua_raid_baoxiang_goods) do
		local maintype_id = goods_cfg.maintype
		local maintype_cfg = lua_raid_baoxiang_maintype[maintype_id]
		if not maintype_cfg then
			errlog(uid,'error maintype',id,maintype_id)
			return false
		end
		table_insert(maintype_cfg.goods_list,goods_cfg)
	end

	all_config.lua_raid_baoxiang_maintype_total_prob = total_prob

	return true
end

local function translate_manor_task(all_config)
	local lua_manor_task = all_config.lua_manor_task
	local lua_manor_task_level = all_config.lua_manor_task_level

	if #lua_manor_task_level ~= 20 then
		return false
	end

	local new_config = {}

	for id,manor_task_cfg in pairs(lua_manor_task) do
		local task_level = manor_task_cfg.task_level
		local task_category = new_config[task_level]
		if not task_category then
			task_category = {}
			new_config[task_level] = task_category
		end

		if id < 900 then
			table_insert(task_category,manor_task_cfg)
		end
	end
	
	all_config.lua_manor_task_category = new_config

	if #new_config ~= 20 then
		return false
	end

	local new_config = {}
	for task_id = 900,902 do
		if not lua_manor_task[task_id] then
			errlog(uid,'get task_id failed',task_id)
			return false
		end
		table_insert(new_config,task_id)
	end
	all_config.lua_manor_first_task = new_config

	return true
end

local function translate_manor_shangbu(all_config)
	local lua_manor_shangbu_shop = all_config.lua_manor_shangbu_shop

	local new_config = {}
	for id,cfg in pairs(lua_manor_shangbu_shop) do
		local level = math_floor(id / 100)
		local cfg_by_shangbu_level = new_config[level]
		if not cfg_by_shangbu_level then
			cfg_by_shangbu_level = { goods_list = {} , total_prob = 0}
			new_config[level] = cfg_by_shangbu_level
		end

		table_insert(cfg_by_shangbu_level.goods_list,cfg)
		cfg_by_shangbu_level.total_prob = cfg_by_shangbu_level.total_prob + cfg.prob
	end

	all_config.lua_manor_shangbu_shop_ex = new_config
	return true
end

local function translate_item_gift_bag(all_config)
	local lua_item_gift_bag = all_config.lua_item_gift_bag
	for id,cfg in pairs(lua_item_gift_bag) do
		local award_item_list = {}
		for i = 1, 10 do
			local item_id = cfg['item_id' .. i]
			local count = cfg['item_count' .. i]
			local prob = cfg['prob' .. i]
			if item_id ~= 0 and count > 0 then
				local o ={
					item_id = item_id,
					count = count,
					prob = prob,
				}

				table_insert(award_item_list,o)
			end
		end

		local extend_item_id_list = cfg.extend_item_id_list
		local extend_item_count_list = cfg.extend_item_count_list
		local extend_prob_list = cfg.extend_prob_list

		for idx,item_id in ipairs(extend_item_id_list) do
			local o ={
				item_id = item_id,
				count = extend_item_count_list[idx],
				prob = extend_prob_list[idx],
			}

			table_insert(award_item_list,o)
		end

		cfg.award_item_list = award_item_list
	end

	return true
end

local function translate_zhenfa(all_config)
	local lua_zhenfa = all_config.lua_zhenfa
	local lua_horse_zhenfa_item_map = all_config.lua_horse_zhenfa_item_map
	if not lua_horse_zhenfa_item_map then
		lua_horse_zhenfa_item_map = {}
		all_config.lua_horse_zhenfa_item_map = lua_horse_zhenfa_item_map
	end

	for id,cfg in pairs(lua_zhenfa) do
		local material_list = {}
		for i = 1, 4 do
			local item_id = cfg['item'..i]
			local count = cfg['count'..i]
			if item_id > 0 and count > 0 then
				local tmp_count = material_list[item_id] or 0
				material_list[item_id] = tmp_count + count
			end
		end
		cfg.material_list = material_list

		for item_id,count in pairs(material_list) do
			lua_horse_zhenfa_item_map[item_id] = material_list
		end
	end
	return true
end

local function translate_sign_award(all_config)
	local lua_sign_award = all_config.lua_sign_award
	local curr_date = os.date("*t")
	for id,cfg in pairs(lua_sign_award) do
		local award_list = {}
		local this_month_days = get_month_days(curr_date.year,id)
		for i = 1,31 do
			local award_id = cfg['award_id' .. i]
			local award_count = cfg['award_count' .. i]
			if award_id ~= 0 and award_count > 0 then
				table_insert(award_list,{award_id = award_id,award_count = award_count})
			end

			if #award_list >= this_month_days then
				break
			end
		end

		cfg.award_list = award_list
	end

	return true
end

local function translate_toc_baoxiang(all_config)
	local lua_toc_baoxiang = all_config.lua_toc_baoxiang
	local new_config = {}
	
	for id,cfg in pairs(lua_toc_baoxiang) do
		local maintype = cfg.maintype
		local maintype_config = new_config[maintype]
		if not maintype_config then
			maintype_config = {}
			new_config[maintype] = maintype_config
		end
		maintype_config[id] = cfg
	end

	all_config.lua_toc_baoxiang_ex = new_config

	return true
end

local function translate_equip_stage_material(all_config)
	local lua_equip_stage_material = all_config.lua_equip_stage_material

	for id,cfg in pairs(lua_equip_stage_material) do
		local stage_material_list = {}
		for stage = 1,11 do
			local material_list = {}
			for i = 1,4 do
				local item_id = cfg['stage' .. stage .. '_item_id' .. i]
				local item_count = cfg['stage' .. stage .. '_count' .. i]

				if item_id ~= 0 and item_count ~= 0 then
					material_list[item_id] = item_count
				end
			end
			stage_material_list[stage] = material_list
		end

		cfg.stage_material_list = stage_material_list
	end

	return true
end

local function translate_equip_stage_required(all_config)
	local lua_equip_stage_required = all_config.lua_equip_stage_required

	for id,cfg in pairs(lua_equip_stage_required) do
		local stage_required_list = {}
		for i = 1,11 do
			local level = cfg['stage_equip_level'..i]
			local price = cfg['stage_price' .. i]
			if level > 0 and price > 0 then
				stage_required_list[i] = {level = level,price = price}
			end
		end
		cfg.stage_required_list = stage_required_list
	end

	return true
end

function translate_item_to_equip(all_config)
	local lua_item = all_config.lua_item
	local lua_equip = all_config.lua_equip
	for equip_id,cfg in pairs(lua_equip) do
		local item_cfg = lua_item[equip_id]
		if not item_cfg then
			errlog(uid,'getting lua_item was failed',equip_id)
			return 
		end
		cfg.part = item_cfg.subtype
	end

	return true
end

function translate_arena_award(all_config)
	local lua_arena_award = all_config.lua_arena_award
	for _,cfg in pairs(lua_arena_award) do
		local award_list = {}
		for i = 1,6 do
			local item_id = cfg['award_id' .. i]
			local count = cfg['award_count'..i]
			if item_id ~= 0 and count ~= 0 then
				award_list[item_id] = count
			end
		end

		cfg.award_list = award_list
	end

	return true
end

function translate_world_boss_award(all_config)
	local lua_world_boss_award = all_config.lua_world_boss_award
	local lua_world_boss_harm_subsection = all_config.lua_world_boss_harm_subsection
	local lua_item = all_config.lua_item

	for _,cfg in pairs(lua_world_boss_award) do
		local harm_award_list = {}
		for i = 1,#lua_world_boss_harm_subsection do
			local award_list = {}
			for j = 1,2 do
				local item_id = cfg['harm' .. i ..'_item' .. j]
				local count = cfg['harm' .. i ..'_count' .. j]
				if item_id ~= 0 and count ~= 0 then
					if not lua_item[item_id] then 
						errlog('check lua_world_boss_award have nil',item_id)
						return false
					end
					award_list[item_id] = count
				end
			end
			table_insert(harm_award_list,award_list)
		end
		cfg.harm_award_list = harm_award_list
	end

	return true
end

function translate_liezhuan(all_config)
	local lua_liezhuan = all_config.lua_liezhuan
	
	for _,cfg in pairs(lua_liezhuan) do
		local drop_item_list = {}
		local total_prob = 0
		for i = 1,12 do
			local item_id = cfg['item' .. i]
			local prob = cfg['prob' .. i]

			if item_id > 0 and prob > 0 then
				table_insert(drop_item_list,{item_id = item_id, prob = prob})
				total_prob = total_prob + prob
			end
		end

		cfg.drop_item_list = drop_item_list
		cfg.total_prob = total_prob

		if cfg.drop_floor > cfg.drop_ceil then
			errlog(uid,'invalid floor ceil',cfg.drop_floor,cfg.drop_ceil)
			return
		end

		local extra_list = {}
		for i = 1,2 do
			local extra_id = cfg['extra_id' .. i]
			local extra_count = cfg['extra_count' .. i]
			if extra_id > 0 and extra_count > 0 then
				extra_list[extra_id] = extra_count
			end
		end
		cfg.extra_list = extra_list
	end

	return true
end

function translate_nianshou(all_config)
	local lua_nianshou = all_config.lua_nianshou
	
	for _,cfg in pairs(lua_nianshou) do
		local drop_item_list = {}
		local total_prob = 0
		for i = 1,12 do
			local item_id = cfg['item' .. i]
			local prob = cfg['prob' .. i]

			if item_id > 0 and prob > 0 then
				table_insert(drop_item_list,{item_id = item_id, prob = prob})
				total_prob = total_prob + prob
			end
		end

		cfg.drop_item_list = drop_item_list
		cfg.total_prob = total_prob

		if cfg.drop_floor > cfg.drop_ceil then
			errlog(uid,'invalid floor ceil',cfg.drop_floor,cfg.drop_ceil)
			return
		end

		--[[local extra_list = {}
		for i = 1,2 do
			local extra_id = cfg['extra_id' .. i]
			local extra_count = cfg['extra_count' .. i]
			if extra_id > 0 and extra_count > 0 then
				extra_list[extra_id] = extra_count
			end
		end
		cfg.extra_list = extra_list--]]
	end

	return true
end

function translate_mojin(all_config)
	local lua_mojin = all_config.lua_mojin
	
	for _,cfg in pairs(lua_mojin) do
		local extra_list = {}
		for i = 1,2 do
			local extra_id = cfg['extra_id' .. i]
			local extra_count = cfg['extra_count' .. i]
			if extra_id > 0 and extra_count > 0 then
				extra_list[extra_id] = extra_count
			end
		end
		cfg.extra_list = extra_list
	end

	return true
end

local function translate_lottery_blue_item(all_config)
	local lua_lottery_general_item = all_config.lua_lottery_general_item
	local lua_item = all_config.lua_item
	all_config.lottery_blue_item = {}
	local lottery_blue_item = all_config.lottery_blue_item 

	for id,lottery_item in pairs(lua_lottery_general_item) do
		local item_obj = lua_item[id]
		if not item_obj then
			errlog(uid,'can not find item from item_table',id)
			return 
		else
			if item_obj.color == 2 then --判断材料是否为蓝色
				lottery_blue_item[id] = lottery_item
			end
		end
	end 
	return true
end

function translate_activity_award(all_config)
	local lua_activity = all_config.lua_activity
	for _,cfg in pairs(lua_activity) do
		local award_list = {}
		for i = 1,2 do
			local item_id = cfg['award_id' .. i]
			local count = cfg['award_count'..i]
			if item_id ~= 0 and count ~= 0 then
				award_list[item_id] = count
			end
		end

		cfg.award_list = award_list
	end

	return true
end

function translate_warlords_day_guild_award(all_config)
	local lua_warlords_guild_rank_award = all_config.lua_warlords_guild_rank_award
	for _,cfg in pairs(lua_warlords_guild_rank_award) do
		local award_list = {}
		for i = 1,3 do
			local item_id = cfg['award_id' .. i]
			local count = cfg['award_count'..i]
			if item_id ~= 0 and count ~= 0 then
				award_list[item_id] = count
			end
		end
		cfg.award_list = award_list
	end

	return true
end

function translate_warlords_day_user_award(all_config)
	local lua_warlords_user_rank_award = all_config.lua_warlords_user_rank_award
	for _,cfg in pairs(lua_warlords_user_rank_award) do
		local award_list = {}
		for i = 1,3 do
			local item_id = cfg['award_id' .. i]
			local count = cfg['award_count'..i]
			if item_id ~= 0 and count ~= 0 then
				award_list[item_id] = count
			end
		end
		cfg.award_list = award_list
	end

	return true
end

function translate_wujiang_no_show(all_config)
	local lua_role = all_config.lua_role
	local new_config = {}
	for id,cfg in pairs(lua_role) do
		if id < WUJIANG_ID_LIMIT and cfg.xinwu_id ~= 0 and cfg.is_show == 0 then
			local role = new_config[id]
			if not role then 
				new_config[id] = cfg
			end
		end
	end
	all_config.lua_wujiang_no_show = new_config
	
	return true
end

function translate_camps_floor(all_config)
	local lua_camps_drop = all_config.lua_camps_drop
	local new_config = {}
	for copy_id,cfg in pairs(lua_camps_drop) do
		local _floor = cfg.floor
		new_config[_floor] = cfg
	end
	all_config.lua_camps_floor = new_config
	return true
end

function translate_zhaoqin_week(all_config)
	local lua_zhaoqing_week = all_config.lua_zhaoqing_week

	for _,cfg in pairs(lua_zhaoqing_week) do
		local shop_item_list = {}
		for i = 1,8 do
			local item_id = cfg['shop_item_id' .. i]
			local item_num = cfg['shop_num' .. i]
			local item_price = cfg['shop_price' .. i]

			if item_id > 0 and item_num > 0 and item_price > 0 then
				table_insert(shop_item_list,1,{
					item_id = item_id,
					num = item_num,
					price = item_price,
				})
			end
		end

		cfg.shop_item_list = shop_item_list
	end

	all_config.lua_zhaoqing_week = lua_zhaoqing_week

	return true
end

function translate_dianjiang_other_week(all_config)
	local lua_dianjiang_week = all_config.lua_dianjiang_week
	local other_dianjiang_week = {}
	local normal_dianjiang_week = {}
	for id,dianjiang_week in pairs(lua_dianjiang_week) do
		if id < 10000 then
			table_insert(normal_dianjiang_week,dianjiang_week)
		else
			table_insert(other_dianjiang_week,dianjiang_week)
		end
	end

	table_sort(normal_dianjiang_week,function (left,right) return left.id < right.id end)
	table_sort(other_dianjiang_week,function (left,right) return left.id < right.id end)
	--检查合法性
	for index,dianjiang_week in ipairs(normal_dianjiang_week) do
		if dianjiang_week.id ~= index then 
			return 
		end 
	end
	all_config.lua_normal_dianjiang_week = normal_dianjiang_week
	all_config.lua_other_dianjiang_week = other_dianjiang_week
	return true 
end

--检查有没有配置需要转化成另外一种格式的
local function translate(all_config)
	if not translate_wujiang_stage(all_config) then
		errlog('translate_wujiang_stage failed')
		return false
	end

	if not translate_shop_goods(all_config) then
		errlog('translate_shop_goods failed')
		return false
	end
	
	if not translate_blackmarket_shop_goods(all_config) then
		errlog('translate_blackmarket_shop_goods failed')
		return false
	end
	
	if not translate_qshop_goods(all_config) then
		errlog('translate_qshop_goods failed')
		return false
	end

	if not translate_lottery_blue_item(all_config) then 
		errlog(uid,'translate_lottery_blue_item failed')
		return false
	end
	
	if not translate_lottery_general_wujiang(all_config) then
		errlog('translate_lottery_general_wujiang failed')
		return false
	end

	if not translate_lottery_senior_wujiang(all_config) then
		errlog('translate_lottery_senior_wujiang failed')
		return false
	end

	if not translate_lottery_general_item(all_config) then
		errlog('translate_lottery_general_item failed')
		return false
	end

	if not translate_lottery_senior_item(all_config) then
		errlog('translate_lottery_senior_item failed')
		return false
	end

	if not translate_achieve(all_config) then
		errlog('translate_achieve failed')
		return false
	end

	if not translate_copy_drop(all_config) then
		errlog('translate_copy_drop failed')
		return false
	end

	if not translate_copy_section(all_config) then
		errlog('translate_copy_section failed')
		return false
	end

	if not translate_sys_open(all_config) then
		errlog('translate_sys_open failed')
		return false
	end

	if not translate_skill_attr(all_config) then
		errlog('translate_skill_attr failed')
		return false
	end

	if not translate_world_boss_rank_award(all_config) then
		errlog('translate_world_boss_rank_award failed')
		return false
	end

	if not translate_raid_robots(all_config) then
		errlog('translate_raid_robots failed')
		return false
	end

	if not translate_raid_baoxiang_maintype(all_config) then
		errlog('translate_raid_baoxiang_maintype failed')
		return false
	end

	if not translate_manor_task(all_config) then
		errlog('translate_manor_task failed')
		return false
	end

	if not translate_manor_shangbu(all_config) then
		errlog('translate_manor_shangbu failed')
		return false
	end

	if not translate_item_gift_bag(all_config) then
		errlog('translate_item_gift_bag failed')
		return false
	end

	if not translate_zhenfa(all_config) then
		errlog('translate_zhenfa failed')
		return false
	end

	if not translate_sign_award(all_config) then
		errlog(uid,'translate_sign_award failed')
		return false
	end

	if not translate_toc_baoxiang(all_config) then
		errlog(uid,'translate_toc_baoxiang failed')
		return false
	end

	if not translate_equip_stage_material(all_config) then
		errlog(uid,'translate_equip_stage_material failed')
		return false
	end

	if not translate_equip_stage_required(all_config) then
		errlog(uid,'translate_equip_stage_required failed')
		return false
	end

	if not translate_item_to_equip(all_config) then
		errlog(uid,'translate_item_to_equip failed')
		return false
	end

	if not translate_arena_award(all_config) then
		errlog(uid,'translate_arena_award failed')
		return false
	end

	if not translate_world_boss_award(all_config) then
		errlog(uid,'translate_world_boss_award failed')
		return false
	end

	if not translate_liezhuan(all_config) then
		errlog(uid,'translate_liezhuan failed')
		return false
	end
	
	if not translate_nianshou(all_config) then
		errlog(uid,'translate_nianshou failed')
		return false
	end
	
	if not translate_mojin(all_config) then
		errlog(uid,'translate_mojin failed')
		return false
	end
	
	if not translate_activity_award(all_config) then
		errlog(uid,'translate_activity_award failed')
		return false
	end
	
	if not translate_warlords_day_guild_award(all_config) then
		errlog(uid,'translate_warlords_day_guild_award failed')
		return false
	end
	
	if not translate_warlords_day_user_award(all_config) then
		errlog(uid,'translate_warlords_day_user_award failed')
		return false
	end
	
	if not translate_lottery_senior_prob_item(all_config) then
		errlog(uid,'translate_lottery_senior_prob_item failed')
		return false
	end
	
	if not translate_lottery_general_prob_item(all_config) then
		errlog(uid,'translate_lottery_general_prob_item failed')
		return false
	end
	
	if not translate_wujiang_no_show(all_config) then
		errlog(uid,'translate_wujiang_no_show failed')
		return false
	end
	
	if not translate_camps_floor(all_config) then
		errlog(uid,'translate_camps_floor failed')
		return false
	end

	if not translate_zhaoqin_week(all_config) then
		errlog(uid,'translate_zhaoqin_week failed')
		return false
	end
	
	if not translate_dianjiang_other_week(all_config) then
		errlog(uid,'translate_dianjiang_other_week failed')
		return false
	end
	
	if not translate_turntable_box_item(all_config) then 
		errlog(uid,'translate_turntable_box_item failed')
		return false
	end
	
	if not translate_landlady_box(all_config) then 
		errlog(uid,'translate_landlady_box failed')
		return false
	end
	
	if not translate_landlady_item(all_config) then 
		errlog(uid,'translate_landlady_item failed')
		return false
	end
	
	if not translate_special_gift_item(all_config) then
		errlog(uid,'translate_special_gift_item failed')
		return false
	end

	if not translate_nslg_city_connect(all_config) then
		errlog(uid, "translate_nslg_city_connect failed")
		return false
	end
	
	if not translate_nslg_public_city(all_config) then
		errlog(uid, "translate_nslg_public_city failed")
		return false
	end
	
	
	return true
end

local function check_item(all_config)

end

local function check_copy(all_config)

end

local function check_level_limit(all_config)
	--检查主公的经验配置
	local lua_user_exp = all_config.lua_user_exp
	if not lua_user_exp then
		errlog('check lua_user_exp nil')
		return false
	end

	local last_cfg = lua_user_exp[#lua_user_exp]
	if not last_cfg then
		errlog('check user last_cfg failed nil')
		return false
	end

	if last_cfg.exp ~= -1 then
		errlog('check user last_cfg exp failed nil',last_cfg.id,last_cfg.exp)
		return false
	end

	--检查武将的经验配置
	local lua_wujiang_exp = all_config.lua_wujiang_exp
	if not lua_wujiang_exp then
		errlog('check lua_wujiang_exp nil')
		return false
	end

	local last_cfg = lua_wujiang_exp[#lua_wujiang_exp]
	if not last_cfg then
		errlog('check wujiang last_cfg failed nil')
		return false
	end

	if last_cfg.exp ~= -1 then
		errlog('check wujiang last_cfg exp failed nil',last_cfg.id,last_cfg.exp)
		return false
	end

	--检查装备的经验配置
	local lua_equip_exp = all_config.lua_equip_exp
	if not lua_equip_exp then
		errlog('check lua_wujiang_exp nil')
		return false
	end

	local last_cfg = lua_equip_exp[#lua_equip_exp]
	if not last_cfg then
		errlog('check equip last_cfg failed nil')
		return false
	end

	if last_cfg.exp ~= -1 then
		errlog('check equip last_cfg exp failed nil',last_cfg.id,last_cfg.exp)
		return false
	end


	return true
end

local function check_skill_improve(all_config)
	local lua_skill_improve = all_config.lua_skill_improve
	if not lua_skill_improve then
		errlog('check lua_skill_improve nil')
		return false
	end

	local last_cfg = lua_skill_improve[#lua_skill_improve]
	if not last_cfg then
		errlog('check skill_improve last_cfg failed nil')
		return false
	end

	if last_cfg.price ~= -1 then
		errlog('check skill_improve last_cfg exp failed nil',last_cfg.id,last_cfg.price)
		return false
	end

	return true
end

local function check_bag_gift_exist(all_config)
	local lua_item_gift_bag = all_config.lua_item_gift_bag
	local lua_item = all_config.lua_item
	if not lua_item_gift_bag then 
		errlog('check lua_item_gift_bag nil')
		return false
	end
	for gift_id,gift in pairs(lua_item_gift_bag) do
		if not lua_item[gift_id] then
			errlog('check lua_item_gift_bag nil',gift_id) 
			return false
		end
		for i=1,10 do
			local check_key = gift['item_id'..i]
			if check_key ~= 0 then
				if not lua_item[check_key] then
					errlog(check_key,'check lua_item_gift_bag nil',gift_id,i) 
					return false
				end
			end 
		end 
	end
	return true 
end
--------------------------check item 相关配置------------------------------
local function check_all_item_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_item_gift_bag = all_config.lua_item_gift_bag
	local lua_equip_stage_material = all_config.lua_equip_stage_material
	local lua_equip = all_config.lua_equip
	local lua_item_func = all_config.lua_item_func
	local lua_lottery_general_item = all_config.lua_lottery_general_item
	local lua_raid_item_prob = all_config.lua_raid_item_prob
	local lua_raid_robot_item_prob = all_config.lua_raid_robot_item_prob
	local lua_lottery_senior_item = all_config.lua_lottery_senior_item
	local function Check(check_table,lua_item,check_table_name)
		if not check_table then 
			return false
		end
		for check_id,check in pairs(check_table) do
			if not lua_item[check_id] then
				errlog(check_table_name,check_id)
				return false
			end
		end
		return true 
	end
	if not Check(lua_equip_stage_material,lua_item,'lua_equip_stage_material') then 
		errlog('check lua_equip_stage_material nil')
		return false 
	end 
	if not Check(lua_equip,lua_item,'lua_equip') then 
		errlog('check lua_equip nil') 
		return false
	end 
	if not Check(lua_item_func,lua_item,'lua_item_func') then 
		errlog('check lua_item_func nil') 
		return false
	end 
	if not Check(lua_lottery_general_item,lua_item,'lua_lottery_general_item') then 
		errlog('check lua_lottery_general_item nil') 
		return false
	end
	if not Check(lua_raid_item_prob,lua_item,'lua_raid_item_prob') then 
		errlog('check lua_raid_item_prob nil') 
		return false
	end 
	if not Check(lua_raid_robot_item_prob,lua_item,'lua_raid_robot_item_prob') then 
		errlog('check lua_raid_robot_item_prob nil') 
		return false
	end
	if not Check(lua_lottery_senior_item,lua_item,'lua_lottery_senior_item') then 
		errlog('check lua_lottery_senior_item nil') 
		return false
	end 
	----------校验礼物的打开方式是否存在-----------------------
	for id,item in pairs(lua_item) do
		if id >= 33001 and id <= 33999 and item.maintype == 5 then --礼物盒的ID区间
			if item.subtype ~= 2 then 
				errlog('item.subtype is wrong',id,item.subtype)
				return false 
			end 
			
			if not lua_item_gift_bag[id] then 
				errlog('find item_gift from lua_item_gift_bag failed',id)
				return false 
			end 
			
		end
	end 
	return true
end 

local function check_activity_item_exist(all_config)
	local lua_activity = all_config.lua_activity
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_activity) do
		for i=1,2 do
			local check_key = check['award_id'..i]
			if check_key ~= 0 then 
				if not lua_item[check_key] then 
					errlog('check lua_activity have nil',check_key,id)
					return false
				end
			end
		end
		if not check.need_level or check.need_level == 0 then--防止存在活动开放等级未填的情况 
			errlog('activity open level is not exist',check.need_level,id)
			return false
		end 
	end
	return true
end 

local function check_arena_award_item_exist(all_config)
	local lua_arena_award = all_config.lua_arena_award
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_arena_award) do
		for i=1,6 do
			local check_key=check['award_id'..i]
			if check_key ~= 0 then 
				if not lua_item[check_key] then 
					errlog('check lua_arena_award have nil',check_key,id,i)
					return false
				end
			end
		end 
	end
	return true
end 

local function check_arena_shop_fixed_goods_item_exist(all_config)
	local lua_arena_shop_fixed_goods = all_config.lua_arena_shop_fixed_goods
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_arena_shop_fixed_goods) do
		if check.item_id and check.item_id ~= 0 then 
			if not lua_item[check.item_id] then 
				errlog('check lua_arena_shop_fixed_goods have nil',check.item_id)
				return false
			end
		else
			if check.count < 1 then 
				errlog('goods count is nil or 0',id)
				return false
			end
			if check.price < 1 then 
				errlog('goods price is nil or 0',id)
				return false
			end
		end 
	end
	return true
end 

local function check_copy_drop_item_exist(all_config)
	local lua_copy_drop = all_config.lua_copy_drop
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_copy_drop) do
		for i=1,16 do
			local check_key=check['item_id'..i]
			if check_key and check_key ~=0 then 
				if not lua_item[check_key] then 
					errlog('check lua_copy_drop have nil',check_key,id)
					return false
				end
			end
		end 
	end
	return true
end 

local function check_dragon_item_exist(all_config)
	local lua_dragon = all_config.lua_dragon
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_dragon) do
		if check.longhun_id then 
			if not lua_item[check.longhun_id] then 
				errlog('check lua_dragon have nil',check.longhun_id)
				return false
			end
		end
	end
	return true
end 

local function check_dragon_improve_exist(all_config)
	lua_dragon_improve = all_config.lua_dragon_improve
	for id,check in pairs(lua_dragon_improve) do
		if check.dragon_exp == 1 then 
			errlog('dragon_improve count is nil or 0',id,check.dragon_exp)
			return false
		end
	end
	return true
end

local function check_equip_stage_material_item_exist(all_config)
	local lua_equip_stage_material = all_config.lua_equip_stage_material
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_equip_stage_material) do
		for i=1,4 do
			for j=1,4 do
				local check_key=check['stage'..i..'_item_id'..j]
				if check_key ~= 0 then
					local lua_item_check = lua_item[check_key]
					if not lua_item_check then 
						errlog('check lua_equip_stage_material have nil',check_key)
						return false
					end
					
					if lua_item_check.maintype~=EQUIP_STAGE then 
						errlog(check_key,'type is not equip material',check.id)
						return false
					end
				end
			end
		end 
	end
	return true
end 

local function check_item_blend_item_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_item_blend = all_config.lua_item_blend
	for id,check in pairs(lua_item_blend) do
		local check_key = check.product_id
		if check_key then 
			if not lua_item[check_key] then 
				errlog('check lua_item_blend have nil',check_key,id)
				return false
			end
		end
		for i=1,4 do
			local check_key = check['item_id'..i]
			if check_key ~= 0 then 
				if not lua_item[check_key] then 
					errlog('check lua_item_blend have nil',check_key,id)
					return false
				end
			end
		end 
	end
	return true
end 

local function check_qshop_goods_item_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_qshop_goods = all_config.lua_qshop_goods
	for id,check in pairs(lua_qshop_goods) do
		local check_key = check.item_id
		if check_key ~= 0 then 
			if not lua_item[check_key] then 
				errlog('check lua_qshop_goods have nil',check_key)
				return false
			end
		end
	end
	return true
end 

local function check_raid_baoxiang_goods_item_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_raid_baoxiang_goods = all_config.lua_raid_baoxiang_goods
	for id,check in pairs(lua_raid_baoxiang_goods) do
		local check_key = check.item_id
		if check_key ~= 0 then 
			if not lua_item[check_key] then 
				errlog('check lua_raid_baoxiang_goods have nil',check_key)
				return false
			end
		end
	end
	return true
end 

local function check_shop_goods_item_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_shop_goods = all_config.lua_shop_goods
	local lua_shop_maintype = all_config.lua_shop_maintype
	for id,check in pairs(lua_shop_goods) do
		local check_key = check.item_id
		local check_type = check.maintype
		if check_key ~= 0 then 
			if not lua_item[check_key] then 
				errlog('check lua_shop_goods have nil',check_key)
				return false
			end
			if not lua_shop_maintype[check_type] then 
				errlog('check lua_shop_goods maintype is nil',check_type)
				return false
			end
		end
		if check.price_yuanbao <= 0 and check.price_money <=0 then 
			errlog('check lua_shop_goods yuanbao and money is zero',check_key,check.price_yuanbao,check.price_money)
			return false
		end
	end
	
	return true
end 

local function check_shop_maintype_exist(all_config)
	local lua_shop_maintype = all_config.lua_shop_maintype
	local lua_arena_shop_maintype = all_config.lua_arena_shop_maintype
	local lua_toc_shop_maintype = all_config.lua_toc_shop_maintype
	local lua_guild_shop_maintype = all_config.lua_guild_shop_maintype
	local lua_shop_goods_ex = all_config.lua_shop_goods_ex
	--检查商店商品类型
	for id,check in pairs(lua_shop_maintype) do
		if not lua_shop_goods_ex[id] then 
			errlog('shop_goods_config_ex[id] is nil',id)
			return false
		end 
	end
	--检查竞技场商品类型
	for id,check in pairs(lua_arena_shop_maintype) do
		if not lua_shop_goods_ex[id] then 
			errlog('shop_goods_config_ex[id] is nil',id)
			return false
		end 
	end
	--检查过关斩将商品类型
	for id,check in pairs(lua_toc_shop_maintype) do
		if not lua_shop_goods_ex[id] then 
			errlog('shop_goods_config_ex[id] is nil',id)
			return false
		end 
	end
	--检查军团商店商品类型
	for id,check in pairs(lua_guild_shop_maintype) do
		if not lua_shop_goods_ex[id] then 
			errlog('shop_goods_config_ex[id] is nil',id)
			return false
		end 
	end
	return true
end

local function check_raid_baoxiang_maintype_exist(all_config)
	local lua_raid_baoxiang_maintype = all_config.lua_raid_baoxiang_maintype
	for id,check in pairs(lua_raid_baoxiang_maintype) do
		if check.goods_list == {} then 
			errlog('lua_raid_baoxiang_maintype.goods_list is nil',id)
			return false
		end 
	end 
	return true
end

local function check_toc_baoxiang_maintype_exist(all_config)
	local lua_toc_baoxiang_ex = all_config.lua_toc_baoxiang_ex
	local lua_toc_baoxiang_maintype = all_config.lua_toc_baoxiang_maintype
	for id,check in pairs(lua_toc_baoxiang_maintype) do
		if not lua_toc_baoxiang_ex[id] then 
			errlog('lua_toc_baoxiang_ex[id] is nil',id)
			return false
		end 
	end 
	return true
end 

local function check_sign_award_item_exist(all_config)
	local lua_sign_award = all_config.lua_sign_award
	local lua_item = all_config.lua_item
	local lua_role = all_config.lua_role
	for id,check in pairs(lua_sign_award) do
		for i=1,31 do
			local check_key=check['award_id'..i]
			if check_key ~= 0 then 
				if not lua_item[check_key] and not lua_role[check_key] then 
					errlog('check lua_sign_award is nil',check_key)
					return false
				end
			end
		end 
	end
	return true
end 

local function check_toc_baoxiang_item_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_toc_baoxiang = all_config.lua_toc_baoxiang
	local lua_role = all_config.lua_role
	for id,check in pairs(lua_toc_baoxiang) do
		local check_key = check.item_id
		if check_key ~= 0 then 
			if not lua_item[check_key] and not lua_role[check_key] then 
				errlog('check lua_toc_baoxiang have nil',check_key)
				return false
			end
		end
	end
	return true
end 

local function check_toc_shop_fixed_goods_item_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_toc_shop_fixed_goods = all_config.lua_toc_shop_fixed_goods
	for id,check in pairs(lua_toc_shop_fixed_goods) do
		local check_key = check.item_id
		if check_key ~= 0 then 
			if not lua_item[check_key] then 
				errlog('check lua_toc_shop_fixed_goods have nil',check_key)
				return false
			end
		end
	end
	return true
end 

local function check_world_boss_rank_award_item_exist(all_config)
	local lua_world_boss_rank_award = all_config.lua_world_boss_rank_award
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_world_boss_rank_award) do
		for i=1,6 do
			local check_key=check['award'..i..'_id']
			if check_key ~= 0 then 
				if not lua_item[check_key] then 
					errlog('check lua_world_boss_rank_award have nil',check_key)
					return false
				end
			end
		end 
	end
	return true
end 

local function check_wujiang_stage_item_exist(all_config)
	local lua_wujiang_stage = all_config.lua_wujiang_stage
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_wujiang_stage) do
		if id > 0 then
			local need_level = check.need_level
			local price = check.price
			
			if need_level < 1 then 
				errlog('wujiang stage need_level cano not 0',need_level)
				return false
			end 
			
			if price < 1 then
				errlog('wujiang stage price cano not ',price)
				return false
			end
			
			for j=1,4 do
				local check_key = check['item_id'..j]
				local check_count = check['item_count'..j]
				if check_key ~= 0 then 
					local lua_item_check = lua_item[check_key]
					if not lua_item_check then 
						errlog('check lua_wujiang_stage have nil',check_key,check.id)
						return false
					else
						if lua_item_check.maintype~=WUJIANG_STAGE then 
							errlog(check_key,'type is not wujiang stage material',check.id)
						end
					end
					if check_count < 1 then 
						errlog('wujiang_tage_item count is 0',check_count)
						return false
					end 
				end
			end 
		end
	end
	return true
end

local function check_zhenfa_item_exist(all_config)
	local lua_zhenfa = all_config.lua_zhenfa
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_zhenfa) do
		for i=1,4 do
			local check_key=check['item'..i]
			if check_key ~= 0 then
				local lua_item_check = lua_item[check_key]
				if not lua_item_check then 
					errlog('check lua_zhenfa have nil',check_key,check.id)
					return false
				else
					if lua_item_check.maintype~=ZHENFA then 
						errlog(check_key,'type is not zhenfa material',check.id)
						return false
					end 
				end
			end
			for _id,_check in pairs(lua_zhenfa) do
				for j=1,4 do
					if j ~= i or id ~= _id then 
						local _check_key=_check['item'..j]
						if check_key == _check_key then 
							errlog(id,check_key,'is same to',_id,_check_key)
							return false
						end 
					end 
				end 
			end 
		end 
	end
	return true
end

local function check_manor_shangbu_shop_item_exist(all_config)
	local lua_manor_shangbu_shop = all_config.lua_manor_shangbu_shop
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_manor_shangbu_shop) do
		if check.item_id then 
			if not lua_item[check.item_id] then 
				errlog('check lua_manor_shangbu_shop have nil',check.item_id)
				return false
			end
		end
	end
	return true
end

local function check_manor_task_item_exist(all_config)
	local lua_manor_task = all_config.lua_manor_task
	local lua_item = all_config.lua_item
	for id,check in pairs(lua_manor_task) do
		for i=1,3 do
			local check_key=check['star'..i..'_item_id']
			if check_key ~= 0 then 
				if not lua_item[check_key] then 
					errlog('check lua_manor_task have nil',check_key)
					return false
				end
			end
		end 
	end
	return true
end 

local function check_manor_building_expend(all_config)
	local lua_manor_building = all_config.lua_manor_building
	for id,check in pairs(lua_manor_building) do
		local expend_num = 0
		if check.levelup_price > 0 then 
			expend_num = expend_num + 1 
		end
		if check.levelup_material > 0 then 
			expend_num = expend_num + 1 
		end
		if check.levelup_food > 0 then 
			expend_num = expend_num + 1 
		end
		if expend_num < 1 and check.level > 1 then
			errlog('manor building expend_num is not 2',expend_num,id)
			return false
		end 
	end
	return true
end 
--------------------------------------------武器相关配置检查----------------------------------------
local function check_equip_attr_exist(all_config)
	local part={}
	part[1]={'phy_atk','magic_atk','mingzhong'}
	part[2]={'max_hp','phy_def','magic_def','mingzhong'}
	part[3]={'phy_def','magic_def','phy_baoji','magic_baoji'}
	part[4]={'max_hp','shanbi','phy_baoji','magic_baoji'}
	part[5]={'max_hp','phy_atk','magic_atk','shanbi'}
	part[6]={'phy_atk','magic_atk','nuqi_recover','hp_recover'}
	local lua_equip_attr = all_config.lua_equip_attr
	for id,equip_attr in pairs(lua_equip_attr) do
		local l_part = equip_attr.part
		local stage = equip_attr.stage
		if l_part ~= 6 and stage ~= 1 then
			if equip_attr.part then
				for _,attr in pairs( part[equip_attr.part]) do
					local attreNum = equip_attr[attr]
					if attreNum == 0 or attreNum == nil then 
						errlog('check lua_equip_attr nil',attr,equip_attr.id)
						return false
					end
				end
			end
		end 
	end	
	return true
end

local function check_equip_exist(all_config)
	local part={}
	part[1]={'phy_atk','magic_atk','mingzhong'}
	part[2]={'max_hp','phy_def','magic_def','mingzhong'}
	part[3]={'phy_def','magic_def','phy_baoji','magic_baoji'}
	part[4]={'max_hp','shanbi','phy_baoji','magic_baoji'}
	part[5]={'max_hp','phy_atk','magic_atk','shanbi'}
	part[6]={'phy_atk','magic_atk','nuqi_recover','hp_recover'}
	local lua_equip = all_config.lua_equip
	local lua_item = all_config.lua_item
	local lua_equip_skill = all_config.lua_equip_skill
	for id,equip in pairs(lua_equip) do
		if equip.part then
			for _,attr in pairs( part[equip.part]) do
				local attreNum = equip[attr]
				if attreNum == 0 or attreNum == nil then 
					errlog('check lua_equip nil',attr,equip.id)
					return false
				end
			end
		end
		if lua_item[id] then
			local l_item = lua_item[id] 
			if l_item.maintype ~= EQUIP_MAINTYPE then 
				errlog(id,'item mantype is error')
			end 
			if equip.part ~= lua_item[id].subtype then
				errlog(id,'equip part not item subtype',equip.part)
				return false
			end
		else
			errlog(id,'is not exist table item ',id)
		end
		for i=2,4 do
			local check_key = equip['stage_skill_list'..i]
			if check_key~={} then
				for _id,skill in pairs(check_key) do
					if not lua_equip_skill[skill] then 
						errlog(skill,'is not exist equip_skill table',id,_id)
						return false
					end 
				end
			end  	
		end 
	end
	return true	
end
---------------------------------------------vip相关校验----------------------------------
local function check_vip_privilege_data_exist(all_config)
	local lua_vip_privilege = all_config.lua_vip_privilege
	local lua_raid_buy_times_cost = all_config.lua_raid_buy_times_cost
	local lua_arena_buy_times_cost = all_config.lua_arena_buy_times_cost
	local vip_lenth = #lua_vip_privilege
	local raid_cost_lenth = #lua_raid_buy_times_cost
	local arena_cost_lenth = #lua_arena_buy_times_cost
	if lua_vip_privilege[vip_lenth].arena_buy_times ~=  arena_cost_lenth then 
		errlog('vip_privilege table max arena_buy_times != arena_cost_lenth',arena_buy_times)
		return false
	end
	if lua_vip_privilege[vip_lenth].raid_buy_times ~=  raid_cost_lenth then 
		errlog('vip_privilege table max raid_buy_times != raid_cost_lenth',raid_buy_times)
		return false
	end
	return true
end 
---------------------------------------------武将相关配置检查-------------------------------
local function check_role_all_data_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_equip = all_config.lua_equip
	local lua_role = all_config.lua_role
	local lua_skill = all_config.lua_skill
	local lua_youli_qiyu = all_config.lua_youli_qiyu 
	local part={'weapon_id','clothes_id','hat_id','ornament_id','ring','horse_id',}
	for role_id,role in pairs(lua_role) do
		if role_id < WUJIANG_ID_LIMIT and role_id~=1099 then --张让为特殊角色，不能作为武将
			for part_id,equip_type in ipairs(part) do
				if role[equip_type] and role[equip_type]~=0 then 
					if lua_item[role[equip_type]] then
						local l_item = lua_item[role[equip_type]] 
						if l_item.maintype ~= EQUIP_MAINTYPE then 
							errlog('l_item.maintype is not EQUIP_MAINTYPE',role_id)
							return false
						end
						if l_item.subtype ~= part_id then 
							errlog('l_item.subtype is not part',role_id,role[equip_type])
							return false
						end
					else
						errlog(role[equip_type],'is not exist item_table',role_id,equip_type)
						return false
					end
					if lua_equip[role[equip_type]] then
						local role_equip = role[equip_type]
						if role_equip < 50001 then--大于50000的装备无职业限制 
							local l_equip = lua_equip[role[equip_type]]
							if l_equip.job ~= role.job then 
								errlog(role[equip_type],'l_item.job is not equip.job',role_id)
								return false
							end
							if l_equip.part ~= part_id then 
								errlog('equip.part is not role.'..equip_type,role_id,role[equip_type])
								return false
							end
						end
					else
						errlog(role[equip_type],'is not exist equip_table',role_id,equip_type)
						return false
					end 
				else
					errlog(role[equip_type],'is not exist role_table',role_id,equip_type)
					return false
				end 
			end
			 
			if role.embryo_weapon_id and role.embryo_weapon_id~=0 then 
				if lua_item[role.embryo_weapon_id] then
					local l_item = lua_item[role.embryo_weapon_id] 
					if l_item.maintype ~= EQUIP_MAINTYPE then 
						errlog('l_item.maintype is not EQUIP_MAINTYPE',role_id)
						return false
					end
					if l_item.subtype ~= EMBRYO_WEAPON_PART then 
						errlog('l_item.subtype is not part',role_id,role.embryo_weapon_id)
						return false
					end
				else
					errlog('l_item.subtype is not part',role_id)
					return false
				end
			else
				errlog('embryo_weapon_id is not exist item_table',role_id,'embryo_weapon_id',role.embryo_weapon_id)
				return false
			end
			----------信物-------------
			if role.xinwu_id and role.xinwu_id~=0 then 
				if not lua_item[role.xinwu_id] then
					errlog('role.xinwu_id is not exist item_table',role_id,'xinwu_id',role.xinwu_id)
					return false
				else
					if lua_item[role.xinwu_id].maintype ~= XINWU then
						errlog(role.xinwu_id,'type is not xinwu',role_id)
						return false
					end
				end 
			end
			-----奇遇配置----
			if not lua_youli_qiyu[role_id] then 
				errlog('can not find wujiang from lua_youli_qiyu',role_id)
				return false
			end 
		end
		----------检查星星数目----------
		if not role.born_star or role.born_star > 5 then 
			errlog('role.born_star number is error',role_id)
			return false
		end
		----------检查职业配置----------
		if not role.job or role.job > 5 then 
			errlog('role.job type is error',role_id)
			return false
		end
		----------检查技能列表-----------
		if role.skill_list then 
			for _,skill_id in pairs(role.skill_list) do
				if not lua_skill[skill_id] then 
					errlog('this skill is not exist',role_id,skill_id)
					return false
				end
			end
		else
			errlog('skill_list is nill',role_id)
			return false
		end
	end 
	return true
end 	
-------------------武将ID校验-------------------
local function check_role_exist(all_config)
	local lua_role = all_config.lua_role
	local lua_lottery_general_wujiang = all_config.lua_lottery_general_wujiang
	local lua_lottery_senior_first = all_config.lua_lottery_senior_first
	local lua_lottery_general_first = all_config.lua_lottery_general_first
	for role_id,lottery in pairs(lua_lottery_senior_first) do
		if not lua_role[role_id] then 
			errlog('lua_lottery_senior_first',role_id,'is not exist role_table')
			return false
		end
	end	
	for role_id,lottery in pairs(lua_lottery_general_wujiang) do
		if not lua_role[role_id] then 
			errlog('lua_lottery_general_wujiang',role_id,'is not exist role_table')
			return false
		end
	end	
	for role_id,lottery in pairs(lua_lottery_general_first) do
		if not lua_role[role_id] then 
			errlog('lua_lottery_general_first',role_id,'is not exist role_table')
			return false
		end
	end	
	return true
end 
---------------------玩法配置校验-----------------
local function check_lottery_general_first_exist(all_config)
	local lua_lottery_general_first = all_config.lua_lottery_general_first
	local lenth = 0
	for _,_ in pairs(lua_lottery_general_first) do
		lenth = lenth + 1
	end
	if lenth < 1 then 
		errlog('lottery_general_first is nil table',lenth)
		return false
	end
	return true
end

local function check_toc_baoxiang_round_exist(all_config)
	local lua_toc_baoxiang_round = all_config.lua_toc_baoxiang_round
	local lenth = 0
	for _,_ in pairs(lua_toc_baoxiang_round) do
		lenth = lenth + 1
	end
	if lenth < 5 then 
		errlog('lua_toc_baoxiang_round is nil table',lenth)
		return false
	end
	return true
end 

local function check_talent_translate_exist(all_config)
	local lua_talent_translate = all_config.lua_talent_translate
	local lenth = 0
	for _,_ in pairs(lua_talent_translate) do
		lenth = lenth + 1
	end
	if lenth < 7 then 
		errlog('lua_talent_translate is nil table',lenth)
		return false
	end
	return true
end 

local function check_lottery_item_type_exist(all_config)
	local lua_lottery_general_prob = all_config.lua_lottery_general_prob
	local lua_lottery_senior_prob = all_config.lua_lottery_senior_prob
--普通点将奖励类型校验
	local lua_lottery_general_item = all_config.lua_lottery_general_item
	for id,_ in pairs(lua_lottery_general_prob) do
		if id < 100 then 
			if not lua_lottery_general_item[id] then 
				errlog('find lua_lottery_general_item type failed',id)
				return false
			end 
		end 
	end
--高级点将奖励类型校验
	local lua_lottery_senior_item = all_config.lua_lottery_senior_item
	for id,_ in pairs(lua_lottery_senior_prob) do
		if id < 100 then 
			if not lua_lottery_senior_item[id] then 
				errlog('find lua_lottery_senior_item type failed',id)
				return false
			end 
		end 
	end
	return true 
end

local function check_skill_activate_exist(all_config)
	local lua_skill_activate = all_config.lua_skill_activate
	local lenth = 0
	for _,_ in pairs(lua_skill_activate) do
		lenth = lenth + 1
	end
	if lenth < 5 then 
		errlog('lua_skill_activate is nil table',lenth)
		return false
	end
	return true
end

local function check_arena_shop_maintype_exist(all_config)
	local lua_arena_shop_maintype = all_config.lua_arena_shop_maintype
	for id,arena_type in pairs(lua_arena_shop_maintype) do
		if arena_type.probability < 0 then
			errlog('arena_shop_maintype probability is nil or 0',id)
			return false
		end
		if arena_type.max_count < 0 then
			errlog('arena_shop_maintype max_count is nil or 0',id)
			return false
		end
	end
	return true
end

local function Check_all_wanfa_seting(all_config)
	check_skill_activate_exist(all_config)
	check_talent_translate_exist(all_config)
	check_toc_baoxiang_round_exist(all_config)
	check_lottery_general_first_exist(all_config)
	return true
end 
----------------------宝箱物品类型的校验-----------------------------------
local function Check_toc_baoxiang_maintype_data_exist(all_config)
	local lua_toc_baoxiang_maintype = all_config.lua_toc_baoxiang_maintype
	local lua_toc_baoxiang = all_config.lua_toc_baoxiang
	for id,baoxiang in pairs(lua_toc_baoxiang) do
		if baoxiang.maintype then
			local check_baoxiang = lua_toc_baoxiang_maintype[baoxiang.maintype]
			if not check_baoxiang then
				errlog('toc_baoxiang_maintype is not exist this type',baoxiang.maintype,id)
				return false
			end
		else
			errlog('toc_baoxiang is not exist this type',id)
			return false
		end 
	end
	return true
end 
---------------月卡签到奖励校验------------------------------------------
local function Check_sign_award_exist(all_config)
	local lua_sign_award = all_config.lua_sign_award
	local lua_item = all_config.lua_item
	local lua_role = all_config.lua_role
	for id,check in pairs(lua_sign_award) do
		for i=1,25 do
			local check_key = check['award_id'..i]
			if not check_key or check_key == 0 then 
				errlog('lua_sign_award sign_award is nil',id,i)
				return false
			else
				local l_item = lua_item[check_key]
				local l_role = lua_role[check_key]
				if not l_item and not l_role then 
					errlog('sign_award is not exist lua_item table and lua_role table',id,check_key,i)
					return false
				end 					
			end 
		end
		
		local check_key = check['role_yueka_gift_id']
		if not check_key or check_key == 0 then 
			errlog('lua_sign_award role_yueka_gift_id is nil',id)
			return false
		else
			local l_item = lua_item[check_key]
			if not l_item then 
				errlog('sign_award is not exist lua_item table',id,check_key)
				return false
			end 					
		end 
		
		check_key = check['roleid']
		if not check_key or check_key == 0 then 
			errlog('lua_sign_award roleid is nil',id)
			return false
		else
			local l_role = lua_role[check_key]
			if not l_role then 
				errlog('sign_award is not exist lua_role table',id,check_key)
				return false
			end 					
		end 
	end
	
	return true
end
------------------------游历的配置校验---------------------------------------------
local function check_qiyu_award_exist(all_config)
	local lua_qiyu_award = all_config.lua_qiyu_award
	local lua_item = all_config.lua_item
	local qiyu_award_cfg = {}
	local award = {}
	local l_qiyu_award = {}
	local choose = {}
	all_config.qiyu_award_cfg={}
	for id,qiyu_award in pairs(lua_qiyu_award) do 
		choose = {}
		for i=1,1 do
			award = {}
			for j=1,2 do
				local check_key = qiyu_award['choose'..i..'_award_id'..j]
				if not check_key or check_key == 0 then 
					errlog('lua_qiyu_award qiyu_award is nil',id,i,j)
					return false
				else
					local l_item = lua_item[check_key]
					if not l_item then 
						errlog('qiyu_award is not exist lua_item table',id,check_key,i,j)
						return false
					end 					
				end 
				
				local check_count = qiyu_award['choose'..i..'_award_count'..j]
				if not check_count or check_count < 1 then 
					errlog('qiyu_award_count is',check_count,id,i,j)
					return false
				end 
				award[check_key] = check_count
			end
			table_insert(choose,award)
		end
		all_config.qiyu_award_cfg[id]=choose	
	end
	
	return true
end

local function check_wujiang_qiyu_exist(all_config)
	local lua_qiyu_award = all_config.lua_qiyu_award
	local lua_youli_qiyu = all_config.lua_youli_qiyu 
	local lua_role = all_config.lua_role
	all_config.lua_wujiang_qiyu_cfg={}
	local lua_wujiang_qiyu_cfg = all_config.lua_wujiang_qiyu_cfg
	for id,youli_qiyu in pairs(lua_youli_qiyu) do
		local lenth = 10
		local qiyu_list={}
		for i=1,lenth do
			local check_key = youli_qiyu['qiyu_id'..i]
			if check_key ~= 0 then 
				if not lua_qiyu_award[check_key] then 
					errlog('qiyu_id can not find in lua_qiyu_award',id,i,check_key)
					return false
				end
				table_insert(qiyu_list,check_key)
			end 
		end 
		if #qiyu_list < 1 then 
			errlog('qiyu_list lenth is 0',id)
			return false
		end 
		lua_wujiang_qiyu_cfg[id] = qiyu_list
	end 

	return true
end 

local function check_liezhuan_item_exist(all_config)
	local lua_item = all_config.lua_item
	local lua_liezhuan = all_config.lua_liezhuan
	for id,liezhuan in pairs(lua_liezhuan) do
		local stamina = liezhuan.stamina
		local country = liezhuan.country
		local level = liezhuan.level
		local drop_floor = liezhuan.drop_floor
		local drop_ceil = liezhuan.drop_ceil
		if stamina < 0 then 
			errlog('stamina is 0',id,stamina)
			return false
		end
		
		if country <= 0 or country > 3 then 
			errlog('country is not exist',id,country)
			return false
		end
		
		if level <=0 then 
			errlog('level is 0,level must max than 0',id,level)
			return false
		end
		
		if drop_ceil < drop_floor then 
			errlog('drop_floor can not max than drop_ceil',id,drop_ceil,drop_floor)
			return false
		end 
		
		for i=1,12 do
			local check_key = liezhuan['item'..i]
			local check_prob = liezhuan['prob'..i]
			if check_key > 0 then 
				if not lua_item[check_key] then
					errlog('can not find this item from item_table',id,i,check_key)
					return false
				end
				
				if check_prob <= 0 then 
					errlog('item prob is 0',id,i,check_prob,check_key)
					return false
				end 
			end 
		end 
	end 
	
	return true
end 

local function check_arena_highest_rank_award(all_config)
	local lua_arena_highest_rank_award = all_config.lua_arena_highest_rank_award
	local count = #lua_arena_highest_rank_award
	for id,limit_tb in ipairs(lua_arena_highest_rank_award) do
		if limit_tb.high_rank >= limit_tb.low_rank then 
			errlog('high_rank is big than low_rank')
			return false
		end
		if id < count then 
			local next_limit_tb = lua_arena_highest_rank_award[id + 1]
			local low_rank = limit_tb.low_rank
			local next_high_rank = next_limit_tb.high_rank
			if low_rank ~= next_high_rank - 1 then 
				errlog('zhe rank not Continuous',low_rank,next_high_rank)
				return false
			end 
		end
	end
	return true
end
-----------------------------------------------招亲配置校验-------------------------------
local function check_zhaoqing_week_sort(all_config)
	local lua_item = all_config.lua_item
	local lua_zhaoqing_week = all_config.lua_zhaoqing_week
	local lua_role = all_config.lua_role
	local count = 0
	for id,zhaoqing_week in pairs(lua_zhaoqing_week) do
		local xinwu_id = zhaoqing_week.xinwu_id
		local wujiang_id = zhaoqing_week.wujiang_id1
		if not lua_item[xinwu_id] then 
			errlog('this xinwu can not find',xinwu_id)
			return false
		end
		if not lua_role[wujiang_id] then 
			errlog('this wujiang_id can not find',wujiang_id)
			return false
		end
		for i=1,8 do
			local award_id = zhaoqing_week['award_id'..i]
			if not lua_item[award_id] then 
				errlog('this award_id can not find',award_id)
				return false
			end
		end
		local award_id = zhaoqing_week.weed_award
		if not lua_item[award_id] then 
			errlog('this award_id can not find',award_id)
			return false
		end
		local award_id = zhaoqing_week.join_award
		if not lua_item[award_id] then 
			errlog('this award_id can not find',award_id)
			return false
		end
		local award_id = zhaoqing_week.stop_match_award
		if not lua_item[award_id] then 
			errlog('this award_id can not find',award_id)
			return false
		end
	end
	return true
end
---------------------------------------------------------------------------------------------
--检查特殊宝箱
local function check_special_gift_exist(all_config)
	local lua_special_gift_item_ex = all_config.lua_special_gift_item_ex
	local lua_special_gift = all_config.lua_special_gift
	for id,special_gift_cfg in pairs(lua_special_gift) do
		for i=1,5 do
			local type = special_gift_cfg['item_type'..i]
			if type > 0 then 
				if not lua_special_gift_item_ex[type] then
					errlog('this type can not find in lua_special_gift_item_ex',type)
					return false
				end
			end
		end
		if special_gift_cfg.count <= 0 then 
			errlog('the type count must big than 0', special_gift_cfg.count)
			return false
		end
	end
	return true
end

--检查特殊宝箱
local function check_nianshou_item_exist(all_config)
	local lua_nianshou = all_config.lua_nianshou
	local lua_item = all_config.lua_item
	for id,nianshou_cfg in pairs(lua_nianshou) do
		for i=1,12 do
			local item_id = nianshou_cfg['item'..i]
			if item_id > 0 then 
				if not lua_item[item_id] then 
					errlog('can not find this item in table item',item_id)
					return false
				end
				local prob = nianshou_cfg['prob'..i]
				if prob <= 0 then 
					errlog('prob must big than 0',prob)
					return false
				end
			end
		end
		
		for i=1,3 do
			local item_id = nianshou_cfg['extra_id'..i]
			local count = nianshou_cfg['extra_count'..i]
			if item_id > 0 then 
				if not lua_item[item_id] then 
					errlog('can not find this item in table item',item_id)
					return false
				end 
				if count <= 0 then 
					errlog('count must big than 0',count)
					return false
				end
			end
		end
	end
	return true
end

local function check_fixed_goods_shop(all_config)
	return true
end

--针对配置一一检查
local function check_all_config(all_config)
	--检查等级上限
	if not check_level_limit(all_config) then
		errlog('check_level_limit all_config failed')
		return false
	end

	--[[
	--检查武将合成的配置
	if not check_wujiang_blend(all_config) then
		errlog('check_wujiang_blend all_config failed')
		return false
	end
	--]]

	--检查技能提升
	if not check_skill_improve(all_config) then
		errlog('check_skill_improve all_config failed')
		return false
	end

	--检查固定商品
	if not check_fixed_goods_shop(all_config) then
		errlog('check_fixed_goods_shop all_config failed')
		return false
	end
	
	if not check_all_item_exist(all_config) then 
		errlog('check_all_item_exist all_config failed')
		return false
	end
	
	if not check_equip_exist(all_config) then 
		errlog('check_equip_exist all_config failed')
		return false
	end
	
	if not check_equip_attr_exist(all_config) then 
		errlog('check_equip_attr_exist all_config failed')
		return false
	end
	
	if not check_wujiang_stage_item_exist(all_config) then 
		errlog('check_wujiang_stage_item_exist all_config failed')
		return false
	end
	
	if not check_arena_award_item_exist(all_config) then 
		errlog('check_arena_award_item_exist all_config failed')
		return false
	end
	
	if not check_activity_item_exist(all_config) then 
		errlog('check_activity_item_exist all_config failed')
		return false
	end
	
	if not check_arena_shop_fixed_goods_item_exist(all_config) then 
		errlog('check_arena_shop_fixed_goods_item_exist all_config failed')
		return false
	end
	
	if not check_copy_drop_item_exist(all_config) then 
		errlog('check_copy_drop_item_exist all_config failed')
		return false
	end

	if not check_dragon_item_exist(all_config) then 
		errlog('check_dragon_item_exist all_config failed')
		return false
	end
	
	if not check_equip_stage_material_item_exist(all_config) then 
		errlog('check_equip_stage_material_item_exist all_config failed')
		return false
	end
	
	if not check_qshop_goods_item_exist(all_config) then 
		errlog('check_qshop_goods_item_exist all_config failed')
		return false
	end
	
	if not check_raid_baoxiang_goods_item_exist(all_config) then 
		errlog('check_raid_baoxiang_goods_item_exist all_config failed')
		return false
	end
	
	if not check_shop_goods_item_exist(all_config) then 
		errlog('check_shop_goods_item_exist all_config failed')
		return false
	end
	
	if not check_sign_award_item_exist(all_config) then 
		errlog('check_sign_award_item_exist all_config failed')
		return false
	end
	
	if not check_toc_shop_fixed_goods_item_exist(all_config) then 
		errlog('check_toc_shop_fixed_goods_item_exist all_config failed')
		return false
	end
	
	if not check_world_boss_rank_award_item_exist(all_config) then 
		errlog('check_world_boss_rank_award_item_exist all_config failed')
		return false
	end
	
	if not check_manor_shangbu_shop_item_exist(all_config) then 
		errlog('check_manor_shangbu_shop_item_exist all_config failed')
		return false
	end
	
	if not check_role_all_data_exist(all_config) then
		errlog('check_role_all_data_exist all_config failed')
		return false
	end
	
	if not check_role_exist(all_config) then
		errlog('check_role_exist all_config failed')
		return false
	end
	
	if not check_zhenfa_item_exist(all_config) then 
		errlog('check_zhenfa_item_exist all_config failed')
		return false
	end 
	
	if not Check_all_wanfa_seting(all_config) then 
		errlog('Check_all_wanfa_seting all_config failed')
		return false
	end 
	
	if not Check_toc_baoxiang_maintype_data_exist(all_config) then 
		errlog('Check_toc_baoxiang_maintype_data_exist all_config failed')
		return false
	end
	
	if not check_arena_shop_maintype_exist(all_config) then 
		errlog('check_arena_shop_maintype_exist all_config failed')
		return false
	end
	
	if not check_dragon_improve_exist(all_config) then 
		errlog('check_dragon_improve_exist all_config failed')
		return false
	end
	
	if not check_vip_privilege_data_exist(all_config) then 
		errlog('check_vip_privilege_data_exist all_config failed')
		return false
	end
	
	if not check_bag_gift_exist(all_config) then 
		errlog('check_bag_gift_exist all_config failed')
		return false
	end
	
	if not check_qiyu_award_exist(all_config) then 
		errlog('check_qiyu_award_exist all_config failed')
		return false
	end 
	
	if not check_wujiang_qiyu_exist(all_config) then 
		errlog('check_wujiang_qiyu_exist all_config failed')
		return false
	end 
	if not check_liezhuan_item_exist(all_config) then 
		errlog('check_liezhuan_item_exist all_config failed')
		return false
	end 
	if not check_item_blend_item_exist(all_config) then 
		errlog('check_item_blend_item_exist all_config failed')
		return false
	end 
	if not check_toc_baoxiang_item_exist(all_config) then 
		errlog('check_toc_baoxiang_item_exist all_config failed')
		return false
	end
	if not check_manor_task_item_exist(all_config) then
		errlog('check_manor_task_item_exist all_config failed')
		return false
	end
	if not check_arena_highest_rank_award(all_config) then
		errlog('check_arena_highest_rank_award all_config failed')
		return false
	end 
	
	if not check_manor_building_expend(all_config) then 
		errlog('check_manor_building_expend all_config failed')
		return false
	end

	if not Check_sign_award_exist(all_config) then 
		errlog('Check_sign_award_exist all_config failed')
		return false
	end
	
	if not check_zhaoqing_week_sort(all_config) then
		errlog('check_zhaoqing_week_sort all_config failed')
		return false
	end
	
	if not check_nianshou_item_exist(all_config) then 
		errlog('check_nianshou_item_exist all_config failed')
		return false
	end
	--转表操作
	if not translate(all_config) then
		errlog('translate all_config failed')
		return false
	end

	---------------------------------------必须要在转表后
	--检查点将奖励的类型是否匹配
	if not check_lottery_item_type_exist(all_config) then 
		errlog('check_lottery_item_type_exist failed')
		return false
	end 
	--检查商店的主类型是否匹配
	if not check_shop_maintype_exist(all_config) then 
		errlog('check_shop_maintype_exist failed')
		return false
	end 
	
	if not check_toc_baoxiang_maintype_exist(all_config) then 
		errlog('check_toc_baoxiang_maintype_exist failed')
		return false
	end 
	
	if not check_raid_baoxiang_maintype_exist(all_config) then 
		errlog('check_raid_baoxiang_maintype_exist failed')
		return false
	end
	
	if not check_special_gift_exist(all_config) then 
		errlog('check_special_gift_exist failed')
		return false
	end
	
	return true
end

return check_all_config
