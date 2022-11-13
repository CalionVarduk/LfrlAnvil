using System.Linq;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Dependencies.Tests.DependencyScopeTests;

public class DependencyScopeTests : DependencyTestsBase
{
    [Fact]
    public void ToString_ForRootScope_ShouldReturnCorrectResult()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        var result = sut.ToString();

        result.Should().Be( "RootDependencyScope [Level: 0]" );
    }

    [Fact]
    public void ToString_ForChildScope_ShouldReturnCorrectResult()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();

        var result = sut.ToString();

        result.Should().Be( $"ChildDependencyScope [Level: 1, ThreadId: {threadId}]" );
    }

    [Fact]
    public void ToString_ForNamedChildScope_ShouldReturnCorrectResult()
    {
        var name = Fixture.Create<string>();
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( name );

        var result = sut.ToString();

        result.Should().Be( $"ChildDependencyScope [Name: '{name}', Level: 1, ThreadId: {threadId}]" );
    }

    [Fact]
    public void ToString_ForGrandchildScope_ShouldReturnCorrectResult()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope().BeginScope();

        var result = sut.ToString();

        result.Should().Be( $"ChildDependencyScope [Level: 2, ThreadId: {threadId}]" );
    }

    [Fact]
    public void RootScope_ShouldHaveCorrectProperties()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        using ( new AssertionScope() )
        {
            sut.Container.Should().BeSameAs( container );
            sut.Level.Should().Be( 0 );
            sut.Locator.AttachedScope.Should().BeSameAs( sut );
            sut.IsActive.Should().BeTrue();
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeTrue();
            sut.ParentScope.Should().BeNull();
            sut.ThreadId.Should().BeNull();
            sut.Name.Should().BeNull();
        }
    }

    [Fact]
    public void BeginScope_ThroughRootScope_ShouldCreateChildScopeForCurrentThreadAndMarkItAsActive()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();

        using ( new AssertionScope() )
        {
            sut.Container.Should().BeSameAs( container );
            sut.Level.Should().Be( 1 );
            sut.Locator.AttachedScope.Should().BeSameAs( sut );
            sut.IsActive.Should().BeTrue();
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeFalse();
            sut.ParentScope.Should().BeSameAs( container.RootScope );
            sut.ThreadId.Should().Be( threadId );
            sut.Name.Should().BeNull();
            container.RootScope.IsActive.Should().BeFalse();
        }
    }

    [Fact]
    public void BeginScope_ThroughRootScope_ShouldCreateNamedChildScopeForCurrentThreadAndMarkItAsActive()
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
            sut.IsActive.Should().BeTrue();
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeFalse();
            sut.ParentScope.Should().BeSameAs( container.RootScope );
            sut.ThreadId.Should().Be( threadId );
            sut.Name.Should().Be( name );
            container.RootScope.IsActive.Should().BeFalse();
        }
    }

    [Fact]
    public void BeginScope_ThroughChildScope_ShouldCreateChildScopeForCurrentThreadAndMarkItAsActive()
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
            sut.IsActive.Should().BeTrue();
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeFalse();
            sut.ParentScope.Should().BeSameAs( parent );
            sut.ThreadId.Should().Be( threadId );
            sut.Name.Should().BeNull();
            parent.IsActive.Should().BeFalse();
            container.RootScope.IsActive.Should().BeFalse();
        }
    }

    [Fact]
    public void BeginScope_ThroughChildScope_ShouldCreateNamedChildScopeForCurrentThreadAndMarkItAsActive()
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
            sut.IsActive.Should().BeTrue();
            sut.IsDisposed.Should().BeFalse();
            sut.IsRoot.Should().BeFalse();
            sut.ParentScope.Should().BeSameAs( parent );
            sut.ThreadId.Should().Be( threadId );
            sut.Name.Should().Be( name );
            parent.IsActive.Should().BeFalse();
            container.RootScope.IsActive.Should().BeFalse();
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
    public void BeginScope_ShouldThrowDependencyScopeCreationException_WhenScopeIsNotMarkedAsActive()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;
        var active = sut.BeginScope();

        var action = Lambda.Of( () => sut.BeginScope() );

        action.Should()
            .ThrowExactly<DependencyScopeCreationException>()
            .AndMatch( e => e.Scope == sut && e.ExpectedScope == active && e.ThreadId == threadId );
    }

    [Fact]
    public async Task BeginScope_ShouldThrowDependencyScopeCreationException_WhenScopeIsAttachedToAnotherThread()
    {
        using var context = new DedicatedThreadSynchronizationContext();
        var taskFactory = new TaskFactory( TaskSchedulerCapture.FromSynchronizationContext( context ) );
        var container = new DependencyContainerBuilder().Build();
        var sut = await taskFactory.StartNew( () => container.RootScope.BeginScope() );
        var threadId = Environment.CurrentManagedThreadId;

        var action = Lambda.Of( () => sut.BeginScope() );

        action.Should()
            .ThrowExactly<DependencyScopeCreationException>()
            .AndMatch( e => e.Scope == sut && e.ExpectedScope == container.RootScope && e.ThreadId == threadId );
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
    public void UseScope_ShouldReturnNull_WhenNamedScopeDoesNotExist()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        var result = sut.UseScope( Fixture.Create<string>() );

        result.Should().BeNull();
    }

    [Fact]
    public void UseScope_ShouldReturnCorrectScope_WhenNamedScopeExists()
    {
        var name = Fixture.Create<string>();
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;
        var expected = sut.BeginScope( name );

        var result = sut.UseScope( name );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void EndScope_ShouldReturnTrueAndBeEquivalentToItsDisposal_WhenScopeExists()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( "foo" );
        var child = sut.BeginScope();

        var result = container.RootScope.EndScope( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.IsDisposed.Should().BeTrue();
            child.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void EndScope_ShouldReturnFalseAndDoNothing_WhenScopeDoesNotExist()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( "foo" );

        var result = container.RootScope.EndScope( "bar" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.IsDisposed.Should().BeFalse();
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeChildScopeAndItsDescendantsAndMarkItsParentAsActive()
    {
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();
        var child = sut.BeginScope();
        var grandchild = child.BeginScope();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeTrue();
            child.IsDisposed.Should().BeTrue();
            grandchild.IsDisposed.Should().BeTrue();
            parent.IsActive.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeContainer_WhenScopeIsRoot()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = (IDisposable)container.RootScope;

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

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeTrue();
            container.RootScope.IsActive.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldFreeNamesOfDisposedScopes()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( "foo" );
        var _ = sut.BeginScope( "bar" );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            container.RootScope.UseScope( "foo" ).Should().BeNull();
            container.RootScope.UseScope( "bar" ).Should().BeNull();
        }
    }

    [Fact]
    public async Task Dispose_ShouldThrowDependencyScopeDisposalException_WhenScopeIsAttachedToAnotherThread()
    {
        using var context = new DedicatedThreadSynchronizationContext();
        var taskFactory = new TaskFactory( TaskSchedulerCapture.FromSynchronizationContext( context ) );
        var container = new DependencyContainerBuilder().Build();
        var sut = await taskFactory.StartNew( () => container.RootScope.BeginScope() );
        var threadId = Environment.CurrentManagedThreadId;

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should()
            .ThrowExactly<DependencyScopeDisposalException>()
            .AndMatch( e => e.Scope == sut && e.ActualThreadId == threadId );
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

        var _ = parent.Locator.Resolve<IDisposable>();
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
