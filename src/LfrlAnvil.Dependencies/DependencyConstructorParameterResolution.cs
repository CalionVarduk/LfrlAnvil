using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

public readonly struct DependencyConstructorParameterResolution
{
    private DependencyConstructorParameterResolution(
        Func<ParameterInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>>? factory,
        IDependencyImplementorKey? implementorKey)
    {
        Predicate = predicate;
        Factory = factory;
        ImplementorKey = implementorKey;
    }

    public Func<ParameterInfo, bool> Predicate { get; }
    public Expression<Func<IDependencyScope, object>>? Factory { get; }
    public IDependencyImplementorKey? ImplementorKey { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyConstructorParameterResolution FromFactory(
        Func<ParameterInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory)
    {
        return new DependencyConstructorParameterResolution( predicate, factory, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyConstructorParameterResolution FromImplementorKey(
        Func<ParameterInfo, bool> predicate,
        IDependencyImplementorKey implementorKey)
    {
        return new DependencyConstructorParameterResolution( predicate, null, implementorKey );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyConstructorParameterResolution Unspecified(Func<ParameterInfo, bool> predicate)
    {
        return new DependencyConstructorParameterResolution( predicate, null, null );
    }
}
