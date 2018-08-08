
/// <summary>
/// 说明：proto网络数据缓存池需要实现的接口
/// 
/// @by wsh 2017-07-01
/// </summary>

public interface IProtoPool
{
    // 获取数据
    object Get();

    // 回收数据
    void Recycle(object data);

    // 清除指定数据
    void ClearData(object data);
    
    // 深拷贝指定数据
    object DeepCopy(object data);
    
    // 释放缓存池
    void Dispose();
}
