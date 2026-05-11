using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Functional;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LfrlAnvil.Dependencies.MicrosoftExtensions.Tests;

public class DependencyContainerServiceProviderFactoryTests : TestsBase
{
    [Fact]
    public void ShouldCreateCorrectServiceProvider()
    {
        var sut = new DependencyContainerServiceProviderFactory();
        var builder = sut.CreateBuilder( new ServiceCollection() );
        var provider = sut.CreateServiceProvider( builder );

        var a = provider.GetRequiredService<ISupportRequiredService>();
        var b = provider.GetRequiredService<IKeyedServiceProvider>();
        var c = provider.GetRequiredService<IServiceProvider>();
        var d = provider.GetRequiredService<IServiceProviderIsKeyedService>();
        var e = provider.GetRequiredService<IServiceProviderIsService>();
        var f = provider.GetService<IServiceScope>() ?? new object();
        var g = provider.GetRequiredService<IDependencyContainer>();
        var h = provider.GetRequiredService<IDependencyScope>();
        var i = provider.GetRequiredService<IDependencyScopeFactory>();

        var j = provider.GetRequiredKeyedService<ISupportRequiredService>( 1 );
        var k = provider.GetRequiredKeyedService<IKeyedServiceProvider>( 1 );
        var l = provider.GetRequiredKeyedService<IServiceProvider>( 1 );
        var m = provider.GetRequiredKeyedService<IServiceProviderIsKeyedService>( 1 );
        var n = provider.GetRequiredKeyedService<IServiceProviderIsService>( 1 );
        var o = provider.GetKeyedService<IServiceScope>( 1 ) ?? new object();
        var p = provider.GetRequiredKeyedService<IDependencyContainer>( 1 );
        var q = provider.GetRequiredKeyedService<IDependencyScope>( 1 );
        var r = provider.GetRequiredKeyedService<IDependencyScopeFactory>( 1 );

        Assertion.All(
                new[] { provider, a, b, c, d, e, f, j, k, l, m, n, o }.Distinct().Count().TestEquals( 1 ),
                a.ToString().TestEquals( h.ToString() ),
                new object[] { g.RootScope, h, i, p.RootScope, q, r }.Distinct().Count().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task ShouldCreateCorrectServiceScopeFactory()
    {
        var sut = new DependencyContainerServiceProviderFactory();
        var builder = sut.CreateBuilder( new ServiceCollection() );
        var provider = sut.CreateServiceProvider( builder );

        var factory = provider.GetRequiredService<IServiceScopeFactory>();
        var factory2 = provider.GetRequiredKeyedService<IServiceScopeFactory>( 1 );
        var root = provider.GetRequiredService<IDependencyContainer>().RootScope;

        using var scope1 = factory.CreateScope();
        var factory3 = scope1.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        var a = scope1.ServiceProvider.GetRequiredService<IServiceScope>();
        var b = scope1.ServiceProvider.GetRequiredService<IServiceScope>();

        await using var scope2 = factory.CreateAsyncScope();
        var c = scope2.ServiceProvider.GetRequiredService<IServiceScope>();
        var d = scope2.ServiceProvider.GetRequiredService<IServiceScope>();

        var children = root.GetChildren();
        Assertion.All(
                new object[] { factory, factory2, factory3 }.Distinct().Count().TestEquals( 1 ),
                new object[] { a, b }.Distinct().Count().TestEquals( 1 ),
                new object[] { c, d }.Distinct().Count().TestEquals( 1 ),
                a.TestNotRefEquals( c ),
                children.TestCount( count => count.TestEquals( 2 ) )
                    .Then( ch => Assertion.All(
                        "children",
                        ch[0].ToString().TestEquals( scope1.ToString() ),
                        ch[1].ToString().TestEquals( scope2.ServiceProvider.ToString() ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( ServiceLifetime.Transient, DependencyLifetime.Transient )]
    [InlineData( ServiceLifetime.Scoped, DependencyLifetime.Scoped )]
    [InlineData( ServiceLifetime.Singleton, DependencyLifetime.Singleton )]
    public void ShouldTranslateBasicServices(ServiceLifetime serviceLifetime, DependencyLifetime dependencyLifetime)
    {
        var services = new ServiceCollection();
        services.Add( new ServiceDescriptor( typeof( IFoo ), typeof( FooBar ), serviceLifetime ) );
        services.Add( new ServiceDescriptor( typeof( IBar ), typeof( FooBar ), serviceLifetime ) );
        services.Add( new ServiceDescriptor( typeof( IQux ), 1, typeof( FooBar ), serviceLifetime ) );
        services.Add(
            new ServiceDescriptor(
                typeof( Parameterized<IDependencyContainer> ),
                typeof( Parameterized<IDependencyContainer> ),
                serviceLifetime ) );

        services.Add(
            new ServiceDescriptor(
                typeof( Parameterized<IServiceScope> ),
                typeof( Parameterized<IServiceScope> ),
                serviceLifetime ) );

        var sut = new DependencyContainerServiceProviderFactory();
        var builder = sut.CreateBuilder( services );
        var provider = sut.CreateServiceProvider( builder );
        var factory = provider.GetRequiredService<IServiceScopeFactory>();

        using var scope = factory.CreateScope();

        var a = scope.ServiceProvider.GetRequiredKeyedService<IServiceProviderIsKeyedService>( null )
            .IsKeyedService( typeof( IFoo ), null );

        var b = scope.ServiceProvider.GetKeyedService<IServiceProviderIsService>( null )?.IsService( typeof( IFoo ) ) ?? false;
        var c = scope.ServiceProvider.GetKeyedService<IServiceProviderIsService>( null )?.IsService( typeof( IGenericFoo<> ) ) ?? false;

        var foo = scope.ServiceProvider.GetRequiredService<IFoo>();
        var bar = scope.ServiceProvider.GetRequiredService<IBar>();
        var qux = scope.ServiceProvider.GetRequiredKeyedService<IQux>( 1 );
        var param1 = scope.ServiceProvider.GetService<Parameterized<IDependencyContainer>>();
        var param2 = scope.ServiceProvider.GetService<Parameterized<IServiceScope>>();

        Assertion.All(
                a.TestTrue(),
                b.TestTrue(),
                c.TestFalse(),
                foo.TestType().Exact<FooBar>(),
                bar.TestType().Exact<FooBar>(),
                qux.TestType().Exact<FooBar>(),
                param1.TestNotNull( p => Assertion.All(
                    "param1",
                    p.Inner.RootScope.Locator.TryGetLifetime( typeof( IFoo ) ).TestEquals( dependencyLifetime ) ) ),
                param2.TestNotNull( p => p.Inner.TestRefEquals( scope.ServiceProvider ) ) )
            .Go();
    }

    [Fact]
    public void ShouldTranslateGenericServices()
    {
        var services = new ServiceCollection();
        services.Add( new ServiceDescriptor( typeof( IGenericFoo<> ), typeof( GenericFooBar<> ), ServiceLifetime.Transient ) );
        services.Add( new ServiceDescriptor( typeof( IGenericBar<> ), typeof( GenericFooBar<> ), ServiceLifetime.Transient ) );
        services.Add( new ServiceDescriptor( typeof( IGenericQux<> ), 1, typeof( GenericFooBar<> ), ServiceLifetime.Transient ) );

        var sut = new DependencyContainerServiceProviderFactory();
        var builder = sut.CreateBuilder( services );
        var provider = sut.CreateServiceProvider( builder );
        var factory = provider.GetRequiredService<IServiceScopeFactory>();

        using var scope = factory.CreateScope();

        var foo = scope.ServiceProvider.GetRequiredService<IGenericFoo<string>>();
        var bar = scope.ServiceProvider.GetRequiredService<IGenericBar<int>>();
        var qux = scope.ServiceProvider.GetRequiredKeyedService<IGenericQux<double>>( 1 );

        Assertion.All(
                foo.TestType().Exact<GenericFooBar<string>>(),
                bar.TestType().Exact<GenericFooBar<int>>(),
                qux.TestType().Exact<GenericFooBar<double>>() )
            .Go();
    }

    [Fact]
    public void ShouldTranslateFactoryBasedServices()
    {
        var services = new ServiceCollection();
        services.Add( new ServiceDescriptor( typeof( IFoo ), _ => new FooBar(), ServiceLifetime.Transient ) );
        services.Add( new ServiceDescriptor( typeof( IBar ), _ => new FooBar(), ServiceLifetime.Transient ) );
        services.Add( new ServiceDescriptor( typeof( IQux ), 1, (_, _) => new FooBar(), ServiceLifetime.Transient ) );

        var sut = new DependencyContainerServiceProviderFactory();
        var builder = sut.CreateBuilder( services );
        var provider = sut.CreateServiceProvider( builder );
        var factory = provider.GetRequiredService<IServiceScopeFactory>();

        using var scope = factory.CreateScope();

        var foo = scope.ServiceProvider.GetRequiredService<IFoo>();
        var bar = scope.ServiceProvider.GetRequiredService<IBar>();
        var qux = scope.ServiceProvider.GetRequiredKeyedService<IQux>( 1 );

        Assertion.All(
                foo.TestType().Exact<FooBar>(),
                bar.TestType().Exact<FooBar>(),
                qux.TestType().Exact<FooBar>() )
            .Go();
    }

    [Fact]
    public void ShouldTranslateInstanceServices()
    {
        var services = new ServiceCollection();
        services.Add( new ServiceDescriptor( typeof( IFoo ), new FooBar() ) );
        services.Add( new ServiceDescriptor( typeof( IBar ), new FooBar() ) );
        services.Add( new ServiceDescriptor( typeof( IQux ), 1, new FooBar() ) );

        var sut = new DependencyContainerServiceProviderFactory();
        var builder = sut.CreateBuilder( services );
        var provider = sut.CreateServiceProvider( builder );
        var factory = provider.GetRequiredService<IServiceScopeFactory>();

        using var scope = factory.CreateScope();

        var foo = scope.ServiceProvider.GetRequiredService<IFoo>();
        var bar = scope.ServiceProvider.GetRequiredService<IBar>();
        var qux = scope.ServiceProvider.GetRequiredKeyedService<IQux>( 1 );

        Assertion.All(
                foo.TestType().Exact<FooBar>(),
                bar.TestType().Exact<FooBar>(),
                qux.TestType().Exact<FooBar>() )
            .Go();
    }

    [Fact]
    public void Callbacks_ShouldBeInvoked()
    {
        DependencyContainerBuilder? builderFromCallback = null;
        DependencyContainerBuildResult<DependencyContainer>? resultFromCallback = null;
        var sut = new DependencyContainerServiceProviderFactory(
            onBuild: b => builderFromCallback = b,
            onCreated: r => resultFromCallback = r );

        var builder = sut.CreateBuilder( new ServiceCollection() );
        var foundBuilder = builderFromCallback;
        _ = sut.CreateServiceProvider( builder );
        var foundResult = resultFromCallback;

        Assertion.All(
                builderFromCallback.TestRefEquals( foundBuilder ),
                builderFromCallback.TestRefEquals( builder ),
                foundResult.TestEquals( resultFromCallback ) )
            .Go();
    }

    [Fact]
    public void FromKeyedServices_ShouldBeRespectedByDefault()
    {
        var services = new ServiceCollection();
        services.Add( new ServiceDescriptor( typeof( FromKeyedServices ), typeof( FromKeyedServices ), ServiceLifetime.Transient ) );
        var sut = new DependencyContainerServiceProviderFactory(
            onBuild: b =>
            {
                b.GetKeyedLocator( 1 ).Add<IFoo>().FromType<FooBar>();
                b.Add<IFoo>().FromFactory( _ => Substitute.For<IFoo>() );
            } );

        var builder = sut.CreateBuilder( services );
        var provider = sut.CreateServiceProvider( builder );
        var factory = provider.GetRequiredService<IServiceScopeFactory>();

        using var scope = factory.CreateScope();

        var result = scope.ServiceProvider.GetRequiredService<FromKeyedServices>();

        result.Foo.TestType().Exact<FooBar>().Go();
    }

    [Fact]
    public void FromKeyedServices_ShouldBePossibleToDisable()
    {
        var services = new ServiceCollection();
        services.Add( new ServiceDescriptor( typeof( FromKeyedServices ), typeof( FromKeyedServices ), ServiceLifetime.Transient ) );
        var sut = new DependencyContainerServiceProviderFactory(
            supportFromKeyedServicesAttribute: false,
            onBuild: b =>
            {
                b.Add<IFoo>().FromType<FooBar>();
                b.GetKeyedLocator( 1 ).Add<IFoo>().FromFactory( _ => Substitute.For<IFoo>() );
            } );

        var builder = sut.CreateBuilder( services );
        var provider = sut.CreateServiceProvider( builder );
        var factory = provider.GetRequiredService<IServiceScopeFactory>();

        using var scope = factory.CreateScope();

        var result = scope.ServiceProvider.GetRequiredService<FromKeyedServices>();

        result.Foo.TestType().Exact<FooBar>().Go();
    }

    [Fact]
    public void CustomConstructorParameterKeyProvider_ShouldBeRespected_WhenFromKeyedServicesIsSupported()
    {
        var sut = new DependencyContainerServiceProviderFactory(
            onBuild: b =>
            {
                b.GetKeyedLocator( 1 ).Add<IFoo>().FromType<FooBar>();
                b.Add<IFoo>().FromFactory( _ => Substitute.For<IFoo>() );
                b.Add<Parameterized<IFoo>>();
                b.Configuration.SetConstructorParameterKeyProvider( p => p.ParameterType == typeof( IFoo ) ? 1 : null );
            } );

        var builder = sut.CreateBuilder( new ServiceCollection() );
        var provider = sut.CreateServiceProvider( builder );
        var factory = provider.GetRequiredService<IServiceScopeFactory>();

        using var scope = factory.CreateScope();

        var result = scope.ServiceProvider.GetRequiredService<Parameterized<IFoo>>();

        result.Inner.TestType().Exact<FooBar>().Go();
    }

    [Fact]
    public void AnyKey_ShouldNotBeSupported()
    {
        var services = new ServiceCollection();
        services.Add( new ServiceDescriptor( typeof( IFoo ), KeyedService.AnyKey, typeof( FooBar ), ServiceLifetime.Transient ) );
        var sut1 = new DependencyContainerServiceProviderFactory();
        var sut2 = new DependencyContainerServiceProviderFactory();
        var builder = sut2.CreateBuilder( new ServiceCollection() );
        var provider = sut2.CreateServiceProvider( builder );

        var action1 = Lambda.Of( () => sut1.CreateBuilder( services ) );
        var action2 = Lambda.Of( () => provider.GetKeyedService<IFoo>( KeyedService.AnyKey ) );
        var action3 = Lambda.Of( () => provider.GetKeyedService<IEnumerable<IFoo>>( KeyedService.AnyKey ) );

        Assertion.All(
                action1.Test( exc => exc.TestType().Exact<InvalidOperationException>() ),
                action2.Test( exc => exc.TestType().Exact<InvalidOperationException>() ),
                action3.Test( exc => exc.TestType().Exact<InvalidOperationException>() ) )
            .Go();
    }

    public interface IFoo { }

    public interface IBar { }

    public interface IQux { }

    public interface IGenericFoo<T> { }

    public interface IGenericBar<T> { }

    public interface IGenericQux<T> { }

    public class FooBar : IFoo, IBar, IQux { }

    public class GenericFooBar<T> : IGenericFoo<T>, IGenericBar<T>, IGenericQux<T> { }

    public class Parameterized<T>
    {
        public Parameterized(T inner)
        {
            Inner = inner;
        }

        public T Inner { get; }
    }

    public class FromKeyedServices
    {
        public FromKeyedServices([FromKeyedServices( 1 )] IFoo foo)
        {
            Foo = foo;
        }

        public IFoo Foo { get; }
    }
}
