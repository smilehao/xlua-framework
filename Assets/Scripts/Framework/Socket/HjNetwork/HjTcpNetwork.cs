using System;
using System.Net.Sockets;
using CustomDataStruct;
using System.Threading;
using System.Collections.Generic;
using XLua;

namespace networks
{
    [LuaCallCSharp]
    public class HjTcpNetwork : HjNetworkBase
    {
        private Thread mSendThread = null;
        private volatile bool mSendWork = false;
        private HjSemaphore mSendSemaphore = null;

        protected IMessageQueue mSendMsgQueue = null;

        public HjTcpNetwork(int maxBytesOnceSent = 1024 * 100, int maxReceiveBuffer = 1024 * 512) : base(maxBytesOnceSent, maxReceiveBuffer)
        {
            mSendSemaphore = new HjSemaphore();
            mSendMsgQueue = new MessageQueue();
        }

        public override void Dispose()
        {
            mSendMsgQueue.Dispose();
            base.Dispose();
        }

        protected override void DoConnect()
        {
            String newServerIp = "";
            AddressFamily newAddressFamily = AddressFamily.InterNetwork;
            IPv6SupportMidleware.getIPType(mIp, mPort.ToString(), out newServerIp, out newAddressFamily);
            if (!string.IsNullOrEmpty(newServerIp))
            {
                mIp = newServerIp;
            }

            mClientSocket = new Socket(newAddressFamily, SocketType.Stream, ProtocolType.Tcp);
            mClientSocket.BeginConnect(mIp, mPort, (IAsyncResult ia) =>
            {
                mClientSocket.EndConnect(ia);
                OnConnected();
            }, null);
        }
        
        protected override void DoClose()
        {
            // 关闭socket，Tcp下要等待现有数据发送、接受完成
            // https://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.shutdown(v=vs.90).aspx
            // mClientSocket.Shutdown(SocketShutdown.Both);
            base.DoClose();
        }

        public override void StartAllThread()
        {
            base.StartAllThread();

            if (mSendThread == null)
            {
                mSendThread = new Thread(SendThread);
                mSendWork = true;
                mSendThread.Start(null);
            }
        }

        public override void StopAllThread()
        {
            base.StopAllThread();
            //先把队列清掉
            mSendMsgQueue.Dispose();

            if (mSendThread != null)
            {
                mSendWork = false;
                mSendSemaphore.ProduceResrouce();// 唤醒线程
                mSendThread.Join();// 等待子线程退出
                mSendThread = null;
            }
        }

        private void SendThread(object o)
        {
            List<byte[]> workList = new List<byte[]>(10);

            while (mSendWork)
            {
                if (!mSendWork)
                {
                    break;
                }

                if (mClientSocket == null || !mClientSocket.Connected)
                {
                    continue;
                }

                mSendSemaphore.WaitResource();
                if (mSendMsgQueue.Empty())
                {
                    continue;
                }

                mSendMsgQueue.MoveTo(workList);
                try
                {
                    for (int k = 0; k < workList.Count; ++k)
                    {
                        var msgObj = workList[k];
                        if (mSendWork)
                        {
                            mClientSocket.Send(msgObj, msgObj.Length, SocketFlags.None);
                        }
                    }
                }
                catch (ObjectDisposedException e)
                {
                    ReportSocketError(ESocketError.ERROR_1, e.Message);
                    break;
                }
                catch (Exception e)
                {
                    ReportSocketError(ESocketError.ERROR_2, e.Message);
                    break;
                }
                finally
                {
                    for (int k = 0; k < workList.Count; ++k)
                    {
                        var msgObj = workList[k];
                        StreamBufferPool.RecycleBuffer(msgObj);
                    }
                    workList.Clear();
                }
            }
            
            if (mStatus == SOCKSTAT.CONNECTED)
            {
                mStatus = SOCKSTAT.CLOSED;
            }
        }

        protected override void DoReceive(StreamBuffer streamBuffer, ref int bufferCurLen)
        {
            // 组包、拆包
            int start = 0;
            streamBuffer.ResetStream();
            while (true)
            {
                if (bufferCurLen < sizeof(int))
                {
                    break;
                }

                byte[] data = streamBuffer.GetBuffer();
                int msgLen = BitConverter.ToInt32(data, start);
                if (bufferCurLen < msgLen + sizeof(int))
                {
                    break;
                }

                // 提取字节流，去掉开头表示长度的4字节
                start += sizeof(int);
                mReceiveMsgQueue.Add(streamBuffer.ToArray(start, msgLen));

                // 下一次组包
                start += msgLen;
            }

            if (start > 0)
            {
                bufferCurLen -= start;
                streamBuffer.CopyFrom(streamBuffer.GetBuffer(), start, 0, bufferCurLen);
            }
        }

        public override void SendMessage(byte[] msgObj)
        {
            mSendMsgQueue.Add(msgObj);
            mSendSemaphore.ProduceResrouce();
        }
    }
}
