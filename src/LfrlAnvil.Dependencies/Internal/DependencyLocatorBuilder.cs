using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyLocatorBuilder : IDependencyLocatorBuilder
{
    internal DependencyLocatorBuilder()
    {
        DefaultLifetime = DependencyLifetime.Transient;
        SharedImplementors = new Dictionary<Type, DependencyImplementorBuilder>();
        Dependencies = new Dictionary<Type, DependencyBuilder>();
    }

    public DependencyLifetime DefaultLifetime { get; private set; }
    internal Dictionary<Type, DependencyImplementorBuilder> SharedImplementors { get; }
    internal Dictionary<Type, DependencyBuilder> Dependencies { get; }

    public IDependencyImplementorBuilder AddSharedImplementor(Type type)
    {
        if ( ! SharedImplementors.TryGetValue( type, out var result ) )
        {
            Ensure.Equals( type.IsGenericTypeDefinition, false, nameof( type ) + '.' + nameof( type.IsGenericTypeDefinition ) );
            result = new DependencyImplementorBuilder( type );
            SharedImplementors.Add( type, result );
        }

        return result;
    }

    public IDependencyBuilder Add(Type type)
    {
        Ensure.Equals( type.IsGenericTypeDefinition, false, nameof( type ) + '.' + nameof( type.IsGenericTypeDefinition ) );
        var dependency = new DependencyBuilder( type, DefaultLifetime );
        Dependencies[type] = dependency;
        return dependency;
    }

    public IDependencyLocatorBuilder SetDefaultLifetime(DependencyLifetime lifetime)
    {
        Ensure.IsDefined( lifetime, nameof( lifetime ) );
        DefaultLifetime = lifetime;
        return this;
    }

    [Pure]
    public IDependencyImplementorBuilder? TryGetSharedImplementor(Type type)
    {
        return SharedImplementors.GetValueOrDefault( type );
    }

    [Pure]
    public IDependencyBuilder? TryGetDependency(Type type)
    {
        return Dependencies.GetValueOrDefault( type );
    }
}
