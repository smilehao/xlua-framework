namespace CustomDataStruct
{
    public delegate void DelegateCallback();
    public delegate void DelegateCallback<T>(T arg1);
    public delegate void DelegateCallback<T, U>(T arg1, U arg2);
    public delegate void DelegateCallback<T, U, V>(T arg1, U arg2, V arg3);
    public delegate void DelegateCallback<T, U, V, J>(T arg1, U arg2, V arg3, J arg4);
    public delegate void DelegateCallback<T, U, V, J, K>(T arg1, U arg2, V arg3, J arg4, K arg5);
    public delegate void DelegateCallback<T, U, V, J, K, M>(T arg1, U arg2, V arg3, J arg4, K arg5, M arg6);
}