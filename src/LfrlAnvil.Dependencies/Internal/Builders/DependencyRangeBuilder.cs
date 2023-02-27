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
    }

    public Type DependencyType { get; }
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
    public IEnumerator<IDependencyBuilder> GetEnumerator()
    {
        return Elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
