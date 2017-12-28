using UnityEngine;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// 说明：GC优化版StringBuilder，对StringBuilder类的扩展，用缓存、改写实现方式等来降低GC
/// 
/// 注意：
///     1）如果某个串拼接操作出现的结果是一个有限集合，且会大量执行拼接，则使用本类能带来极佳的效果
///     2) 绝大多数情况下，推荐使用本地缓存，如果有多处使用一个规则的串拼接，建议想办法共享本类实例
///     3）如果串拼接的结果在多处是要使用到的，则使用全局缓存，但是全局缓存越大，会导致性能越低（慎用）
///     4）对于启用全局缓存的，同时开启本地缓存能够以内存换取极大部分的本地命中，提升效率
///     5）在Append、Insert非char、string类型和Replace string类型时效率会比StringBuilder低，追求效率的地方慎用
///     6）某些方法使用不当仍然会有GC，这些方法在函数体前均有说明，使用时注意
/// 
/// TODO：
///     1）优化Append、Insert非char、string类型和Replace string类型时的效率
/// 
/// by wsh @ 2017-06-15
/// </summary>

namespace CustomDataStruct
{
    public enum BetterStringBuilderBufferType
    {
        None = 0,//不使用缓存
        GlobalOnly = 0,// 只使用全局缓存
        LocalOnly,//只使用本地缓存
        Both,//两者同时使用
    }

    public sealed class BetterStringBuilder
    {
#if UNITY_EDITOR
        MemoryLeakDetecter localDetecter = MemoryLeakDetecter.Add(typeof(string).FullName, 1000, 1000);
        static MemoryLeakDetecter globalDetecter = MemoryLeakDetecter.Add(typeof(string).FullName, 1000, 1000);
#endif
        private const int POOL_SIZE_LIMIT = 1000;
        private const string pattern0 = "{0}";
        private const string pattern1 = "{1}";
        private const string pattern2 = "{2}";

        StringBuilder mStringBuilder;
        BetterStringBuilderBufferType mBufferType;
        static List<string> mGlobalBuffer;
        List<string> mLocalBuffer;

        private BetterStringBuilder()
        {
        }

        // 说明：其它构造函数一并丢弃，凸显capacity设置的重要性，避免GC
        // stringCapacity：StringBuilder字符串缓存大小，这个值取决于要操作的字符串可能的最大长度
        // localBufferCapacity：字符串缓存列表的大小，这个值取决于结果有限集合的大小
        // 比如，一个串由颜色代码"[aa0022]"和等级信息"1"拼接，颜色最多有3种可能，等级为1~5级，那么：
        // 1）stringCapacity：最大长度为Length("[aa0022]1") = 9
        // 2）localBufferCapacity：结果有限集大小为：3 * 5 = 15种可能
        public BetterStringBuilder(int stringCapacity, int localBufferCapacity,
            BetterStringBuilderBufferType bufferType = BetterStringBuilderBufferType.LocalOnly)
        {
            mStringBuilder = new StringBuilder(stringCapacity);
            mBufferType = bufferType;
            InitBuffer(localBufferCapacity);
        }

#if UNITY_EDITOR
        ~BetterStringBuilder()
        {
            MemoryLeakDetecter.Remove(localDetecter);
        }
#endif

        private void InitBuffer(int localBufferCapacity)
        {
            if (mBufferType == BetterStringBuilderBufferType.LocalOnly ||
                mBufferType == BetterStringBuilderBufferType.Both)
            {
                if (mLocalBuffer == null)
                {
                    mLocalBuffer = new List<string>(localBufferCapacity > 0 ? localBufferCapacity : 32);
                }
            }
            if (mBufferType == BetterStringBuilderBufferType.GlobalOnly ||
                mBufferType == BetterStringBuilderBufferType.Both)
            {
                if (mGlobalBuffer == null)
                {
                    mGlobalBuffer = new List<string>(1024);
                }
            }
        }

        // 说明：内存释放
        public static void Cleanup()
        {
            if (mGlobalBuffer != null)
            {
                mGlobalBuffer.Clear();
                mGlobalBuffer = null;
            }
        }

        public void ClearBuffer()
        {
            if (mLocalBuffer != null)
            {
                mLocalBuffer.Clear();
            }
        }

        public int BufferSize
        {
            get
            {
                return mLocalBuffer != null ? mLocalBuffer.Count : 0;
            }
        }

        public char this[int index]
        {
            get
            {
                return mStringBuilder[index];
            }
            set
            {
                mStringBuilder[index] = value;
            }
        }

        public int Capacity
        {
            get
            {
                return mStringBuilder.Capacity;
            }
            set
            {
                mStringBuilder.Capacity = value;
            }
        }

        public int Length
        {
            get
            {
                return mStringBuilder.Length;
            }
            set
            {
                mStringBuilder.Length = value;
            }
        }

        public int MaxCapacity
        {
            get
            {
                return mStringBuilder.MaxCapacity;
            }
        }

        private void Wrap(int startIndex, int endIndex)
        {
            // 字符串翻转："abcde" -> "edcba"
            char c;
            while (startIndex < endIndex)
            {
                c = this[startIndex];
                this[startIndex] = this[endIndex];
                this[endIndex] = c;
                startIndex++;
                endIndex--;
            }
        }

        public void Append(bool value)
        {
            mStringBuilder = mStringBuilder.Append(value);
        }

        public void Append(string value)
        {
            mStringBuilder = mStringBuilder.Append(value);
        }

        public void Append(byte value)
        {
            Append((long)value);
        }

        public void Append(double value, int precision)
        {
            if (precision < 0) precision = 0;
            long integralPart = (long)value;
            long fractionalPartEx = (long)((value - integralPart) * Mathf.Pow(10, precision + 1));
            if (fractionalPartEx < 0) fractionalPartEx = -fractionalPartEx;
            long fractionalPart = fractionalPartEx / 10L;
            if (fractionalPartEx % 10 >= 5)
            {
                // 四舍五入
                if (precision > 0) fractionalPart ++;
                else integralPart ++;
            }
            Append(integralPart);
            Append('.');
            Append(fractionalPart);
        }

        public void Append(int value)
        {
            Append((long)value);
        }

        // 说明：有GC，慎用（ToString、装箱）
        public void Append(object value)
        {
            mStringBuilder = mStringBuilder.Append(value);
        }

        public void Append(float value, int precision)
        {
            Append((double)value, precision);
        }
        
        public void Append(uint value)
        {
            Append((ulong)value);
        }

        public void Append(char value)
        {
            mStringBuilder = mStringBuilder.Append(value);
        }

        public void Append(ulong value)
        {
            if (value == 0)
            {
                Append('0');
            }
            else
            {
                int startIndex = Length;
                while (value != 0)
                {
                    char c = (char)(value % 10 + '0');
                    Append(c);
                    value /= 10;
                }
                Wrap(startIndex, Length - 1);
            }
        }
        
        public void Append(ushort value)
        {
            Append((ulong)value);
        }

        public void Append(long value)
        {
            if (value < 0)
            {
                Append('-');
                value = -value;
            }

            Append((ulong)value);
        }

        public void Append(short value)
        {
            Append((long)value);
        }

        public void Append(char[] value)
        {
            mStringBuilder = mStringBuilder.Append(value);
        }

        public void Append(char value, int repeatCount)
        {
            mStringBuilder = mStringBuilder.Append(value, repeatCount);
        }

        public void Append(string value, int startIndex, int count)
        {
            mStringBuilder = mStringBuilder.Append(value, startIndex, count);
        }

        public void Append(char[] value, int startIndex, int charCount)
        {
            mStringBuilder = mStringBuilder.Append(value, startIndex, startIndex);
        }

        private bool CheckIsPattern(string format, int index, string pattern)
        {
            char c = format[index];
            if (c != '{')
            {
                return false;
            }
            if (index > format.Length - pattern.Length)
            {
                return false;
            }

            for (int j = 0; j < pattern.Length; j++)
            {
                if (format[index + j] != pattern[j])
                {
                    return false;
                }
            }
            return true;
        }

        // 说明：尽量使用Append，如果参数object arg为值类型会进行装箱，同样arg.ToString()也有少许GC
        // 注意：格式串只支持最多3个参数{0}、{1}、{2}，只支持替换操作
        public void AppendFormat(string format, object arg0)
        {
            // 原版有GC
            // mStringBuilder = mStringBuilder.AppendFormat(format, arg0);
            
            string str0 = arg0.ToString();
            for (int i = 0; i < format.Length;)
            {
                if (CheckIsPattern(format, i, pattern0))
                {
                    Append(str0);
                    i += pattern0.Length;
                }
                else
                {
                    Append(format[i]);
                    i++;
                }
            }
        }
        
        public void AppendFormat(string format, object arg0, object arg1)
        {
            //mStringBuilder = mStringBuilder.AppendFormat(format, arg0, arg1);

            string str0 = arg0.ToString();
            string str1 = arg1.ToString();
            for (int i = 0; i < format.Length;)
            {
                if (CheckIsPattern(format, i, pattern0))
                {
                    Append(str0);
                    i += pattern0.Length;
                }
                else if (CheckIsPattern(format, i, pattern1))
                {
                    Append(str1);
                    i += pattern1.Length;
                }
                else
                {
                    Append(format[i]);
                    i++;
                }
            }
        }

        public void AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            // mStringBuilder = mStringBuilder.AppendFormat(format, arg0, arg1, arg2);

            string str0 = arg0.ToString();
            string str1 = arg1.ToString();
            string str2 = arg2.ToString();
            for (int i = 0; i < format.Length;)
            {
                if (CheckIsPattern(format, i, pattern0))
                {
                    Append(str0);
                    i += pattern0.Length;
                }
                else if (CheckIsPattern(format, i, pattern1))
                {
                    Append(str1);
                    i += pattern1.Length;
                }
                else if (CheckIsPattern(format, i, pattern2))
                {
                    Append(str2);
                    i += pattern2.Length;
                }
                else
                {
                    Append(format[i]);
                    i++;
                }
            }
        }

        // 说明：针对AppendFormat参数arg为int类型执行装箱和ToString带来GC的优化
        public void AppendFormat(string format, int arg0)
        {
            for (int i = 0; i < format.Length;)
            {
                if (CheckIsPattern(format, i, pattern0))
                {
                    Append(arg0);
                    i += pattern0.Length;
                }
                else
                {
                    Append(format[i]);
                    i++;
                }
            }
        }

        public void AppendFormat(string format, int arg0, int arg1)
        {
            for (int i = 0; i < format.Length;)
            {
                if (CheckIsPattern(format, i, pattern0))
                {
                    Append(arg0);
                    i += pattern0.Length;
                }
                else if (CheckIsPattern(format, i, pattern1))
                {
                    Append(arg1);
                    i += pattern1.Length;
                }
                else
                {
                    Append(format[i]);
                    i++;
                }
            }
        }

        public void AppendFormat(string format, int arg0, int arg1, int arg2)
        {
            for (int i = 0; i < format.Length;)
            {
                if (CheckIsPattern(format, i, pattern0))
                {
                    Append(arg0);
                    i += pattern0.Length;
                }
                else if (CheckIsPattern(format, i, pattern1))
                {
                    Append(arg1);
                    i += pattern1.Length;
                }
                else if (CheckIsPattern(format, i, pattern2))
                {
                    Append(arg2);
                    i += pattern2.Length;
                }
                else
                {
                    Append(format[i]);
                    i++;
                }
            }
        }

        public void AppendLine()
        {
            // 原版有GC
            // mStringBuilder = mStringBuilder.AppendLine();
            Append('\n');
        }

        public void AppendLine(string value)
        {
            // 原版有GC
            //mStringBuilder = mStringBuilder.AppendLine(value);

            Append(value);
            Append('\n');
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            mStringBuilder.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public int EnsureCapacity(int capacity)
        {
            return mStringBuilder.EnsureCapacity(capacity);
        }

        public bool Equals(StringBuilder sb)
        {
            return mStringBuilder.Equals(sb);
        }

        public bool Equals(BetterStringBuilder sb)
        {
            return ReferenceEquals(this, sb);
        }

        // Add: cmp to string
        public bool Equals(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Length == 0;
            }
            else
            {
                return Equals(str, 0, str.Length);
            }
        }

        // Add: cmp to part of string
        public bool Equals(string str, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Length == 0;
            }
            else
            {
                if (startIndex < 0 || startIndex >= str.Length) return false;
                if (length < 0 || length > str.Length - startIndex) return false;
                if (Length != length) return false;
                for (int i = 0; i < Length; i++)
                {
                    if (this[i] != str[startIndex + i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        // Add: cmp part to string
        public bool PartEquals(string str, int selfPartStartIndex, int slefPartLength)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Length == 0;
            }
            else
            {
                if (selfPartStartIndex < 0 || selfPartStartIndex >= Length) return false;
                if (slefPartLength < 0 || slefPartLength > Length - selfPartStartIndex) return false;
                if (slefPartLength != str.Length) return false;
                for (int i = 0; i < slefPartLength; i++)
                {
                    if (this[selfPartStartIndex + i] != str[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private void LoopShift(int startIndex, int endIndex, int shift)
        {
            // 循环移位：shift > 0为循环右移；shift < 0为循环左移
            // 例如右移2位："abcde"->"deabc"
            int zone = endIndex - startIndex + 1;
            if (zone <= 0) return;

            while (shift < 0) shift += zone;
            while (shift >= zone) shift -= zone;
            if (shift == 0) return;

            Wrap(startIndex, endIndex - shift);
            Wrap(endIndex - shift + 1, endIndex);
            Wrap(startIndex, endIndex);
        }

        public void Insert(int index, short value)
        {
            Insert(index, (long)value);
        }

        public void Insert(int index, int value)
        {
            Insert(index, (long)value);
        }

        public void Insert(int index, long value)
        {
            int oldLength = Length;
            Append(value);
            LoopShift(index, Length, Length - oldLength);
        }

        public void Insert(int index, float value, int precision)
        {
            Insert(index, (double)value, precision);
        }

        public void Insert(int index, ushort value)
        {
            Insert(index, (ulong)value);
        }

        public void Insert(int index, ulong value)
        {
            int oldLength = Length;
            Append(value);
            LoopShift(index, Length, Length - oldLength);
        }

        public void Insert(int index, uint value)
        {
            Insert(index, (ulong)value);
        }

        // 说明：有GC，慎用
        public void Insert(int index, object value)
        {
            mStringBuilder = mStringBuilder.Insert(index, value);
        }

        public void Insert(int index, double value, int precision)
        {
            int oldLength = Length;
            Append(value, precision);
            LoopShift(index, Length, Length - oldLength);
        }

        public void Insert(int index, bool value)
        {
            mStringBuilder = mStringBuilder.Insert(index, value);
        }

        public void Insert(int index, byte value)
        {
            mStringBuilder = mStringBuilder.Insert(index, value);
        }

        public void Insert(int index, char value)
        {
            mStringBuilder = mStringBuilder.Insert(index, value);
        }

        public void Insert(int index, string value)
        {
            mStringBuilder = mStringBuilder.Insert(index, value);
        }

        public void Insert(int index, char[] value)
        {
            mStringBuilder = mStringBuilder.Insert(index, value);
        }

        public void Insert(int index, string value, int count)
        {
            mStringBuilder = mStringBuilder.Insert(index, value, count);
        }

        public void Insert(int index, char[] value, int startIndex, int charCount)
        {
            mStringBuilder = mStringBuilder.Insert(index, value, startIndex, charCount);
        }

        public void Remove(int startIndex, int length)
        {
            mStringBuilder = mStringBuilder.Remove(startIndex, length);
        }

        // Add:for clear the sb
        public void Clear()
        {
            mStringBuilder.Remove(0, mStringBuilder.Length);
        }

        public void Replace(string oldValue, string newValue)
        {
            Replace(oldValue, newValue, 0, Length);
        }

        public void Replace(char oldChar, char newChar)
        {
            mStringBuilder = mStringBuilder.Replace(oldChar, newChar);
        }

        public void Replace(string oldValue, string newValue, int startIndex, int count)
        {
            // 说明：原版有GC
            //mStringBuilder = mStringBuilder.Replace(oldValue, newValue, startIndex, count);

            for (int i = startIndex; i < startIndex + count;)
            {
                if (PartEquals(oldValue, i, oldValue.Length))
                {
                    Remove(i, oldValue.Length);
                    Insert(i, newValue);
                    i += oldValue.Length;
                }
                else
                {
                    i++;
                }
            }
        }

        public void Replace(char oldChar, char newChar, int startIndex, int count)
        {
            mStringBuilder = mStringBuilder.Replace(oldChar, newChar, startIndex, count);
        }

        private string TryGetFromBuffer(int startIndex, int length)
        {
            if (mBufferType == BetterStringBuilderBufferType.Both ||
                mBufferType == BetterStringBuilderBufferType.LocalOnly)
            {
                for (int i = 0; i < mLocalBuffer.Count; i++)
                {
                    if (PartEquals(mLocalBuffer[i], startIndex, length))
                    {
                        return mLocalBuffer[i];
                    }
                }
            }

            if (mBufferType == BetterStringBuilderBufferType.Both ||
                mBufferType == BetterStringBuilderBufferType.GlobalOnly)
            {
                for (int i = 0; i < mGlobalBuffer.Count; i++)
                {
                    if (PartEquals(mGlobalBuffer[i], startIndex, length))
                    {
                        return mGlobalBuffer[i];
                    }
                }
            }

            return null;
        }

        private void SaveToBuffer(string newString)
        {
            if (mBufferType == BetterStringBuilderBufferType.Both ||
                mBufferType == BetterStringBuilderBufferType.LocalOnly)
            {
                if (mLocalBuffer.Count < POOL_SIZE_LIMIT)
                {
                    mLocalBuffer.Add(newString);
                }
#if UNITY_EDITOR
                localDetecter.SetPooledObjectCount(mLocalBuffer.Count);
#endif
            }

            if (mBufferType == BetterStringBuilderBufferType.Both ||
                mBufferType == BetterStringBuilderBufferType.GlobalOnly)
            {
                if (mGlobalBuffer.Count < POOL_SIZE_LIMIT)
                {
                    mGlobalBuffer.Add(newString);
                }
#if UNITY_EDITOR
                globalDetecter.SetPooledObjectCount(mGlobalBuffer.Count);
#endif
            }
        }

        public override string ToString()
        {
            if (Length == 0) return string.Empty;
            string bufferStr = TryGetFromBuffer(0, Length);
            if (bufferStr != null)
            {
                return bufferStr;
            }

            // 说明：这里将分配新内存，分配以后手动托管
            string newString = mStringBuilder.ToString();
            SaveToBuffer(newString);
            return newString;
        }

        public string ToString(int startIndex, int length)
        {
            if (Length == 0) return string.Empty;
            string bufferStr = TryGetFromBuffer(startIndex, length);
            if (bufferStr != null)
            {
                return bufferStr;
            }

            string newString = mStringBuilder.ToString(startIndex, length);
            SaveToBuffer(newString);
            return newString;
        }

        // Add: to lower
        public void ToLower()
        {
            for (int i = 0; i < Length; i++)
            {
                if ((this[i] >= 'A') && (this[i] <= 'Z'))
                {
                    this[i] = (char)(this[i] + ('a' - 'A'));
                }
            }
        }

        // Add: to upper
        public void ToUpper()
        {
            for (int i = 0; i < Length; i++)
            {
                if ((this[i] >= 'a') && (this[i] <= 'z'))
                {
                    this[i] = (char)(this[i] + ('A' - 'a'));
                }
            }
        }
    }
}