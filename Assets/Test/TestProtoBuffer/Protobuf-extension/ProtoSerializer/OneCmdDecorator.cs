using CustomDataStruct;
using ProtoBuf.Serializers;

namespace battle
{
    public sealed class OneCmdDecorator : ICustomProtoSerializer
    {
        public void SetValue(object target, object value, int fieldNumber)
        {
            one_cmd data = target as one_cmd;
            if (data == null)
            {
                return;
            }

            switch (fieldNumber)
            {
                case 1:
                    data.cmd_id = ValueObject.Value<int>(value);
                    break;
                case 2:
                    data.UID = ValueObject.Value<uint>(value);
                    break;
                case 3:
                    data.cmd_data = (byte[])value;
                    break;
                default:
                    break;
            }
        }

        public object GetValue(object target, int fieldNumber)
        {
            one_cmd data = target as one_cmd;
            if (data == null)
            {
                return null;
            }

            switch (fieldNumber)
            {
                case 1:
                    return ValueObject.Get(data.cmd_id);
                case 2:
                    return ValueObject.Get(data.UID);
                case 3:
                    return data.cmd_data;
            }

            return null;
        }
    }
}