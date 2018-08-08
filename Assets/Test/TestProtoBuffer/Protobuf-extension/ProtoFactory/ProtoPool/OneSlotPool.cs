using battle;

public sealed class OneSlotPool : ProtoPoolBase<ntf_battle_frame_data.one_slot>
{
    protected override void RecycleChildren(ntf_battle_frame_data.one_slot netData)
    {
        for (int i = 0; i < netData.cmd_list.Count; i++)
        {
            ProtoFactory.Recycle(netData.cmd_list[i]);
        }
    }

    protected override void ClearNetData(ntf_battle_frame_data.one_slot netData)
    {
        netData.cmd_list.Clear();
        netData.slot = 0;
    }
}