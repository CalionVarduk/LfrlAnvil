using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Dependencies.Tests.DependencyContainerTests;

public class DependencyContainerTests : DependencyTestsBase
{
    [Fact]
    public void ResolvingDependencyContainer_ThroughRootScope_ShouldReturnContainerItself()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope;

        var result = scope.Locator.Resolve<IDependencyContainer>();

        result.Should().Be( sut );
    }

    [Fact]
    public void ResolvingDependencyContainer_ThroughChildScope_ShouldReturnContainerItself()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.Locator.Resolve<IDependencyContainer>();

        result.Should().Be( sut );
    }

    [Fact]
    public void ResolvingDependencyScope_ThroughRootScope_ShouldReturnRootScope()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope;

        var result = scope.Locator.Resolve<IDependencyScope>();

        result.Should().Be( scope );
    }

    [Fact]
    public void ResolvingDependencyScope_ThroughChildScope_ShouldReturnChildScope()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.Locator.Resolve<IDependencyScope>();

        result.Should().Be( scope );
    }

    [Fact]
    public void ResolvingDependencyContainer_ThroughUnregisteredKeyedLocator_ShouldReturnContainerItself()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IDependencyContainer>();

        result.Should().Be( sut );
    }

    [Fact]
    public void ResolvingDependencyScope_ThroughUnregisteredKeyedLocator_ShouldReturnScopeItself()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.GetKeyedLocator( 1 ).Resolve<IDependencyScope>();

        result.Should().Be( scope );
    }

    [Fact]
    public void ResolvingDependencyContainer_ThroughRegisteredKeyedLocator_ShouldReturnContainerItself()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IDependencyContainer>();

        result.Should().Be( sut );
    }

    [Fact]
    public void ResolvingDependencyScope_ThroughRegisteredKeyedLocator_ShouldReturnScopeItself()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.GetKeyedLocator( 1 ).Resolve<IDependencyScope>();

        result.Should().Be( scope );
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 4 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 2 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            factory.Verify().CallAt( 3 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            new[] { result1, result2, result3, result4 }.Should().OnlyHaveUniqueItems();
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 4 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            factory.Verify().CallAt( 2 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 3 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            new[] { result1, result2, result3, result4 }.Should().OnlyHaveUniqueItems();
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 4 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 2 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            factory.Verify().CallAt( 3 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            new[] { result1, result2, result3, result4 }.Should().OnlyHaveUniqueItems();
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 4 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            factory.Verify().CallAt( 2 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 3 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            new[] { result1, result2, result3, result4 }.Should().OnlyHaveUniqueItems();
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            new[] { result1, result2, result3, result4 }.Distinct().Should().HaveCount( 1 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            new[] { result1, result2, result3, result4 }.Distinct().Should().HaveCount( 1 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            new[] { result1, result2, result3, result4 }.Distinct().Should().HaveCount( 1 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            new[] { result1, result2, result3, result4 }.Distinct().Should().HaveCount( 1 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 2 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            result1.Should().NotBeSameAs( result2 );
            result1.Should().BeSameAs( result3 );
            result2.Should().BeSameAs( result4 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 2 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            result1.Should().NotBeSameAs( result2 );
            result1.Should().BeSameAs( result3 );
            result2.Should().BeSameAs( result4 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 2 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            result1.Should().NotBeSameAs( result2 );
            result1.Should().BeSameAs( result3 );
            result2.Should().BeSameAs( result4 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 2 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            result1.Should().NotBeSameAs( result2 );
            result1.Should().BeSameAs( result3 );
            result2.Should().BeSameAs( result4 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            new[] { result1, result2, result3, result4 }.Distinct().Should().HaveCount( 1 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 2 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( rootScope );
            result1.Should().NotBeSameAs( result2 );
            result1.Should().BeSameAs( result3 );
            result2.Should().BeSameAs( result4 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            new[] { result1, result2, result3, result4 }.Distinct().Should().HaveCount( 1 );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 2 );
            factory.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( childScope );
            factory.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( parentScope );
            result1.Should().NotBeSameAs( result2 );
            result1.Should().BeSameAs( result3 );
            result2.Should().BeSameAs( result4 );
        }
    }

    [Fact]
    public void ResolvingTransientDependency_WithSharedImplementor_ShouldReturnNewInstanceEachTimeForAllSharingDependencies()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => new Implementor() );

        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>().FromFactory( factory );
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Transient );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Transient );

        var sut = builder.Build();
        var scope = sut.RootScope;

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.Locator.Resolve<IBar>();

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 2 );
            new object[] { result1, result2 }.Should().OnlyHaveUniqueItems();
        }
    }

    [Fact]
    public void ResolvingSingletonDependency_WithSharedImplementor_ShouldReturnSameInstanceEachTimeForAllSharingDependencies()
    {
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        factory.WithAnyArgs( _ => new Implementor() );

        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>().FromFactory( factory );
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Singleton );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Singleton );

        var sut = builder.Build();
        var scope = sut.RootScope;

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.Locator.Resolve<IBar>();

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            result1.Should().BeSameAs( result2 );
        }
    }

    [Fact]
    public void
        ResolvingScopedSingletonDependency_WithSharedImplementor_ShouldReturnNewInstancePerScopeAndItsChildrenForAllSharingDependencies()
    {
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        factory.WithAnyArgs( _ => new Implementor() );

        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>().FromFactory( factory );
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.ScopedSingleton );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.ScopedSingleton );

        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.BeginScope().Locator.Resolve<IBar>();

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            result1.Should().BeSameAs( result2 );
        }
    }

    [Fact]
    public void ResolvingScopedDependency_WithSharedImplementor_ShouldReturnSameInstancePerScopeEachTimeForAllSharingDependencies()
    {
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        factory.WithAnyArgs( _ => new Implementor() );

        var builder = new DependencyContainerBuilder();
        builder.AddSharedImplementor<Implementor>().FromFactory( factory );
        builder.Add<IFoo>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Scoped );
        builder.Add<IBar>().FromSharedImplementor<Implementor>().SetLifetime( DependencyLifetime.Scoped );

        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result1 = scope.Locator.Resolve<IFoo>();
        var result2 = scope.Locator.Resolve<IBar>();

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 1 );
            result1.Should().BeSameAs( result2 );
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldInvokeOnResolvingCallbackEveryTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        factory.WithAnyArgs( _ => new Implementor() );

        var onResolvingCallback = Substitute.For<Action<Type, IDependencyScope>>();

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>()
            .SetLifetime( DependencyLifetime.Singleton )
            .FromFactory( factory )
            .SetOnResolvingCallback( onResolvingCallback );

        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var _ = scope.Locator.Resolve<IFoo>();
        var __ = scope.Locator.Resolve<IFoo>();

        using ( new AssertionScope() )
        {
            onResolvingCallback.Verify().CallCount.Should().Be( 2 );
            onResolvingCallback.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( typeof( IFoo ), scope );
            onResolvingCallback.Verify().CallAt( 1 ).Arguments.Should().BeSequentiallyEqualTo( typeof( IFoo ), scope );
        }
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

        using ( new AssertionScope() )
        {
            factory.Verify().CallCount.Should().Be( 2 );
            result1.Should().BeSameAs( result2 );
            result1.Should().NotBeSameAs( result3 );
        }
    }

    [Fact]
    public void KeyedDependencyResolutionAttempt_ThroughGlobalLocator_ShouldReturnNull()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.Locator.TryResolve<IFoo>();

        result.Should().BeNull();
    }

    [Fact]
    public void KeyedDependencyResolutionAttempt_ThroughKeyedLocatorWithCorrectKeyTypeButDifferentKey_ShouldReturnNull()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 2 ).TryResolve<IFoo>();

        result.Should().BeNull();
    }

    [Fact]
    public void KeyedDependencyResolutionAttempt_ThroughKeyedLocatorWithIncorrectKeyType_ShouldReturnNull()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( "foo" ).TryResolve<IFoo>();

        result.Should().BeNull();
    }

    [Fact]
    public void KeyedDependencyResolutionAttempt_ThroughKeyedLocatorWithCorrectKey_ShouldReturnCorrectInstance()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => new Implementor() );
        var sut = builder.Build();

        var result = sut.RootScope.GetKeyedLocator( 1 ).TryResolve<IFoo>();

        result.Should().NotBeNull();
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

        using ( new AssertionScope() )
        {
            result1.Should().BeSameAs( result2 );
            result2.Should().BeSameAs( result3 );
        }
    }

    [Fact]
    public void ResolvingDependency_ThroughNonGenericMethod_ShouldBeEquivalentToResolvingThroughGenericMethod()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var result = sut.RootScope.Locator.Resolve( typeof( IDependencyContainer ) );

        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowMissingDependencyException_WhenDependencyTypeHasNotBeenRegistered()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Should().ThrowExactly<MissingDependencyException>().AndMatch( e => e.DependencyType == typeof( IFoo ) );
    }

    [Fact]
    public void ResolvingDependency_ThroughNonGenericMethod_ShouldThrowMissingDependencyException_WhenDependencyTypeHasNotBeenRegistered()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve( typeof( IFoo ) ) );

        action.Should().ThrowExactly<MissingDependencyException>().AndMatch( e => e.DependencyType == typeof( IFoo ) );
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

        action.Should()
            .ThrowExactly<InvalidDependencyCastException>()
            .AndMatch( e => e.DependencyType == typeof( IFoo ) && e.ResultType == typeof( string ) );
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

        action.Should()
            .ThrowExactly<InvalidDependencyCastException>()
            .AndMatch( e => e.DependencyType == typeof( IFoo ) && e.ResultType == typeof( string ) );
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowObjectDisposedException_WhenScopeIsDisposed()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();
        scope.Dispose();

        var action = Lambda.Of( () => scope.Locator.Resolve<IDependencyContainer>() );

        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenCircularReferenceToSelfHasBeenDetected()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromFactory( s => s.Locator.Resolve<IFoo>() );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IFoo>() );

        action.Should()
            .ThrowExactly<CircularDependencyReferenceException>()
            .AndMatch(
                e => e.DependencyType == typeof( IFoo ) &&
                    e.ImplementorType == typeof( IFoo ) &&
                    e.InnerException is CircularDependencyReferenceException inner &&
                    inner.DependencyType == typeof( IFoo ) &&
                    inner.ImplementorType == typeof( IFoo ) &&
                    inner.InnerException is null );
    }

    [Fact]
    public void ResolvingDependency_ShouldThrowCircularDependencyReferenceException_WhenIndirectCircularReferenceHasBeenDetected()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromFactory( s => s.Locator.Resolve<Implementor>() );
        builder.Add<IBar>().FromFactory( s => s.Locator.Resolve<IQux>() );
        builder.Add<IFoo>().FromFactory( s => s.Locator.Resolve<IBar>() );
        builder.Add<Implementor>().FromFactory( s => s.Locator.Resolve<IFoo>() );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<Implementor>() );

        action.Should()
            .ThrowExactly<CircularDependencyReferenceException>()
            .AndMatch(
                e => e.DependencyType == typeof( Implementor ) &&
                    e.ImplementorType == typeof( Implementor ) &&
                    e.InnerException is CircularDependencyReferenceException inner1 &&
                    inner1.DependencyType == typeof( IFoo ) &&
                    inner1.ImplementorType == typeof( IFoo ) &&
                    inner1.InnerException is CircularDependencyReferenceException inner2 &&
                    inner2.DependencyType == typeof( IBar ) &&
                    inner2.ImplementorType == typeof( IBar ) &&
                    inner2.InnerException is CircularDependencyReferenceException inner3 &&
                    inner3.DependencyType == typeof( IQux ) &&
                    inner3.ImplementorType == typeof( IQux ) &&
                    inner3.InnerException is CircularDependencyReferenceException inner4 &&
                    inner4.DependencyType == typeof( Implementor ) &&
                    inner4.ImplementorType == typeof( Implementor ) &&
                    inner4.InnerException is null );
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( ExplicitCtorImplementor ) );
            result.Text.Should().BeSameAs( value );
        }
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

        var result = sut.ActiveScope.GetKeyedLocator( 1 ).Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( ExplicitCtorImplementor ) );
            result.Text.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ResolvingDependency_WithParameterlessCtor_ShouldReturnCorrectInstance()
    {
        var ctor = typeof( Implementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IFoo>();

        result.GetType().Should().Be( typeof( Implementor ) );
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParametersAreBuiltIn()
    {
        var ctor = typeof( BuiltInCtorParamImplementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IBuiltIn>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IBuiltIn>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( BuiltInCtorParamImplementor ) );
            result.Container.Should().BeSameAs( sut );
            result.Scope.Should().BeSameAs( sut.ActiveScope );
        }
    }

    [Fact]
    public void ResolvingDependency_WithCtorAndDefaultParameter_ShouldReturnCorrectInstance_WhenParameterIsNotResolvable()
    {
        var ctor = typeof( DefaultCtorParamImplementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( DefaultCtorParamImplementor ) );
            result.Text.Should().Be( "foo" );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( DefaultCtorParamImplementor ) );
            result.Text.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ResolvingDependency_WithCtorAndOptionalParameter_ShouldReturnCorrectInstance_WhenParameterIsNotResolvable()
    {
        var ctor = typeof( OptionalCtorParamImplementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( OptionalCtorParamImplementor ) );
            result.Text.Should().BeNull();
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( OptionalCtorParamImplementor ) );
            result.Text.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromFactory()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromConstructor( ctor, o => o.ResolveParameter( p => p.Name == "text", _ => value ) );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( ExplicitCtorImplementor ) );
            result.Text.Should().BeSameAs( value );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( ExplicitCtorImplementor ) );
            result.Text.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromKeyedImplementorType()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();
        var value = Fixture.Create<string>();

        var builder = new DependencyContainerBuilder();
        builder.GetKeyedLocator( 1 ).Add<string>().FromFactory( _ => value );
        builder.Add<IWithText>()
            .FromConstructor(
                ctor,
                o => o.ResolveParameter( p => p.Name == "text", typeof( string ), c => c.Keyed( 1 ) ) );

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( ExplicitCtorImplementor ) );
            result.Text.Should().BeSameAs( value );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( implementorType );
            result.Text.Should().BeSameAs( value );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( implementorType );
            result.Text.Should().BeNull();
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( implementorType );
            result.Text.Should().BeSameAs( value );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( implementorType );
            result.Text.Should().BeSameAs( value );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( implementorType );
            result.Text.Should().BeSameAs( value );
        }
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
            .FromConstructor(
                ctor,
                o => o.ResolveMember( m => m.Name.Contains( "_text" ), typeof( string ), c => c.Keyed( 1 ) ) );

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( implementorType );
            result.Text.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenMembersAreBuiltIn()
    {
        var ctor = typeof( BuiltInCtorMemberImplementor ).GetConstructors().First();

        var builder = new DependencyContainerBuilder();
        builder.Add<IBuiltIn>().FromConstructor( ctor );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IBuiltIn>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( BuiltInCtorMemberImplementor ) );
            result.Container.Should().BeSameAs( sut );
            result.Scope.Should().BeSameAs( sut.ActiveScope );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IFoo>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( ChainableFoo ) );
            if ( result is not ChainableFoo foo )
                return;

            foo.Bar.GetType().Should().Be( typeof( ChainableBar ) );
            if ( foo.Bar is not ChainableBar bar )
                return;

            bar.Qux.GetType().Should().Be( typeof( Implementor ) );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IFoo>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( ChainableFieldFoo ) );
            if ( result is not ChainableFieldFoo foo )
                return;

            foo.Bar.GetType().Should().Be( typeof( ChainableFieldBar ) );
            if ( foo.Bar is not ChainableFieldBar bar )
                return;

            bar.Qux.GetType().Should().Be( typeof( Implementor ) );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( CtorAndRefMemberImplementor ) );
            result.Text.Should().Be( $"{stringValue}{intValue}" );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( CtorAndRefMemberImplementor ) );
            result.Text.Should().Be( $"{stringValue}{intValue}" );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( CtorAndValueMemberImplementor ) );
            result.Text.Should().Be( $"{intValue}{byteValue}" );
        }
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

        var action = Lambda.Of( () => sut.ActiveScope.Locator.Resolve<IWithText>() );

        action.Should()
            .ThrowExactly<InvalidDependencyCastException>()
            .AndMatch( e => e.DependencyType == typeof( string ) && e.ResultType is null );
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

        var action = Lambda.Of( () => sut.ActiveScope.Locator.Resolve<IWithText>() );

        action.Should()
            .ThrowExactly<InvalidDependencyCastException>()
            .AndMatch( e => e.DependencyType == typeof( int ) && e.ResultType is null );
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

        var action = Lambda.Of( () => sut.ActiveScope.Locator.Resolve<IWithText>() );

        action.Should()
            .ThrowExactly<InvalidDependencyCastException>()
            .AndMatch( e => e.DependencyType == typeof( byte? ) && e.ResultType is null );
    }

    [Fact]
    public void ResolvingDependency_WithDefaultSetup_ShouldReturnCorrectInstance()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IBar>().FromType<Implementor>();
        builder.Add<ChainableFoo>();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<ChainableFoo>();

        result.Bar.Should().BeOfType( typeof( Implementor ) );
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithOptionalParameterIsResolvable()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<MultiCtorImplementor>().FromConstructor();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<MultiCtorImplementor>();

        using ( new AssertionScope() )
        {
            result.Bar.Should().BeNull();
            result.Qux.Should().BeOfType( typeof( Implementor ) );
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithNonOptionalParameterIsResolvable()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IBar>().FromType<Implementor>();
        builder.Add<MultiCtorImplementor>().FromConstructor();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<MultiCtorImplementor>();

        using ( new AssertionScope() )
        {
            result.Bar.Should().BeOfType( typeof( Implementor ) );
            result.Qux.Should().BeNull();
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitFactoryIsChosenOverCtorWithNormallyInjectedDependency()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromType<Implementor>();
        builder.Add<MultiCtorImplementor>()
            .FromConstructor(
                o => o.ResolveParameter( p => p.ParameterType == typeof( IBar ), _ => new Implementor() ) );

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<MultiCtorImplementor>();

        using ( new AssertionScope() )
        {
            result.Bar.Should().BeOfType( typeof( Implementor ) );
            result.Qux.Should().BeNull();
        }
    }

    [Fact]
    public void
        ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitInjectionIsChosenOverCtorWithNormallyInjectedDependency()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromType<Implementor>();
        builder.Add<Implementor>();
        builder.Add<MultiCtorImplementor>()
            .FromConstructor(
                o => o.ResolveParameter( p => p.ParameterType == typeof( IBar ), typeof( Implementor ) ) );

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<MultiCtorImplementor>();

        using ( new AssertionScope() )
        {
            result.Bar.Should().BeOfType( typeof( Implementor ) );
            result.Qux.Should().BeNull();
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitInjectionIsInvalidAndOtherCtorIsChosen()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().FromType<Implementor>();
        builder.Add<MultiCtorImplementor>()
            .FromConstructor(
                o => o.ResolveParameter( p => p.ParameterType == typeof( IBar ), typeof( ChainableFoo ) ) );

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<MultiCtorImplementor>();

        using ( new AssertionScope() )
        {
            result.Bar.Should().BeNull();
            result.Qux.Should().BeOfType( typeof( Implementor ) );
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithInjectedDependencyIsChosenOverCtorWithCaptiveDependency()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().SetLifetime( DependencyLifetime.Transient ).FromType<Implementor>();
        builder.Add<IBar>().SetLifetime( DependencyLifetime.Singleton ).FromType<Implementor>();
        builder.Add<MultiCtorImplementor>().SetLifetime( DependencyLifetime.Scoped ).FromConstructor();

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<MultiCtorImplementor>();

        using ( new AssertionScope() )
        {
            result.Bar.Should().BeOfType( typeof( Implementor ) );
            result.Qux.Should().BeNull();
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithInjectedCaptiveDependencyIsChosen()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IQux>().SetLifetime( DependencyLifetime.Transient ).FromType<Implementor>();
        builder.Add<MultiCtorImplementor>().SetLifetime( DependencyLifetime.Scoped ).FromConstructor();

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<MultiCtorImplementor>();

        using ( new AssertionScope() )
        {
            result.Bar.Should().BeNull();
            result.Qux.Should().BeOfType( typeof( Implementor ) );
        }
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

        var result = sut.ActiveScope.Locator.Resolve<SameCtorScoreImplementor>();

        using ( new AssertionScope() )
        {
            result.Bar.Should().BeOfType( typeof( Implementor ) );
            result.Qux.Should().BeOfType( typeof( Implementor ) );
            result.Text.Should().NotBeNull();
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldCallOnCreatedCallback_WhenThereAreNoDependencies()
    {
        var callback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>()
            .SetLifetime( DependencyLifetime.Singleton )
            .FromType<Implementor>( o => o.SetOnCreatedCallback( callback ) );

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IFoo>();
        var _ = sut.ActiveScope.Locator.Resolve<IFoo>();

        using ( new AssertionScope() )
        {
            callback.Verify().CallCount.Should().Be( 1 );
            callback.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( result, typeof( IFoo ), sut.ActiveScope );
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldCallOnCreatedCallback_WhenNoDependencyIsOfRequiredValueType()
    {
        var callback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var builder = new DependencyContainerBuilder();
        builder.Add<IBar>().SetLifetime( DependencyLifetime.Singleton ).FromType<Implementor>();

        builder.Add<IFoo>()
            .SetLifetime( DependencyLifetime.Singleton )
            .FromType<ChainableFoo>( o => o.SetOnCreatedCallback( callback ) );

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IFoo>();
        var _ = sut.ActiveScope.Locator.Resolve<IFoo>();

        using ( new AssertionScope() )
        {
            callback.Verify().CallCount.Should().Be( 1 );
            callback.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( result, typeof( IFoo ), sut.ActiveScope );
        }
    }

    [Fact]
    public void ResolvingDependency_ShouldCallOnCreatedCallback_WhenSomeDependenciesAreOfRequiredValueType()
    {
        var callback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var builder = new DependencyContainerBuilder();
        builder.Add<int>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( _ => 0 );
        builder.Add<byte?>().SetLifetime( DependencyLifetime.Singleton ).FromFactory( _ => (byte?)0 );

        builder.Add<IWithText>()
            .SetLifetime( DependencyLifetime.Singleton )
            .FromType<CtorAndValueMemberImplementor>( o => o.SetOnCreatedCallback( callback ) );

        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();
        var _ = sut.ActiveScope.Locator.Resolve<IWithText>();

        using ( new AssertionScope() )
        {
            callback.Verify().CallCount.Should().Be( 1 );
            callback.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( result, typeof( IWithText ), sut.ActiveScope );
        }
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

        var _ = sut.ActiveScope.Locator.Resolve<IEnumerable<IFoo>>();
        var __ = sut.ActiveScope.Locator.Resolve<IEnumerable<IFoo>>();

        using ( new AssertionScope() )
        {
            onResolvingCallback.Verify().CallCount.Should().Be( 2 );

            onResolvingCallback.Verify()
                .CallAt( 0 )
                .Arguments.Should()
                .BeSequentiallyEqualTo( typeof( IEnumerable<IFoo> ), sut.ActiveScope );

            onResolvingCallback.Verify()
                .CallAt( 1 )
                .Arguments.Should()
                .BeSequentiallyEqualTo( typeof( IEnumerable<IFoo> ), sut.ActiveScope );
        }
    }

    [Fact]
    public void ResolvingUnregisteredRangeDependency_ShouldReturnEmptyCollection_WhenDoingItForTheFirstTime()
    {
        var sut = new DependencyContainerBuilder().Build();
        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<IFoo>>();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ResolvingUnregisteredRangeDependency_ShouldReturnEmptyCollection_WhenDoingItForTheSecondTime()
    {
        var sut = new DependencyContainerBuilder().Build();
        var first = sut.ActiveScope.Locator.Resolve<IEnumerable<IFoo>>();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<IFoo>>();

        result.Should().BeSameAs( first );
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorRequiresUnregisteredRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<RangeFoo>();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<RangeFoo>();

        result.Texts.Should().BeEmpty();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorRequiresEmptyRegisteredRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetDependencyRange<string>();
        builder.Add<RangeFoo>();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<RangeFoo>();

        result.Texts.Should().BeEmpty();
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenMemberRequiresUnregisteredRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<RangeBar>();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<RangeBar>();

        result.Texts.Should().BeEmpty();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeDoesNotContainsAnyElements()
    {
        var builder = new DependencyContainerBuilder();
        builder.GetDependencyRange<IFoo>();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<IFoo>>();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeOnlyContainsElementsExcludedFromRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().IncludeInRange( false ).FromType<Implementor>();
        builder.Add<IFoo>().IncludeInRange( false ).FromType<Implementor>();
        builder.Add<IFoo>().IncludeInRange( false ).FromType<Implementor>();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<IFoo>>();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeIsOfRefType()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<string>>();

        result.Should().BeSequentiallyEqualTo( "foo", "bar", "qux" );
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeIsOfNullableValueType()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<int?>().FromFactory( _ => (int?)1 );
        builder.Add<int?>().FromFactory( _ => (int?)2 );
        builder.Add<int?>().FromFactory( _ => (int?)3 );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<int?>>();

        result.Should().BeSequentiallyEqualTo( 1, 2, 3 );
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeIsOfRequiredValueType()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<int>().FromFactory( _ => 1 );
        builder.Add<int>().FromFactory( _ => 2 );
        builder.Add<int>().FromFactory( _ => 3 );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<int>>();

        result.Should().BeSequentiallyEqualTo( 1, 2, 3 );
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnLastRegisteredInstance_WhenMoreThanOneElementIsRegisteredInRange()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<string>();

        result.Should().Be( "qux" );
    }

    [Fact]
    public void ResolvingDependency_ShouldReturnCorrectInstance_WhenDependencyIsDecoratedRangeExcludingSelf()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IWithText>().FromFactory( _ => new ExplicitCtorImplementor( "foo" ) );
        builder.Add<IWithText>().FromFactory( _ => new ExplicitCtorImplementor( "bar" ) );
        builder.Add<IWithText>().IncludeInRange( false ).FromType<RangeDecorator>();
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IWithText>();

        result.Text.Should().Be( "foo|bar" );
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenFirstElementIsExcluded()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().IncludeInRange( false ).FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<string>>();

        result.Should().BeSequentiallyEqualTo( "bar", "qux" );
    }

    [Fact]
    public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenSecondElementIsExcluded()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().IncludeInRange( false ).FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<string>>();

        result.Should().BeSequentiallyEqualTo( "foo", "qux" );
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

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<string>>();

        result.Should().BeSequentiallyEqualTo( "lorem", "ipsum" );
    }

    [Fact]
    public void ResolvingRangeOfRangeDependency_ShouldReturnEmptyCollection_WhenRangeDependencyIsCreatedAutomatically()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<string>().FromFactory( _ => "foo" );
        builder.Add<string>().FromFactory( _ => "bar" );
        builder.Add<string>().FromFactory( _ => "qux" );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<IEnumerable<string>>>();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ResolvingRangeOfRangeDependency_ShouldReturnCorrectInstance_WhenRangeDependencyIsRegisteredExplicitly()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IEnumerable<string>>().FromFactory( _ => new[] { "foo", "bar" } );
        builder.Add<IEnumerable<string>>().FromFactory( _ => new[] { "qux", "baz" } );
        var sut = builder.Build();

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<IEnumerable<string>>>();

        result.SelectMany( t => t ).Should().BeSequentiallyEqualTo( "foo", "bar", "qux", "baz" );
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

        var result = sut.ActiveScope.Locator.Resolve<IEnumerable<string>>();
        var keyedResult = sut.ActiveScope.GetKeyedLocator( 1 ).Resolve<IEnumerable<string>>();

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( "foo", "bar" );
            keyedResult.Should().BeSequentiallyEqualTo( "qux", "baz" );
        }
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

        using ( new AssertionScope() )
        {
            rootScope.IsDisposed.Should().BeTrue();
            childScope.IsDisposed.Should().BeTrue();
            grandchildScope.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_CalledMoreThanOnce_ShouldDoNothing()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        sut.Dispose();

        sut.Dispose();

        sut.RootScope.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ActiveScope_ShouldBeRootScope_WhenChildScopeForCurrentThreadDoesNotExist()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var expected = sut.RootScope;

        var result = sut.ActiveScope;

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void ActiveScope_ShouldBeChildScopeForCurrentThread_WhenChildScopeForCurrentThreadExists()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var expected = sut.RootScope.BeginScope();

        var result = sut.ActiveScope;

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void DependencyLocator_ResolvableTypes_ShouldReturnAllResolvableTypesWithinThatLocator()
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromFactory( _ => new Implementor() );
        builder.Add<IBar>().FromFactory( _ => new Implementor() );
        builder.Add<IQux>().FromFactory( _ => new Implementor() );
        builder.GetKeyedLocator( 1 ).Add<IWithText>().FromFactory( _ => new ExplicitCtorImplementor( string.Empty ) );
        var container = builder.Build();

        var sut = container.ActiveScope.Locator;

        var result = sut.ResolvableTypes;

        result.Should()
            .BeEquivalentTo(
                typeof( IFoo ),
                typeof( IBar ),
                typeof( IQux ),
                typeof( IDependencyContainer ),
                typeof( IDependencyScope ),
                typeof( IEnumerable<IFoo> ),
                typeof( IEnumerable<IBar> ),
                typeof( IEnumerable<IQux> ) );
    }

    [Theory]
    [InlineData( typeof( IDependencyContainer ), DependencyLifetime.Singleton )]
    [InlineData( typeof( IDependencyScope ), DependencyLifetime.ScopedSingleton )]
    [InlineData( typeof( IFoo ), DependencyLifetime.Transient )]
    [InlineData( typeof( IBar ), DependencyLifetime.Scoped )]
    [InlineData( typeof( IQux ), DependencyLifetime.ScopedSingleton )]
    [InlineData( typeof( IWithText ), DependencyLifetime.Singleton )]
    [InlineData( typeof( string ), null )]
    public void DependencyLocator_TryGetLifetime_ShouldReturnCorrectResult(Type type, DependencyLifetime? expected)
    {
        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().SetLifetime( DependencyLifetime.Transient ).FromFactory( _ => new Implementor() );
        builder.Add<IBar>().SetLifetime( DependencyLifetime.Scoped ).FromFactory( _ => new Implementor() );
        builder.Add<IQux>().SetLifetime( DependencyLifetime.ScopedSingleton ).FromFactory( _ => new Implementor() );
        builder.Add<IWithText>()
            .SetLifetime( DependencyLifetime.Singleton )
            .FromFactory( _ => new ExplicitCtorImplementor( string.Empty ) );

        var container = builder.Build();

        var sut = container.ActiveScope.Locator;

        var result = sut.TryGetLifetime( type );

        result.Should().Be( expected );
    }
}
