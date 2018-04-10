--
--------------------------------------------------------------------------------
--  FILE:  wire_format.lua
--  DESCRIPTION:  protoc-gen-lua
--      Google's Protocol Buffers project, ported to lua.
--      https://code.google.com/p/protoc-gen-lua/
--
--      Copyright (c) 2010 , 林卓毅 (Zhuoyi Lin) netsnail@gmail.com
--      All rights reserved.
--
--      Use, modification and distribution are subject to the "New BSD License"
--      as listed at <url: http://www.opensource.org/licenses/bsd-license.php >.
--  COMPANY:  NetEase
--  CREATED:  2010年07月30日 15时59分53秒 CST
--------------------------------------------------------------------------------
--
-- added by wsh @ 2017-12-26
-- 注意：代码已经被重构，不能再简简单单做升级

local pb = require "pb"

local WIRETYPE_VARINT = 0
local WIRETYPE_FIXED64 = 1
local WIRETYPE_LENGTH_DELIMITED = 2
local WIRETYPE_START_GROUP = 3
local WIRETYPE_END_GROUP = 4
local WIRETYPE_FIXED32 = 5
local WIRETYPE_MAX = 5

local ZigZagEncode32 = pb.zig_zag_encode32
local ZigZagDecode32 = pb.zig_zag_decode32
local ZigZagEncode64 = pb.zig_zag_encode64
local ZigZagDecode64 = pb.zig_zag_decode64

local function PackTag(field_number, wire_type)
    return field_number * 8 + wire_type
end

local function UnpackTag(tag)
    local wire_type = tag % 8
    return (tag - wire_type) / 8, wire_type
end

-- yeah, we don't need uint64
local function _VarUInt64ByteSizeNoTag(uint64)
    if uint64 <= 0x7f then return 1 end
    if uint64 <= 0x3fff then return 2 end
    if uint64 <= 0x1fffff then return 3 end
    if uint64 <= 0xfffffff then return 4 end
    return 5
end

local function TagByteSize(field_number)
    return _VarUInt64ByteSizeNoTag(PackTag(field_number, 0))
end

local function UInt64ByteSize(field_number, uint64)
  return TagByteSize(field_number) + _VarUInt64ByteSizeNoTag(uint64)
end

local function SInt64ByteSize(field_number, int64)
  return UInt64ByteSize(field_number, ZigZagEncode(int64))
end

local function Int64ByteSize(field_number, int64)
  return UInt64ByteSize(field_number, int64)
end

local function UInt32ByteSize(field_number, uint32)
  return UInt64ByteSize(field_number, uint32)
end

local function SInt32ByteSize(field_number, int32)
  return UInt32ByteSize(field_number, ZigZagEncode(int32))
end

local function Int32ByteSize(field_number, int32)
  return Int64ByteSize(field_number, int32)
end

local function Int32ByteSizeNoTag(int32)
  return _VarUInt64ByteSizeNoTag(int32)
end

local function Fixed64ByteSize(field_number, fixed64)
  return TagByteSize(field_number) + 8
end

local function SFixed64ByteSize(field_number, sfixed64)
  return TagByteSize(field_number) + 8
end

local function Fixed32ByteSize(field_number, fixed32)
  return TagByteSize(field_number) + 4
end

local function SFixed32ByteSize(field_number, sfixed32)
  return TagByteSize(field_number) + 4
end

local function FloatByteSize(field_number, flt)
  return TagByteSize(field_number) + 4
end

local function DoubleByteSize(field_number, double)
  return TagByteSize(field_number) + 8
end

local function BoolByteSize(field_number, b)
  return TagByteSize(field_number) + 1
end

local function EnumByteSize(field_number, enum)
  return UInt32ByteSize(field_number, enum)
end

local function BytesByteSize(field_number, b)
    return TagByteSize(field_number) + _VarUInt64ByteSizeNoTag(#b) + #b
end

local function StringByteSize(field_number, string)
  return BytesByteSize(field_number, string)
end

local function MessageByteSize(field_number, message)
    return TagByteSize(field_number) + _VarUInt64ByteSizeNoTag(message.ByteSize()) + message.ByteSize()
end

local function MessageSetItemByteSize(field_number, msg)
    local total_size = 2 * TagByteSize(1) + TagByteSize(2) + TagByteSize(3) 
    total_size = total_size + _VarUInt64ByteSizeNoTag(field_number)
    local message_size = msg.ByteSize()
    total_size = total_size + _VarUInt64ByteSizeNoTag(message_size)
    total_size = total_size + message_size
    return total_size
end

local function GroupByteSize(...)
	-- added by wsh @ 2017-12--27
	error("not supported yet!!!")
end

return {
	WIRETYPE_VARINT = WIRETYPE_VARINT,
	WIRETYPE_FIXED64 = WIRETYPE_FIXED64,
	WIRETYPE_LENGTH_DELIMITED = WIRETYPE_LENGTH_DELIMITED,
	WIRETYPE_START_GROUP = WIRETYPE_START_GROUP,
	WIRETYPE_END_GROUP = WIRETYPE_END_GROUP,
	WIRETYPE_FIXED32 = WIRETYPE_FIXED32,
	WIRETYPE_MAX = WIRETYPE_MAX,
	ZigZagEncode32 = ZigZagEncode32,
	ZigZagDecode32 = ZigZagDecode32,
	ZigZagEncode64 = ZigZagEncode64,
	ZigZagDecode64 = ZigZagDecode64,
	PackTag = PackTag,
	UnpackTag = UnpackTag,
	TagByteSize = TagByteSize,
	UInt64ByteSize = UInt64ByteSize,
	SInt64ByteSize = SInt64ByteSize,
	Int64ByteSize = Int64ByteSize,
	UInt32ByteSize = UInt32ByteSize,
	SInt32ByteSize = SInt32ByteSize,
	Int32ByteSize = Int32ByteSize,
	Int32ByteSizeNoTag = Int32ByteSizeNoTag,
	Fixed64ByteSize = Fixed64ByteSize,
	SFixed64ByteSize = SFixed64ByteSize,
	Fixed32ByteSize = Fixed32ByteSize,
	SFixed32ByteSize = SFixed32ByteSize,
	FloatByteSize = FloatByteSize,
	DoubleByteSize = DoubleByteSize,
	BoolByteSize = BoolByteSize,
	EnumByteSize = EnumByteSize,
	BytesByteSize = BytesByteSize,
	StringByteSize = StringByteSize,
	MessageByteSize = MessageByteSize,
	MessageSetItemByteSize = MessageSetItemByteSize,
	GroupByteSize = GroupByteSize,
}
