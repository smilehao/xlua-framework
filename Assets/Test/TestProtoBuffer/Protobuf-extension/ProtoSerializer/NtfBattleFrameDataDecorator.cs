using CustomDataStruct;
using ProtoBuf.Serializers;

namespace battle
{
    public sealed class NtfBattleFrameDataDecorator : ICustomProtoSerializer
    {
        public void SetValue(object target, object value, int fieldNumber)
        {
            ntf_battle_frame_data data = target as ntf_battle_frame_data;
            if (data == null)
            {
                return;
            }

            switch (fieldNumber)
            {
                case 1:
                    data.time = ValueObject.Value<int>(value);
                    break;
                case 3:
                    data.slot_list.Add((ntf_battle_frame_data.one_slot)value);
                    break;
                case 5:
                    data.server_from_slot = ValueObject.Value<int>(value);
                    break;
                case 6:
                    data.server_to_slot = ValueObject.Value<int>(value);
                    break;
                case 7:
                    data.server_curr_frame = ValueObject.Value<int>(value);
                    break;
                case 8:
                    data.is_check_frame = ValueObject.Value<int>(value);
                    break;
                default:
                    break;
            }
        }

        public object GetValue(object target, int fieldNumber)
        {
            ntf_battle_frame_data data = target as ntf_battle_frame_data;
            if (data == null)
            {
                return null;
            }

            switch (fieldNumber)
            {
                case 1:
                    return ValueObject.Get(data.time);
                case 3:
                    return data.slot_list;
                case 5:
                    return ValueObject.Get(data.server_from_slot);
                case 6:
                    return ValueObject.Get(data.server_to_slot);
                case 7:
                    return ValueObject.Get(data.server_curr_frame);
            }

            return null;
        }
    }
}