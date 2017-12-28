-- added by wsh @ 2017-09-07 for helper function proxy

local unpack = unpack or table.unpack

-- 获取字典value
local function try_get_value(dic, key)
	local ret,value = dic:TryGetValue(key)
	return ret and value or nil
end

-- new任意类型object
local function new_object(obj_type)
	return CS.System.Activator.CreateInstance(obj_type)
end

-- new泛型array
local function new_array(item_type, item_count)
	return CS.XLuaHelper.CreateArrayInstance(item_type, item_count)
end

-- new泛型list
local function new_list(item_type)
	return CS.XLuaHelper.CreateListInstance(item_type)
end

-- new泛型字典
local function new_dictionary(key_type, value_type)
	return CS.XLuaHelper.CreateDictionaryInstance(key_type, value_type)
end

-- new泛型Action
local function new_action(cs_obj, method_name, ...)
	return CS.XLuaHelper.CreateActionDelegate(cs_obj, method_name, ...)
end

-- new泛型Callback
local function new_callback(cs_obj, method_name, ...)
	return CS.XLuaHelper.CreateCallbackDelegate(cs_obj, method_name, ...)
end

-- cs列表迭代器：含包括Array、ArrayList、泛型List在内的所有列表
local function list_iter(cs_ilist, index)
	index = index + 1
	if index < cs_ilist.Count then
		return index, cs_ilist[index]
	end
end

local function list_ipairs(cs_ilist)
	return list_iter, cs_ilist, -1
end

-- cs字典迭代器
local function dictionary_iter(cs_enumerator)
	if cs_enumerator:MoveNext() then
		local current = cs_enumerator.Current
		return current.Key, current.Value
	end
end

local function dictionary_ipairs(cs_idictionary)
	local cs_enumerator = cs_idictionary:GetEnumerator()
	return dictionary_iter, cs_enumerator
end

-- list类型构建
local function make_generic_list_type(item_type)
	return CS.XLuaHelper.MakeGenericListType(item_type)
end

-- dictionary类型构建
local function make_generic_dictionary_type(key_type, value_type)
	return CS.XLuaHelper.MakeGenericDictionaryType(key_type, value_type)
end

-- callback类型构建
local function make_generic_callback_type(...)
	return CS.XLuaHelper.MakeGenericCallbackType(...)
end

-- action类型构建
local function make_generic_action_type(...)
	return CS.XLuaHelper.MakeGenericActionType(...)
end

-- added by wsh @ 2017-09-14
-- 和util.hotfix_ex的区别：
--	1）热更代码处于保护调用模式，出现异常时，游戏不会挂掉
--	2）热更代码异常时，本次热更失效，执行原有的cs旧代码逻辑
--	3）实际意义不大，除非修复小bug怕引发大bug
local function hotfix_safe(cs, field, func)
    assert(type(field) == 'string' and type(func) == 'function', 'invalid argument: #2 string needed, #3 function needed!')
    local function func_safe(self, ...)
        xlua.hotfix(cs, field, nil)
        local ret = {pcall(func, self, ...)}
		local status, err = ret[1], ret[2]
		table.remove(ret, 1)
		if not status and self then
			-- 如果lua的热更代码出现错误，则执行cs代码中的旧逻辑
			CS.Logger.LogError('hotfix_safe '..field..' failed(call cs func instead) with err : '..err)
			local cs_func = load('return function(self, ...) self:'..field..'(...) end')
			ret = {cs_func()(self, ...)}
		end
        xlua.hotfix(cs, field, func_safe)
		return unpack(ret)
    end
    xlua.hotfix(cs, field, func_safe)
end

return {
	-- cs字典
    try_get_value = try_get_value,
	-- cs对象
	new_object = new_object,
	new_array = new_array,
	new_list = new_list,
	new_dictionary = new_dictionary,
	new_action = new_action,
	new_callback = new_callback,
	-- cs迭代器
	list_ipairs = list_ipairs,
	dictionary_ipairs = dictionary_ipairs,
	-- cs类型
	make_generic_list_type = make_generic_list_type,
	make_generic_dictionary_type = make_generic_dictionary_type,
	make_generic_callback_type = make_generic_callback_type,
	make_generic_action_type = make_generic_action_type,
	-- 热更
	hotfix_safe = hotfix_safe,
}
