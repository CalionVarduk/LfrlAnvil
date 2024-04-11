﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyScope : IDependencyScope, IDisposable
{
    private int _level;

    protected DependencyScope(DependencyContainer container, DependencyScope? parentScope, string? name)
    {
        OriginalThreadId = Environment.CurrentManagedThreadId;
        Name = name;
        InternalContainer = container;
        InternalParentScope = parentScope;
        IsDisposed = false;
        InternalDisposers = new List<DependencyDisposer>();
        ScopedInstancesByResolverId = new Dictionary<ulong, object>();
        InternalLocatorStore = DependencyLocatorStore.Create( container.KeyedResolversStore, container.GlobalResolvers, this );
        FirstChild = null;
        LastChild = null;
        _level = -1;
    }

    public string? Name { get; }
    public int OriginalThreadId { get; }
    public bool IsDisposed { get; private set; }
    public IDependencyContainer Container => InternalContainer;
    public IDependencyScope? ParentScope => InternalParentScope;
    public IDependencyLocator Locator => InternalLocatorStore.Global;
    public bool IsRoot => InternalParentScope is null;

    public int Level
    {
        get
        {
            if ( _level == -1 )
                _level = InternalParentScope is null ? 0 : InternalParentScope.Level + 1;

            return _level;
        }
    }

    internal DependencyContainer InternalContainer { get; }
    internal DependencyScope? InternalParentScope { get; }
    internal DependencyLocatorStore InternalLocatorStore { get; }
    internal List<DependencyDisposer> InternalDisposers { get; }
    internal Dictionary<ulong, object> ScopedInstancesByResolverId { get; }
    internal ChildDependencyScope? FirstChild { get; private set; }
    internal ChildDependencyScope? LastChild { get; private set; }

    public void Dispose()
    {
        InternalContainer.DisposeScope( this );
    }

    [Pure]
    public IDependencyScope[] GetChildren()
    {
        return InternalContainer.GetScopeChildren( this );
    }

    internal DependencyLocator<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull
    {
        return InternalContainer.GetOrCreateKeyedLocator( this, key );
    }

    internal ChildDependencyScope BeginScope(string? name = null)
    {
        return InternalContainer.CreateChildScope( this, name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddChild(ChildDependencyScope child)
    {
        if ( LastChild is null )
        {
            FirstChild = child;
            LastChild = child;
        }
        else
        {
            Assume.IsNull( LastChild.NextSibling );
            LastChild.NextSibling = child;
            child.PrevSibling = LastChild;
            LastChild = child;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveChild(ChildDependencyScope child)
    {
        Assume.Equals( this, child.InternalParentScope );
        Assume.IsNotNull( FirstChild );
        Assume.IsNotNull( LastChild );

        if ( ReferenceEquals( FirstChild, LastChild ) )
        {
            Assume.Equals( FirstChild, child );
            FirstChild = null;
            LastChild = null;
            return;
        }

        if ( ReferenceEquals( FirstChild, child ) )
        {
            Assume.IsNotNull( FirstChild.NextSibling );
            FirstChild.NextSibling.PrevSibling = null;
            FirstChild = FirstChild.NextSibling;
            return;
        }

        if ( ReferenceEquals( LastChild, child ) )
        {
            Assume.IsNotNull( LastChild.PrevSibling );
            LastChild.PrevSibling.NextSibling = null;
            LastChild = LastChild.PrevSibling;
            return;
        }

        Assume.IsNotNull( child.PrevSibling );
        Assume.IsNotNull( child.NextSibling );
        child.PrevSibling.NextSibling = child.NextSibling;
        child.NextSibling.PrevSibling = child.PrevSibling;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsDisposed()
    {
        FirstChild = null;
        LastChild = null;
        IsDisposed = true;
    }

    internal Chain<OwnedDependencyDisposalException> DisposeInstances()
    {
        var exceptions = Chain<OwnedDependencyDisposalException>.Empty;

        foreach ( var disposer in InternalDisposers )
        {
            var exception = disposer.TryDispose();
            if ( exception is not null )
                exceptions = exceptions.Extend( new OwnedDependencyDisposalException( this, exception ) );
        }

        InternalDisposers.Clear();
        ScopedInstancesByResolverId.Clear();
        InternalLocatorStore.Clear();
        return exceptions;
    }

    IDependencyLocator<TKey> IDependencyScope.GetKeyedLocator<TKey>(TKey key)
    {
        return GetKeyedLocator( key );
    }

    IChildDependencyScope IDependencyScope.BeginScope(string? name)
    {
        return BeginScope( name );
    }
}
