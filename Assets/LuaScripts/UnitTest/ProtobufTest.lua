local common_pb = require "Net.Protol.test_common_pb"
local person_pb = require "Net.Protol.test_person_pb"
local user_pb = require 'Net.Protol.user_pb'
local login_pb = require 'Net.Protol.login_pb'

function Decoder(pb_data)
	local msg = person_pb.Person()
	msg:ParseFromString(pb_data)
	-- TODO：后续测试int64的支持
	--assert(tonumber(msg.header.cmd) == 10010, 'msg.header.cmd')
	assert(msg.header.cmd == 10010)
	assert(msg.header.seq == 1)
	-- TODO：后续测试int64的支持
	--assert(tonumber(msg.id) == 1223372036854775807, 'msg.id')
	assert(msg.id == "1223372036854775807")
	assert(msg.name == "foo")
	assert(msg.array[1] == 1)
	assert(msg.array[2] == 2)
	assert(msg.age == 18)
	assert(msg.email == "703016035@qq.com")
	assert(msg.Extensions[person_pb.Phone.phones][1].num == "13788888888")
	assert(msg.Extensions[person_pb.Phone.phones][1].type == person_pb.Phone.MOBILE)	
end

function Encoder()
	local msg = person_pb.Person()                                 
	msg.header.cmd = 10010                                
	msg.header.seq = 1
	msg.id = "1223372036854775807"            
	msg.name = "foo"
	--数组添加                              
	msg.array:append(1)                              
	msg.array:append(2)            
	--extensions 添加
	local phone = msg.Extensions[person_pb.Phone.phones]:add()
	phone.num = '13788888888'      
	phone.type = person_pb.Phone.MOBILE 
	return msg:SerializeToString()
end

local function Login()
	print("login_in_lua...")
    local msg = login_pb.req_login()  
	msg.flag = 0
	msg.test_bytes = "11aaa\0\0\0aaaaaa"
	local pb_data = msg:SerializeToString()
	print(#pb_data)
	print(string.byte(pb_data, 0, #pb_data))
	
	local msg2 = login_pb.req_login()
	msg2:ParseFromString(pb_data)
	if msg2.flag ~= 0 then
		error("111111111111111")
	end
	if msg2.test_bytes ~= "11aaa\0\0\0aaaaaa" then
		error("2222222222222222")
	end
	
	local hallSocket = CS.Networks.HjTcpNetwork()
	hallSocket:SendMessage(pb_data)
end

local function RspLogin(pb_data)
	-- print("RspLogin_in_lua...")
	UnityEngine.Debug.Log("RspLogin_in_lua...")
	local buffer_str = Common.Tools.tolstring(pb_data)
	UnityEngine.Debug.Log("buffer_str len = "..#buffer_str)
	local msg = user_pb.rsp_login()
	msg:ParseFromString(buffer_str)
	--tostring 不会打印默认值
	--UnityEngine.Debug.Log('rsp_login decoder: '..tostring(msg))
	--UnityEngine.Debug.Log('buffer_str : '..msg.test.test2.test3.test4.cmd_data)
	UnityEngine.Debug.Log('buffer_str len : '..#msg.test.test2.test3.test4.cmd_data)
	UnityEngine.Debug.Log('rsp_login decoder: '..tostring(msg))
end

local function Run()
	local pb_data = Encoder()
	Decoder(pb_data)
	print("ProtobufTest Pass!")
end

return {
	Run = Run
}