--[[
-- added by wsh @ 2017-12-11
-- Lua侧带有可复用功能的管理组，配合Unity原生组件UIScrollRect使用，负责管理和调度组下的可复用组件刷新
-- 可根据需要带有按钮组---注意：按钮组中按钮的bind_data会被占用，不要再绑定数据
-- 注意：
-- 1、添加的组件必须是UIWrapComponent的子类、或者实现了它的接口的任何其它类
-- 2、如果组件逻辑功能足够简单，不想再另行创建UIWrapComponent子类，可以直接设置本脚本回调
-- 3、Unity侧在ScrollContent节点挂GridLayoutGroup脚本执行配置，对配置要求如下：
--    A）ScrollRect的父节点锚点和中心点都在中间，配置可滑动区域大小和位置
--    B）ScrollRect节点设置为向4个方向的Stretch
--    C）RectTansform锚点、中心点设置到左上角，保证初始化位置的localPosition必须为(0, 0, 0)
--    D）使用GridLayoutGroup组件padding中的Top、Left来设置ScrollContent在可滑动区域的偏移
--    E）使用GridLayoutGroup组件cellSize来配置Item的尺寸（宽度、高度）
--    F）使用GridLayoutGroup组件spacing来配置Item间隙
--    G）GridLayoutGroup组件startConer、childAlignment设置为UpperLeft
--    H）ScrollRect水平滑动时，StartAxis设置为Vertical，Constraint设置为FixedRowCount，ConstraintCount指定每列Item的个数
--    I）ScrollRect竖直滑动时，StartAxis设置为Horizontal，Constraint设置为FixedColumnCount，ConstraintCount指定每行Item的个数
--    J）Item锚点会自动设置到左上角、中心点设置到重点，Item拼预设的时候按这个配置来拼
-- 4、GridLayoutGroup脚本只是用来执行配置，以及Prefab效果预览，配置读取后将禁用，Item排布全部由本脚本托管，另行计算
-- 5、初始Item数量要保证在最大可能分辨率下能布满整个可滑动区域，再额外多3个行（竖直滑动）/列（水平滑动），否则Item不能复用
-- 使用方式：
-- self.xxx_wrapgroup = self:AddComponent(UIWrapGroup, scroll_content_path, UIWrapComponentTypeClass)--初始化
-- self.xxx_wrapgroup:SetLength(100)--数据长度
-- self.xxx_wrapgroup:ResetToBeginning()--复位
-- TODO：
-- 1、由官方最佳实践得知，带重复项的Item最好只加载一个再执行instantiate：https://unity3d.com/cn/learn/tutorials/temas/best-practices/assets-objects-and-serialization
--]]

local UIWrapGroup = BaseClass("UIWrapGroup", UIBaseContainer)
local base = UIBaseContainer

-- 创建
local function OnCreate(self, wrap_class, ...)
	assert(type(wrap_class) == "table" and wrap_class.__ctype == ClassType.class, "wrap_class err : "..tostring(wrap_class))
	base.OnCreate(self)
	
	-- Unity侧原生组件
	self.unity_scrollrect = self.transform:GetComponentInParent(typeof(CS.UnityEngine.UI.ScrollRect))
	self.unity_grid = self.transform:GetComponent(typeof(CS.UnityEngine.UI.GridLayoutGroup))
	self.unity_sizefitter = self.transform:GetComponent(typeof(CS.UnityEngine.UI.ContentSizeFitter))
	assert(not IsNull(self.unity_scrollrect), "No found UnityEngine.UI.ScrollRect!")
	assert(not IsNull(self.unity_grid), "No found UnityEngine.UI.GridLayoutGroup!")
	self.unity_grid.enabled = false
	if not IsNull(self.unity_sizefitter) then
		self.unity_sizefitter.enabled = fasle
	end
	
	-- 由原生组件配置初始化数据
	-- 尺寸、间隔、边框--->左上角顶点偏移，粘合边框的Item尺寸
	local cell_size = self.unity_grid.cellSize
	local spacing = self.unity_grid.spacing
	local padding = self.unity_grid.padding
	self.item_size = cell_size
	self.item_spacing = spacing
	self.cell_size = Vector2.New(cell_size.x + spacing.x, cell_size.y + spacing.y)
	self.topleft_offset = Vector3.New(padding.left - spacing.x / 2, padding.top - spacing.y / 2, 0)
	-- 行/列数限制
	self.constraint_count = self.unity_grid.constraintCount
	-- 是否为水平拖动
	self.horizontal = self.unity_scrollrect.horizontal
	-- 通过四角坐标（左下、左上、右上、右下）计算scroll_rect中心点在局部坐标系中的坐标
	self.rectTransform.anchoredPosition = Vector2.zero
	local scroll_rect_trans = self.unity_scrollrect.transform:GetComponent(typeof(CS.UnityEngine.RectTransform))
	local scroll_world_corners = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero}
	scroll_rect_trans:GetWorldCorners(scroll_world_corners)
	local bottom_left = self.transform:InverseTransformPoint(scroll_world_corners[1])
	local top_right = self.transform:InverseTransformPoint(scroll_world_corners[3])
	
	self.center = Vector2.zero
	self.center_original = Vector2.zero
	self.center_original.x = (bottom_left.x + top_right.x) / 2
	self.center_original.y = (bottom_left.y + top_right.y) / 2
	
	-- 其它成员数据
	-- 最小索引
	self.min_index = 0
	-- 最大索引
	self.max_index = 0
	-- 缓存用
	self.tmp_vec3 = Vector3.zero
	-- 按钮组
	self.__btngroup = nil
	-- 当前选中虚拟索引
	self.__cur_check_index = nil
	
	-- 自动添加当前挂载节点下的所有孩子
	local child_count = self.transform.childCount
	for i = 0, child_count - 1 do
		self:AddComponent(wrap_class, i, ...)
	end
	-- 计算孩子布局宽度/高度
	local count = math.floor(((self:GetComponentsCount() - 1) / self.constraint_count) + 1)
	if self.horizontal then
		self.extents = self.cell_size.x * count
	else
		self.extents = self.cell_size.y * count
	end
	self.half_extents = self.extents / 2
	
	-- 回调
	-- 注册scroll拖动回调
	self.__onmove = function(vec2)
		self:WrapContent(false)
	end
	self.unity_scrollrect.onValueChanged:AddListener(self.__onmove)
	-- 刷新回调：void __onrefresh(wrap_component, real_index, check)
	self.__onrefresh = nil
	-- 点击回调：void __onclick(wrap_component, toggle_btn, virtual_index, check)
	self.__onclick = nil
end

-- 设置刷新回调
local function SetOnRefresh(self, ...)
	self.__onrefresh = BindCallback(...)
end

-- 设置点击回调
local function SetOnClick(self, ...)
	self.__onclick = BindCallback(...)
end

-- 获取索引对应位置
local function GetLocalPosition(self, real_index, local_position)
	if self.horizontal then
		local_position.x = math.floor(real_index / self.constraint_count) * self.cell_size.x
		local_position.x = local_position.x + self.cell_size.x / 2
		local_position.y = -math.floor(real_index % self.constraint_count) * self.cell_size.y
		local_position.y = local_position.y - self.cell_size.y / 2
		local_position.z = 0
		local_position = local_position + self.topleft_offset
	else
		local_position.x = math.floor(real_index % self.constraint_count) * self.cell_size.x
		local_position.x = local_position.x + self.cell_size.x / 2
		local_position.y = -math.floor(real_index / self.constraint_count) * self.cell_size.y
		local_position.y = local_position.y - self.cell_size.y / 2
		local_position.z = 0
		local_position = local_position + self.topleft_offset
	end
end
	
-- 获取真实索引                                                                                                                                                                                                                                                                             
local function GetRealIndex(self, local_position)
	local offset = local_position - self.topleft_offset
	local raw_index = math.floor(-offset.y / self.cell_size.y)
	local column_index = math.floor(offset.x / self.cell_size.x)
	if self.horizontal then
		return column_index * self.constraint_count + raw_index
	else
		return raw_index * self.constraint_count + column_index
	end
end

-- 设置长度：多余的孩子会被隐藏
local function SetLength(self, length)
	assert(length >= 0)
	self.min_index = 0
	self.max_index = length - 1
	local index = 0
	self:Walk(function(component)
		component:SetActive(index <= self.max_index)
		index = index + 1
	end)
	-- 设置滑动范围
	local scroll_size = Vector2.New(0, 0)
	local alignment_index = math.floor((length + self.constraint_count - 1)/ self.constraint_count) * self.constraint_count - 1
	GetLocalPosition(self, alignment_index, self.tmp_vec3)
	scroll_size.x = math.abs(self.tmp_vec3.x) + self.cell_size.x / 2
	scroll_size.y = math.abs(self.tmp_vec3.y) + self.cell_size.y / 2
	self.rectTransform.sizeDelta = scroll_size
end

-- 添加UIWrapComponent：必须保证一个名字只对应一个组件
local function AddComponent(self, component_target, var_arg, ...)
	local cmp = base.AddComponent(self, component_target, var_arg, ...)
	-- 调试模式下强行检测，避免手误
	if Config.Debug then
		local lookup_table = {}
		self:Walk(function(componet)
			local cmp_name = componet:GetName()
			assert(lookup_table[cmp_name] == nil, "Aready exists component named : "..cmp_name)
			lookup_table[cmp_name] = true
		end)
	end
	-- 设置锚点到左上角、中心点到中点
	cmp.rectTransform.anchorMin = Vector2.New(0, 1)
	cmp.rectTransform.anchorMax = Vector2.New(0, 1)
	cmp.rectTransform.pivot = Vector2.New(0.5, 0.5)
	-- 设置Item宽、高
	cmp.rectTransform.sizeDelta = self.item_size
	return cmp
end

-- 按钮点击统一回调
local function ButtonGroupOnClick(self, toggle_btn, check)
	local virtual_index = toggle_btn:GetBindData()
	if virtual_index == nil then
		return
	end
	
	-- 选中时记录选中索引
	if check then
		self.__cur_check_index = virtual_index
	end
	
	-- 上层回调
	local wrap_component = nil
	if  virtual_index >= 0 then
		wrap_component = self:GetComponent(toggle_btn:GetName())
		wrap_component:OnClick(toggle_btn, virtual_index, check)
	end
	if self.__onclick ~= nil then
		self.__onclick(wrap_component, toggle_btn, virtual_index, check)
	end
end

-- 添加按钮组
-- @togglebtn_class：按钮组中的按钮类
-- 注意：
-- 1、按钮的bind_data被占用，用于保存该按钮对于的real_index
local function AddButtonGroup(self, togglebtn_class, ...)
	local btngroup = UIButtonGroup.New(self, "")
	btngroup:OnCreate(togglebtn_class)
	btngroup.__name = "__btngroup"
	btngroup:SetOnClick(ButtonGroupOnClick, self)
	self.__btngroup = btngroup
	return btngroup
end

-- 检测虚拟索引是否已经存在
local function CheckVirtualIndexExists(self, virtual_index)
	local found_cmp = nil
	self.__btngroup:Walk(function(btn_cmp)
		if btn_cmp:GetBindData() == virtual_index then
			found_cmp = btn_cmp
		end
	end)
	return found_cmp
end

-- 往按钮组中添加其它按钮
-- 多种重载方式
-- 注意：
-- 1、按钮的bind_data被占用，由于列表中的项都有real_index做索引，所以这里要加个虚拟索引到bind_data
-- 2、所有外部回调、交互都需要这个虚拟索引来标识是哪个按钮
-- 3、虚拟索引一定要是负数，避免与列表中的按钮冲突
-- 4、虚拟索引要保证唯一性
local function AddButton(self, togglebtn_class, var_arg, virtual_index, ...)
	assert(self.__btngroup ~= nil, "You should add button group first!")
	assert(type(virtual_index) == "number" and virtual_index < 0, "Virtual index must be negative number!")
	local togglebtn = self.__btngroup:AddComponent(togglebtn_class, var_arg, ...)
	-- 调试模式下检测virtual_index是否存在，避免手误
	local found_cmp = CheckVirtualIndexExists(self, virtual_index)
	if Config.Debug and found_cmp then
		error("Aready exists virtual index : "..tostring(virtual_index))
	end
	
	togglebtn:SetBindData(virtual_index)
	return togglebtn
end

-- 设置按钮初始选中索引
local function SetOriginal(self, original_index)
	assert(self.__btngroup ~= nil, "You should add button group first!")
	assert(original_index == nil or type(original_index) == "number", "Original index must be nil or number!")
	-- 只有在初始选中外部按钮时，才需要在这里设置初始按钮，列表中的按钮在复用检测中会自定虚拟点击
	if original_index ~= nil and original_index < 0 then	
		local found_cmp = nil
		found_cmp = CheckVirtualIndexExists(self, original_index)
		self.__btngroup:SetOriginal(found_cmp and found_cmp:GetName())
	else
		self.__btngroup:SetOriginal(nil)
	end
	
	self.__cur_check_index = original_index
end

-- 子节点复位
local function ResetChildPositions(self)
	local index = 0
	self:Walk(function(component)
		GetLocalPosition(self, index, self.tmp_vec3)
		component.rectTransform.localPosition = self.tmp_vec3
		index = index + 1
	end)
end

-- 如有必要，清空列表绑定数据
local function ResetChildBindData(self)
	if self.__btngroup == nil then
		return
	end
	
	self:Walk(function(component)
		component:SetBindData(nil)
	end)
end

-- 如果有按钮组，复位
local function ResetButtonGroup(self)
	if self.__btngroup ~= nil then
		self.__btngroup:ResetToBeginning()
	end
end

-- 复位
local function ResetToBeginning(self)
	self.rectTransform.anchoredPosition = Vector2.zero
	ResetChildPositions(self)
	ResetChildBindData(self)
	-- 如果初始按钮为外部按钮，这里会选中，如果不是，则全部切换到非选中（包括列表按钮）
	ResetButtonGroup(self)
	-- 这里强制刷新时，如果初始按钮为列表按钮，会执行一次虚拟点击
	self:WrapContent(true)
end

-- 检测，必要时刷新Item
local function CheckAndUpdateItemIfNeeded(self, component, local_position)
	local real_index = GetRealIndex(self, local_position)
	if self.min_index <= real_index and real_index <= self.max_index then
		-- 调试用
		if Config.Debug then
			component.gameObject.name = tostring(real_index)
		end
		
		-- 重置位置
		component.transform.localPosition = local_position
		
		local check = real_index == self.__cur_check_index
		-- 如果有按钮组则更新
		if self.__btngroup ~= nil then
			local current = self.__btngroup:GetCurrent()
			local toggle_btn = self.__btngroup:GetComponent(component:GetName())
			local bind_data = toggle_btn:GetBindData()
			-- 如果被复用的是选中按钮，则允许交互
			if bind_data ~= nil and bind_data == self.__cur_check_index then
				toggle_btn.unity_uibutton.interactable = true
			end
			-- 更新绑定数据
			toggle_btn:SetBindData(real_index)
			-- 如果要更新的是选中按钮，则关闭交互；首次刷新时执行虚拟点击
			if check then
				toggle_btn.unity_uibutton.interactable = false
				if not current then
					self.__btngroup:ClickOn(component:GetName())
				end
			end
		end
		
		-- 刷新回调
		component:OnRefresh(real_index, check)
		if self.__onrefresh ~= nil then
			self.__onrefresh(component, real_index, check)
		end
	
	end
end

-- 检测复用
local function WrapContent(self, force_reset)
	if self:GetComponentsCount() > self.max_index and not force_reset then
		return
	end
	
	-- 更新scroll_rect中心点在局部坐标系中的坐标
	local anchored_position = self.rectTransform.anchoredPosition
	self.center.x = self.center_original.x - anchored_position.x
	self.center.y = self.center_original.y - anchored_position.y
	
	self:Walk(function(component)
		local unity_position = component.transform.localPosition
		self.tmp_vec3:Set(unity_position.x, unity_position.y, 0)
		local distance = self.horizontal and (self.tmp_vec3.x - self.center.x) or (self.tmp_vec3.y - self.center.y)
		
		if force_reset then
			-- update all
			CheckAndUpdateItemIfNeeded(self, component, self.tmp_vec3)
		elseif math.abs(distance) > self.half_extents then
			-- if out of panel, update
			if self.horizontal then
				self.tmp_vec3.x = distance < 0 and (self.tmp_vec3.x + self.extents) or (self.tmp_vec3.x - self.extents)
			else
				self.tmp_vec3.y = distance < 0 and (self.tmp_vec3.y + self.extents) or (self.tmp_vec3.y - self.extents)
			end
			CheckAndUpdateItemIfNeeded(self, component, self.tmp_vec3)
		end
	end)
end

-- 销毁
local function OnDestroy(self)
	if self.__onmove ~= nil then
		self.unity_scrollrect.onValueChanged:RemoveListener(self.__onmove)
	end
	if self.__btngroup ~= nil then
		self.__btngroup:Delete()
	end
	self.unity_scrollrect = nil
	self.unity_grid = nil
	self.unity_sizefitter = nil
	self.__btngroup = nil
	self.__onmove = nil
	self.__onrefresh = nil
	self.__onclick = nil
	base.OnDestroy(self)
end

UIWrapGroup.OnCreate = OnCreate
UIWrapGroup.SetOnRefresh = SetOnRefresh
UIWrapGroup.SetOnClick = SetOnClick
UIWrapGroup.SetLength = SetLength
UIWrapGroup.AddComponent = AddComponent
UIWrapGroup.AddButtonGroup = AddButtonGroup
UIWrapGroup.AddButton = AddButton
UIWrapGroup.SetOriginal = SetOriginal
UIWrapGroup.ResetToBeginning = ResetToBeginning
UIWrapGroup.WrapContent = WrapContent
UIWrapGroup.OnDestroy = OnDestroy

return UIWrapGroup