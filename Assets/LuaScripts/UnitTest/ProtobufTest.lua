local common_pb = require "Net.Protol.test_common_pb"
local person_pb = require "Net.Protol.test_person_pb"

function Decoder(pb_data)
	local msg = person_pb.Person()
	msg:ParseFromString(pb_data)
	--tostring 不会打印默认值
	assert(tonumber(msg.header.cmd) == 10010, 'msg.header.cmd')
	assert(msg.header.seq == 1, 'msg.header.cmd')
	assert(tonumber(msg.id) == 1223372036854775807, 'msg.id')
	assert(msg.name == 'foo', 'msg.name')
	assert(msg.array[1] == 1, 'msg.array[1]')
	assert(msg.array[2] == 2, 'msg.array[2]')
	assert(msg.age == 18, 'msg.age')
	assert(msg.email == 'topameng@qq.com',  'msg.email')
	assert(msg.Extensions[person_pb.Phone.phones][1].num == '13788888888', 'msg.Extensions.num')
	assert(msg.Extensions[person_pb.Phone.phones][1].type == person_pb.Phone.MOBILE, 'msg.Extensions.type')		
end

function Encoder()
	local msg = person_pb.Person()                                 
	msg.header.cmd = 10010                                
	msg.header.seq = 1
	msg.id = '1223372036854775807'
	msg.name = 'foo'              
	--数组添加                              
	msg.array:append(1)                              
	msg.array:append(2)            
	--extensions 添加
	local phone = msg.Extensions[person_pb.Phone.phones]:add()
	phone.num = '13788888888'      
	phone.type = person_pb.Phone.MOBILE 
	return msg:SerializeToString()
end
		
local function Run()
	local pb_data = Encoder()
	Decoder(pb_data)
end

return {
	Run = Run
}