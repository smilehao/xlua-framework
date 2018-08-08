using System;
using ProtoBuf.Meta;
using System.Reflection;

/// <summary>
/// 说明：用于支持自定义Serializer的装饰类
/// 
/// 注意：
///     1）为了解决频繁序列化/反序列化时由于反射产生的严重GC问题
/// 
/// @by wsh 2017-06-29
/// </summary>

namespace ProtoBuf.Serializers
{
    sealed class CustomDecorator : CustomDecoratorBase
    {
        public override Type ExpectedType { get { return forType; } }
        private readonly Type forType;
        private readonly int fieldNumber;
        private readonly ICustomProtoSerializer customSerializer;
        public override bool RequiresOldValue { get { return true; } }
        public override bool ReturnsValue { get { return false; } }

        public CustomDecorator(Type forType, ICustomProtoSerializer customSerializer, int fieldNumber, IProtoSerializer tail) :
            base(tail)
        {
            Helpers.DebugAssert(forType != null);
            Helpers.DebugAssert(customSerializer != null);
            this.forType = forType;
            this.fieldNumber = fieldNumber;
            this.customSerializer = customSerializer;
        }

        public override void Write(object value, ProtoWriter dest)
        {
            Helpers.DebugAssert(value != null);
            value = customSerializer.GetValue(value, fieldNumber);
            if (value != null) Tail.Write(value, dest);
        }

        public override object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value != null);

            object oldVal = Tail.RequiresOldValue ? customSerializer.GetValue(value, fieldNumber) : null;
            object newVal = Tail.Read(oldVal, source);
            if (newVal != null) // if the tail returns a null, intepret that as *no assign*
            {
                customSerializer.SetValue(value, newVal, fieldNumber);
            }
            return null;
        }
    }
}