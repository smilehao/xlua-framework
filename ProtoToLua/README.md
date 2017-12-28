protoc-gen-lua
==============
**默认放在C盘目录C:\protoc-gen-lua-master\protoc-gen-lua-master， 生成lua放在 Lua/Protol 目录<br>
修改说明：<br>
1. 支持嵌套类（必需在使用类之前声明）<br>
2. 支持int64, uint64 <br>
3. 解包的协议对象支持 tostring 操作 <br>
编译环境: <br>
1. Python2.7.8<br>
https://www.python.org/downloads/release/python-278/ <br>
2. 编译好的protobuf2.5下载地址<br>
http://pan.baidu.com/s/1slEUfXb<br>
注意:<br>
1. 嵌套的proto 必须在当前 proto 之前声明<br>
2. 安装python后, 需要在protobuf2.5 的python 目录下执行命令 Python setup.py install <br>**


Google's Protocol Buffers project, ported to Lua

"[Protocol Buffers](http://code.google.com/p/protobuf/)" is a binary serialization format and technology, released to the open source community by Google in 2008.

There are various implementations of Protocol Buffers and this is for Lua.

## Install

Install python runtime and the protobuf 2.3 for python.

checkout the code.

Compile the C code:

`$cd protobuf  && make`

Make a link to protoc-gen-lua  in your $PATH:

`$cd /usr/local/bin && sudo ln -s /path/to/protoc-gen-lua/plugin/protoc-gen-lua`

Then you can compile the .proto like this:

`protoc --lua_out=./ foo.proto`


## Quick Example
You write a .proto file like this:

person.proto :
```
  message Person {
    required int32 id = 1;
    required string name = 2;
    optional string email = 3;
  }
```

Then you compile it.

Then,  make sure that protobuf/ in package.cpath and package.path,  you use that code like this:

```
require "person_pb"

-- Serialize Example
local msg = person_pb.Person()
msg.id = 100
msg.name = "foo"
msg.email = "bar"
local pb_data = msg:SerializeToString()

-- Parse Example
local msg = person_pb.Person()
msg:ParseFromString(pb_data)
print(msg.id, msg.name, msg.email)
```

The API of this library is similar the protobuf library for python.
For a more complete example,  read the [python documentation](http://code.google.com/apis/protocolbuffers/docs/pythontutorial.html).


