 
protoc --proto_path=./luamsg --cpp_out=../pvpsvr/cppmsg/ ./luamsg/battle.proto ./luamsg/inner_battle.proto
protoc --proto_path=./cppmsg --cpp_out=../gamesvr/cppmsg/ ./cppmsg/*proto
protoc --proto_path=./cppmsg --cpp_out=../pvpsvr/cppmsg/ ./cppmsg/*proto
