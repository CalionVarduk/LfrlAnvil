using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyRangeBuilder : IDependencyRangeBuilder
{
    internal DependencyRangeBuilder(DependencyLocatorBuilder locatorBuilder, Type dependencyType)
    {
        InternalElements = new List<DependencyBuilder>();
        DependencyType = dependencyType;
        LocatorBuilder = locatorBuilder;
        OnResolvingCallback = null;
    }

    public Type DependencyType { get; }
    public Action<Type, IDependencyScope>? OnResolvingCallback { get; private set; }
    public IReadOnlyList<IDependencyBuilder> Elements => InternalElements;
    internal List<DependencyBuilder> InternalElements { get; }
    internal DependencyLocatorBuilder LocatorBuilder { get; }

    public IDependencyBuilder Add()
    {
        var result = new DependencyBuilder( this );
        InternalElements.Add( result );
        return result;
    }

    [Pure]
    public IDependencyBuilder? TryGetLast()
    {
        return InternalElements.Count > 0 ? InternalElements[^1] : null;
    }

    public IDependencyRangeBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback)
    {
        OnResolvingCallback = callback;
        return this;
    }
}
