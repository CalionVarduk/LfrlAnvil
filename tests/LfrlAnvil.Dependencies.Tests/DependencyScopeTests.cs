using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Functional;
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

        result.TestEquals( $"RootDependencyScope [Level: 0, OriginalThreadId: {threadId}]" ).Go();
    }

    [Fact]
    public void ToString_ForChildScope_ShouldReturnCorrectResult()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();

        var result = sut.ToString();

        result.TestEquals( $"ChildDependencyScope [Level: 1, OriginalThreadId: {threadId}]" ).Go();
    }

    [Fact]
    public void ToString_ForNamedChildScope_ShouldReturnCorrectResult()
    {
        var name = Fixture.Create<string>();
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( name );

        var result = sut.ToString();

        result.TestEquals( $"ChildDependencyScope [Name: '{name}', Level: 1, OriginalThreadId: {threadId}]" ).Go();
    }

    [Fact]
    public void ToString_ForGrandchildScope_ShouldReturnCorrectResult()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope().BeginScope();

        var result = sut.ToString();

        result.TestEquals( $"ChildDependencyScope [Level: 2, OriginalThreadId: {threadId}]" ).Go();
    }

    [Fact]
    public void RootScope_ShouldHaveCorrectProperties()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        Assertion.All(
                sut.Container.TestRefEquals( container ),
                sut.Level.TestEquals( 0 ),
                sut.Locator.AttachedScope.TestRefEquals( sut ),
                sut.Locator.Key.TestNull(),
                sut.Locator.KeyType.TestNull(),
                sut.Locator.IsKeyed.TestFalse(),
                sut.IsDisposed.TestFalse(),
                sut.IsRoot.TestTrue(),
                sut.ParentScope.TestNull(),
                sut.OriginalThreadId.TestEquals( threadId ),
                sut.Name.TestNull(),
                sut.GetChildren().TestEmpty() )
            .Go();
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

        Assertion.All(
                result.Key.TestEquals( key ),
                (( IDependencyLocator )result).Key.TestEquals( key ),
                result.KeyType.TestEquals( typeof( int ) ),
                result.IsKeyed.TestTrue(),
                result.AttachedScope.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void GetKeyedLocator_ShouldReturnLocatorWithCorrectKey_WhenKeyTypeIsCached()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;
        _ = sut.GetKeyedLocator( 1 );

        var result = sut.GetKeyedLocator( 2 );

        Assertion.All(
                result.Key.TestEquals( 2 ),
                (( IDependencyLocator )result).Key.TestEquals( 2 ),
                result.KeyType.TestEquals( typeof( int ) ),
                result.IsKeyed.TestTrue(),
                result.AttachedScope.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void GetKeyedLocator_ShouldReturnCorrectCachedLocator_WhenCalledMoreThanOnceWithTheSameKey()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;

        var result1 = sut.GetKeyedLocator( 1 );
        var result2 = sut.GetKeyedLocator( 1 );

        result1.TestRefEquals( result2 ).Go();
    }

    [Fact]
    public void GetKeyedLocator_ShouldThrowObjectDisposedException_WhenScopeIsDisposed()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;
        container.Dispose();

        var action = Lambda.Of( () => sut.GetKeyedLocator( 1 ) );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void BeginScope_ThroughRootScope_ShouldCreateChildScope()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();

        Assertion.All(
                sut.Container.TestRefEquals( container ),
                sut.Level.TestEquals( 1 ),
                sut.Locator.AttachedScope.TestRefEquals( sut ),
                sut.Locator.Key.TestNull(),
                sut.Locator.KeyType.TestNull(),
                sut.Locator.IsKeyed.TestFalse(),
                sut.IsDisposed.TestFalse(),
                sut.IsRoot.TestFalse(),
                sut.ParentScope.TestRefEquals( container.RootScope ),
                sut.OriginalThreadId.TestEquals( threadId ),
                sut.Name.TestNull(),
                container.RootScope.GetChildren().TestSequence( [ sut ] ),
                sut.GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void BeginScope_ThroughRootScope_ShouldCreateNamedChildScope()
    {
        var name = Fixture.Create<string>();
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( name );

        Assertion.All(
                sut.Container.TestRefEquals( container ),
                sut.Level.TestEquals( 1 ),
                sut.Locator.AttachedScope.TestRefEquals( sut ),
                sut.IsDisposed.TestFalse(),
                sut.IsRoot.TestFalse(),
                sut.ParentScope.TestRefEquals( container.RootScope ),
                sut.OriginalThreadId.TestEquals( threadId ),
                sut.Name.TestEquals( name ),
                container.RootScope.GetChildren().TestSequence( [ sut ] ),
                sut.GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void BeginScope_CalledMultipleTimesFromRootScope_ShouldAttachCreatedScopesAsRootScopeChildren()
    {
        var container = new DependencyContainerBuilder().Build();

        var child1 = container.RootScope.BeginScope();
        var child2 = container.RootScope.BeginScope();
        var child3 = container.RootScope.BeginScope();

        container.RootScope.GetChildren().TestSequence( [ child1, child2, child3 ] ).Go();
    }

    [Fact]
    public void BeginScope_ThroughChildScope_ShouldCreateChildScope()
    {
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope();

        Assertion.All(
                sut.Container.TestRefEquals( container ),
                sut.Level.TestEquals( 2 ),
                sut.Locator.AttachedScope.TestRefEquals( sut ),
                sut.IsDisposed.TestFalse(),
                sut.IsRoot.TestFalse(),
                sut.ParentScope.TestRefEquals( parent ),
                sut.OriginalThreadId.TestEquals( threadId ),
                sut.Name.TestNull(),
                parent.GetChildren().TestSequence( [ sut ] ),
                sut.GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void BeginScope_ThroughChildScope_ShouldCreateNamedChildScope()
    {
        var name = Fixture.Create<string>();
        var threadId = Environment.CurrentManagedThreadId;
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();
        var sut = parent.BeginScope( name );

        Assertion.All(
                sut.Container.TestRefEquals( container ),
                sut.Level.TestEquals( 2 ),
                sut.Locator.AttachedScope.TestRefEquals( sut ),
                sut.IsDisposed.TestFalse(),
                sut.IsRoot.TestFalse(),
                sut.ParentScope.TestRefEquals( parent ),
                sut.OriginalThreadId.TestEquals( threadId ),
                sut.Name.TestEquals( name ),
                parent.GetChildren().TestSequence( [ sut ] ),
                sut.GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void BeginScope_CalledMultipleTimesFromChildScope_ShouldAttachCreatedScopesAsRootScopeChildren()
    {
        var container = new DependencyContainerBuilder().Build();
        var parent = container.RootScope.BeginScope();

        var child1 = parent.BeginScope();
        var child2 = parent.BeginScope();
        var child3 = parent.BeginScope();

        parent.GetChildren().TestSequence( [ child1, child2, child3 ] ).Go();
    }

    [Fact]
    public void BeginScope_ShouldCreateChildScope_WhenChildScopeHasItsOwnChildren()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope;
        var child = sut.BeginScope();
        var grandchild = child.BeginScope();

        var result = sut.BeginScope();

        Assertion.All(
                sut.GetChildren().TestSequence( [ child, result ] ),
                child.GetChildren().TestSequence( [ grandchild ] ) )
            .Go();
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

        Assertion.All(
                result.Container.TestRefEquals( container ),
                result.Level.TestEquals( 2 ),
                result.Locator.AttachedScope.TestRefEquals( result ),
                result.IsDisposed.TestFalse(),
                result.IsRoot.TestFalse(),
                result.ParentScope.TestRefEquals( sut ),
                result.OriginalThreadId.TestEquals( threadId ),
                sut.GetChildren().TestSequence( [ result ] ),
                result.GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void BeginScope_ShouldThrowObjectDisposedException_WhenScopeIsDisposed()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();
        sut.Dispose();

        var action = Lambda.Of( () => sut.BeginScope() );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void BeginScope_ShouldThrowNamedDependencyScopeCreationException_WhenScopeWithProvidedNameAlreadyExists()
    {
        var name = Fixture.Create<string>();
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope( name );

        var action = Lambda.Of( () => sut.BeginScope( name ) );

        action.Test( exc => exc.TestType()
                .Exact<NamedDependencyScopeCreationException>( e => Assertion.All(
                    e.ParentScope.TestRefEquals( sut ),
                    e.Name.TestEquals( name ) ) ) )
            .Go();
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

        Assertion.All(
                sut.IsDisposed.TestTrue(),
                child1.IsDisposed.TestTrue(),
                child2.IsDisposed.TestTrue(),
                child3.IsDisposed.TestTrue(),
                grandchild1.IsDisposed.TestTrue(),
                grandchild2.IsDisposed.TestTrue(),
                grandchild3.IsDisposed.TestTrue(),
                sut.GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeContainer_WhenScopeIsRoot()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = ( IDisposable )container.RootScope;

        sut.Dispose();

        container.RootScope.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenScopeIsRootAndIsAlreadyDisposed()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = ( IDisposable )container.RootScope;
        sut.Dispose();

        sut.Dispose();

        container.RootScope.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenScopeIsAlreadyDisposed()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();
        sut.Dispose();

        sut.Dispose();

        sut.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenParentScopeIsAlreadyDisposed()
    {
        var container = new DependencyContainerBuilder().Build();
        var sut = container.RootScope.BeginScope();
        container.Dispose();

        sut.Dispose();

        sut.IsDisposed.TestTrue().Go();
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

        Assertion.All(
                child1.IsDisposed.TestTrue(),
                child2.IsDisposed.TestFalse(),
                child3.IsDisposed.TestFalse(),
                sut.GetChildren().TestSequence( [ child2, child3 ] ) )
            .Go();
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

        Assertion.All(
                child3.IsDisposed.TestTrue(),
                child1.IsDisposed.TestFalse(),
                child2.IsDisposed.TestFalse(),
                sut.GetChildren().TestSequence( [ child1, child2 ] ) )
            .Go();
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

        Assertion.All(
                child2.IsDisposed.TestTrue(),
                child1.IsDisposed.TestFalse(),
                child3.IsDisposed.TestFalse(),
                sut.GetChildren().TestSequence( [ child1, child3 ] ) )
            .Go();
    }

    [Fact]
    public async Task Dispose_ShouldDisposeScopeOriginatingFromAnotherThread()
    {
        var context = new DedicatedThreadSynchronizationContext();
        var taskFactory = new TaskFactory( TaskSchedulerCapture.FromSynchronizationContext( context ) );
        var container = new DependencyContainerBuilder().Build();
        var sut = await taskFactory.StartNew( () => container.RootScope.BeginScope() );

        sut.Dispose();

        sut.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public void Dispose_ShouldFreeScopeName()
    {
        var container = new DependencyContainerBuilder().Build();
        var scope = container.RootScope.BeginScope( "foo" );

        scope.Dispose();
        var action = Lambda.Of( () => container.RootScope.BeginScope( "foo" ) );

        action.Test( exc => Assertion.All(
                exc.TestNull(),
                scope.IsDisposed.TestTrue(),
                scope.TestNotRefEquals( container.TryGetScope( "foo" ) ),
                container.RootScope.GetChildren().TestSequence( [ container.GetScope( "foo" ) ] ) ) )
            .Go();
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

        Assertion.All(
                resolved1.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved2.TestReceivedCalls( x => x.Dispose(), count: 1 ) )
            .Go();
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

        resolved.TestDidNotReceiveCall( x => x.Dispose() ).Go();
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

        resolved.TestReceivedCalls( x => x.Dispose(), count: 1 ).Go();
    }

    [Fact]
    public void Dispose_ThroughRootScope_ShouldDisposeOwnedSingletonDisposableDependenciesResolvedByChildScope_BasedOnImplementor()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IDisposable>().SetLifetime( DependencyLifetime.Singleton ).FromType<DisposableDependency>();
        var container = builder.Build();
        var sut = container.RootScope.BeginScope();

        var resolved = sut.Locator.Resolve<IDisposable>();

        container.Dispose();

        ((resolved as DisposableDependency)?.IsDisposed).TestEquals( true ).Go();
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

        resolved.TestDidNotReceiveCall( x => x.Dispose() ).Go();
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

        resolved.TestReceivedCalls( x => x.Dispose(), count: 1 ).Go();
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

        resolved.TestDidNotReceiveCall( x => x.Dispose() ).Go();
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

        resolved.TestReceivedCalls( x => x.Dispose(), count: 1 ).Go();
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

        resolved.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void
        Dispose_ThroughRootScope_ShouldDisposeAllOwnedDisposableDependenciesAndThrowOwnedDependenciesDisposalAggregateException_WhenDependencyDisposalHasThrown()
    {
        var exception = new Exception();
        var factory = Substitute.For<Func<IDependencyScope, IDisposableDependency>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposableDependency>() );
        var throwingFactory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        throwingFactory.WithAnyArgs( _ =>
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

        action.Test( exc => Assertion.All(
                resolved1.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved2.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved3.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved4.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved5.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved6.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                container.RootScope.IsDisposed.TestTrue(),
                childScope.IsDisposed.TestTrue(),
                grandchildScope.IsDisposed.TestTrue(),
                exc.TestType()
                    .Exact<OwnedDependenciesDisposalAggregateException>( aggregateException => Assertion.All(
                        aggregateException.InnerExceptions.Count.TestEquals( 3 ),
                        aggregateException.InnerExceptions.OfType<OwnedDependencyDisposalException>()
                            .TestAll( (e, _) => Assertion.All(
                                e.InnerException.TestRefEquals( exception ),
                                Assertion.Any(
                                    e.Scope.TestRefEquals( container.RootScope ),
                                    e.Scope.TestRefEquals( childScope ),
                                    e.Scope.TestRefEquals( grandchildScope ) ) ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void
        Dispose_ThroughChildScope_ShouldDisposeAllNestedOwnedDisposableDependenciesAndThrowOwnedDependenciesDisposalAggregateException_WhenDependencyDisposalHasThrown()
    {
        var exception = new Exception();
        var factory = Substitute.For<Func<IDependencyScope, IDisposableDependency>>();
        factory.WithAnyArgs( _ => Substitute.For<IDisposableDependency>() );
        var throwingFactory = Substitute.For<Func<IDependencyScope, IDisposable>>();
        throwingFactory.WithAnyArgs( _ =>
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

        action.Test( exc => Assertion.All(
                resolved1.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved2.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved3.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved4.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved5.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                resolved6.TestReceivedCalls( x => x.Dispose(), count: 1 ),
                sut.IsDisposed.TestTrue(),
                childScope.IsDisposed.TestTrue(),
                grandchildScope.IsDisposed.TestTrue(),
                exc.TestType()
                    .Exact<OwnedDependenciesDisposalAggregateException>( aggregateException => Assertion.All(
                        aggregateException.InnerExceptions.Count.TestEquals( 3 ),
                        aggregateException.InnerExceptions.OfType<OwnedDependencyDisposalException>()
                            .TestAll( (e, _) => Assertion.All(
                                e.InnerException.TestRefEquals( exception ),
                                Assertion.Any(
                                    e.Scope.TestRefEquals( sut ),
                                    e.Scope.TestRefEquals( childScope ),
                                    e.Scope.TestRefEquals( grandchildScope ) ) ) ) ) ) ) )
            .Go();
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

        resolved.TestDidNotReceiveCall( x => x.Dispose() ).Go();
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

        Assertion.All(
                resolved.TestDidNotReceiveCall( x => x.Dispose() ),
                callback.CallAt( 0 ).Exists.TestTrue(),
                callback.CallAt( 0 ).Arguments.TestSequence( [ resolved ] ) )
            .Go();
    }
}
