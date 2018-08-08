using battle;
using CustomDataStruct;

public sealed class OneCmdPool : ProtoPoolBase<one_cmd>
{
    protected override void RecycleChildren(one_cmd netData)
    {
        StreamBufferPool.RecycleBuffer(netData.cmd_data);
    }

    protected override void ClearNetData(one_cmd netData)
    {
        netData.cmd_id = 0;
        netData.UID = 0;
        netData.cmd_data = null;
    }

    public override object DeepCopy(object data)
    {
        one_cmd fromData = data as one_cmd;
        if (fromData == null) throw new System.ArgumentNullException("data");

        one_cmd toData = ProtoFactory.Get<one_cmd>();
        toData.cmd_id = fromData.cmd_id;
        toData.UID = fromData.UID;
        toData.cmd_data = StreamBufferPool.DeepCopy(fromData.cmd_data);
        return toData;
    }
}
