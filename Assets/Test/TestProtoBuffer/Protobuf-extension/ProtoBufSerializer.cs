using battle;
using CustomDataStruct;
using ProtoBuf.Serializers;
using System.IO;

/// <summary>
/// 说明：ProtoBuf初始化、缓存等管理；序列化、反序列化等封装
/// 
/// @by wsh 2017-07-01
/// </summary>

public class ProtoBufSerializer : Singleton<ProtoBufSerializer>
{
    ProtoBuf.Meta.RuntimeTypeModel model;

    public override void Init()
    {
        base.Init();

        model = ProtoBuf.Meta.RuntimeTypeModel.Default;
        AddCustomSerializer();
        AddProtoPool();
        model.netDataPoolDelegate = ProtoFactory.Get;
        model.bufferPoolDelegate = StreamBufferPool.GetBuffer;
    }

    public override void Dispose()
    {
        model = null;
        ClearCustomSerializer();
        ClearProtoPool();
    }

    static public void Serialize(Stream dest, object instance)
    {
        ProtoBufSerializer.instance.model.Serialize(dest, instance);
    }

    static public object Deserialize(Stream source, System.Type type, int length = -1)
    {
        return ProtoBufSerializer.instance.model.Deserialize(source, null, type, length, null);
    }

    void AddCustomSerializer()
    {
        // 自定义Serializer以避免ProtoBuf反射
        CustomSetting.AddCustomSerializer(typeof(ntf_battle_frame_data), new NtfBattleFrameDataDecorator());
        CustomSetting.AddCustomSerializer(typeof(ntf_battle_frame_data.one_slot), new OneSlotDecorator());
        CustomSetting.AddCustomSerializer(typeof(ntf_battle_frame_data.cmd_with_frame), new CmdWithFrameDecorator());
        CustomSetting.AddCustomSerializer(typeof(one_cmd), new OneCmdDecorator());
    }

    void ClearCustomSerializer()
    {
        CustomSetting.CrearCustomSerializer();
    }


    void AddProtoPool()
    {
        // 自定义缓存池以避免ProtoBuf创建实例
        ProtoFactory.AddProtoPool(typeof(ntf_battle_frame_data), new NtfBattleFrameDataPool());
        ProtoFactory.AddProtoPool(typeof(ntf_battle_frame_data.one_slot), new OneSlotPool());
        ProtoFactory.AddProtoPool(typeof(ntf_battle_frame_data.cmd_with_frame), new CmdWithFramePool());
        ProtoFactory.AddProtoPool(typeof(one_cmd), new OneCmdPool());
    }

    void ClearProtoPool()
    {
        ProtoFactory.ClearProtoPool();
    }
}
