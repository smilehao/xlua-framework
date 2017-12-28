namespace networks
{
    public delegate void HjNetworkEvtHandle(object sender, int result, string msg);

    public struct HjNetworkEvt
    {
        public object sender;
        public int result;
        public string msg;
        public HjNetworkEvtHandle evtHandle;
        public HjNetworkEvt(object sender, int result, string msg, HjNetworkEvtHandle evtHandle)
        {
            this.sender = sender;
            this.result = result;
            this.msg = msg;
            this.evtHandle = evtHandle;
        }
    }
}