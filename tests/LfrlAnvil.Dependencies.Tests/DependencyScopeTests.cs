using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Dependencies.Tests;

public class DependencyScopeTests : DependencyTestsBase
{
    [Fact]
    public void ToString_ForRootScope_ShouldReturnCorrectResult()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        var result = sut.ToString();

        result.Should().Be( $"RootDependencyScope [Level: 0, OriginalThreadId: {threadId}]" );
    }

    [Fact]
    public void ToString_ForChildScope_ShouldReturnCorrectResult()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();

        var result = sut.ToString();

        result.Should().Be( $"ChildDependencyScope [Level: 1, OriginalThreadId: {threadId}]" );
    }

    [Fact]
    public void ToString_ForNamedChildScope_ShouldReturnCorrectResult()
    {
        var name = Fixture.Create<string>();
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( name );

        var result = sut.ToString();

        result.Should().Be( $"ChildDependencyScope [Name: '{name}', Level: 1, OriginalThreadId: {threadId}]" );
    }

    [Fact]
    public void ToString_ForGrandchildScope_ShouldReturnCorrectResult()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope().BeginScope();

        var result = sut.ToString();

        result.Should().Be( $"ChildDependencyScope [Level: 2, OriginalThreadId: {threadId}]" );
    }

    [Fact]
    public void RootScope_ShouldHaveCorrectProperties()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        using ( new AssertionScope() )
        {
            sut.Container.Should().BeSameAs( container );
            sut.Level.Should().Be( 0 );
            sut.Locator.AttachedScope.Should().BeSameAs( sut );
            sut.Locator.Key.Should().BeNull();
            sut.Locator.KeyType.Should().BeNull();
            sut.Locator.IsKeyed.Should().BeFalse();
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeTrue();
            sut.ParentScope.Should().BeNull();
            sut.OriginalThreadId.Should().Be( threadId );
            sut.Name.Should().BeNull();
            sut.GetChildren().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void GetKeyedLocator_ShouldReturnLocatorWithCorrectKey(int key)
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        var result = sut.GetKeyedLocator( key );

        using ( new AssertionScope() )
        {
            result.Key.Should().Be( key );
            (( IDependencyLocator )result).Key.Should().Be( key );
            result.KeyType.Should().Be( typeof( int ) );
            result.IsKeyed.Should().BeTrue();
            result.AttachedScope.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void GetKeyedLocator_ShouldReturnCorrectCachedLocator_WhenCalledMoreThanOnceWithTheSameKey()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        var result1 = sut.GetKeyedLocator( 1 );
        var result2 = sut.GetKeyedLocator( 1 );

        result1.Should().BeSameAs( result2 );
    }

    [Fact]
    public void GetKeyedLocator_ShouldThrowObjectDisposedException_WhenScopeIsDisposed()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;
        container.Dispose();

        var action = Lambda.Of( () => sut.GetKeyedLocator( 1 ) );

        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    [Fact]
    public void BeginScope_ThroughRootScope_ShouldCreateChildScope()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();

        using ( new AssertionScope() )
        {
            sut.Container.Should().BeSameAs( container );
            sut.Level.Should().Be( 1 );
            sut.Locator.AttachedScope.Should().BeSameAs( sut );
            sut.Locator.Key.Should().BeNull();
            sut.Locator.KeyType.Should().BeNull();
            sut.Locator.IsKeyed.Should().BeFalse();
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeFalse();
            sut.ParentScope.Should().BeSameAs( container.RootScope );
            sut.OriginalThreadId.Should().Be( threadId );
            sut.Name.Should().BeNull();
            container.RootScope.GetChildren().Should().BeSequentiallyEqualTo( sut );
            sut.GetChildren().Should().BeEmpty();
        }
    }

    [Fact]
    public void BeginScope_ThroughRootScope_ShouldCreateNamedChildScope()
    {
        var name = Fixture.Create<string>();
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( name );

        using ( new AssertionScope() )
        {
            sut.Container.Should().BeSameAs( container );
            sut.Level.Should().Be( 1 );
            sut.Locator.AttachedScope.Should().BeSameAs( sut );
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeFalse();
            sut.ParentScope.Should().BeSameAs( container.RootScope );
            sut.OriginalThreadId.Should().Be( threadId );
            sut.Name.Should().Be( name );
            container.RootScope.GetChildren().Should().BeSequentiallyEqualTo( sut );
            sut.GetChildren().Should().BeEmpty();
        }
    }

    [Fact]
    public void BeginScope_CalledMultipleTimesFromRootScope_ShouldAttachCreatedScopesAsRootScopeChildren()
    {
        var container = new DependencyContainerBuilder().Build();

        var child1 = container.RootScope.BeginScope();
        var child2 = container.RootScope.BeginScope();
        var child3 = container.RootScope.BeginScope();

        container.RootScope.GetChildren().Should().BeSequentiallyEqualTo( child1, child2, child3 );
    }

    [Fact]
    public void BeginScope_ThroughChildScope_ShouldCreateChildScope()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();

        using ( new AssertionScope() )
        {
            sut.Container.Should().BeSameAs( container );
            sut.Level.Should().Be( 2 );
            sut.Locator.AttachedScope.Should().BeSameAs( sut );
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeFalse();
            sut.ParentScope.Should().BeSameAs( parent );
            sut.OriginalThreadId.Should().Be( threadId );
            sut.Name.Should().BeNull();
            parent.GetChildren().Should().BeSequentiallyEqualTo( sut );
            sut.GetChildren().Should().BeEmpty();
        }
    }

    [Fact]
    public void BeginScope_ThroughChildScope_ShouldCreateNamedChildScope()
    {
        var name = Fixture.Create<string>();
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope( name );

        using ( new AssertionScope() )
        {
            sut.Container.Should().BeSameAs( container );
            sut.Level.Should().Be( 2 );
            sut.Locator.AttachedScope.Should().BeSameAs( sut );
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeFalse();
            sut.ParentScope.Should().BeSameAs( parent );
            sut.OriginalThreadId.Should().Be( threadId );
            sut.Name.Should().Be( name );
            parent.GetChildren().Should().BeSequentiallyEqualTo( sut );
            sut.GetChildren().Should().BeEmpty();
        }
    }

    [Fact]
    public void BeginScope_CalledMultipleTimesFromChildScope_ShouldAttachCreatedScopesAsRootScopeChildren()
    {
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();

        var child1 = parent.BeginScope();
        var child2 = parent.BeginScope();
        var child3 = parent.BeginScope();

        parent.GetChildren().Should().BeSequentiallyEqualTo( child1, child2, child3 );
    }

    [Fact]
    public void BeginScope_ShouldCreateChildScope_WhenChildScopeHasItsOwnChildren()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;
        var child = sut.BeginScope();
        var grandchild = child.BeginScope();

        var result = sut.BeginScope();

        using ( new AssertionScope() )
        {
            sut.GetChildren().Should().BeSequentiallyEqualTo( child, result );
            child.GetChildren().Should().BeSequentiallyEqualTo( grandchild );
        }
    }

    [Fact]
    public async Task BeginScope_ShouldCreateChildScope_WhenParentScopeOriginatedFromDifferentThread()
    {
        var context = new DedicatedThreadSynchronizationContext();
        var taskFactory = new TaskFactory( TaskSchedulerCapture.FromSynchronizationContext( context ) );
        var container = new DependencyContainerBuilder().Build();
        var sut = await taskFactory.StartNew( () => container.RootScope.BeginScope() );
        var threadId = Environment.CurrentManagedThreadId;

        var result = sut.BeginScope();

        using ( new AssertionScope() )
        {
            result.Container.Should().BeSameAs( container );
            result.Level.Should().Be( 2 );
            result.Locator.AttachedScope.Should().BeSameAs( result );
            result.IsDisposed.Should().BeFalse();
            result.IsRoot.Should().BeFalse();
            result.ParentScope.Should().BeSameAs( sut );
            result.OriginalThreadId.Should().Be( threadId );
            sut.GetChildren().Should().BeSequentiallyEqualTo( result );
            result.GetChildren().Should().BeEmpty();
        }
    }

    [Fact]
    public void BeginScope_ShouldThrowObjectDisposedException_WhenScopeIsDisposed()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();
        sut.Dispose();

        var action = Lambda.Of( () => sut.BeginScope() );

        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    [Fact]
    public void BeginScope_ShouldThrowNamedDependencyScopeCreationException_WhenScopeWithProvidedNameAlreadyExists()
    {
        var name = Fixture.Create<string>();
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( name );

        var action = Lambda.Of( () => sut.BeginScope( name ) );

        action.Should().ThrowExactly<NamedDependencyScopeCreationException>().AndMatch( e => e.ParentScope == sut && e.Name == name );
    }

    [Fact]
    public void Dispose_ShouldDisposeChildScopeAndItsDescendants()
    {
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();
        var child1 = sut.BeginScope();
        var child2 = sut.BeginScope();
        var child3 = sut.BeginScope();
        var grandchild1 = child1.BeginScope();
        var grandchild2 = child1.BeginScope();
        var grandchild3 = child3.BeginScope();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeTrue();
            child1.IsDisposed.Should().BeTrue();
            child2.IsDisposed.Should().BeTrue();
            child3.IsDisposed.Should().BeTrue();
            grandchild1.IsDisposed.Should().BeTrue();
            grandchild2.IsDisposed.Should().BeTrue();
            grandchild3.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeContainer_WhenScopeIsRoot()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = ( IDisposable )container.RootScope;

        sut.Dispose();

        container.RootScope.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenScopeIsAlreadyDisposed()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();
        sut.Dispose();

        sut.Dispose();

        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldDisposeFirstChildScopeCorrectly()
    {
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();
        var child1 = sut.BeginScope();
        var child2 = sut.BeginScope();
        var child3 = sut.BeginScope();

        child1.Dispose();

        using ( new AssertionScope() )
        {
            child1.IsDisposed.Should().BeTrue();
            child2.IsDisposed.Should().BeFalse();
            child3.IsDisposed.Should().BeFalse();
            sut.GetChildren().Should().BeSequentiallyEqualTo( child2, child3 );
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeLastChildScopeCorrectly()
    {
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();
        var child1 = sut.BeginScope();
        var child2 = sut.BeginScope();
        var child3 = sut.BeginScope();

        child3.Dispose();

        using ( new AssertionScope() )
        {
            child3.IsDisposed.Should().BeTrue();
            child1.IsDisposed.Should().BeFalse();
            child2.IsDisposed.Should().BeFalse();
            sut.GetChildren().Should().BeSequentiallyEqualTo( child1, child2 );
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeAnyChildScopeCorrectly()
    {
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();
        var child1 = sut.BeginScope();
        var child2 = sut.BeginScope();
        var child3 = sut.BeginScope();

        child2.Dispose();

        using ( new AssertionScope() )
        {
            child2.IsDisposed.Should().BeTrue();
            child1.IsDisposed.Should().BeFalse();
            child3.IsDisposed.Should().BeFalse();
            sut.GetChildren().Should().BeSequentiallyEqualTo( child1, child3 );
        }
    }

    [Fact]
    public async Task Dispose_ShouldDisposeScopeOriginatingFromAnotherThread()
    {
        var context = new DedicatedThreadSynchronizationContext();
        var taskFactory = new TaskFactory( TaskSchedulerCapture.FromSynchronizationContext( context ) );
        var container = new DependencyContainerBuilder().Build();
        var sut = await taskFactory.StartNew( () => container.RootScope.BeginScope() );

        sut.Dispose();

        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldFreeScopeName()
    {
        var container = new DependencyContainerBuilder().Build();
        var scope = container.RootScope.BeginScope( "foo" );

        scope.Dispose();
        var action = Lambda.Of( () => container.RootScope.BeginScope( "foo" ) );

        using ( new AssertionScope() )
        {
            action.Should().NotThrow();
            scope.IsDisposed.Should().BeTrue();
            scope.Should().NotBeSameAs( container.TryGetScope( "foo" ) );
            container.RootScope.GetChildren().Should().BeSequentiallyEqualTo( container.GetScope( "foo" ) );
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeOwnedTransientDisposableDependencies()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.Transient ).FromFactory( factory );
        var container = builder.Build();
        var sut = container.RootScope.BeginScope();

        var resolved1 = sut.Locator.Resolve<IDisposable>();
        var resolved2 = sut.Locator.Resolve<IDisposable>();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            resolved1.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved2.VerifyCalls().Received( x => x.Dispose(), 1 );
        }
    }

    [Fact]
    public void Dispose_ThroughChildScope_ShouldNotDisposeOwnedTransientDisposableDependenciesFromParentScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.Transient ).FromFactory( factory );
        var container = builder.Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();

        var resolved = parent.Locator.Resolve<IDisposable>();

        sut.Dispose();

        resolved.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Dispose_ThroughRootScope_ShouldDisposeOwnedSingletonDisposableDependenciesResolvedByChildScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( factory );
        var container = builder.Build();
        var sut = container.RootScope.BeginScope();

        var resolved = sut.Locator.Resolve<IDisposable>();

        container.Dispose();

        resolved.VerifyCalls().Received( x => x.Dispose(), 1 );
    }

    [Fact]
    public void Dispose_ThroughChildScope_ShouldNotDisposeOwnedSingletonDisposableDependenciesResolvedByChildScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( factory );
        var container = builder.Build();
        var sut = container.RootScope.BeginScope();

        var resolved = sut.Locator.Resolve<IDisposable>();

        sut.Dispose();

        resolved.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Dispose_ShouldDisposeOwnedScopedSingletonDisposableDependencies()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromFactory( factory );
        var container = builder.Build();
        var sut = container.RootScope.BeginScope();

        var resolved = sut.Locator.Resolve<IDisposable>();

        sut.Dispose();

        resolved.VerifyCalls().Received( x => x.Dispose(), 1 );
    }

    [Fact]
    public void Dispose_ThroughChildScope_ShouldNotDisposeOwnedScopedSingletonDisposableDependenciesFromParentScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromFactory( factory );
        var container = builder.Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();

        _ = parent.Locator.Resolve<IDisposable>();
        var resolved = sut.Locator.Resolve<IDisposable>();

        sut.Dispose();

        resolved.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Dispose_ShouldDisposeOwnedScopedDisposableDependencies()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.Scoped ).FromFactory( factory );
        var container = builder.Build();
        var sut = container.RootScope.BeginScope();

        var resolved = sut.Locator.Resolve<IDisposable>();

        sut.Dispose();

        resolved.VerifyCalls().Received( x => x.Dispose(), 1 );
    }

    [Fact]
    public void Dispose_ThroughChildScope_ShouldNotDisposeOwnedScopedDisposableDependenciesFromParentScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.Scoped ).FromFactory( factory );
        var container = builder.Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();

        var resolved = parent.Locator.Resolve<IDisposable>();

        sut.Dispose();

        resolved.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void
        Dispose_ThroughRootScope_ShouldDisposeAllOwnedDisposableDependenciesAndThrowOwnedDependenciesDisposalAggregateException_WhenDependencyDisposalHasThrown()
    {
        var exception = new Exception();
        var factory = Substitute.For<Func<IDependencyScope, IDisposableDependency>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposableDependency>() );
        var throwingFactory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        throwingFactory.WithAnyArgs(
            _ =>
            {
                var result = Substitute.For<IDisposable>();
                result.When( x => x.Dispose() ).Throw( exception );
                return result;
            } );

        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposableDependency>().FromFactory( factory );
        builder.Add<IDisposable>().FromFactory( throwingFactory );
        var container = builder.Build();
        var childScope = container.RootScope.BeginScope();
        var grandchildScope = childScope.BeginScope();

        var resolved1 = container.RootScope.Locator.Resolve<IDisposableDependency>();
        var resolved2 = container.RootScope.Locator.Resolve<IDisposable>();
        var resolved3 = childScope.Locator.Resolve<IDisposableDependency>();
        var resolved4 = childScope.Locator.Resolve<IDisposable>();
        var resolved5 = grandchildScope.Locator.Resolve<IDisposableDependency>();
        var resolved6 = grandchildScope.Locator.Resolve<IDisposable>();

        var action = Lambda.Of( () => container.Dispose() );

        using ( new AssertionScope() )
        {
            var aggregateException = action.Should().ThrowExactly<OwnedDependenciesDisposalAggregateException>().And;

            resolved1.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved2.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved3.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved4.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved5.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved6.VerifyCalls().Received( x => x.Dispose(), 1 );

            container.RootScope.IsDisposed.Should().BeTrue();
            childScope.IsDisposed.Should().BeTrue();
            grandchildScope.IsDisposed.Should().BeTrue();

            aggregateException.InnerExceptions.Should().HaveCount( 3 );

            var rootException = aggregateException.InnerExceptions.OfType<OwnedDependencyDisposalException>()
                .FirstOrDefault( e => e.Scope == container.RootScope );

            var childException = aggregateException.InnerExceptions.OfType<OwnedDependencyDisposalException>()
                .FirstOrDefault( e => e.Scope == childScope );

            var grandchildException = aggregateException.InnerExceptions.OfType<OwnedDependencyDisposalException>()
                .FirstOrDefault( e => e.Scope == grandchildScope );

            rootException.Should().NotBeNull();
            rootException?.InnerException.Should().BeSameAs( exception );
            childException.Should().NotBeNull();
            childException?.InnerException.Should().BeSameAs( exception );
            grandchildException.Should().NotBeNull();
            grandchildException?.InnerException.Should().BeSameAs( exception );
        }
    }

    [Fact]
    public void
        Dispose_ThroughChildScope_ShouldDisposeAllNestedOwnedDisposableDependenciesAndThrowOwnedDependenciesDisposalAggregateException_WhenDependencyDisposalHasThrown()
    {
        var exception = new Exception();
        var factory = Substitute.For<Func<IDependencyScope, IDisposableDependency>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposableDependency>() );
        var throwingFactory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        throwingFactory.WithAnyArgs(
            _ =>
            {
                var result = Substitute.For<IDisposable>();
                result.When( x => x.Dispose() ).Throw( exception );
                return result;
            } );

        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposableDependency>().FromFactory( factory );
        builder.Add<IDisposable>().FromFactory( throwingFactory );
        var container = builder.Build();
        var sut = container.RootScope.BeginScope();
        var childScope = sut.BeginScope();
        var grandchildScope = childScope.BeginScope();

        var resolved1 = sut.Locator.Resolve<IDisposableDependency>();
        var resolved2 = sut.Locator.Resolve<IDisposable>();
        var resolved3 = childScope.Locator.Resolve<IDisposableDependency>();
        var resolved4 = childScope.Locator.Resolve<IDisposable>();
        var resolved5 = grandchildScope.Locator.Resolve<IDisposableDependency>();
        var resolved6 = grandchildScope.Locator.Resolve<IDisposable>();

        var action = Lambda.Of( () => sut.Dispose() );

        using ( new AssertionScope() )
        {
            var aggregateException = action.Should().ThrowExactly<OwnedDependenciesDisposalAggregateException>().And;

            resolved1.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved2.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved3.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved4.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved5.VerifyCalls().Received( x => x.Dispose(), 1 );
            resolved6.VerifyCalls().Received( x => x.Dispose(), 1 );

            sut.IsDisposed.Should().BeTrue();
            childScope.IsDisposed.Should().BeTrue();
            grandchildScope.IsDisposed.Should().BeTrue();

            aggregateException.InnerExceptions.Should().HaveCount( 3 );

            var sutException = aggregateException.InnerExceptions.OfType<OwnedDependencyDisposalException>()
                .FirstOrDefault( e => e.Scope == sut );

            var childException = aggregateException.InnerExceptions.OfType<OwnedDependencyDisposalException>()
                .FirstOrDefault( e => e.Scope == childScope );

            var grandchildException = aggregateException.InnerExceptions.OfType<OwnedDependencyDisposalException>()
                .FirstOrDefault( e => e.Scope == grandchildScope );

            sutException.Should().NotBeNull();
            sutException?.InnerException.Should().BeSameAs( exception );
            childException.Should().NotBeNull();
            childException?.InnerException.Should().BeSameAs( exception );
            grandchildException.Should().NotBeNull();
            grandchildException?.InnerException.Should().BeSameAs( exception );
        }
    }

    [Fact]
    public void Dispose_ShouldNotDisposeDisposableDependencies_WhenTheirDisposalStrategyIsSetToRenounceOwnership()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().FromFactory( factory ).SetDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );
        var container = builder.Build();

        var resolved = container.RootScope.Locator.Resolve<IDisposable>();

        container.Dispose();

        resolved.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Dispose_ShouldCallCustomDisposableCallback_WhenTheirDisposalStrategyIsSetToUseCallback()
    {
        var factory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposable>() );
        var callback = Substitute.For<Action<object>>();

        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>()
            .FromFactory( factory )
            .SetDisposalStrategy( DependencyImplementorDisposalStrategy.UseCallback( callback ) );

        var container = builder.Build();

        var resolved = container.RootScope.Locator.Resolve<IDisposable>();

        container.Dispose();

        using ( new AssertionScope() )
        {
            resolved.VerifyCalls().DidNotReceive( x => x.Dispose() );
            callback.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( resolved );
        }
    }
}
