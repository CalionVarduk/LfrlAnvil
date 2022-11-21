using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyConstructorParameterResolutionOptions : IDependencyConstructorParameterResolutionOptions
{
    internal DependencyConstructorParameterResolutionOptions(DependencyLocatorBuilder locatorBuilder, Func<ParameterInfo, bool> predicate)
    {
        LocatorBuilder = locatorBuilder;
        Predicate = predicate;
    }

    internal DependencyLocatorBuilder LocatorBuilder { get; }
    internal Func<ParameterInfo, bool> Predicate { get; }
    internal Expression<Func<IDependencyScope, object>>? Factory { get; private set; }
    internal IInternalDependencyImplementorKey? ImplementorKey { get; private set; }

    public void FromFactory(Expression<Func<IDependencyScope, object>> factory)
    {
        Factory = factory;
        ImplementorKey = null;
    }

    public void FromImplementor(Type type, Action<IDependencyImplementorOptions>? configuration = null)
    {
        ImplementorKey = DependencyImplementorOptions.CreateImplementorKey( LocatorBuilder.CreateImplementorKey( type ), configuration );
        Factory = null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyConstructorParameterResolution CreateResolution()
    {
        if ( Factory is not null )
            return DependencyConstructorParameterResolution.FromFactory( Predicate, Factory );

        if ( ImplementorKey is not null )
            return DependencyConstructorParameterResolution.FromImplementorKey( Predicate, ImplementorKey );

        return DependencyConstructorParameterResolution.Unspecified( Predicate );
    }
}
