using CustomDataStruct;
using System;
using System.Collections.Generic;

namespace Networks
{
    class MessageQueue : IMessageQueue
    {
        object mMutex;
        List<byte[]> mMessageList;

        public MessageQueue(int capacity = 10)
        {
            mMutex = new object();
            mMessageList = new List<byte[]>(capacity);
        }

        public void Add(byte[] o)
        {
            lock (mMutex)
            {
                mMessageList.Add(o);
            }
        }

        public void MoveTo(List<byte[]> bytesList)
        {
            lock (mMutex)
            {
                bytesList.AddRange(mMessageList);
                mMessageList.Clear();
            }
        }

        public bool Empty()
        {
            lock (mMutex)
            {
                return mMessageList.Count <= 0;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && !Empty())
                {
                    lock (mMutex)
                    {
                        for (int i = 0; i < mMessageList.Count; i++)
                        {
                            StreamBufferPool.RecycleBuffer(mMessageList[i]);
                        }
                        mMessageList.Clear();
                    }
                }
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
