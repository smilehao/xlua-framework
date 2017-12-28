using System;
using System.Collections.Generic;
using System.Net.Sockets;
using CustomDataStruct;
using System.Threading;
using UnityEngine;

namespace networks
{
    public delegate void HjReceivePkgHandle(byte[] buffer);//消息处理委托
    //public delegate byte[] HjMemAllocator(int len);//内存分配器
    //public delegate void HjMemCollector(byte[] buffer);//内存回收器

    public enum SOCKSTAT
    {
        CLOSED = 0,
        CONNECTING,
        CONNECTED,
    }

    public abstract class HjNetworkBase
    {
        private HjNetworkEvtHandle mOnConnect = null;
        private HjNetworkEvtHandle mOnClosed = null;
        private List<HjNetworkEvt> mNetworkEvtList = null;
        private object mNetworkEvtLock = null;

        protected int mMaxBytesOnceSent = 0;
        protected int mMaxReceiveBuffer = 0;

        protected Socket mClientSocket = null;
        protected string mIp;
        protected int mPort;
        protected volatile SOCKSTAT mStatus = SOCKSTAT.CLOSED;


        private Thread mReceiveThread = null;
        private volatile bool mReceiveWork = false;
        private List<byte[]> mTempMsgList = null;
        protected IMessageQueue mReceiveMsgQueue = null;
        private HjReceivePkgHandle mReceivePkgHandle = null;

        public HjNetworkBase(int maxBytesOnceSent = 1024 * 512, int maxReceiveBuffer = 1024 * 1024 * 2)
        {
            mStatus = SOCKSTAT.CLOSED;
            
            mMaxBytesOnceSent = maxBytesOnceSent;
            mMaxReceiveBuffer = maxReceiveBuffer;

            mNetworkEvtList = new List<HjNetworkEvt>();
            mNetworkEvtLock = new object();
            mTempMsgList = new List<byte[]>();
            mReceiveMsgQueue = new MessageQueue();
        }

        public virtual void Dispose()
        {
            Close();
        }

        public Socket ClientSocket
        {
            get
            {
                return mClientSocket;
            }
        }

        public void SetHostPort(string ip, int port)
        {
            mIp = ip;
            mPort = port;
        }

        public void SetOnConnect(HjNetworkEvtHandle handle)
        {
            mOnConnect = handle;
        }

        public void SetOnClosed(HjNetworkEvtHandle handle)
        {
            mOnClosed = handle;
        }

        public void SetPkgHandle(HjReceivePkgHandle handle)
        {
            mReceivePkgHandle = handle;
        }

        protected abstract void DoConnect();
        public void Connect()
        {
            Close();

            int result = ESocketError.NORMAL;
            string msg = null;
            try
            {
                DoConnect();
            }
            catch (ObjectDisposedException ex)
            {
                result = ESocketError.ERROR_3;
                msg = ex.Message;
                mStatus = SOCKSTAT.CLOSED;
            }
            catch (Exception ex)
            {
                result = ESocketError.ERROR_4;
                msg = ex.Message;
                mStatus = SOCKSTAT.CLOSED;
            }
            finally
            {
                if (result != ESocketError.NORMAL && mOnConnect != null)
                {
                    HjNetworkEvt _evt = new HjNetworkEvt(this, result, msg, mOnConnect);
                    AddNetworkEvt(_evt);
                }
            }
        }

        protected virtual void OnConnected()
        {
            StartAllThread();
            mStatus = SOCKSTAT.CONNECTED;
            string msg = "Connected successful";
            if (mOnConnect != null)
            {
                HjNetworkEvt _evt = new HjNetworkEvt(this, ESocketError.NORMAL, msg, mOnConnect);
                AddNetworkEvt(_evt);
            }
        }

        public virtual void StartAllThread()
        {
            if (mReceiveThread == null)
            {
                mReceiveThread = new Thread(ReceiveThread);
                mReceiveWork = true;
                mReceiveThread.Start(null);
            }
        }

        public virtual void StopAllThread()
        {
            mReceiveMsgQueue.Dispose();

            if (mReceiveThread != null)
            {
                mReceiveWork = false;
                mReceiveThread.Join();
                mReceiveThread = null;
            }
        }

        protected virtual void DoClose()
        {
            mClientSocket.Close();
            if (mClientSocket.Connected)
            {
                throw new InvalidOperationException("Should close socket first!");
            }
            mClientSocket = null;
            StopAllThread();
        }

        public virtual void Close()
        {
            if (mClientSocket == null) return;

            mStatus = SOCKSTAT.CLOSED;
            try
            {
                DoClose();

                int result = ESocketError.ERROR_5;
                string msg = "Disconnected!";
                HjNetworkEvt evt = new HjNetworkEvt(this, result, msg, mOnClosed);
                AddNetworkEvt(evt);
            }
            catch (Exception e)
            {
                ReportSocketError(ESocketError.ERROR_4, e.Message);
            }
        }
        
        protected void ReportSocketError(int result, string msg)
        {
            if (mOnClosed != null)
            {
                AddNetworkEvt(new HjNetworkEvt(this, result, msg, mOnClosed));
            }
        }

        protected abstract void DoReceive(StreamBuffer receiveStreamBuffer, ref int bufferCurLen);
        private void ReceiveThread(object o)
        {
            StreamBuffer receiveStreamBuffer = StreamBufferPool.GetStream(mMaxReceiveBuffer, false, true);
            int bufferCurLen = 0;
            while (mReceiveWork)
            {
                try
                {
                    if (!mReceiveWork) break;
                    if (mClientSocket != null)
                    {
                        int bufferLeftLen = receiveStreamBuffer.size - bufferCurLen;
                        int readLen = mClientSocket.Receive(receiveStreamBuffer.GetBuffer(), bufferCurLen, bufferLeftLen, SocketFlags.None);
                        if (readLen == 0) throw new ObjectDisposedException("DisposeEX", "receive from server 0 bytes,closed it");
                        if (readLen < 0) throw new Exception("Unknow exception, readLen < 0" + readLen);

                        bufferCurLen += readLen;
                        DoReceive(receiveStreamBuffer, ref bufferCurLen);
                        if (bufferCurLen == receiveStreamBuffer.size)
                            throw new Exception("Receive from sever no enough buff size:" + bufferCurLen);
                    }
                }
                catch (ObjectDisposedException e)
                {
                    ReportSocketError(ESocketError.ERROR_3, e.Message);
                    break;
                }
                catch (Exception e)
                {
                    ReportSocketError(ESocketError.ERROR_4, e.Message);
                    break;
                }
            }

            StreamBufferPool.RecycleStream(receiveStreamBuffer);
            if (mStatus == SOCKSTAT.CONNECTED)
            {
                mStatus = SOCKSTAT.CLOSED;
            }
        }
        
        protected void AddNetworkEvt(HjNetworkEvt evt)
        {
            lock (mNetworkEvtLock)
            {
                mNetworkEvtList.Add(evt);
            }
        }

        private void UpdateEvt()
        {
            lock (mNetworkEvtLock)
            {
                try
                {
                    for (int i = 0; i < mNetworkEvtList.Count; ++i)
                    {
                        HjNetworkEvt evt = mNetworkEvtList[i];
                        evt.evtHandle(evt.sender, evt.result, evt.msg);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Got the fucking exception :" + e.Message);
                }
                finally
                {
                    mNetworkEvtList.Clear();
                }
            }
        }

        private void UpdatePacket()
        {
            if (!mReceiveMsgQueue.Empty())
            {
                mReceiveMsgQueue.MoveTo(mTempMsgList);

                try
                {
                    for (int i = 0; i < mTempMsgList.Count; ++i)
                    {
                        var objMsg = mTempMsgList[i];
                        if (mReceivePkgHandle != null)
                        {
                            mReceivePkgHandle(objMsg);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Got the fucking exception :" + e.Message);
                }
                finally
                {
                    for (int i = 0; i < mTempMsgList.Count; ++i)
                    {
                        StreamBufferPool.RecycleBuffer(mTempMsgList[i]);
                    }
                    mTempMsgList.Clear();
                }
            }
        }

        public virtual void UpdateNetwork()
        {
            UpdatePacket();
            UpdateEvt();
        }

        // 发送消息的时候要注意对buffer进行拷贝，网络层发送完毕以后会对buffer执行回收
        public virtual void SendMessage(byte[] msgObj)
        {
        }

        public bool IsConnect()
        {
            return mStatus == SOCKSTAT.CONNECTED;
        }
    }
}
