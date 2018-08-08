using System;
/// <summary>
/// 说明：自定义Serializer类需要实现的接口
/// 
/// @by wsh 2017-06-29
/// </summary>

namespace ProtoBuf.Serializers
{
    public interface ICustomProtoSerializer
    {
        void SetValue(object target, object value, int fieldNumber);

        object GetValue(object target, int fieldNumber);
    }
}