using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Dependencies.Tests;

public class DependencyContainerTests : DependencyTestsBase
{
    [Fact]
    public void ResolvingDependencyContainer_ThroughRootScope_ShouldReturnContainerItself()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope;

        var result = scope.Locator.Resolve<IDependencyContainer>();

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void ResolvingDependencyContainer_ThroughChildScope_ShouldReturnContainerItself()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.Locator.Resolve<IDependencyContainer>();

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void ResolvingDependencyScope_ThroughRootScope_ShouldReturnRootScope()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope;

        var result = scope.Locator.Resolve<IDependencyScope>();

        result.TestRefEquals( scope ).Go();
    }

    [Fact]
    public void ResolvingDependencyScope_ThroughChildScope_ShouldReturnChildScope()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.Locator.Resolve<IDependencyScope>();

        result.TestRefEquals( scope ).Go();
    }

    [Fact]
    public void ResolvingDependencyContainer_ThroughUnregisteredKeyedLocator_ShouldReturnContainerItself()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IDependencyContainer>();

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void ResolvingDependencyScope_ThroughUnregisteredKeyedLocator_ShouldReturnScopeItself()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.GetKeyedLocator( 1 ).Resolve<IDependencyScope>();

        result.TestRefEquals( scope ).Go();
    }

    [Fact]
    public void ResolvingDependencyContainer_ThroughRegisteredKeyedLocator_ShouldReturnContainerItself()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IDependencyContainer>();

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void ResolvingDependencyScope_ThroughRegisteredKeyedLocator_ShouldReturnScopeItself()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.GetKeyedLocator( 1 ).Resolve<IDependencyScope>();

        result.TestRefEquals( scope ).Go();
    }

    [Fact]
    public void ResolvingTransientDependency_ThroughRootThenChildScope_ShouldReturnNewInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Transient ).FromFactory( factory );
        var sut = builder.Build();
        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();

        var result1 = rootScope.Locator.Resolve<IFoo>();
        var result2 = childScope.Locator.Resolve<IFoo>();
        var result3 = rootScope.Locator.Resolve<IFoo>();
        var result4 = childScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 4 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ rootScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 2 ).Arguments.TestSequence( [ rootScope ] ),
                factory.CallAt( 3 ).Arguments.TestSequence( [ childScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingTransientDependency_ThroughChildThenRootScope_ShouldReturnNewInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Transient ).FromFactory( factory );
        var sut = builder.Build();
        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();

        var result1 = childScope.Locator.Resolve<IFoo>();
        var result2 = rootScope.Locator.Resolve<IFoo>();
        var result3 = childScope.Locator.Resolve<IFoo>();
        var result4 = rootScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 4 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ rootScope ] ),
                factory.CallAt( 2 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 3 ).Arguments.TestSequence( [ rootScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingTransientDependency_ThroughParentThenChildScope_ShouldReturnNewInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Transient ).FromFactory( factory );
        var sut = builder.Build();
        var parentScope = sut.RootScope.BeginScope();
        var childScope = parentScope.BeginScope();

        var result1 = parentScope.Locator.Resolve<IFoo>();
        var result2 = childScope.Locator.Resolve<IFoo>();
        var result3 = parentScope.Locator.Resolve<IFoo>();
        var result4 = childScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 4 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ parentScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 2 ).Arguments.TestSequence( [ parentScope ] ),
                factory.CallAt( 3 ).Arguments.TestSequence( [ childScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingTransientDependency_ThroughChildThenParentScope_ShouldReturnNewInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Transient ).FromFactory( factory );
        var sut = builder.Build();
        var parentScope = sut.RootScope.BeginScope();
        var childScope = parentScope.BeginScope();

        var result1 = childScope.Locator.Resolve<IFoo>();
        var result2 = parentScope.Locator.Resolve<IFoo>();
        var result3 = childScope.Locator.Resolve<IFoo>();
        var result4 = parentScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 4 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ parentScope ] ),
                factory.CallAt( 2 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 3 ).Arguments.TestSequence( [ parentScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingSingletonDependency_ThroughRootThenChildScope_ShouldReturnSameInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( factory );
        var sut = builder.Build();
        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();

        var result1 = rootScope.Locator.Resolve<IFoo>();
        var result2 = childScope.Locator.Resolve<IFoo>();
        var result3 = rootScope.Locator.Resolve<IFoo>();
        var result4 = childScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 1 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ rootScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void ResolvingSingletonDependency_ThroughChildThenRootScope_ShouldReturnSameInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( factory );
        var sut = builder.Build();
        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();

        var result1 = childScope.Locator.Resolve<IFoo>();
        var result2 = rootScope.Locator.Resolve<IFoo>();
        var result3 = childScope.Locator.Resolve<IFoo>();
        var result4 = rootScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 1 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ childScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void ResolvingSingletonDependency_ThroughParentThenChildScope_ShouldReturnSameInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( factory );
        var sut = builder.Build();
        var parentScope = sut.RootScope.BeginScope();
        var childScope = parentScope.BeginScope();

        var result1 = parentScope.Locator.Resolve<IFoo>();
        var result2 = childScope.Locator.Resolve<IFoo>();
        var result3 = parentScope.Locator.Resolve<IFoo>();
        var result4 = childScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 1 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ parentScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void ResolvingSingletonDependency_ThroughChildThenParentScope_ShouldReturnSameInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( factory );
        var sut = builder.Build();
        var parentScope = sut.RootScope.BeginScope();
        var childScope = parentScope.BeginScope();

        var result1 = childScope.Locator.Resolve<IFoo>();
        var result2 = parentScope.Locator.Resolve<IFoo>();
        var result3 = childScope.Locator.Resolve<IFoo>();
        var result4 = parentScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 1 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ childScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedDependency_ThroughRootThenChildScope_ShouldReturnNewInstancePerScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Scoped ).FromFactory( factory );
        var sut = builder.Build();
        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();

        var result1 = rootScope.Locator.Resolve<IFoo>();
        var result2 = childScope.Locator.Resolve<IFoo>();
        var result3 = rootScope.Locator.Resolve<IFoo>();
        var result4 = childScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 2 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ rootScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ childScope ] ),
                result1.TestNotRefEquals( result2 ),
                result1.TestRefEquals( result3 ),
                result2.TestRefEquals( result4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedDependency_ThroughChildThenRootScope_ShouldReturnNewInstancePerScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Scoped ).FromFactory( factory );
        var sut = builder.Build();
        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();

        var result1 = childScope.Locator.Resolve<IFoo>();
        var result2 = rootScope.Locator.Resolve<IFoo>();
        var result3 = childScope.Locator.Resolve<IFoo>();
        var result4 = rootScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 2 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ rootScope ] ),
                result1.TestNotRefEquals( result2 ),
                result1.TestRefEquals( result3 ),
                result2.TestRefEquals( result4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedDependency_ThroughParentThenChildScope_ShouldReturnNewInstancePerScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Scoped ).FromFactory( factory );
        var sut = builder.Build();
        var parentScope = sut.RootScope.BeginScope();
        var childScope = parentScope.BeginScope();

        var result1 = parentScope.Locator.Resolve<IFoo>();
        var result2 = childScope.Locator.Resolve<IFoo>();
        var result3 = parentScope.Locator.Resolve<IFoo>();
        var result4 = childScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 2 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ parentScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ childScope ] ),
                result1.TestNotRefEquals( result2 ),
                result1.TestRefEquals( result3 ),
                result2.TestRefEquals( result4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedDependency_ThroughChildThenParentScope_ShouldReturnNewInstancePerScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Scoped ).FromFactory( factory );
        var sut = builder.Build();
        var parentScope = sut.RootScope.BeginScope();
        var childScope = parentScope.BeginScope();

        var result1 = childScope.Locator.Resolve<IFoo>();
        var result2 = parentScope.Locator.Resolve<IFoo>();
        var result3 = childScope.Locator.Resolve<IFoo>();
        var result4 = parentScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 2 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ parentScope ] ),
                result1.TestNotRefEquals( result2 ),
                result1.TestRefEquals( result3 ),
                result2.TestRefEquals( result4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedSingletonDependency_ThroughRootThenChildScope_ShouldReturnSameInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromFactory( factory );
        var sut = builder.Build();
        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();

        var result1 = rootScope.Locator.Resolve<IFoo>();
        var result2 = childScope.Locator.Resolve<IFoo>();
        var result3 = rootScope.Locator.Resolve<IFoo>();
        var result4 = childScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 1 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ rootScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedSingletonDependency_ThroughChildThenRootScope_ShouldReturnNewInstancePerScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromFactory( factory );
        var sut = builder.Build();
        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();

        var result1 = childScope.Locator.Resolve<IFoo>();
        var result2 = rootScope.Locator.Resolve<IFoo>();
        var result3 = childScope.Locator.Resolve<IFoo>();
        var result4 = rootScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 2 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ rootScope ] ),
                result1.TestNotRefEquals( result2 ),
                result1.TestRefEquals( result3 ),
                result2.TestRefEquals( result4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedSingletonDependency_ThroughParentThenChildScope_ShouldReturnSameInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromFactory( factory );
        var sut = builder.Build();
        var parentScope = sut.RootScope.BeginScope();
        var childScope = parentScope.BeginScope();

        var result1 = parentScope.Locator.Resolve<IFoo>();
        var result2 = childScope.Locator.Resolve<IFoo>();
        var result3 = parentScope.Locator.Resolve<IFoo>();
        var result4 = childScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 1 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ parentScope ] ),
                new[] { result1, result2, result3, result4 }.Distinct().Count().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedSingletonDependency_ThroughChildThenParentScope_ShouldReturnNewInstancePerScope()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromFactory( factory );
        var sut = builder.Build();
        var parentScope = sut.RootScope.BeginScope();
        var childScope = parentScope.BeginScope();

        var result1 = childScope.Locator.Resolve<IFoo>();
        var result2 = parentScope.Locator.Resolve<IFoo>();
        var result3 = childScope.Locator.Resolve<IFoo>();
        var result4 = parentScope.Locator.Resolve<IFoo>();

        Assertion.All(
                factory.CallCount().TestEquals( 2 ),
                factory.CallAt( 0 ).Arguments.TestSequence( [ childScope ] ),
                factory.CallAt( 1 ).Arguments.TestSequence( [ parentScope ] ),
                result1.TestNotRefEquals( result2 ),
                result1.TestRefEquals( result3 ),
                result2.TestRefEquals( result4 ) )
            .Go();
    }

    [Fact]
    public void ResolvingTransientDependency_WithSharedImplementor_ShouldReturnNewInstanceEachTimeForAllSharingDependencies()
    {
        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>();
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Transient );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Transient );

        var sut = builder.Build();
        var scope = sut.RootScope;

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.Locator.Resolve<IBar>();

        new object[] { result1, result2 }.Distinct().Count().TestEquals( 2 ).Go();
    }

    [Fact]
    public void ResolvingSingletonDependency_WithSharedImplementor_ShouldReturnSameInstanceEachTimeForAllSharingDependencies()
    {
        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>();
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Singleton );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Singleton );

        var sut = builder.Build();
        var scope = sut.RootScope;

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.Locator.Resolve<IBar>();

        result1.TestRefEquals( result2 ).Go();
    }

    [Fact]
    public void
        ResolvingScopedSingletonDependency_WithSharedImplementor_ShouldReturnNewInstancePerScopeAndItsChildrenForAllSharingDependencies()
    {
        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>();
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.ScopedSingleton );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.ScopedSingleton );

        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.BeginScope().Locator.Resolve<IBar>();
        var result3 = scope.Locator.Resolve<IFoo>();

        Assertion.All(
                result1.TestRefEquals( result2 ),
                result1.TestRefEquals( result3 ) )
            .Go();
    }

    [Fact]
    public void ResolvingScopedDependency_WithSharedImplementor_ShouldReturnSameInstancePerScopeEachTimeForAllSharingDependencies()
    {
        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>();
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Scoped );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Scoped );

        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.Locator.Resolve<IBar>();

        result1.TestRefEquals( result2 ).Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void ResolvingDependency_ShouldInvokeOnResolvingCallbackEveryTime_BasedOnFactory(DependencyLifetime lifetime)
    {
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        factory.WithAnyArgs( _ => new Implementor() );

        var onResolvingCallback = Substitute.For<Action<Type, IDependencyScope>>();

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( lifetime ).FromFactory( factory ).SetOnResolvingCallback( onResolvingCallback );
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        _ = scope.Locator.Resolve<IFoo>();
        _ = scope.Locator.Resolve<IFoo>();

        Assertion.All(
                onResolvingCallback.CallCount().TestEquals( 2 ),
                onResolvingCallback.CallAt( 0 ).Arguments.TestSequence( [ typeof( IFoo ), scope ] ),
                onResolvingCallback.CallAt( 1 ).Arguments.TestSequence( [ typeof( IFoo ), scope ] ) )
            .Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void ResolvingDependency_ShouldInvokeOnResolvingCallbackEveryTime_BasedOnImplementor(DependencyLifetime lifetime)
    {
        var onResolvingCallback = Substitute.For<Action<Type, IDependencyScope>>();

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( lifetime ).FromType<Implementor>().SetOnResolvingCallback( onResolvingCallback );
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        _ = scope.Locator.Resolve<IFoo>();
        _ = scope.Locator.Resolve<IFoo>();

        Assertion.All(
                onResolvingCallback.CallCount().TestEquals( 2 ),
                onResolvingCallback.CallAt( 0 ).Arguments.TestSequence( [ typeof( IFoo ), scope ] ),
                onResolvingCallback.CallAt( 1 ).Arguments.TestSequence( [ typeof( IFoo ), scope ] ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithSharedImplementor_ShouldGroupSharedImplementorsByDependencyLifetimes()
    {
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        factory.WithAnyArgs( _ => new Implementor() );

        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>().FromFactory( factory );
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Singleton );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Singleton );
        builder.Add<IQux>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Scoped );

        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.Locator.Resolve<IBar>();
        var result3 = scope.Locator.Resolve<IQux>();

        Assertion.All(
                factory.CallCount().TestEquals( 2 ),
                result1.TestRefEquals( result2 ),
                result1.TestNotRefEquals( result3 ) )
            .Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    public void ResolvingScopedDependency_ShouldThrowExceptionAndNotModifyScopeState_WhenInstanceFactoryThrows(DependencyLifetime lifetime)
    {
        var exception = new Exception();
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        factory.WithAnyArgs( _ => throw exception );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( lifetime ).FromFactory( factory );
        var sut = builder.Build();
        var scope = ( DependencyScope )sut.RootScope.BeginScope();

        var action = Lambda.Of( () => scope.Locator.Resolve<IFoo>() );

        action.Test( exc => Assertion.All( exc.TestRefEquals( exception ), scope.ScopedInstancesByResolverId.TestEmpty() ) ).Go();
    }

    [Fact]
    public void KeyedDependencyResolutionAttempt_ThroughGlobalLocator_ShouldReturnNull()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.TryResolve<IFoo>();

        result.TestNull().Go();
    }

    [Fact]
    public void KeyedDependencyResolutionAttempt_ThroughKeyedLocatorWithCorrectKeyTypeButDifferentKey_ShouldReturnNull()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 2 ).TryResolve<IFoo>();

        result.TestNull().Go();
    }

    [Fact]
    public void KeyedDependencyResolutionAttempt_ThroughKeyedLocatorWithIncorrectKeyType_ShouldReturnNull()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( "foo" ).TryResolve<IFoo>();

        result.TestNull().Go();
    }

    [Fact]
    public void KeyedDependencyResolutionAttempt_ThroughKeyedLocatorWithCorrectKey_ShouldReturnCorrectInstance()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 1 ).TryResolve<IFoo>();

        result.TestNotNull().Go();
    }

    [Fact]
    public void ResolvingDependency_WithSharedKeyedImplementor_ShouldReturnCorrectInstances()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).AddSharedImplementor<Implementor>().FromType<Implementor>();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Singleton );
        builder.Add<IBar>().FromSharedImplementor<Implementor>( o => o.Keyed( 1 ) ).SetLifetime( DependencyLifetime.Singleton );
        builder.GetKeyedLocator( "foo" )
            .Add<IQux>()
            .FromSharedImplementor<Implementor>( o => o.Keyed( 1 ) )
            .SetLifetime( DependencyLifetime.Singleton );

        var sut = builder.Build();

        var result1 = sut.RootScope.Locator.Resolve<IBar>();
        var result2 = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IFoo>();
        var result3 = sut.RootScope.GetKeyedLocator( "foo" ).Resolve<IQux>();

        Assertion.All(
                result1.TestRefEquals( result2 ),
                result2.TestRefEquals( result3 ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ThroughNonGenericMethod_ShouldBeEquivalentToResolvingThroughGenericMethod()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve( typeof( IDependencyContainer ) );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowMissingDependencyException_WhenDependencyTypeHasNotBeenRegistered()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<MissingDependencyException>(),
                    exc.TestIf().OfType<MissingDependencyException>( e => e.DependencyType.TestEquals( typeof( IFoo ) ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ThroughNonGenericMethod_ShouldThrowMissingDependencyException_WhenDependencyTypeHasNotBeenRegistered()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve( typeof( IFoo ) ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<MissingDependencyException>(),
                    exc.TestIf().OfType<MissingDependencyException>( e => e.DependencyType.TestEquals( typeof( IFoo ) ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowInvalidDependencyCastException_WhenCreatedObjectIsOfInvalidType()
    {
        var factory = Substitute.For<Func<IDependencyScope, object>>();
        factory.WithAnyArgs( _ => "foo" );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromFactory( factory );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<InvalidDependencyCastException>(),
                    exc.TestIf()
                        .OfType<InvalidDependencyCastException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( typeof( IFoo ) ),
                                e.ResultType.TestEquals( typeof( string ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ThroughNonGenericMethod_ShouldThrowInvalidDependencyCastException_WhenCreatedObjectIsOfInvalidType()
    {
        var factory = Substitute.For<Func<IDependencyScope, object>>();
        factory.WithAnyArgs( _ => "foo" );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromFactory( factory );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve( typeof( IFoo ) ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<InvalidDependencyCastException>(),
                    exc.TestIf()
                        .OfType<InvalidDependencyCastException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( typeof( IFoo ) ),
                                e.ResultType.TestEquals( typeof( string ) ) ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void ResolvingDependency_BasedOnFactory_ShouldThrowObjectDisposedException_WhenScopeIsDisposed(DependencyLifetime lifetime)
    {
        var factory = Substitute.For<Func<IDependencyScope, object>>();
        factory.WithAnyArgs( _ => new Implementor() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( lifetime ).FromFactory( factory );
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();
        scope.Dispose();

        var action = Lambda.Of( () => scope.Locator.Resolve<IFoo>() );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void ResolvingDependency_BasedOnImplementor_ShouldThrowObjectDisposedException_WhenScopeIsDisposed(DependencyLifetime lifetime)
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( lifetime ).FromType<Implementor>();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();
        scope.Dispose();

        var action = Lambda.Of( () => scope.Locator.Resolve<IFoo>() );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenCircularReferenceToSelfHasBeenDetected()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromFactory( s => s.Locator.Resolve<IFoo>() );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<CircularDependencyReferenceException>(),
                    exc.TestIf()
                        .OfType<CircularDependencyReferenceException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( typeof( IFoo ) ),
                                e.ImplementorType.TestEquals( typeof( IFoo ) ),
                                e.InnerException.TestType().AssignableTo<CircularDependencyReferenceException>(),
                                e.InnerException.TestIf()
                                    .OfType<CircularDependencyReferenceException>(
                                        inner => Assertion.All(
                                            inner.DependencyType.TestEquals( typeof( IFoo ) ),
                                            inner.ImplementorType.TestEquals( typeof( IFoo ) ),
                                            inner.InnerException.TestNull() ) ) ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( Implementor ) )]
    [InlineData( typeof( IQux ) )]
    [InlineData( typeof( IBar ) )]
    [InlineData( typeof( IFoo ) )]
    public void ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenIndirectCircularReferenceHasBeenDetected(Type type)
    {
        var cycle = new[] { typeof( IQux ), typeof( Implementor ), typeof( IFoo ), typeof( IBar ) };
        var index = Array.IndexOf( cycle, type );
        var firstType = ++index >= cycle.Length ? cycle[index = 0] : cycle[index];
        var secondType = ++index >= cycle.Length ? cycle[index = 0] : cycle[index];
        var thirdType = ++index >= cycle.Length ? cycle[0] : cycle[index];

        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromFactory( s => s.Locator.Resolve<Implementor>() );
        builder.Add<IBar>().FromFactory( s => s.Locator.Resolve<IQux>() );
        builder.Add<IFoo>().FromFactory( s => s.Locator.Resolve<IBar>() );
        builder.Add<Implementor>().FromFactory( s => s.Locator.Resolve<IFoo>() );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve( type ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<CircularDependencyReferenceException>(),
                    exc.TestIf()
                        .OfType<CircularDependencyReferenceException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( type ),
                                e.ImplementorType.TestEquals( type ),
                                e.InnerException.TestType().AssignableTo<CircularDependencyReferenceException>(),
                                e.InnerException.TestIf()
                                    .OfType<CircularDependencyReferenceException>(
                                        inner1 => Assertion.All(
                                            inner1.DependencyType.TestEquals( firstType ),
                                            inner1.ImplementorType.TestEquals( firstType ),
                                            inner1.InnerException.TestType().AssignableTo<CircularDependencyReferenceException>(),
                                            inner1.InnerException.TestIf()
                                                .OfType<CircularDependencyReferenceException>(
                                                    inner2 => Assertion.All(
                                                        inner2.DependencyType.TestEquals( secondType ),
                                                        inner2.ImplementorType.TestEquals( secondType ),
                                                        inner2.InnerException.TestType()
                                                            .AssignableTo<CircularDependencyReferenceException>(),
                                                        inner2.InnerException.TestIf()
                                                            .OfType<CircularDependencyReferenceException>(
                                                                inner3 => Assertion.All(
                                                                    inner3.DependencyType.TestEquals( thirdType ),
                                                                    inner3.ImplementorType.TestEquals( thirdType ),
                                                                    inner3.InnerException.TestType()
                                                                        .AssignableTo<CircularDependencyReferenceException>(),
                                                                    inner3.InnerException.TestIf()
                                                                        .OfType<CircularDependencyReferenceException>(
                                                                            inner4 => Assertion.All(
                                                                                inner4.DependencyType.TestEquals( type ),
                                                                                inner4.ImplementorType.TestEquals( type ),
                                                                                inner4.InnerException.TestNull() ) ) ) ) ) ) ) ) ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( IQux ) )]
    [InlineData( typeof( IBar ) )]
    [InlineData( typeof( IFoo ) )]
    public void ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenOnlyOneDependencyInCycleBasedOnFactory(Type type)
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromFactory( s => new ChainableQux( s.Locator.Resolve<IFoo>() ) );
        builder.Add<IBar>().FromType<ChainableBar>();
        builder.Add<IFoo>().FromType<ChainableFoo>();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve( type ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<CircularDependencyReferenceException>(),
                    exc.TestIf()
                        .OfType<CircularDependencyReferenceException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( type ),
                                e.ImplementorType.TestEquals( type ),
                                e.InnerException.TestNotNull() ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenOriginalTypeItselfIsNotPartOfCycle()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>()
            .FromFactory(
                s =>
                {
                    _ = s.Locator.Resolve<IBar>();
                    return new Implementor();
                } );

        builder.Add<IBar>().FromFactory( s => new ChainableBar( s.Locator.Resolve<IQux>() ) );
        builder.Add<IFoo>().FromType<ChainableFoo>();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<CircularDependencyReferenceException>(),
                    exc.TestIf()
                        .OfType<CircularDependencyReferenceException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( typeof( IFoo ) ),
                                e.ImplementorType.TestEquals( typeof( IFoo ) ),
                                e.InnerException.TestType().AssignableTo<CircularDependencyReferenceException>(),
                                e.InnerException.TestIf()
                                    .OfType<CircularDependencyReferenceException>(
                                        inner1 => Assertion.All(
                                            inner1.DependencyType.TestEquals( typeof( IBar ) ),
                                            inner1.ImplementorType.TestEquals( typeof( IBar ) ),
                                            inner1.InnerException.TestType().AssignableTo<CircularDependencyReferenceException>(),
                                            inner1.InnerException.TestIf()
                                                .OfType<CircularDependencyReferenceException>(
                                                    inner2 => Assertion.All(
                                                        inner2.DependencyType.TestEquals( typeof( IQux ) ),
                                                        inner2.ImplementorType.TestEquals( typeof( IQux ) ),
                                                        inner2.InnerException.TestType()
                                                            .AssignableTo<CircularDependencyReferenceException>(),
                                                        inner2.InnerException.TestIf()
                                                            .OfType<CircularDependencyReferenceException>(
                                                                inner3 => Assertion.All(
                                                                    inner3.DependencyType.TestEquals( typeof( IBar ) ),
                                                                    inner3.ImplementorType.TestEquals( typeof( IBar ) ),
                                                                    inner3.InnerException.TestNull() ) ) ) ) ) ) ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void
        ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenCircularReferenceHasBeenDetectedDuringOnResolvingCallback(
            DependencyLifetime lifetime)
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>()
            .SetLifetime( lifetime )
            .FromType<Implementor>()
            .SetOnResolvingCallback(
                (_, s) =>
                {
                    var __ = s.Locator.Resolve<IFoo>();
                } );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<CircularDependencyReferenceException>(),
                    exc.TestIf()
                        .OfType<CircularDependencyReferenceException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( typeof( IFoo ) ),
                                e.ImplementorType.TestEquals( typeof( IFoo ) ),
                                e.InnerException.TestType().AssignableTo<CircularDependencyReferenceException>(),
                                e.InnerException.TestIf()
                                    .OfType<CircularDependencyReferenceException>(
                                        inner => Assertion.All(
                                            inner.DependencyType.TestEquals( typeof( IFoo ) ),
                                            inner.ImplementorType.TestEquals( typeof( IFoo ) ),
                                            inner.InnerException.TestNull() ) ) ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void
        ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenCircularReferenceHasBeenDetectedDuringOnResolvingCallbackForCachedInstances(
            DependencyLifetime lifetime)
    {
        var forceCycle = false;
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>()
            .SetLifetime( lifetime )
            .FromType<Implementor>()
            .SetOnResolvingCallback(
                (_, s) =>
                {
                    if ( forceCycle )
                    {
                        var __ = s.Locator.Resolve<IFoo>();
                    }
                    else
                        forceCycle = true;
                } );

        var sut = builder.Build();

        _ = sut.RootScope.Locator.Resolve<IFoo>();
        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<CircularDependencyReferenceException>(),
                    exc.TestIf()
                        .OfType<CircularDependencyReferenceException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( typeof( IFoo ) ),
                                e.ImplementorType.TestEquals( typeof( IFoo ) ),
                                e.InnerException.TestType().AssignableTo<CircularDependencyReferenceException>(),
                                e.InnerException.TestIf()
                                    .OfType<CircularDependencyReferenceException>(
                                        inner => Assertion.All(
                                            inner.DependencyType.TestEquals( typeof( IFoo ) ),
                                            inner.ImplementorType.TestEquals( typeof( IFoo ) ),
                                            inner.InnerException.TestNull() ) ) ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void
        ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenCircularReferenceHasBeenDetectedDuringOnCreatedCallback(
            DependencyLifetime lifetime)
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>()
            .SetLifetime( lifetime )
            .FromType<Implementor>( o => o.SetOnCreatedCallback( (_, _, s) => { _ = s.Locator.Resolve<IFoo>(); } ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<CircularDependencyReferenceException>(),
                    exc.TestIf()
                        .OfType<CircularDependencyReferenceException>(
                            e => Assertion.All(
                                e.DependencyType.TestEquals( typeof( IFoo ) ),
                                e.ImplementorType.TestEquals( typeof( IFoo ) ),
                                e.InnerException.TestType().AssignableTo<CircularDependencyReferenceException>(),
                                e.InnerException.TestIf()
                                    .OfType<CircularDependencyReferenceException>(
                                        inner => Assertion.All(
                                            inner.DependencyType.TestEquals( typeof( IFoo ) ),
                                            inner.ImplementorType.TestEquals( typeof( IFoo ) ),
                                            inner.InnerException.TestNull() ) ) ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance(DependencyLifetime lifetime)
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().SetLifetime( lifetime ).FromFactory( _ => value );
        builder.Add<IWithText>().SetLifetime( lifetime ).FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( ExplicitCtorImplementor ) ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstanceForKeyedLocator(DependencyLifetime lifetime)
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<string>().SetLifetime( lifetime ).FromFactory( _ => value );
        builder.GetKeyedLocator( 1 ).Add<IWithText>().SetLifetime( lifetime ).FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( ExplicitCtorImplementor ) ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithParameterlessCtor_ShouldReturnCorrectInstance()
    {
        var ctor = typeof( Implementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IFoo>();

        result.GetType().TestEquals( typeof( Implementor ) ).Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParametersAreBuiltIn()
    {
        var ctor = typeof( BuiltInCtorParamImplementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IBuiltIn>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IBuiltIn>();

        Assertion.All(
                result.GetType().TestEquals( typeof( BuiltInCtorParamImplementor ) ),
                result.Container.TestRefEquals( sut ),
                result.Scope.TestRefEquals( sut.RootScope ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtorAndDefaultParameter_ShouldReturnCorrectInstance_WhenParameterIsNotResolvable()
    {
        var ctor = typeof( DefaultCtorParamImplementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( DefaultCtorParamImplementor ) ),
                result.Text.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtorAndDefaultParameter_ShouldReturnCorrectInstance_WhenParameterIsResolvable()
    {
        var ctor = typeof( DefaultCtorParamImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( DefaultCtorParamImplementor ) ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtorAndOptionalParameter_ShouldReturnCorrectInstance_WhenParameterIsNotResolvable()
    {
        var ctor = typeof( OptionalCtorParamImplementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( OptionalCtorParamImplementor ) ),
                result.Text.TestNull() )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtorAndOptionalParameter_ShouldReturnCorrectInstance_WhenParameterIsResolvable()
    {
        var ctor = typeof( OptionalCtorParamImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( OptionalCtorParamImplementor ) ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromFactory()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromConstructor( ctor, o => o.ResolveParameter( p => p.Name == "text", _ => value ) );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( ExplicitCtorImplementor ) ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromImplementorType()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>().FromConstructor( ctor, o => o.ResolveParameter( p => p.Name == "text", typeof( string ) ) );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( ExplicitCtorImplementor ) ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromKeyedImplementorType()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>()
            .FromConstructor( ctor, o => o.ResolveParameter( p => p.Name == "text", typeof( string ), c => c.Keyed( 1 ) ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( ExplicitCtorImplementor ) ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( FieldImplementor ) )]
    [InlineData( typeof( BackedPropertyImplementor ) )]
    [InlineData( typeof( BackedReadOnlyPropertyImplementor ) )]
    [InlineData( typeof( CustomPropertyImplementor ) )]
    public void ResolvingDependency_WithCtorAndMember_ShouldReturnCorrectInstance(Type implementorType)
    {
        var ctor = implementorType.GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( implementorType ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( OptionalFieldImplementor ) )]
    [InlineData( typeof( OptionalBackedPropertyImplementor ) )]
    [InlineData( typeof( OptionalBackedReadOnlyPropertyImplementor ) )]
    [InlineData( typeof( OptionalCustomPropertyImplementor ) )]
    public void ResolvingDependency_WithCtorAndOptionalMember_ShouldReturnCorrectInstance_WhenMemberIsNotResolvable(Type implementorType)
    {
        var ctor = implementorType.GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( implementorType ),
                result.Text.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( typeof( OptionalFieldImplementor ) )]
    [InlineData( typeof( OptionalBackedPropertyImplementor ) )]
    [InlineData( typeof( OptionalBackedReadOnlyPropertyImplementor ) )]
    [InlineData( typeof( OptionalCustomPropertyImplementor ) )]
    public void ResolvingDependency_WithCtorAndOptionalMember_ShouldReturnCorrectInstance_WhenMemberIsResolvable(Type implementorType)
    {
        var ctor = implementorType.GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( implementorType ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( FieldImplementor ) )]
    [InlineData( typeof( BackedPropertyImplementor ) )]
    [InlineData( typeof( BackedReadOnlyPropertyImplementor ) )]
    [InlineData( typeof( CustomPropertyImplementor ) )]
    public void ResolvingDependency_WithCtorAndMember_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromFactory(
        Type implementorType)
    {
        var ctor = implementorType.GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromConstructor( ctor, o => o.ResolveMember( m => m.Name.Contains( "_text" ), _ => value ) );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( implementorType ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( FieldImplementor ) )]
    [InlineData( typeof( BackedPropertyImplementor ) )]
    [InlineData( typeof( BackedReadOnlyPropertyImplementor ) )]
    [InlineData( typeof( CustomPropertyImplementor ) )]
    public void ResolvingDependency_WithCtorAndMember_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromImplementorType(
        Type implementorType)
    {
        var ctor = implementorType.GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>().FromConstructor( ctor, o => o.ResolveMember( m => m.Name.Contains( "_text" ), typeof( string ) ) );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( implementorType ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( FieldImplementor ) )]
    [InlineData( typeof( BackedPropertyImplementor ) )]
    [InlineData( typeof( BackedReadOnlyPropertyImplementor ) )]
    [InlineData( typeof( CustomPropertyImplementor ) )]
    public void ResolvingDependency_WithCtorAndMember_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromKeyedImplementorType(
        Type implementorType)
    {
        var ctor = implementorType.GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>()
            .FromConstructor( ctor, o => o.ResolveMember( m => m.Name.Contains( "_text" ), typeof( string ), c => c.Keyed( 1 ) ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( implementorType ),
                result.Text.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenMembersAreBuiltIn()
    {
        var ctor = typeof( BuiltInCtorMemberImplementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IBuiltIn>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IBuiltIn>();

        Assertion.All(
                result.GetType().TestEquals( typeof( BuiltInCtorMemberImplementor ) ),
                result.Container.TestRefEquals( sut ),
                result.Scope.TestRefEquals( sut.RootScope ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenResolvingChainOfDependencies()
    {
        var fooCtor = typeof( ChainableFoo ).GetConstructors().First();
        var barCtor = typeof( ChainableBar ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromConstructor( fooCtor );
        builder.Add<IBar>().FromConstructor( barCtor );
        builder.Add<IQux>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IFoo>();

        Assertion.All(
                result.TestType().Exact<ChainableFoo>(),
                result.TestIf()
                    .OfType<ChainableFoo>(
                        foo => Assertion.All(
                            foo.Bar.TestType().Exact<ChainableBar>(),
                            foo.Bar.TestIf().OfType<ChainableBar>( bar => bar.Qux.TestType().Exact<Implementor>() ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtorAndMember_ShouldReturnCorrectInstance_WhenResolvingChainOfDependencies()
    {
        var fooCtor = typeof( ChainableFieldFoo ).GetConstructors().First();
        var barCtor = typeof( ChainableFieldBar ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromConstructor( fooCtor );
        builder.Add<IBar>().FromConstructor( barCtor );
        builder.Add<IQux>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IFoo>();

        Assertion.All(
                result.TestType().Exact<ChainableFieldFoo>(),
                result.TestIf()
                    .OfType<ChainableFieldFoo>(
                        foo => Assertion.All(
                            foo.Bar.TestType().Exact<ChainableFieldBar>(),
                            foo.Bar.TestIf().OfType<ChainableFieldBar>( bar => bar.Qux.TestType().Exact<Implementor>() ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterAndMemberAreInjectable()
    {
        var ctor = typeof( CtorAndRefMemberImplementor ).GetConstructors().First();
        var stringValue = Fixture.Create<string>();
        var intValue = Fixture.Create<int>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => stringValue );
        builder.Add<int>().FromFactory( _ => intValue );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( CtorAndRefMemberImplementor ) ),
                result.Text.TestEquals( $"{stringValue}{intValue}" ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterAndMemberAreInjectableWithExplicitResolution()
    {
        var ctor = typeof( CtorAndRefMemberImplementor ).GetConstructors().First();
        var stringValue = Fixture.Create<string>();
        var intValue = Fixture.Create<int>();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>()
            .FromConstructor(
                ctor,
                o => o.ResolveParameter( p => p.Name == "value", _ => intValue )
                    .ResolveMember( m => m.Name == "_member", _ => stringValue ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( CtorAndRefMemberImplementor ) ),
                result.Text.TestEquals( $"{stringValue}{intValue}" ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterAndMemberAreInjectableWithNullableType()
    {
        var ctor = typeof( CtorAndValueMemberImplementor ).GetConstructors().First();
        var byteValue = Fixture.Create<byte>();
        var intValue = Fixture.Create<int>();

        var builder = new DependencyContainerBuilder();
        builder.Add<byte?>().FromFactory( _ => byteValue );
        builder.Add<int>().FromFactory( _ => intValue );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                result.GetType().TestEquals( typeof( CtorAndValueMemberImplementor ) ),
                result.Text.TestEquals( $"{intValue}{byteValue}" ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowInvalidDependencyCastException_WhenRefTypeDependencyIsOfIncorrectType()
    {
        var ctor = typeof( CtorAndRefMemberImplementor ).GetConstructors().First();
        var intValue = Fixture.Create<int>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => new object() );
        builder.Add<int>().FromFactory( _ => intValue );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IWithText>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<InvalidDependencyCastException>(),
                    exc.TestIf()
                        .OfType<InvalidDependencyCastException>(
                            e => Assertion.All( e.DependencyType.TestEquals( typeof( string ) ), e.ResultType.TestNull() ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowInvalidDependencyCastException_WhenValueTypeDependencyIsOfIncorrectType()
    {
        var ctor = typeof( CtorAndRefMemberImplementor ).GetConstructors().First();
        var stringValue = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => stringValue );
        builder.Add<int>().FromFactory( _ => new object() );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IWithText>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<InvalidDependencyCastException>(),
                    exc.TestIf()
                        .OfType<InvalidDependencyCastException>(
                            e => Assertion.All( e.DependencyType.TestEquals( typeof( int ) ), e.ResultType.TestNull() ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowInvalidDependencyCastException_WhenNullableValueTypeDependencyIsOfIncorrectType()
    {
        var ctor = typeof( CtorAndValueMemberImplementor ).GetConstructors().First();
        var intValue = Fixture.Create<int>();

        var builder = new DependencyContainerBuilder();
        builder.Add<byte?>().FromFactory( _ => new object() );
        builder.Add<int>().FromFactory( _ => intValue );
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IWithText>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<InvalidDependencyCastException>(),
                    exc.TestIf()
                        .OfType<InvalidDependencyCastException>(
                            e => Assertion.All( e.DependencyType.TestEquals( typeof( byte? ) ), e.ResultType.TestNull() ) ) ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_WithDefaultSetup_ShouldReturnCorrectInstance()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IBar>().FromType<Implementor>();
        builder.Add<ChainableFoo>();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<ChainableFoo>();

        result.Bar.TestType().AssignableTo<Implementor>().Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithOptionalParameterIsResolvable()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<MultiCtorImplementor>().FromConstructor();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<MultiCtorImplementor>();

        Assertion.All(
                result.Bar.TestNull(),
                result.Qux.TestType().AssignableTo<Implementor>() )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithNonOptionalParameterIsResolvable()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IBar>().FromType<Implementor>();
        builder.Add<MultiCtorImplementor>().FromConstructor();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<MultiCtorImplementor>();

        Assertion.All(
                result.Bar.TestType().AssignableTo<Implementor>(),
                result.Qux.TestNull() )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitFactoryIsChosenOverCtorWithNormallyInjectedDependency()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromType<Implementor>();
        builder.Add<MultiCtorImplementor>()
            .FromConstructor( o => o.ResolveParameter( p => p.ParameterType == typeof( IBar ), _ => new Implementor() ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<MultiCtorImplementor>();

        Assertion.All(
                result.Bar.TestType().AssignableTo<Implementor>(),
                result.Qux.TestNull() )
            .Go();
    }

    [Fact]
    public void
        ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitInjectionIsChosenOverCtorWithNormallyInjectedDependency()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromType<Implementor>();
        builder.Add<Implementor>();
        builder.Add<MultiCtorImplementor>()
            .FromConstructor( o => o.ResolveParameter( p => p.ParameterType == typeof( IBar ), typeof( Implementor ) ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<MultiCtorImplementor>();

        Assertion.All(
                result.Bar.TestType().AssignableTo<Implementor>(),
                result.Qux.TestNull() )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitInjectionIsInvalidAndOtherCtorIsChosen()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromType<Implementor>();
        builder.Add<MultiCtorImplementor>()
            .FromConstructor( o => o.ResolveParameter( p => p.ParameterType == typeof( IBar ), typeof( ChainableFoo ) ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<MultiCtorImplementor>();

        Assertion.All(
                result.Bar.TestNull(),
                result.Qux.TestType().AssignableTo<Implementor>() )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithInjectedDependencyIsChosenOverCtorWithCaptiveDependency()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().SetLifetime( DependencyLifetime.Transient ).FromType<Implementor>();
        builder.Add<IBar>().SetLifetime( DependencyLifetime.Singleton ).FromType<Implementor>();
        builder.Add<MultiCtorImplementor>().SetLifetime( DependencyLifetime.Scoped ).FromConstructor();

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<MultiCtorImplementor>();

        Assertion.All(
                result.Bar.TestType().AssignableTo<Implementor>(),
                result.Qux.TestNull() )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithInjectedCaptiveDependencyIsChosen()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().SetLifetime( DependencyLifetime.Transient ).FromType<Implementor>();
        builder.Add<MultiCtorImplementor>().SetLifetime( DependencyLifetime.Scoped ).FromConstructor();

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<MultiCtorImplementor>();

        Assertion.All(
                result.Bar.TestNull(),
                result.Qux.TestType().AssignableTo<Implementor>() )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithMostParametersIsChosenWhenManyWithTheSameScoreExist()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => string.Empty );
        builder.Add<IBar>().FromType<Implementor>();
        builder.Add<IQux>().FromType<Implementor>();
        builder.Add<IWithText>().FromType<ExplicitCtorImplementor>();
        builder.Add<SameCtorScoreImplementor>()
            .FromConstructor(
                o => o.ResolveParameter( p => p.Name == "bar1", typeof( IBar ) )
                    .ResolveParameter( p => p.Name == "qux1", typeof( IQux ) ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<SameCtorScoreImplementor>();

        Assertion.All(
                result.Bar.TestType().AssignableTo<Implementor>(),
                result.Qux.TestType().AssignableTo<Implementor>(),
                result.Text.TestNotNull() )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldCallOnCreatedCallback_WhenThereAreNoDependencies()
    {
        var callback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Singleton ).FromType<Implementor>( o => o.SetOnCreatedCallback( callback ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IFoo>();
        _ = sut.RootScope.Locator.Resolve<IFoo>();

        Assertion.All(
                callback.CallCount().TestEquals( 1 ),
                callback.CallAt( 0 ).Exists.TestTrue(),
                callback.CallAt( 0 ).Arguments.TestSequence( [ result, typeof( IFoo ), sut.RootScope ] ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldCallOnCreatedCallback_WhenNoDependencyIsOfRequiredValueType()
    {
        var callback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var builder = new DependencyContainerBuilder();
        builder.Add<IBar>().SetLifetime( DependencyLifetime.Singleton ).FromType<Implementor>();

        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Singleton ).FromType<ChainableFoo>( o => o.SetOnCreatedCallback( callback ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IFoo>();
        _ = sut.RootScope.Locator.Resolve<IFoo>();

        Assertion.All(
                callback.CallCount().TestEquals( 1 ),
                callback.CallAt( 0 ).Exists.TestTrue(),
                callback.CallAt( 0 ).Arguments.TestSequence( [ result, typeof( IFoo ), sut.RootScope ] ) )
            .Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldCallOnCreatedCallback_WhenSomeDependenciesAreOfRequiredValueType()
    {
        var callback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var builder = new DependencyContainerBuilder();
        builder.Add<int>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( _ => 0 );
        builder.Add<byte?>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( _ => ( byte? )0 );

        builder.Add<IWithText>()
            .SetLifetime( DependencyLifetime.Singleton )
            .FromType<CtorAndValueMemberImplementor>( o => o.SetOnCreatedCallback( callback ) );

        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();
        _ = sut.RootScope.Locator.Resolve<IWithText>();

        Assertion.All(
                callback.CallCount().TestEquals( 1 ),
                callback.CallAt( 0 ).Exists.TestTrue(),
                callback.CallAt( 0 ).Arguments.TestSequence( [ result, typeof( IWithText ), sut.RootScope ] ) )
            .Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldInvokeOnResolvingCallbackEveryTime()
    {
        var onResolvingCallback = Substitute.For<Action<Type, IDependencyScope>>();

        var builder = new DependencyContainerBuilder();
        builder.GetDependencyRange<IFoo>()
            .SetOnResolvingCallback( onResolvingCallback )
            .Add()
            .SetLifetime( DependencyLifetime.Singleton )
            .FromFactory( _ => new Implementor() );

        var sut = builder.Build();

        _ = sut.RootScope.Locator.Resolve<IEnumerable<IFoo>>();
        _ = sut.RootScope.Locator.Resolve<IEnumerable<IFoo>>();

        Assertion.All(
                onResolvingCallback.CallCount().TestEquals( 2 ),
                onResolvingCallback.CallAt( 0 ).Arguments.TestSequence( [ typeof( IEnumerable<IFoo> ), sut.RootScope ] ),
                onResolvingCallback.CallAt( 1 ).Arguments.TestSequence( [ typeof( IEnumerable<IFoo> ), sut.RootScope ] ) )
            .Go();
    }

    [Fact]
    public void ResolvingUnregisteredRangeDependency_ShouldReturnEmptyCollection_WhenDoingItForTheFirstTime()
    {
        var sut = new DependencyContainerBuilder().Build();
        var result = sut.RootScope.Locator.Resolve<IEnumerable<IFoo>>();
        result.TestEmpty().Go();
    }

    [Fact]
    public void ResolvingUnregisteredRangeDependency_ShouldReturnEmptyCollection_WhenDoingItForTheSecondTime()
    {
        var sut = new DependencyContainerBuilder().Build();
        var first = sut.RootScope.Locator.Resolve<IEnumerable<IFoo>>();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<IFoo>>();

        result.TestRefEquals( first ).Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorRequiresUnregisteredRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<RangeFoo>();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<RangeFoo>();

        result.Texts.TestEmpty().Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorRequiresEmptyRegisteredRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetDependencyRange<string>();
        builder.Add<RangeFoo>();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<RangeFoo>();

        result.Texts.TestEmpty().Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenMemberRequiresUnregisteredRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<RangeBar>();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<RangeBar>();

        result.Texts.TestEmpty().Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeDoesNotContainsAnyElements()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetDependencyRange<IFoo>();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<IFoo>>();

        result.TestEmpty().Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeOnlyContainsElementsExcludedFromRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().IncludeInRange( false ).FromType<Implementor>();
        builder.Add<IFoo>().IncludeInRange( false ).FromType<Implementor>();
        builder.Add<IFoo>().IncludeInRange( false ).FromType<Implementor>();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<IFoo>>();

        result.TestEmpty().Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeIsOfRefType()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<string>>();

        result.TestSequence( [ "foo", "bar", "qux" ] ).Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeIsOfNullableValueType()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<int?>().FromFactory( _ => ( int? )1 );
        builder.Add<int?>().FromFactory( _ => ( int? )2 );
        builder.Add<int?>().FromFactory( _ => ( int? )3 );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<int?>>();

        result.TestSequence( [ 1, 2, 3 ] ).Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeIsOfRequiredValueType()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<int>().FromFactory( _ => 1 );
        builder.Add<int>().FromFactory( _ => 2 );
        builder.Add<int>().FromFactory( _ => 3 );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<int>>();

        result.TestSequence( [ 1, 2, 3 ] ).Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnLastRegisteredInstance_WhenMoreThanOneElementIsRegisteredInRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<string>();

        result.TestEquals( "qux" ).Go();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenDependencyIsDecoratedRangeExcludingSelf()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromFactory( _ => new ExplicitCtorImplementor( "foo" ) );
        builder.Add<IWithText>().FromFactory( _ => new ExplicitCtorImplementor( "bar" ) );
        builder.Add<IWithText>().IncludeInRange( false ).FromType<RangeDecorator>();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IWithText>();

        result.Text.TestEquals( "foo|bar" ).Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenFirstElementIsExcluded()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().IncludeInRange( false ).FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<string>>();

        result.TestSequence( [ "bar", "qux" ] ).Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenSecondElementIsExcluded()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().IncludeInRange( false ).FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<string>>();

        result.TestSequence( [ "foo", "qux" ] ).Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeDependencyIsRegisteredExplicitly()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        builder.Add<IEnumerable<string>>().FromFactory( _ => new[] { "lorem", "ipsum" } );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<string>>();

        result.TestSequence( [ "lorem", "ipsum" ] ).Go();
    }

    [Fact]
    public void ResolvingRangeOfRangeDependency_ShouldReturnEmptyCollection_WhenRangeDependencyIsCreatedAutomatically()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<IEnumerable<string>>>();

        result.TestEmpty().Go();
    }

    [Fact]
    public void ResolvingRangeOfRangeDependency_ShouldReturnCorrectInstance_WhenRangeDependencyIsRegisteredExplicitly()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IEnumerable<string>>().FromFactory( _ => new[] { "foo", "bar" } );
        builder.Add<IEnumerable<string>>().FromFactory( _ => new[] { "qux", "baz" } );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<IEnumerable<string>>>();

        result.SelectMany( t => t ).TestSequence( [ "foo", "bar", "qux", "baz" ] ).Go();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenSomeElementsAreRegisteredAsKeyed()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.GetKeyedLocator( 1 ).Add<string>().FromFactory( _ => "qux" );
        builder.GetKeyedLocator( 1 ).Add<string>().FromFactory( _ => "baz" );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve<IEnumerable<string>>();
        var keyedResult = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IEnumerable<string>>();

        Assertion.All(
                result.TestSequence( [ "foo", "bar" ] ),
                keyedResult.TestSequence( [ "qux", "baz" ] ) )
            .Go();
    }

    [Fact]
    public void TryGetScope_ShouldReturnCorrectScope_WhenNameExists()
    {
        var sut = new DependencyContainerBuilder().Build();
        var scope = sut.RootScope.BeginScope( "foo" );

        var result = sut.TryGetScope( "foo" );

        result.TestRefEquals( scope ).Go();
    }

    [Fact]
    public void TryGetScope_ShouldReturnNull_WhenNameDoesNotExist()
    {
        var sut = new DependencyContainerBuilder().Build();
        _ = sut.RootScope.BeginScope( "foo" );

        var result = sut.TryGetScope( "bar" );

        result.TestNull().Go();
    }

    [Fact]
    public void TryGetScope_ShouldReturnNull_WhenContainerIsDisposed()
    {
        var sut = new DependencyContainerBuilder().Build();
        sut.Dispose();

        var result = sut.TryGetScope( "bar" );

        result.TestNull().Go();
    }

    [Fact]
    public void GetScope_ShouldReturnCorrectScope_WhenNameExists()
    {
        var sut = new DependencyContainerBuilder().Build();
        var scope = sut.RootScope.BeginScope( "foo" );

        var result = sut.GetScope( "foo" );

        result.TestRefEquals( scope ).Go();
    }

    [Fact]
    public void GetScope_ShouldThrowDependencyScopeNotFoundException_WhenNameDoesNotExist()
    {
        var sut = new DependencyContainerBuilder().Build();
        _ = sut.RootScope.BeginScope( "foo" );

        var action = Lambda.Of( () => sut.GetScope( "bar" ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<DependencyScopeNotFoundException>(),
                    exc.TestIf().OfType<DependencyScopeNotFoundException>( e => e.ScopeName.TestEquals( "bar" ) ) ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeAllActiveScopes()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var rootScope = sut.RootScope;
        var childScope = rootScope.BeginScope();
        var grandchildScope = childScope.BeginScope();

        sut.Dispose();

        Assertion.All(
                rootScope.IsDisposed.TestTrue(),
                childScope.IsDisposed.TestTrue(),
                grandchildScope.IsDisposed.TestTrue() )
            .Go();
    }

    [Fact]
    public void Dispose_CalledMoreThanOnce_ShouldDoNothing()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        sut.Dispose();

        sut.Dispose();

        sut.RootScope.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public void DependencyLocator_GetResolvableTypes_ShouldReturnAllResolvableTypesWithinThatLocator()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromFactory( _ => new Implementor() );
        builder.Add<IBar>().FromFactory( _ => new Implementor() );
        builder.Add<IQux>().FromFactory( _ => new Implementor() );
        builder.GetKeyedLocator( 1 ).Add<IWithText>().FromFactory( _ => new ExplicitCtorImplementor( string.Empty ) );
        var container = builder.Build();

        var sut = container.RootScope.Locator;

        var result = sut.GetResolvableTypes();

        result.TestSetEqual(
            [
                typeof( IFoo ),
                typeof( IBar ),
                typeof( IQux ),
                typeof( IDependencyContainer ),
                typeof( IDependencyScope ),
                typeof( IEnumerable<IFoo> ),
                typeof( IEnumerable<IBar> ),
                typeof( IEnumerable<IQux> )
            ] )
            .Go();
    }

    [Fact]
    public void DependencyLocator_GetResolvableTypes_ShouldReturnEmpty_WhenContainerIsDisposed()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromFactory( _ => new Implementor() );
        builder.Add<IBar>().FromFactory( _ => new Implementor() );
        builder.Add<IQux>().FromFactory( _ => new Implementor() );
        builder.GetKeyedLocator( 1 ).Add<IWithText>().FromFactory( _ => new ExplicitCtorImplementor( string.Empty ) );
        var container = builder.Build();
        var sut = container.RootScope.Locator;
        container.Dispose();

        var result = sut.GetResolvableTypes();

        result.TestEmpty().Go();
    }

    [Theory]
    [InlineData( typeof( IDependencyContainer ), DependencyLifetime.Singleton )]
    [InlineData( typeof( IDependencyScope ), DependencyLifetime.ScopedSingleton )]
    [InlineData( typeof( IFoo ), DependencyLifetime.Transient )]
    [InlineData( typeof( IBar ), DependencyLifetime.Scoped )]
    [InlineData( typeof( IQux ), DependencyLifetime.ScopedSingleton )]
    [InlineData( typeof( IWithText ), DependencyLifetime.Singleton )]
    [InlineData( typeof( OptionalCtorParamImplementor ), DependencyLifetime.Transient )]
    [InlineData( typeof( DefaultCtorParamImplementor ), DependencyLifetime.Scoped )]
    [InlineData( typeof( ExplicitCtorImplementor ), DependencyLifetime.ScopedSingleton )]
    [InlineData( typeof( FieldImplementor ), DependencyLifetime.Singleton )]
    [InlineData( typeof( IEnumerable<int> ), DependencyLifetime.Transient )]
    [InlineData( typeof( string ), null )]
    public void DependencyLocator_TryGetLifetime_ShouldReturnCorrectResult(Type type, DependencyLifetime? expected)
    {
        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>();
        builder.AddSharedImplementor<DefaultCtorParamImplementor>();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Transient ).FromSharedImplementor<Implementor>();
        builder.Add<IBar>().SetLifetime( DependencyLifetime.Scoped ).FromSharedImplementor<Implementor>();
        builder.Add<IQux>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromSharedImplementor<Implementor>();
        builder.Add<IWithText>().SetLifetime( DependencyLifetime.Singleton ).FromSharedImplementor<DefaultCtorParamImplementor>();
        builder.Add<OptionalCtorParamImplementor>().SetLifetime( DependencyLifetime.Transient ).FromFactory( _ => new object() );
        builder.Add<DefaultCtorParamImplementor>().SetLifetime( DependencyLifetime.Scoped ).FromFactory( _ => new object() );
        builder.Add<ExplicitCtorImplementor>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromFactory( _ => new object() );
        builder.Add<FieldImplementor>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( _ => new object() );

        var container = builder.Build();

        var sut = container.RootScope.Locator;
        _ = sut.Resolve<IEnumerable<int>>();

        var result = sut.TryGetLifetime( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void DependencyLocator_TryGetLifetime_ShouldReturnNull_WhenContainerIsDisposed()
    {
        var builder = new DependencyContainerBuilder();
        var container = builder.Build();
        var sut = container.RootScope.Locator;
        container.Dispose();

        var result = sut.TryGetLifetime( typeof( IDependencyContainer ) );

        result.TestNull().Go();
    }

    [Fact]
    public void LockDisposal_ShouldThrowSynchronizationLockException_WhenCurrentThreadHoldsAnyLocks()
    {
        var sut = new ReaderWriterLockSlim();
        sut.EnterReadLock();

        var action = Lambda.Of( () => sut.DisposeGracefully() );

        action.Test( exc => exc.TestType().Exact<SynchronizationLockException>() ).Go();
    }

    [Fact]
    public async Task LockDisposal_WhenAnotherThreadWaitsToAcquireLock_ShouldNotThrowAndShouldWaitForAnotherThreadToAcquireLock()
    {
        var taskSource = new TaskCompletionSource();
        var context1 = new DedicatedThreadSynchronizationContext();
        var context2 = new DedicatedThreadSynchronizationContext();
        var context3 = new DedicatedThreadSynchronizationContext();
        var taskFactory1 = new TaskFactory( TaskSchedulerCapture.FromSynchronizationContext( context1 ) );
        var taskFactory2 = new TaskFactory( TaskSchedulerCapture.FromSynchronizationContext( context2 ) );
        var taskFactory3 = new TaskFactory( TaskSchedulerCapture.FromSynchronizationContext( context3 ) );
        var sut = new ReaderWriterLockSlim();

        await taskFactory3.StartNew( () => sut.EnterReadLock() );
        _ = taskFactory2.StartNew( () => sut.EnterWriteLock() );
        await Task.Delay( 15 );

        _ = taskFactory1.StartNew(
            () =>
            {
                sut.DisposeGracefully();
                taskSource.SetResult();
            } );

        await Task.Delay( 50 );
        await taskFactory3.StartNew( () => sut.ExitReadLock() );

        await taskSource.Task;
    }

    [Fact]
    public void Helpers_CreateResolverFactory_ShouldReturnCachedCompilationResult_WhenCalledMoreThanOnce()
    {
        var container = new DependencyContainerBuilder().Build();
        var expression = Lambda.ExpressionOf( (DependencyScope _) => new object() );
        var resolver = new ResolverFactorySource( expression );
        var scope = container.InternalRootScope;

        var firstFactory = resolver.Factory;
        _ = firstFactory( scope );
        var secondFactory = resolver.Factory;
        _ = firstFactory( scope );
        var thirdFactory = resolver.Factory;

        Assertion.All(
                firstFactory.TestNotRefEquals( secondFactory ),
                secondFactory.TestRefEquals( thirdFactory ) )
            .Go();
    }

    private sealed class ResolverFactorySource : IResolverFactorySource
    {
        internal ResolverFactorySource(Expression<Func<DependencyScope, object>> expression)
        {
            Factory = expression.CreateResolverFactory( this );
        }

        public Func<DependencyScope, object> Factory { get; set; }
    }
}
