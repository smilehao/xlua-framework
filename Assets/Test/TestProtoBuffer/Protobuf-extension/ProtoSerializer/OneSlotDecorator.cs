using CustomDataStruct;
using ProtoBuf.Serializers;

namespace battle
{
    public sealed class OneSlotDecorator : ICustomProtoSerializer
    {
        public void SetValue(object target, object value, int fieldNumber)
        {
            ntf_battle_frame_data.one_slot data = target as ntf_battle_frame_data.one_slot;
            if (data == null)
            {
                return;
            }

            switch (fieldNumber)
            {
                case 1:
                    data.slot = ValueObject.Value<int>(value);
                    break;
                case 3:
                    data.cmd_list.Add((ntf_battle_frame_data.cmd_with_frame)value);
                    break;
                default:
                    break;
            }
        }

        public object GetValue(object target, int fieldNumber)
        {
            ntf_battle_frame_data.one_slot data = target as ntf_battle_frame_data.one_slot;
            if (data == null)
            {
                return null;
            }

            switch (fieldNumber)
            {
                case 1:
                    return ValueObject.Get(data.slot);
                case 3:
                    return data.cmd_list;
            }

            return null;
        }
    }
}