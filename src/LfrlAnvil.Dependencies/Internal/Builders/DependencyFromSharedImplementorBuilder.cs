using System;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyFromSharedImplementorBuilder : IDependencyFromSharedImplementorBuilder
{
    private readonly Type _baseType;

    internal DependencyFromSharedImplementorBuilder(DependencyBuilder builder)
    {
        Assume.IsNotNull( builder.SharedImplementorKey, nameof( builder.SharedImplementorKey ) );
        Builder = builder;
        _baseType = builder.SharedImplementorKey.Type;
    }

    public Type DependencyType => Builder.DependencyType;
    public DependencyLifetime Lifetime => Builder.Lifetime;
    public ISharedDependencyImplementorKey? SharedImplementorKey => Builder.SharedImplementorKey;
    public IDependencyImplementorBuilder? Implementor => Builder.Implementor;
    internal DependencyBuilder Builder { get; }

    public IDependencyBuilder Keyed<TKey>(TKey key)
        where TKey : notnull
    {
        if ( Builder.InternalSharedImplementorKey is not null )
            Builder.InternalSharedImplementorKey = new SharedDependencyImplementorKey<TKey>( _baseType, key );

        return Builder;
    }

    public IDependencyBuilder NotKeyed()
    {
        if ( Builder.InternalSharedImplementorKey is not null && Builder.InternalSharedImplementorKey.IsKeyed )
            Builder.InternalSharedImplementorKey = new SharedDependencyImplementorKey( _baseType );

        return Builder;
    }

    public IDependencyBuilder SetLifetime(DependencyLifetime lifetime)
    {
        return Builder.SetLifetime( lifetime );
    }

    public IDependencyFromSharedImplementorBuilder FromSharedImplementor(Type type)
    {
        return Builder.FromSharedImplementor( type );
    }

    public IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory)
    {
        return Builder.FromFactory( factory );
    }
}
