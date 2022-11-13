using System;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal abstract class DependencyResolver
{
    private bool _isResolving;

    protected DependencyResolver(ulong id, Type implementorType)
    {
        Id = id;
        ImplementorType = implementorType;
        _isResolving = false;
    }

    internal ulong Id { get; }
    public Type ImplementorType { get; }

    internal object Create(DependencyScope scope, Type dependencyType)
    {
        if ( _isResolving )
            throw new CircularDependencyReferenceException( dependencyType, ImplementorType );

        _isResolving = true;
        try
        {
            return CreateInternal( scope );
        }
        catch ( CircularDependencyReferenceException exc )
        {
            throw new CircularDependencyReferenceException( dependencyType, ImplementorType, exc );
        }
        finally
        {
            _isResolving = false;
        }
    }

    protected abstract object CreateInternal(DependencyScope scope);
}
