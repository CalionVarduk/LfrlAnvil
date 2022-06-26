using System;
using System.Collections.Generic;

namespace LfrlAnvil.Requests;

public sealed class RequestHandlerFactory : IRequestHandlerFactory
{
    private readonly Dictionary<Type, Delegate> _factories;

    public RequestHandlerFactory()
    {
        _factories = new Dictionary<Type, Delegate>();
    }

    public RequestHandlerFactory Register<TRequest, TResult>(Func<IRequestHandler<TRequest, TResult>> factory)
        where TRequest : IRequest<TRequest, TResult>
    {
        _factories[typeof( TRequest )] = factory;
        return this;
    }

    public IRequestHandler<TRequest, TResult>? TryCreate<TRequest, TResult>()
        where TRequest : IRequest<TRequest, TResult>
    {
        return _factories.TryGetValue( typeof( TRequest ), out var factory )
            ? ((Func<IRequestHandler<TRequest, TResult>>)factory)()
            : null;
    }
}
