 protoc --proto_path=./clientmsg --cpp_out=../gamesvr/cppmsg/ ./clientmsg/*proto
 protoc --proto_path=./intermsg --proto_path=./clientmsg --cpp_out=../gamesvr/cppmsg/ ./intermsg/*proto

 protoc --proto_path=./clientmsg --cpp_out=../battlesvr/cppmsg/ ./clientmsg/*proto
 protoc --proto_path=./intermsg --proto_path=./clientmsg --cpp_out=../battlesvr/cppmsg/ ./intermsg/*proto

 protoc --proto_path=./clientmsg --cpp_out=../battlesvr2/cppmsg/ ./clientmsg/*proto
 protoc --proto_path=./intermsg --proto_path=./clientmsg --cpp_out=../battlesvr2/cppmsg/ ./intermsg/*proto
 protoc --proto_path=./luamsg --cpp_out=../battlesvr2/cppmsg/ ./luamsg/common.proto

 protoc --proto_path=./clientmsg --cpp_out=../matchsvr/cppmsg/ ./clientmsg/*proto
 protoc --proto_path=./intermsg --proto_path=./clientmsg --cpp_out=../matchsvr/cppmsg/ ./intermsg/*proto

 protoc --proto_path=./clientmsg --cpp_out=../querysvr/cppmsg/ ./clientmsg/*proto
 protoc --proto_path=./intermsg --proto_path=./clientmsg --cpp_out=../querysvr/cppmsg/ ./intermsg/*proto

 protoc --proto_path=./clientmsg --cpp_out=../mailsvr/cppmsg/ ./clientmsg/*proto
 protoc --proto_path=./intermsg --proto_path=./clientmsg --cpp_out=../mailsvr/cppmsg/ ./intermsg/*proto

protoc --proto_path=./intermsg --cpp_out=../loginsvr/cppmsg/ ./intermsg/inter_login.proto
