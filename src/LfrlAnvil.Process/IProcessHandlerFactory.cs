namespace LfrlAnvil.Process
{
    public interface IProcessHandlerFactory
    {
        IProcessHandler<TArgs, TResult>? TryCreate<TArgs, TResult>()
            where TArgs : IProcessArgs<TResult>;

        IAsyncProcessHandler<TArgs, TResult>? TryCreateAsync<TArgs, TResult>()
            where TArgs : IProcessArgs<TResult>;
    }
}
