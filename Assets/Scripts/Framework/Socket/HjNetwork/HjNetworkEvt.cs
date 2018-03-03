using System;

namespace Networks
{
    public struct HjNetworkEvt
    {
        public object sender;
        public int result;
        public string msg;
        public Action<object, int, string> evtHandle;

        public HjNetworkEvt(object sender, int result, string msg, Action<object, int, string> evtHandle)
        {
            this.sender = sender;
            this.result = result;
            this.msg = msg;
            this.evtHandle = evtHandle;
        }
    }
}