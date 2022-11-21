using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FluentAssertions.Execution;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Dependencies.Tests.DependencyContainerBuilderTests;

public class DependencyContainerBuilderTests : DependencyTestsBase
{
    [Fact]
    public void Ctor_ShouldCreateWithDefaultTransientLifetimeAndUseDisposableInterfaceStrategy()
    {
        var sut = new DependencyContainerBuilder();
        sut.DefaultLifetime.Should().Be( DependencyLifetime.Transient );
        sut.DefaultDisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
        sut.DefaultDisposalStrategy.Callback.Should().BeNull();
        sut.InjectablePropertyType.Should().BeSameAs( typeof( Injected<> ) );
        sut.OptionalDependencyAttributeType.Should().BeSameAs( typeof( AllowNullAttribute ) );
        ((IDependencyLocatorBuilder)sut).KeyType.Should().BeNull();
        ((IDependencyLocatorBuilder)sut).Key.Should().BeNull();
        ((IDependencyLocatorBuilder)sut).IsKeyed.Should().BeFalse();
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void SetDefaultLifetime_ShouldUpdateDefaultLifetimeAndCauseNewDependenciesToStartWithThatLifetime(DependencyLifetime lifetime)
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultLifetime( lifetime );
        var dependency = sut.Add<Implementor>();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultLifetime.Should().Be( lifetime );
            dependency.Lifetime.Should().Be( sut.DefaultLifetime );
        }
    }

    [Fact]
    public void SetDefaultLifetime_ShouldThrowArgumentException_WhenValueIsNotValid()
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => { sut.SetDefaultLifetime( (DependencyLifetime)4 ); } );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void
        SetDefaultDisposalStrategy_ShouldUpdateDisposalStrategyToUseDisposableInterfaceAndCauseNewImplementorsToStartWithThatStrategy()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultDisposalStrategy( DependencyImplementorDisposalStrategy.UseDisposableInterface() );
        var implementor = sut.Add<Implementor>().FromFactory( _ => new object() );
        var sharedImplementor = sut.AddSharedImplementor<Implementor>();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultDisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            sut.DefaultDisposalStrategy.Callback.Should().BeNull();
            implementor.DisposalStrategy.Should().BeEquivalentTo( sut.DefaultDisposalStrategy );
            sharedImplementor.DisposalStrategy.Should().BeEquivalentTo( sut.DefaultDisposalStrategy );
        }
    }

    [Fact]
    public void SetDefaultDisposalStrategy_ShouldUpdateDisposalStrategyToRenounceOwnershipAndCauseNewImplementorsToStartWithThatStrategy()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );
        var implementor = sut.Add<Implementor>().FromFactory( _ => new object() );
        var sharedImplementor = sut.AddSharedImplementor<Implementor>();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultDisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.RenounceOwnership );
            sut.DefaultDisposalStrategy.Callback.Should().BeNull();
            implementor.DisposalStrategy.Should().BeEquivalentTo( sut.DefaultDisposalStrategy );
            sharedImplementor.DisposalStrategy.Should().BeEquivalentTo( sut.DefaultDisposalStrategy );
        }
    }

    [Fact]
    public void SetDefaultDisposalStrategy_ShouldUpdateDisposalStrategyToUseCallbackAndCauseNewImplementorsToStartWithThatStrategy()
    {
        var callback = Substitute.For<Action<object>>();
        var sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultDisposalStrategy( DependencyImplementorDisposalStrategy.UseCallback( callback ) );
        var implementor = sut.Add<Implementor>().FromFactory( _ => new object() );
        var sharedImplementor = sut.AddSharedImplementor<Implementor>();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultDisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseCallback );
            sut.DefaultDisposalStrategy.Callback.Should().BeSameAs( callback );
            implementor.DisposalStrategy.Should().BeEquivalentTo( sut.DefaultDisposalStrategy );
            sharedImplementor.DisposalStrategy.Should().BeEquivalentTo( sut.DefaultDisposalStrategy );
        }
    }

    [Theory]
    [InlineData( typeof( Injected<> ) )]
    [InlineData( typeof( InjectedWithPublicCtor<> ) )]
    [InlineData( typeof( InjectedWithPrivateCtor<> ) )]
    public void SetInjectablePropertyType_ShouldUpdateTheTypeIfItIsValid(Type type)
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();

        var result = sut.SetInjectablePropertyType( type );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.InjectablePropertyType.Should().BeSameAs( type );
        }
    }

    [Theory]
    [InlineData( typeof( InjectedWithPublicCtor<string> ) )]
    [InlineData( typeof( InvalidNonGenericInjected ) )]
    [InlineData( typeof( InvalidInjectedWithTooManyArgs<,> ) )]
    [InlineData( typeof( InvalidInjectedWithParameterlessCtor<> ) )]
    [InlineData( typeof( InvalidInjectedWithIncorrectCtorParamType<> ) )]
    [InlineData( typeof( InvalidInjectedWithTooManyCtorParams<> ) )]
    public void SetInjectablePropertyType_ShouldThrowDependencyContainerBuilderConfigurationException_WhenTypeIsNotValid(Type type)
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.SetInjectablePropertyType( type ) );
        action.Should().ThrowExactly<DependencyContainerBuilderConfigurationException>();
    }

    [Theory]
    [InlineData( typeof( AllowNullAttribute ) )]
    [InlineData( typeof( OptionalAttribute ) )]
    [InlineData( typeof( ExtendedOptionalAttribute ) )]
    public void SetOptionalDependencyAttributeType_ShouldUpdateTheTypeIfItIsValid(Type type)
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();

        var result = sut.SetOptionalDependencyAttributeType( type );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.OptionalDependencyAttributeType.Should().BeSameAs( type );
        }
    }

    [Theory]
    [InlineData( typeof( object ) )]
    [InlineData( typeof( Injected<> ) )]
    [InlineData( typeof( Injected<string> ) )]
    [InlineData( typeof( NonParameterAttribute ) )]
    public void SetOptionalDependencyAttributeType_ShouldThrowDependencyContainerBuilderConfigurationException_WhenTypeIsNotValid(Type type)
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.SetOptionalDependencyAttributeType( type ) );
        action.Should().ThrowExactly<DependencyContainerBuilderConfigurationException>();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void GetKeyedLocator_ShouldReturnLocatorBuilderWithCorrectKey(int key)
    {
        var sut = new DependencyContainerBuilder();
        var result = sut.GetKeyedLocator( key );

        using ( new AssertionScope() )
        {
            result.Key.Should().Be( key );
            ((IDependencyLocatorBuilder)result).Key.Should().Be( key );
            result.KeyType.Should().Be( typeof( int ) );
            result.IsKeyed.Should().BeTrue();
        }
    }

    [Fact]
    public void GetKeyedLocator_ShouldReturnCorrectCachedLocatorBuilder_WhenCalledMoreThanOnceWithTheSameKey()
    {
        var sut = new DependencyContainerBuilder();

        var result1 = sut.GetKeyedLocator( 1 );
        var result2 = sut.GetKeyedLocator( 1 );

        result1.Should().Be( result2 );
    }

    [Fact]
    public void Add_ShouldAddNewDependencyWithoutImplementationDetailsAndWithDefaultLifetime()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.Add( typeof( IFoo ) );

        using ( new AssertionScope() )
        {
            result.DependencyType.Should().Be( typeof( IFoo ) );
            result.Lifetime.Should().Be( sut.DefaultLifetime );
            result.Implementor.Should().BeNull();
            result.SharedImplementorKey.Should().BeNull();
        }
    }

    [Fact]
    public void Add_FromFactory_ShouldSetProvidedFactoryAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromFactory( factory );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder.Implementor );
            builder.SharedImplementorKey.Should().BeNull();
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Constructor.Should().BeNull();
            result.Factory.Should().BeSameAs( factory );
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
            result.OnResolvingCallback.Should().BeNull();
        }
    }

    [Fact]
    public void Add_FromConstructor_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder.Implementor );
            builder.SharedImplementorKey.Should().BeNull();
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Factory.Should().BeNull();
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
            result.OnResolvingCallback.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void Add_FromConstructor_WithConfiguration_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
    {
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( o => o.SetOnCreatedCallback( onCreatedCallback ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder.Implementor );
            builder.SharedImplementorKey.Should().BeNull();
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Factory.Should().BeNull();
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
            result.OnResolvingCallback.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void Add_FromConstructor_WithInfo_ShouldSetConstructorAsCreationDetail()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( ctor );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder.Implementor );
            builder.SharedImplementorKey.Should().BeNull();
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Factory.Should().BeNull();
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
            result.OnResolvingCallback.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeSameAs( ctor );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void Add_FromConstructor_WithInfoAndConfiguration_ShouldSetConstructorAsCreationDetail()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( ctor, o => o.SetOnCreatedCallback( onCreatedCallback ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder.Implementor );
            builder.SharedImplementorKey.Should().BeNull();
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Factory.Should().BeNull();
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
            result.OnResolvingCallback.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeSameAs( ctor );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void Add_FromConstructor_ShouldUpdateConstructorInfoAsCreationDetail_WhenCalledMultipleTimes()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromConstructor();

        var result = builder.FromConstructor( ctor );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder.Implementor );
            builder.SharedImplementorKey.Should().BeNull();
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Factory.Should().BeNull();
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
            result.OnResolvingCallback.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeSameAs( ctor );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Singleton )]
    public void Add_SetLifetime_ShouldUpdateDependencyLifetime(DependencyLifetime lifetime)
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var result = builder.SetLifetime( lifetime );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.Lifetime.Should().Be( lifetime );
        }
    }

    [Fact]
    public void Add_SetLifetime_ShouldThrowArgumentException_WhenValueIsNotValid()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var action = Lambda.Of( () => { builder.SetLifetime( (DependencyLifetime)4 ); } );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Add_FromSharedImplementor_ShouldSetProvidedSharedImplementorTypeAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromFactory( factory );

        var result = builder.FromSharedImplementor( typeof( Implementor ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.Implementor.Should().BeNull();
            result.SharedImplementorKey.Should().NotBeNull();
            if ( result.SharedImplementorKey is null )
                return;

            result.SharedImplementorKey.Type.Should().Be( typeof( Implementor ) );
            result.SharedImplementorKey.Key.Should().BeNull();
            result.SharedImplementorKey.KeyType.Should().BeNull();
            result.SharedImplementorKey.IsKeyed.Should().BeFalse();
        }
    }

    [Fact]
    public void Add_FromSharedImplementor_Keyed_ShouldSetProvidedSharedImplementorTypeAndKeyAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromFactory( factory );

        var result = builder.FromSharedImplementor<Implementor>( o => o.Keyed( 1 ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.Implementor.Should().BeNull();
            result.SharedImplementorKey.Should().NotBeNull();
            if ( result.SharedImplementorKey is null )
                return;

            result.SharedImplementorKey.Type.Should().Be( typeof( Implementor ) );
            result.SharedImplementorKey.Key.Should().Be( 1 );
            result.SharedImplementorKey.KeyType.Should().Be( typeof( int ) );
            result.SharedImplementorKey.IsKeyed.Should().BeTrue();
        }
    }

    [Fact]
    public void Add_FromSharedImplementor_ShouldSetProvidedSharedImplementorTypeAndLocatorKeyAsCreationDetail_WhenLocatorIsKeyed()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.GetKeyedLocator( 1 ).Add( typeof( IFoo ) );
        builder.FromFactory( factory );

        var result = builder.FromSharedImplementor( typeof( Implementor ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.Implementor.Should().BeNull();
            result.SharedImplementorKey.Should().NotBeNull();
            if ( result.SharedImplementorKey is null )
                return;

            result.SharedImplementorKey.Type.Should().Be( typeof( Implementor ) );
            result.SharedImplementorKey.Key.Should().Be( 1 );
            result.SharedImplementorKey.KeyType.Should().Be( typeof( int ) );
            result.SharedImplementorKey.IsKeyed.Should().BeTrue();
        }
    }

    [Fact]
    public void Add_FromSharedImplementor_NotKeyed_ShouldSetProvidedSharedImplementorTypeWithoutKeyAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.GetKeyedLocator( 1 ).Add( typeof( IFoo ) );
        builder.FromFactory( factory );

        var result = builder.FromSharedImplementor<Implementor>( o => o.NotKeyed() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.Implementor.Should().BeNull();
            result.SharedImplementorKey.Should().NotBeNull();
            if ( result.SharedImplementorKey is null )
                return;

            result.SharedImplementorKey.Type.Should().Be( typeof( Implementor ) );
            result.SharedImplementorKey.Key.Should().BeNull();
            result.SharedImplementorKey.KeyType.Should().BeNull();
            result.SharedImplementorKey.IsKeyed.Should().BeFalse();
        }
    }

    [Fact]
    public void AddSharedImplementor_ShouldAddNewSharedImplementorWithoutCreationDetails()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.AddSharedImplementor( typeof( Implementor ) );

        using ( new AssertionScope() )
        {
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
            result.OnResolvingCallback.Should().BeNull();
        }
    }

    [Fact]
    public void AddSharedImplementor_ShouldDoNothingAndReturnPreviousBuilder_WhenAddingExistingSharedImplementorType()
    {
        var sut = new DependencyContainerBuilder();
        var expected = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = sut.AddSharedImplementor( typeof( Implementor ) );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void AddSharedImplementor_FromFactory_ShouldSetProvidedFactoryAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromFactory( factory );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeSameAs( factory );
            result.Constructor.Should().BeNull();
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithConfiguration_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
    {
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( o => o.SetOnCreatedCallback( onCreatedCallback ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithInfo_ShouldSetConstructorAsCreationDetail()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( ctor );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeSameAs( ctor );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithInfoAndConfiguration_ShouldSetConstructorAsCreationDetail()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( ctor, o => o.SetOnCreatedCallback( onCreatedCallback ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeSameAs( ctor );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithParameterResolutions_ShouldAddConstructorParameterResolutionsAsCreationDetail()
    {
        var predicate1 = Substitute.For<Func<ParameterInfo, bool>>();
        var predicate2 = Substitute.For<Func<ParameterInfo, bool>>();
        var predicate3 = Substitute.For<Func<ParameterInfo, bool>>();
        var factory = Lambda.ExpressionOf( (IDependencyScope s) => Substitute.For<Func<IDependencyScope, object>>()( s ) );
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor(
            o => o.AddParameterResolution( predicate1, _ => { } )
                .AddParameterResolution( predicate2, p => p.FromFactory( factory ) )
                .AddParameterResolution( predicate3, p => p.FromImplementor( typeof( IFoo ), i => i.Keyed( 1 ) ) ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().HaveCount( 3 );

            result.Constructor.InvocationOptions.ParameterResolutions.ElementAtOrDefault( 0 )
                .Should()
                .BeEquivalentTo( DependencyConstructorParameterResolution.Unspecified( predicate1 ) );

            result.Constructor.InvocationOptions.ParameterResolutions.ElementAtOrDefault( 1 )
                .Should()
                .BeEquivalentTo( DependencyConstructorParameterResolution.FromFactory( predicate2, factory ) );

            result.Constructor.InvocationOptions.ParameterResolutions.ElementAtOrDefault( 2 )
                .Should()
                .BeEquivalentTo(
                    DependencyConstructorParameterResolution.FromImplementorKey(
                        predicate3,
                        new DependencyImplementorKey<int>( typeof( IFoo ), 1 ) ) );
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithClearedParameterResolutions_ShouldClearConstructorParameterResolutions()
    {
        var predicate = Substitute.For<Func<ParameterInfo, bool>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( o => o.AddParameterResolution( predicate, _ => { } ).ClearParameterResolutions() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddSharedImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToUseDisposableInterfaceCorrectly()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.UseDisposableInterface() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
        }
    }

    [Fact]
    public void AddSharedImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToRenounceOwnershipCorrectly()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.RenounceOwnership );
            result.DisposalStrategy.Callback.Should().BeNull();
        }
    }

    [Fact]
    public void AddSharedImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToUseCallbackCorrectly()
    {
        var callback = Substitute.For<Action<object>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.UseCallback( callback ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseCallback );
            result.DisposalStrategy.Callback.Should().BeSameAs( callback );
        }
    }

    [Fact]
    public void TryGetDependency_ShouldReturnNull_WhenDependencyTypeWasNotRegistered()
    {
        var sut = new DependencyContainerBuilder();
        var result = sut.TryGetDependency( typeof( IFoo ) );
        result.Should().BeNull();
    }

    [Fact]
    public void TryGetDependency_ShouldReturnCorrectBuilder_WhenDependencyTypeWasRegistered()
    {
        var sut = new DependencyContainerBuilder();
        var expected = sut.Add<IFoo>();

        var result = sut.TryGetDependency( typeof( IFoo ) );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void TryGetSharedImplementor_ShouldReturnNull_WhenSharedImplementorTypeWasNotRegistered()
    {
        var sut = new DependencyContainerBuilder();
        var result = sut.TryGetSharedImplementor( typeof( Implementor ) );
        result.Should().BeNull();
    }

    [Fact]
    public void TryGetSharedImplementor_ShouldReturnCorrectBuilder_WhenSharedImplementorTypeWasRegistered()
    {
        var sut = new DependencyContainerBuilder();
        var expected = sut.AddSharedImplementor<Implementor>();

        var result = sut.TryGetSharedImplementor( typeof( Implementor ) );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingSharedImplementor()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add<IFoo>().FromSharedImplementor<Implementor>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.DependencyType.Should().Be( builder.DependencyType );
            message.ImplementorKey.Should().BeSameAs( builder.SharedImplementorKey );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingKeyedSharedImplementorAndNonCachedKeyType()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add<IFoo>().FromSharedImplementor<Implementor>( o => o.Keyed( 1 ) );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.DependencyType.Should().Be( builder.DependencyType );
            message.ImplementorKey.Should().BeSameAs( builder.SharedImplementorKey );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingKeyedSharedImplementorAndCachedKeyType()
    {
        var sut = new DependencyContainerBuilder();
        var _ = sut.GetKeyedLocator( 1 );
        var builder = sut.Add<IFoo>().FromSharedImplementor<Implementor>( o => o.Keyed( 2 ) );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.DependencyType.Should().Be( builder.DependencyType );
            message.ImplementorKey.Should().BeSameAs( builder.SharedImplementorKey );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void Build_ShouldThrowDependencyContainerBuildAggregateException_WhenDependencyContainerCouldNotBeBuilt()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromSharedImplementor<Implementor>();
        sut.Add<IBar>().FromSharedImplementor<Implementor>();

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<DependencyContainerBuildAggregateException>();
    }

    [Fact]
    public void IDependencyContainerBuilder_Build_ShouldBeEquivalentToBuild()
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();
        var result = sut.Build();
        result.Should().BeOfType<DependencyContainer>();
    }

    [Fact]
    public void IDependencyLocatorBuilder_SetDefaultLifetime_ShouldBeEquivalentToSetDefaultLifetime()
    {
        IDependencyLocatorBuilder sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultLifetime( DependencyLifetime.Singleton );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.DefaultLifetime.Should().Be( DependencyLifetime.Singleton );
        }
    }

    [Fact]
    public void IDependencyLocatorBuilder_SetDefaultDisposalStrategy_ShouldBeEquivalentToSetDefaultDisposalStrategy()
    {
        IDependencyLocatorBuilder sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.DefaultDisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.RenounceOwnership );
            result.DefaultDisposalStrategy.Callback.Should().BeNull();
        }
    }
}

public class InjectedWithPublicCtor<T>
{
    public InjectedWithPublicCtor(T value) { }
}

public class InjectedWithPrivateCtor<T>
{
    private InjectedWithPrivateCtor(T value) { }
}

public class InvalidNonGenericInjected
{
    public InvalidNonGenericInjected(string value) { }
}

public class InvalidInjectedWithTooManyArgs<T1, T2>
{
    public InvalidInjectedWithTooManyArgs(T1 value) { }
}

public class InvalidInjectedWithParameterlessCtor<T>
{
    public InvalidInjectedWithParameterlessCtor() { }
}

public class InvalidInjectedWithIncorrectCtorParamType<T>
{
    public InvalidInjectedWithIncorrectCtorParamType(string value) { }
}

public class InvalidInjectedWithTooManyCtorParams<T>
{
    public InvalidInjectedWithTooManyCtorParams(T value1, T value2) { }
}

[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Class )]
public class OptionalAttribute : Attribute { }

public class ExtendedOptionalAttribute : OptionalAttribute { }

[AttributeUsage( AttributeTargets.Class )]
public class NonParameterAttribute : Attribute { }
