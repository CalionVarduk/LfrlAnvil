namespace LfrlAnvil.Requests;

public interface IRequestHandlerFactory
{
    IRequestHandler<TRequest, TResult>? TryCreate<TRequest, TResult>()
        where TRequest : IRequest<TRequest, TResult>;
}
