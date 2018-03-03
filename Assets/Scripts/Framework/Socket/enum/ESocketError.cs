
public struct ESocketError
{
    public const int NORMAL = 0;//正常关闭，在连接前也会关一下
    public const int ERROR_1 = -1;
    public const int ERROR_2 = -2;
    public const int ERROR_3 = -3;//对方已经关闭链接了
    public const int ERROR_4 = -4;//发生了未知错误
    public const int ERROR_5 = -5;//主动断开连接

}
