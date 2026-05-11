// Copyright 2024-2026 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyScope : IDependencyScope, IDisposable, IAsyncDisposable
{
    internal readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
    internal bool IsDisposedInternal;
    private readonly DependencyLocatorStore _locatorStore;
    private Stack<DependencyDisposer>? _disposers;
    private int _level;

    protected DependencyScope(DependencyContainer container, DependencyScope? parentScope, string? name)
    {
        OriginalThreadId = Environment.CurrentManagedThreadId;
        Name = name;
        InternalContainer = container;
        InternalParentScope = parentScope;
        IsDisposedInternal = false;
        _disposers = null;
        ScopedInstancesByResolverId = null;
        _locatorStore = DependencyLocatorStore.Create( container.KeyedResolversStore, container.GlobalResolvers, this );
        FirstChild = null;
        LastChild = null;
        _level = -1;
    }

    public string? Name { get; }
    public int OriginalThreadId { get; }

    public bool IsDisposed
    {
        get
        {
            using ( ReadLockSlim.TryEnter( Lock, out var entered ) )
                return ! entered || IsDisposedInternal;
        }
    }

    public IDependencyContainer Container => InternalContainer;
    public IDependencyScope? ParentScope => InternalParentScope;
    public IDependencyLocator Locator => _locatorStore.Global;
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
    internal Dictionary<ulong, object>? ScopedInstancesByResolverId { get; private set; }
    internal ChildDependencyScope? FirstChild { get; private set; }
    internal ChildDependencyScope? LastChild { get; private set; }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public ValueTask DisposeAsync()
    {
        return InternalContainer.DisposeScopeAsync( this );
    }

    [Pure]
    public IDependencyScope[] GetChildren()
    {
        List<IDependencyScope> result;
        using ( ReadLockSlim.TryEnter( Lock, out var entered ) )
        {
            if ( ! entered )
                return Array.Empty<IDependencyScope>();

            var node = FirstChild;
            if ( node is null )
                return Array.Empty<IDependencyScope>();

            result = new List<IDependencyScope>();
            do
            {
                result.Add( node );
                node = node.NextSibling;
            }
            while ( node is not null );
        }

        return result.ToArray();
    }

    internal DependencyLocator<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull
    {
        return _locatorStore.GetOrCreate( key );
    }

    internal ChildDependencyScope BeginScope(string? name = null)
    {
        return InternalContainer.CreateChildScope( this, name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddChildCore(ChildDependencyScope child)
    {
        Assume.True( Lock.IsWriteLockHeld );

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
        Assume.True( Lock.IsWriteLockHeld );
        Assume.True( child.Lock.IsWriteLockHeld );

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
    internal void AddDisposer(DependencyDisposer disposer)
    {
        Assume.True( Lock.IsWriteLockHeld );
        Assume.False( IsDisposedInternal );
        _disposers ??= new Stack<DependencyDisposer>();
        _disposers.Push( disposer );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Dictionary<ulong, object> GetScopedInstancesByResolverId()
    {
        return ScopedInstancesByResolverId ??= new Dictionary<ulong, object>();
    }

    internal void MarkAsDisposed()
    {
        Assume.True( Lock.IsWriteLockHeld );
        Assume.False( IsDisposedInternal );
        IsDisposedInternal = true;
    }

    internal async ValueTask<Chain<OwnedDependencyDisposalException>> FinalizeDisposalAsync()
    {
        Stack<DependencyDisposer>? disposers;
        using ( WriteLockSlim.Enter( Lock ) )
        {
            Assume.True( IsDisposedInternal );
            disposers = _disposers;
            _disposers = null;
        }

        var exceptions = Chain<OwnedDependencyDisposalException>.Empty;
        if ( disposers is not null )
        {
            while ( disposers.TryPop( out var disposer ) )
            {
                var exception = disposer.IsAsync
                    ? await disposer.TryDisposeAsync().ConfigureAwait( false )
                    : disposer.TryDispose();

                if ( exception is not null )
                    exceptions = exceptions.Extend( new OwnedDependencyDisposalException( this, exception ) );
            }
        }

        using ( WriteLockSlim.Enter( Lock ) )
        {
            Assume.True( IsDisposedInternal );
            ScopedInstancesByResolverId = null;
            _locatorStore.Dispose();
            FirstChild = null;
            LastChild = null;
        }

        return exceptions;
    }

    IDependencyLocator<TKey> IDependencyScope.GetKeyedLocator<TKey>(TKey key)
    {
        return GetKeyedLocator( key );
    }

    IDependencyLocator IDependencyScope.GetTypeErasedKeyedLocator(object key)
    {
        return _locatorStore.GetOrCreateTypeErased( key );
    }

    IChildDependencyScope IDependencyScopeFactory.BeginScope(string? name)
    {
        return BeginScope( name );
    }
}
