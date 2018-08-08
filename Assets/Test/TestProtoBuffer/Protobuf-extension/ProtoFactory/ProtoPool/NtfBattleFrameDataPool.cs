using battle;

public sealed class NtfBattleFrameDataPool : ProtoPoolBase<ntf_battle_frame_data>
{
    protected override void RecycleChildren(ntf_battle_frame_data netData)
    {
        for (int i = 0; i < netData.slot_list.Count; i++)
        {
            ProtoFactory.Recycle(netData.slot_list[i]);
        }
    }

    protected override void ClearNetData(ntf_battle_frame_data netData)
    {
        netData.server_curr_frame = 0;
        netData.server_from_slot = 0;
        netData.server_to_slot = 0;
        netData.slot_list.Clear();
        netData.time = 0;
        netData.is_check_frame = 0;
    }
}
