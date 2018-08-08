using battle;

public sealed class CmdWithFramePool : ProtoPoolBase<ntf_battle_frame_data.cmd_with_frame>
{
    protected override void RecycleChildren(ntf_battle_frame_data.cmd_with_frame netData)
    {
        if (netData.cmd != null)
        {
            ProtoFactory.Recycle(netData.cmd);
        }
    }

    protected override void ClearNetData(ntf_battle_frame_data.cmd_with_frame netData)
    {
        netData.server_frame = 0;
        netData.cmd = null;
    }
}
