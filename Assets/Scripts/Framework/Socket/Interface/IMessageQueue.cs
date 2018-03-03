using System;
using System.Collections.Generic;

namespace Networks
{
    public interface IMessageQueue : IDisposable
    {
        void Add(byte[] o);
        
        void MoveTo(List<byte[]> bytesList);
        
        bool Empty();
    }
}
