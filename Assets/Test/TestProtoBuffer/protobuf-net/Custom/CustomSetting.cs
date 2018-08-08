using System;
using System.Collections.Generic;

/// <summary>
/// 说明：某类型与在该类型上自定义Serializer的映射表
/// 
/// 注意：
///     1）在使用ProtoBuf-net前需要初始化这张表
/// 
/// @by wsh 2017-06-29
/// </summary>

namespace ProtoBuf.Serializers
{
    public static class CustomSetting
    {
        private static readonly Dictionary<Type, ICustomProtoSerializer> customSerializerMap = 
            new Dictionary<Type, ICustomProtoSerializer>();

        static public void AddCustomSerializer(Type type, ICustomProtoSerializer customSerializer)
        {
            customSerializerMap.Add(type, customSerializer);
        }

        static public void RemoveCustomSerializer(Type type)
        {
            customSerializerMap.Remove(type);
        }

        static public void CrearCustomSerializer()
        {
            customSerializerMap.Clear();
        }

        static public ICustomProtoSerializer TryGetCustomSerializer(Type targetType)
        {
            ICustomProtoSerializer customSerializer;
            if (customSerializerMap.TryGetValue(targetType, out customSerializer))
            {
                return customSerializer;
            }
            return null;
        }
    }
}