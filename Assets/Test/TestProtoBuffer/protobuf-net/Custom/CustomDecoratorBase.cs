using System;

/// <summary>
/// 说明：用于支持自定义Serializer的装饰基类
/// 
/// @by wsh 2017-06-29
/// </summary>

namespace ProtoBuf.Serializers
{
    abstract class CustomDecoratorBase : IProtoSerializer
    {
        public abstract Type ExpectedType { get; }
        protected readonly IProtoSerializer Tail;
        protected CustomDecoratorBase(IProtoSerializer tail) { this.Tail = tail; }
        public abstract bool ReturnsValue { get; }
        public abstract bool RequiresOldValue { get; }
        public abstract void Write(object value, ProtoWriter dest);
        public abstract object Read(object value, ProtoReader source);
    }
}