namespace LfrlAnvil.Process
{
    public interface IProcessHandler<in TArgs, out TResult>
        where TArgs : IProcessArgs<TResult>
    {
        TResult Handle(TArgs args);
    }
}
