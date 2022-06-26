using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Requests;

public interface IRequestDispatcher
{
    TResult Dispatch<TRequest, TResult>(IRequest<TRequest, TResult> request)
        where TRequest : class, IRequest<TRequest, TResult>;

    TResult Dispatch<TRequest, TResult>(TRequest request)
        where TRequest : struct, IRequest<TRequest, TResult>;

    bool TryDispatch<TRequest, TResult>(IRequest<TRequest, TResult> request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : class, IRequest<TRequest, TResult>;

    bool TryDispatch<TRequest, TResult>(TRequest request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : struct, IRequest<TRequest, TResult>;
}
