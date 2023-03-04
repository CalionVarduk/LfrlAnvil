using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyRangeBuilder : IDependencyRangeBuilder
{
    internal DependencyRangeBuilder(DependencyLocatorBuilder locatorBuilder, Type dependencyType)
    {
        Elements = new List<DependencyBuilder>();
        DependencyType = dependencyType;
        LocatorBuilder = locatorBuilder;
        OnResolvingCallback = null;
    }

    public Type DependencyType { get; }
    public Action<Type, IDependencyScope>? OnResolvingCallback { get; private set; }
    public IDependencyBuilder this[int index] => Elements[index];
    public int Count => Elements.Count;
    internal List<DependencyBuilder> Elements { get; }
    internal DependencyLocatorBuilder LocatorBuilder { get; }

    public IDependencyBuilder Add()
    {
        var result = new DependencyBuilder( this );
        Elements.Add( result );
        return result;
    }

    [Pure]
    public IDependencyBuilder? TryGetLast()
    {
        return Elements.Count > 0 ? Elements[^1] : null;
    }

    public IDependencyRangeBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback)
    {
        OnResolvingCallback = callback;
        return this;
    }

    [Pure]
    public IEnumerator<IDependencyBuilder> GetEnumerator()
    {
        return Elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
