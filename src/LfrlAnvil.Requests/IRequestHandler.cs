namespace LfrlAnvil.Requests;

public interface IRequestHandler<in TRequest, out TResult>
    where TRequest : IRequest<TRequest, TResult>
{
    TResult Handle(TRequest request);
}