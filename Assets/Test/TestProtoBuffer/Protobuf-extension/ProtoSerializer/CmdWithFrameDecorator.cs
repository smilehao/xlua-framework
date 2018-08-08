using CustomDataStruct;
using ProtoBuf.Serializers;

namespace battle
{
    public sealed class CmdWithFrameDecorator : ICustomProtoSerializer
    {
        public void SetValue(object target, object value, int fieldNumber)
        {
            ntf_battle_frame_data.cmd_with_frame data = target as ntf_battle_frame_data.cmd_with_frame;
            if (data == null)
            {
                return;
            }

            switch (fieldNumber)
            {
                case 1:
                    data.server_frame = ValueObject.Value<int>(value);
                    break;
                case 2:
                    data.cmd = (one_cmd)value;
                    break;
                default:
                    break;
            }
        }

        public object GetValue(object target, int fieldNumber)
        {
            ntf_battle_frame_data.cmd_with_frame data = target as ntf_battle_frame_data.cmd_with_frame;
            if (data == null)
            {
                return null;
            }

            switch (fieldNumber)
            {
                case 1:
                    return ValueObject.Get(data.server_frame);
                case 2:
                    return data.cmd;
            }

            return null;
        }
    }
}