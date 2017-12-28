using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace networks
{
    public interface IMessageQueue : IDisposable
    {
        void Add(byte[] o);
        
        void MoveTo(List<byte[]> bytesList);
        
        bool Empty();
    }
}
