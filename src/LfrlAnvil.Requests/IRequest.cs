namespace LfrlAnvil.Requests
{
    public interface IRequest<TRequest, TResult>
        where TRequest : IRequest<TRequest, TResult> { }
}
