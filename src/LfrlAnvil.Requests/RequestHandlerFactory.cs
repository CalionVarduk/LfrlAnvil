using System;
using System.Collections.Generic;

namespace LfrlAnvil.Requests;

/// <inheritdoc />
public sealed class RequestHandlerFactory : IRequestHandlerFactory
{
    private readonly Dictionary<Type, Delegate> _factories;

    /// <summary>
    /// Creates a new empty <see cref="RequestHandlerFactory"/> instance.
    /// </summary>
    public RequestHandlerFactory()
    {
        _factories = new Dictionary<Type, Delegate>();
    }

    /// <summary>
    /// Sets a request handler <paramref name="factory"/> for the given type of <typeparamref name="TRequest"/>.
    /// </summary>
    /// <param name="factory">Request handler factory.</param>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResult">Request's result type.</typeparam>
    /// <returns><b>this</b>.</returns>
    public RequestHandlerFactory Register<TRequest, TResult>(Func<IRequestHandler<TRequest, TResult>> factory)
        where TRequest : IRequest<TRequest, TResult>
    {
        _factories[typeof( TRequest )] = factory;
        return this;
    }

    /// <inheritdoc />
    public IRequestHandler<TRequest, TResult>? TryCreate<TRequest, TResult>()
        where TRequest : IRequest<TRequest, TResult>
    {
        return _factories.TryGetValue( typeof( TRequest ), out var factory )
            ? (( Func<IRequestHandler<TRequest, TResult>> )factory)()
            : null;
    }
}
