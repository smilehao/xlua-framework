using System;
using System.Collections.Generic;
using System.IO;
using XLua;

/// <summary>
/// 说明：
///     1）封装字节buffer以及MemoryStream、BinaryRender、BinaryWrite操作
///     2）对如上对象执行缓存管理
///     3）多线程安全
/// 
/// by wsh @ 2017-07-03
/// </summary>

namespace CustomDataStruct
{
    [Hotfix]
    public sealed class StreamBufferPool
    {
#if UNITY_EDITOR
        static MemoryLeakDetecter streamDetecter = MemoryLeakDetecter.Add(typeof(StreamBuffer).FullName, 100, 100);
        static MemoryLeakDetecter bufferDetecter = MemoryLeakDetecter.Add(typeof(byte[]).FullName, 500, 1000);
#endif
        const int BUFFER_POOL_SIZE = 500;
        static Dictionary<int, Queue<StreamBuffer>> streamPool = new Dictionary<int, Queue<StreamBuffer>>();
        static Dictionary<int, Queue<byte[]>> bufferPool = new Dictionary<int, Queue<byte[]>>();
        volatile static int streamCount = 0;
        volatile static int bufferCount = 0;
        
        public static StreamBuffer GetStream(int expectedSize, bool canWrite, bool canRead)
        {
            if (expectedSize <= 0) throw new Exception("expectedSize must > 0!");
            Queue<StreamBuffer> streamCache = null;
            StreamBuffer streamBuffer = null;
            lock (streamPool)
            {
#if UNITY_EDITOR
                streamDetecter.IncreseInstance();
#endif
                if (!streamPool.TryGetValue(expectedSize, out streamCache))
                {
                    streamCache = new Queue<StreamBuffer>();
                    streamPool.Add(expectedSize, streamCache);
                }
                if (streamCache.Count > 0)
                {
                    streamCount--;
                    streamBuffer = streamCache.Dequeue();
                    streamBuffer.SetOperate(canWrite, canRead);
                }
            }
            if(streamBuffer == null) streamBuffer = new StreamBuffer(expectedSize, canWrite, canRead);
            return streamBuffer;
        }

        public static void RecycleStream(StreamBuffer stream)
        {
            if (stream == null || stream.size == 0) return;
            Queue<StreamBuffer> streamCache = null;
            lock (streamPool)
            {
                if (!streamPool.TryGetValue(stream.size, out streamCache))
                {
                    streamCache = new Queue<StreamBuffer>();
                    streamPool.Add(stream.size, streamCache);
                }
                stream.ClearBuffer();
                stream.ResetStream();
                streamCount++;
                streamCache.Enqueue(stream);
#if UNITY_EDITOR
                streamDetecter.DecreseInstance();
                streamDetecter.SetPooledObjectCount(streamCount);
#endif
            }
        }

        public static byte[] GetBuffer(StreamBuffer streamBuffer)
        {
            byte[] bytes = GetBuffer(streamBuffer.size);
            streamBuffer.CopyTo(bytes, 0, 0, streamBuffer.size);
            return bytes;
        }

        public static byte[] GetBuffer(StreamBuffer streamBuffer, int start, int length)
        {
            byte[] bytes = GetBuffer(length);
            streamBuffer.CopyTo(bytes, start, 0, length);
            return bytes;
        }

        public static byte[] GetBuffer(int expectedSize)
        {
            if (expectedSize <= 0) throw new Exception("expectedSize must > 0!");
            Queue<byte[]> bufferCache = null;
            byte[] buffer = null;
            lock (bufferPool)
            {
#if UNITY_EDITOR
                bufferDetecter.IncreseInstance();
#endif
                if (!bufferPool.TryGetValue(expectedSize, out bufferCache))
                {
                    bufferCache = new Queue<byte[]>();
                    bufferPool.Add(expectedSize, bufferCache);
                }
                if (bufferCache.Count > 0)
                {
                    bufferCount--;
                    buffer = bufferCache.Dequeue();
                }
            }
            if (buffer == null) buffer = new byte[expectedSize];
            return buffer;
        }
        
        public static byte[] DeepCopy(byte[] bytes)
        {
            if (bytes == null) return null;
            if (bytes.Length == 0) return new byte[0];
            byte[] newBytes = GetBuffer(bytes.Length);
            Buffer.BlockCopy(bytes, 0, newBytes, 0, bytes.Length);
            return newBytes;
        }

        public static void RecycleBuffer(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) return;
            if (bufferCount > BUFFER_POOL_SIZE) return;

            Queue<byte[]> bufferCache = null;
            lock (bufferPool)
            {
                if (!bufferPool.TryGetValue(buffer.Length, out bufferCache))
                {
                    bufferCache = new Queue<byte[]>();
                    bufferPool.Add(buffer.Length, bufferCache);
                }
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)0;
                }
                bufferCount++;
                bufferCache.Enqueue(buffer);
#if UNITY_EDITOR
                bufferDetecter.DecreseInstance();
                bufferDetecter.SetPooledObjectCount(bufferCount);
#endif
            }
        }
    }

    [Hotfix]
    public sealed class StreamBuffer : IDisposable
    {
        byte[] mBuffer;
        MemoryStream mMemStream;
        BinaryReader mBinaryReader;
        BinaryWriter mBinaryWriter;

        public StreamBuffer(int bufferSize, bool canWrite, bool canRead)
        {
            if (bufferSize <= 0) throw new Exception("expectedSize must > 0!");
            mBuffer = new byte[bufferSize];
            SetOperate(canWrite, canRead);
        }

        internal void SetOperate(bool canWrite, bool canRead)
        {
            if (!canWrite && size <= 0) throw new Exception("bufferSize must > 0 when can not write");
            this.canWrite = canWrite;
            this.canRead = canRead;
        }

        public bool canWrite
        {
            get;
            private set;
        }

        public bool canRead
        {
            get;
            private set;
        }

        public MemoryStream memStream
        {
            get
            {
                if (!canRead && !canWrite) throw new Exception("The stream buffer can not read and can not write!");
                if (mMemStream == null) mMemStream = new MemoryStream(mBuffer, 0, mBuffer.Length, true, true);
                return mMemStream;
            }
        }

        public BinaryReader binaryReader
        {
            get
            {
                if (!canRead) throw new Exception("The stream buffer can not read!");
                if (mBinaryReader == null) mBinaryReader = new BinaryReader(memStream);
                return mBinaryReader;
            }
        }

        public BinaryWriter binaryWriter
        {
            get
            {
                if (!canWrite) throw new Exception("The stream buffer can not write!");
                if (mBinaryWriter == null) mBinaryWriter = new BinaryWriter(memStream);
                return mBinaryWriter;
            }
        }

        public int size
        {
            get { return mBuffer.Length; }
        }

        public void CopyFrom(byte[] src, int srcOffset, int dstOffest, int length)
        {
            Buffer.BlockCopy(src, srcOffset, mBuffer, dstOffest, length);
        }

        public void CopyTo(byte[] dst, int srcOffset, int dstOffest, int length)
        {
            Buffer.BlockCopy(mBuffer, srcOffset, dst, dstOffest, length);
        }

        public long Position()
        {
            return mMemStream.Position;
        }

        // 走StreamBuffer缓存，无GC，注意回收
        public byte[] ToArray()
        {
            return StreamBufferPool.GetBuffer(this);
        }

        public byte[] ToArray(int start, int length)
        {
            return StreamBufferPool.GetBuffer(this, start, length);
        }

        // 注意StreamBuffer生命周期，否则数据可能无效
        public byte[] GetBuffer()
        {
            return mBuffer;
        }

        public void ClearBuffer()
        {
            for (int i = 0; i < size; i++)
            {
                mBuffer[i] = (byte)0;
            }
        }

        public void ResetStream()
        {
            memStream.Seek(0, SeekOrigin.Begin);
        }

        public void SetPosition(long offset)
        {
            mMemStream.Position = offset;
        }

        public void Dispose()
        {
            mBuffer = null;
            if (mBinaryReader != null) mBinaryReader.Close();
            if (mBinaryWriter != null) mBinaryWriter.Close();
            if (mMemStream != null) mMemStream.Close();
            if (mMemStream != null) mMemStream.Dispose();
            mBinaryReader = null;
            mBinaryWriter = null;
            mMemStream = null;
        }
    }
    
#if UNITY_EDITOR
    public static class StreamBufferPoolExporter
    {
        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp = new List<Type>()
        {
            typeof(StreamBufferPool),
            typeof(StreamBuffer),
            typeof(MemoryStream),
            typeof(BinaryReader),
            typeof(BinaryWriter),
        };
    }
#endif
}
