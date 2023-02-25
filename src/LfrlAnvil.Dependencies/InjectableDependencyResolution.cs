using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

public readonly struct InjectableDependencyResolution<T>
    where T : class, ICustomAttributeProvider
{
    private InjectableDependencyResolution(
        Func<T, bool> predicate,
        Expression<Func<IDependencyScope, object>>? factory,
        IDependencyKey? implementorKey)
    {
        Predicate = predicate;
        Factory = factory;
        ImplementorKey = implementorKey;
    }

    public Func<T, bool> Predicate { get; }
    public Expression<Func<IDependencyScope, object>>? Factory { get; }
    public IDependencyKey? ImplementorKey { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static InjectableDependencyResolution<T> FromFactory(Func<T, bool> predicate, Expression<Func<IDependencyScope, object>> factory)
    {
        return new InjectableDependencyResolution<T>( predicate, factory, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static InjectableDependencyResolution<T> FromImplementorKey(Func<T, bool> predicate, IDependencyKey implementorKey)
    {
        return new InjectableDependencyResolution<T>( predicate, null, implementorKey );
    }
}
