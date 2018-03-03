using XLua;
using System.IO;
using System;
using System.Collections.Generic;

/// <summary>
/// 说明：解决xlua中对BinaryWriter各种写类型不能进行很好支持的问题
///     
/// @by wsh 2017-12-28
/// </summary>

public static class BinaryWriterExtentions
{
    public static void WriteDouble(this BinaryWriter binaryWriter, double value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteULong(this BinaryWriter binaryWriter, ulong value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteUInt(this BinaryWriter binaryWriter, uint value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteUShort(this BinaryWriter binaryWriter, ushort value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteString(this BinaryWriter binaryWriter, string value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteFloat(this BinaryWriter binaryWriter, float value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteSbyte(this BinaryWriter binaryWriter, sbyte value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteLong(this BinaryWriter binaryWriter, long value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteInt(this BinaryWriter binaryWriter, int value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteChars(this BinaryWriter binaryWriter, char[] chars)
    {
        binaryWriter.Write(chars);
    }

    public static void WriteDecimal(this BinaryWriter binaryWriter, decimal value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteChar(this BinaryWriter binaryWriter, char ch)
    {
        binaryWriter.Write(ch);
    }

    public static void WriteBuffer(this BinaryWriter binaryWriter, byte[] buffer)
    {
        binaryWriter.Write(buffer);
    }

    public static void WriteByte(this BinaryWriter binaryWriter, byte value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteBool(this BinaryWriter binaryWriter, bool value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteShort(this BinaryWriter binaryWriter, short value)
    {
        binaryWriter.Write(value);
    }

    public static void WriteChars(this BinaryWriter binaryWriter, char[] chars, int index, int count)
    {
        binaryWriter.Write(chars, index, count);
    }

    public static void WriteBuffer(this BinaryWriter binaryWriter, byte[] buffer, int index, int count)
    {
        binaryWriter.Write(buffer, index, count);
    }
}

#if UNITY_EDITOR
public static class BinaryWriterExtentionsExporter
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(BinaryWriter),
        typeof(BinaryWriterExtentions),
    };
}
#endif