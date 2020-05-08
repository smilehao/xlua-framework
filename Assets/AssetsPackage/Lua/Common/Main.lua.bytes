-- added by wsh @ 2017-12-27
-- 1、Unity侧部分功能的Lua代码实现用来降低与cs代码的交互来提供性能---移植自tolua
-- 2、这里的模块在游戏逻辑跑之前开始启动，雷同Unity中的Plugin下脚本
-- 3、这里的全局模块一般用于提供对lua语言级别的支持和扩展，和游戏框架以及逻辑无关

print("Common main start...")

-- 加载全局模块
require "Common.Tools.import"
require "Common.LuaUtil"
require "Common.TableUtil"
require "Common.StringUtil"

Mathf		= require "Common.Tools.UnityEngine.Mathf"
Vector2		= require "Common.Tools.UnityEngine.Vector2"
Vector3 	= require "Common.Tools.UnityEngine.Vector3"
Vector4		= require "Common.Tools.UnityEngine.Vector4"
Quaternion	= require "Common.Tools.UnityEngine.Quaternion"
Color		= require "Common.Tools.UnityEngine.Color"
Ray			= require "Common.Tools.UnityEngine.Ray"
Bounds		= require "Common.Tools.UnityEngine.Bounds"
RaycastHit	= require "Common.Tools.UnityEngine.RaycastHit"
Touch		= require "Common.Tools.UnityEngine.Touch"
LayerMask	= require "Common.Tools.UnityEngine.LayerMask"
Plane		= require "Common.Tools.UnityEngine.Plane"
Time		= require "Common.Tools.UnityEngine.Time"
Object		= require "Common.Tools.UnityEngine.Object"

list		= require "Common.Tools.list"

require "Common.Tools.event"