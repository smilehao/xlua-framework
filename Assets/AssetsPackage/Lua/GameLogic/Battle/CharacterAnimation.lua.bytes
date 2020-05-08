--[[
-- added by wsh @ 2018-02-26
-- 临时Demo：战斗场景角色动画控制脚本
--]]

local CharacterAnimation = BaseClass("CharacterAnimation", Updatable)

local function Start(self, chara_go)
	-- 角色gameObject
	self.chara_go = chara_go
	-- 角色控制器
	self.chara_ctrl = chara_go:GetComponentInChildren(typeof(CS.UnityEngine.CharacterController))
	-- 动画控制器
	self.anim_ctrl = chara_go:GetComponentInChildren(typeof(CS.UnityEngine.Animation))
	
	assert(not IsNull(self.chara_ctrl), "chara_ctrl null")
	assert(not IsNull(self.anim_ctrl), "anim_ctrl null")
end

local function LateUpdate(self)
	if IsNull(self.chara_ctrl) or IsNull(self.anim_ctrl) then
		return
	end
	
	if self.chara_ctrl.isGrounded and CS.ETCInput.GetAxis("Vertical") ~= 0 then
		self.anim_ctrl:CrossFade("soldierRun")
	end
	
	if self.chara_ctrl.isGrounded and CS.ETCInput.GetAxis("Vertical") == 0 and CS.ETCInput.GetAxis("Horizontal") == 0 then
		self.anim_ctrl:CrossFade("soldierIdleRelaxed")
	end
	
	if not self.chara_ctrl.isGrounded then
		self.anim_ctrl:CrossFade("soldierFalling")
	end
	
	if self.chara_ctrl.isGrounded and CS.ETCInput.GetAxis("Vertical") == 0 and CS.ETCInput.GetAxis("Horizontal") > 0 then
		self.anim_ctrl:CrossFade("soldierSpinRight")
	end
	
	if self.chara_ctrl.isGrounded and CS.ETCInput.GetAxis("Vertical") == 0 and CS.ETCInput.GetAxis("Horizontal") < 0 then
		self.anim_ctrl:CrossFade("soldierSpinLeft")
	end
end

CharacterAnimation.Start = Start
CharacterAnimation.LateUpdate = LateUpdate

return CharacterAnimation