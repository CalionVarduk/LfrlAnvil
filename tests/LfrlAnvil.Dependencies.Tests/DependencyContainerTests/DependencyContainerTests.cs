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
    public void ResolvingDependencyLocator_ThroughRootScope_ShouldReturnRootScopeLocator()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope;

        var result = scope.Locator.Resolve<IDependencyLocator>();

        result.Should().Be( scope.Locator );
    }

    [Fact]
    public void ResolvingDependencyLocator_ThroughChildScope_ShouldReturnChildScopeLocator()
    {
        var builder = new DependencyContainerBuilder();
        var sut = builder.Build();
        var scope = sut.RootScope.BeginScope();

        var result = scope.Locator.Resolve<IDependencyLocator>();

        result.Should().Be( scope.Locator );
    }

    [Fact]
    public void ResolvingTransientDependency_ThroughRootThenChildScope_ShouldReturnNewInstanceEachTime()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        factory.WithAnyArgs( _ => Substitute.For<IFoo>() );

        var builder = new DependencyContainerBuilder();
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Transient );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Transient );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Transient );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Transient );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Singleton );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Singleton );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Singleton );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Singleton );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Scoped );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Scoped );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Scoped );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.Scoped );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.ScopedSingleton );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.ScopedSingleton );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.ScopedSingleton );
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
        builder.Add<IFoo>().FromFactory( factory ).SetLifetime( DependencyLifetime.ScopedSingleton );
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
    public void ResolvingDependency_ShouldThrowMissingDependencyException_WhenCreatedObjectIsOfInvalidType()
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
    public void ResolvingDependency_ThroughNonGenericMethod_ShouldThrowMissingDependencyException_WhenCreatedObjectIsOfInvalidType()
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
}
