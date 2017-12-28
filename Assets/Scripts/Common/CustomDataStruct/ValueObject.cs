using System;
using System.Collections.Generic;

/// <summary>
/// 说明：值类型object，无GC，消除值类型作为object传参导致的装箱的问题，多线程安全
/// 
/// by wsh @ 2017-06-29
/// </summary>

namespace CustomDataStruct
{
    public sealed class ValueObject : IRelease, IDisposable
    {
        static Queue<ValueObject> mValueObjPool = new Queue<ValueObject>(32);

        object mTypeObj;
        ObjPoolBase mTypeObjHolder;
        Type mObjType;

#if UNITY_EDITOR
        static MemoryLeakDetecter detecter = MemoryLeakDetecter.Add(typeof(ValueObject).FullName, 1000, 1000);
#endif

        private ValueObject()
        {
        }
        
        /// <summary>
        /// 如果valueObj为Object装箱后的值，则将其拆箱并装入ValueObject
        /// 如果valueObj本身就是引用类型，则返回它本身
        /// </summary>
        /// <param name="valueObj">任意object对象</param>
        /// <returns></returns>
        static public object TryGet(object valueObj)
        {
            Type type = valueObj.GetType();
            if (type.IsValueType)
            {
                return Get(valueObj);
            }
            else
            {
                return valueObj;
            }
        }
        
        /// <summary>
        /// 将Object装箱后的值拆箱并装入ValueObject
        /// </summary>
        /// <param name="valueObj">Object装箱后的值</param>
        /// <returns></returns>
        static public ValueObject Get(object valueObj)
        {
#if UNITY_EDITOR
            if (!valueObj.GetType().IsValueType) throw new InvalidOperationException(
                string.Format("Only value type supported! type = <{0}>", valueObj.GetType()));
#endif
            Type type = valueObj.GetType();
            if (type == typeof(TimeSpan)) return Get((TimeSpan)valueObj);
            if (type == typeof(Guid)) return Get((Guid)valueObj);

            TypeCode code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Int32:
                    return Get((int)valueObj);
                case TypeCode.UInt32:
                    return Get((uint)valueObj);
                case TypeCode.Int64:
                    return Get((long)valueObj);
                case TypeCode.UInt64:
                    return Get((ulong)valueObj);
                case TypeCode.Single:
                    return Get((float)valueObj);
                case TypeCode.Double:
                    return Get((double)valueObj);
                case TypeCode.Boolean:
                    return Get((bool)valueObj);
                case TypeCode.DateTime:
                    return Get((DateTime)valueObj);
                case TypeCode.Decimal:
                    return Get((decimal)valueObj);
                case TypeCode.Byte:
                    return Get((byte)valueObj);
                case TypeCode.SByte:
                    return Get((sbyte)valueObj);
                case TypeCode.Char:
                    return Get((char)valueObj);
                case TypeCode.Int16:
                    return Get((short)valueObj);
                case TypeCode.UInt16:
                    return Get((ushort)valueObj);
            }

            throw new InvalidCastException(string.Format("Unknow type <{0}>", type));
        }

        /// <summary>
        /// 将指定值装入ValueObject
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="value">要装入的值</param>
        /// <returns></returns>
        static public ValueObject Get<T>(T value)
        {
#if UNITY_EDITOR
            if (!typeof(T).IsValueType) throw new InvalidOperationException(
                string.Format("Only value type supported! type = <{0}>", typeof(T)));
#endif

            // 装箱
            TypeObj<T> typeObj = ObjPool<TypeObj<T>>.Get();
            typeObj.value = value;

            ValueObject betterObj = null;
            lock (mValueObjPool)
            {
#if UNITY_EDITOR
                detecter.IncreseInstance();
#endif
                betterObj = mValueObjPool.Count > 0 ? mValueObjPool.Dequeue() : new ValueObject();
            }
            betterObj.mObjType = typeof(T);
            betterObj.mTypeObj = typeObj;
            betterObj.mTypeObjHolder = ObjPool<TypeObj<T>>.instance;
            return betterObj;
        }

        public Type ValueType
        {
            get
            {
                return mObjType;
            }
        }

        /// <summary>
        /// 拆箱为所指定的类型数据，不释放缓存
        /// </summary>
        /// <typeparam name="T">要拆箱的数据类型</typeparam>
        /// <returns></returns>
        public T Value<T>()
        {
#if UNITY_EDITOR
            if (!typeof(T).IsValueType) throw new InvalidOperationException(
                string.Format("Only value type supported! type = <{0}>", typeof(T)));
#endif
            // 拆箱
            if (typeof(T) != mObjType)
            {
                throw new InvalidCastException(string.Format("ExceptedType = <{0}>, TryGetType = <{1}>",
                    mObjType, typeof(T)));
            }
            TypeObj<T> target = mTypeObj as TypeObj<T>;
            return target.value;
        }

        /// <summary>
        /// 如果obj为指定类型经Object装箱而来，则对其执行拆箱
        /// 如果obj为ValueObject类型，则将其拆箱为所指定的类型数据，同时释放缓存
        /// 否则，抛出异常
        /// </summary>
        /// <typeparam name="T">要拆箱的数据类型</typeparam>
        /// <param name="obj">待拆箱的对象</param>
        /// <returns></returns>
        public static T Value<T>(object obj)
        {
            if (obj.GetType() == typeof(T))
            {
                return (T)obj;
            }
            using (ValueObject valObj = obj as ValueObject)
            {
                if (valObj == null)
                {
                    throw new InvalidCastException(string.Format("ExceptedType = <{0}>, ObjectType = <{1}>",
                        typeof(ValueObject), obj.GetType()));
                }
                return valObj.Value<T>();
            }
        }

        /// <summary>
        /// ValueObject转Object、不释放缓存（注意：有GC）
        /// </summary>
        /// <returns></returns>
        public object ToObject()
        {
            // Object装箱
            TypeObjBase target = mTypeObj as TypeObjBase;
            if (target == null) throw new InvalidCastException();
            return target.ObjectValue();
        }
        
        /// <summary>
        /// 如果obj为ValueObject类型，则将其拆箱并转换为Object，同时释放缓存（注意：有GC）
        /// 否则，返回其本身
        /// </summary>
        /// <param name="obj">任意类型object</param>
        /// <returns></returns>
        public static object ToObject(object obj)
        {
            using (ValueObject valueObj = obj as ValueObject)
            {
                if (valueObj != null) obj = valueObj.ToObject();
                return obj;
            }
        }

        public void Release()
        {
            if (mTypeObjHolder != null && mTypeObj != null)
            {
                mTypeObjHolder.Release(mTypeObj);
            }
            mTypeObj = null;
            mTypeObjHolder = null;
            lock (mValueObjPool)
            {
                mValueObjPool.Enqueue(this);
#if UNITY_EDITOR
                detecter.DecreseInstance();
                detecter.SetPooledObjectCount(mValueObjPool.Count);
#endif
            }
        }

        public void Dispose()
        {
            Release();
        }

        public static void Cleanup()
        {
            lock (mValueObjPool)
            {
                mValueObjPool.Clear();
            }
        }

        private abstract class TypeObjBase
        {
            public abstract object ObjectValue();
        }

        // 免GC装箱
        private sealed class TypeObj<T> : TypeObjBase, IRelease
        {
            public T value;
            public override object ObjectValue()
            {
                return value;
            }
            public void Release() { value = default(T); }
        }
    }
}
