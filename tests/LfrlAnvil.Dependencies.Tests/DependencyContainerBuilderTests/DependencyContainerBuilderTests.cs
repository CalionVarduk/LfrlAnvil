using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

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
        sut.Configuration.InjectablePropertyType.Should().BeSameAs( typeof( Injected<> ) );
        sut.Configuration.OptionalDependencyAttributeType.Should().BeSameAs( typeof( OptionalDependencyAttribute ) );
        sut.Configuration.TreatCaptiveDependenciesAsErrors.Should().BeFalse();
        (( IDependencyLocatorBuilder )sut).KeyType.Should().BeNull();
        (( IDependencyLocatorBuilder )sut).Key.Should().BeNull();
        (( IDependencyLocatorBuilder )sut).IsKeyed.Should().BeFalse();
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
        var action = Lambda.Of( () => { sut.SetDefaultLifetime( ( DependencyLifetime )4 ); } );
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
    public void Configuration_SetInjectablePropertyType_ShouldUpdateTheTypeIfItIsValid(Type type)
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();

        var result = sut.Configuration.SetInjectablePropertyType( type );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.Configuration );
            sut.Configuration.InjectablePropertyType.Should().BeSameAs( type );
        }
    }

    [Theory]
    [InlineData( typeof( InjectedWithPublicCtor<string> ) )]
    [InlineData( typeof( InvalidNonGenericInjected ) )]
    [InlineData( typeof( InvalidInjectedWithTooManyArgs<,> ) )]
    [InlineData( typeof( InvalidInjectedWithParameterlessCtor<> ) )]
    [InlineData( typeof( InvalidInjectedWithIncorrectCtorParamType<> ) )]
    [InlineData( typeof( InvalidInjectedWithTooManyCtorParams<> ) )]
    public void Configuration_SetInjectablePropertyType_ShouldThrowDependencyContainerBuilderConfigurationException_WhenTypeIsNotValid(
        Type type)
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.Configuration.SetInjectablePropertyType( type ) );
        action.Should().ThrowExactly<DependencyContainerBuilderConfigurationException>();
    }

    [Theory]
    [InlineData( typeof( OptionalDependencyAttribute ) )]
    [InlineData( typeof( OptionalAttribute ) )]
    [InlineData( typeof( ExtendedOptionalAttribute ) )]
    public void Configuration_SetOptionalDependencyAttributeType_ShouldUpdateTheTypeIfItIsValid(Type type)
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();

        var result = sut.Configuration.SetOptionalDependencyAttributeType( type );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.Configuration );
            sut.Configuration.OptionalDependencyAttributeType.Should().BeSameAs( type );
        }
    }

    [Theory]
    [InlineData( typeof( object ) )]
    [InlineData( typeof( Injected<> ) )]
    [InlineData( typeof( Injected<string> ) )]
    [InlineData( typeof( NonParameterAttribute ) )]
    public void
        Configuration_SetOptionalDependencyAttributeType_ShouldThrowDependencyContainerBuilderConfigurationException_WhenTypeIsNotValid(
            Type type)
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.Configuration.SetOptionalDependencyAttributeType( type ) );
        action.Should().ThrowExactly<DependencyContainerBuilderConfigurationException>();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Configuration_SetInjectablePropertyType_ShouldUpdateTheOptions(bool enabled)
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();

        var result = sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors( enabled );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.Configuration );
            sut.Configuration.TreatCaptiveDependenciesAsErrors.Should().Be( enabled );
        }
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
            (( IDependencyLocatorBuilder )result).Key.Should().Be( key );
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
            result.IsIncludedInRange.Should().BeTrue();
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
            result.Constructor.Type.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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
            result.Constructor.Type.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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
            result.Constructor.Type.Should().BeSameAs( ctor.DeclaringType );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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
            result.Constructor.Type.Should().BeSameAs( ctor.DeclaringType );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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
            result.Constructor.Type.Should().BeSameAs( ctor.DeclaringType );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void Add_FromType_ShouldSetTypeAsCreationDetail()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var result = builder.FromType( typeof( Implementor ) );

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
            result.Constructor.Type.Should().BeSameAs( typeof( Implementor ) );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void Add_FromType_WithConfiguration_ShouldSetTypeAsCreationDetail()
    {
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var result = builder.FromType( typeof( Implementor ), o => o.SetOnCreatedCallback( onCreatedCallback ) );

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
            result.Constructor.Type.Should().BeSameAs( typeof( Implementor ) );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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

        var action = Lambda.Of( () => { builder.SetLifetime( ( DependencyLifetime )4 ); } );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Add_IncludeInRange_ShouldUpdateIsIncludedInRangeCorrectly(bool included)
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var result = builder.IncludeInRange( included );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.IsIncludedInRange.Should().Be( included );
        }
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
            result.Constructor.Type.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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
            result.Constructor.Type.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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
            result.Constructor.Type.Should().BeSameAs( ctor.DeclaringType );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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
            result.Constructor.Type.Should().BeSameAs( ctor.DeclaringType );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithParameterResolutions_ShouldAddConstructorParameterResolutionsAsCreationDetail()
    {
        var predicate1 = Substitute.For<Func<ParameterInfo, bool>>();
        var predicate2 = Substitute.For<Func<ParameterInfo, bool>>();
        var factory = Lambda.ExpressionOf( (IDependencyScope s) => Substitute.For<Func<IDependencyScope, object>>()( s ) );
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor(
            o => o.ResolveParameter( predicate1, factory ).ResolveParameter( predicate2, typeof( IFoo ), i => i.Keyed( 1 ) ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.Type.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().HaveCount( 2 );

            result.Constructor.InvocationOptions.ParameterResolutions.ElementAtOrDefault( 0 )
                .Should()
                .BeEquivalentTo( InjectableDependencyResolution<ParameterInfo>.FromFactory( predicate1, factory ) );

            result.Constructor.InvocationOptions.ParameterResolutions.ElementAtOrDefault( 1 )
                .Should()
                .BeEquivalentTo(
                    InjectableDependencyResolution<ParameterInfo>.FromImplementorKey(
                        predicate2,
                        new DependencyKey<int>( typeof( IFoo ), 1 ) ) );
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithMemberResolutions_ShouldAddConstructorMemberResolutionsAsCreationDetail()
    {
        var predicate1 = Substitute.For<Func<MemberInfo, bool>>();
        var predicate2 = Substitute.For<Func<MemberInfo, bool>>();
        var factory = Lambda.ExpressionOf( (IDependencyScope s) => Substitute.For<Func<IDependencyScope, object>>()( s ) );
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor(
            o => o.ResolveMember( predicate1, factory ).ResolveMember( predicate2, typeof( IFoo ), i => i.Keyed( 1 ) ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.Type.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().HaveCount( 2 );

            result.Constructor.InvocationOptions.MemberResolutions.ElementAtOrDefault( 0 )
                .Should()
                .BeEquivalentTo( InjectableDependencyResolution<MemberInfo>.FromFactory( predicate1, factory ) );

            result.Constructor.InvocationOptions.MemberResolutions.ElementAtOrDefault( 1 )
                .Should()
                .BeEquivalentTo(
                    InjectableDependencyResolution<MemberInfo>.FromImplementorKey(
                        predicate2,
                        new DependencyKey<int>( typeof( IFoo ), 1 ) ) );
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithClearedParameterResolutions_ShouldClearConstructorParameterResolutions()
    {
        var predicate = Substitute.For<Func<ParameterInfo, bool>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor(
            o => o.ResolveParameter( predicate, typeof( IFoo ) ).ResolveMember( _ => true, typeof( IFoo ) ).ClearParameterResolutions() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.Type.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithClearedMemberResolutions_ShouldClearConstructorMemberResolutions()
    {
        var predicate = Substitute.For<Func<MemberInfo, bool>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor(
            o => o.ResolveParameter( _ => true, typeof( IFoo ) ).ResolveMember( predicate, typeof( IFoo ) ).ClearMemberResolutions() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( Implementor ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.Type.Should().BeNull();
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().HaveCount( 1 );
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddSharedImplementor_FromType_ShouldSetTypeAsCreationDetail()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( IFoo ) );

        var result = builder.FromType( typeof( Implementor ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.Type.Should().BeSameAs( typeof( Implementor ) );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeNull();
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddSharedImplementor_FromType_WithConfiguration_ShouldSetTypeAsCreationDetail()
    {
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( IFoo ) );

        var result = builder.FromType( typeof( Implementor ), o => o.SetOnCreatedCallback( onCreatedCallback ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Factory.Should().BeNull();
            result.Constructor.Should().NotBeNull();
            if ( result.Constructor is null )
                return;

            result.Constructor.Info.Should().BeNull();
            result.Constructor.Type.Should().BeSameAs( typeof( Implementor ) );
            result.Constructor.InvocationOptions.OnCreatedCallback.Should().BeSameAs( onCreatedCallback );
            result.Constructor.InvocationOptions.ParameterResolutions.Should().BeEmpty();
            result.Constructor.InvocationOptions.MemberResolutions.Should().BeEmpty();
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
    public void AddSharedImplementor_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsOpenGeneric()
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.AddSharedImplementor( typeof( IList<> ) ) );
        action.Should().ThrowExactly<InvalidTypeRegistrationException>().AndMatch( e => e.Type == typeof( IList<> ) );
    }

    [Fact]
    public void AddSharedImplementor_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsClosedGenericWithGenericParameters()
    {
        var sut = new DependencyContainerBuilder();
        var type = typeof( List<> ).GetOpenGenericImplementations( typeof( IList<> ) ).Single();
        var action = Lambda.Of( () => sut.AddSharedImplementor( type ) );
        action.Should().ThrowExactly<InvalidTypeRegistrationException>().AndMatch( e => e.Type == type );
    }

    [Fact]
    public void GetDependencyRange_ShouldReturnNewlyCreatedEmptyRange_WhenDependencyTypeWasNotRegistered()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.GetDependencyRange( typeof( IFoo ) );

        using ( new AssertionScope() )
        {
            result.DependencyType.Should().Be( typeof( IFoo ) );
            result.Elements.Should().BeEmpty();
            result.TryGetLast().Should().BeNull();
        }
    }

    [Fact]
    public void GetDependencyRange_ShouldReturnCorrectBuilder_WhenDependencyTypeWasRegistered()
    {
        var sut = new DependencyContainerBuilder();
        var expected1 = sut.Add<IFoo>();
        var expected2 = sut.Add<IFoo>();

        var result = sut.GetDependencyRange( typeof( IFoo ) );

        using ( new AssertionScope() )
        {
            result.DependencyType.Should().Be( typeof( IFoo ) );
            result.Elements.Should().BeSequentiallyEqualTo( expected1, expected2 );
            result.TryGetLast().Should().BeSameAs( expected2 );
            expected1.RangeBuilder.Should().BeSameAs( result );
            expected2.RangeBuilder.Should().BeSameAs( result );
        }
    }

    [Fact]
    public void GetDependencyRange_SetOnResolvingCallback_ShouldUpdateCallback()
    {
        var callback = Substitute.For<Action<Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();

        var result = sut.GetDependencyRange<IFoo>().SetOnResolvingCallback( callback );

        result.OnResolvingCallback.Should().BeSameAs( callback );
    }

    [Fact]
    public void GetDependencyRange_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsOpenGeneric()
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.GetDependencyRange( typeof( IList<> ) ) );
        action.Should().ThrowExactly<InvalidTypeRegistrationException>().AndMatch( e => e.Type == typeof( IList<> ) );
    }

    [Fact]
    public void GetDependencyRange_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsClosedGenericWithGenericParameters()
    {
        var sut = new DependencyContainerBuilder();
        var type = typeof( List<> ).GetOpenGenericImplementations( typeof( IList<> ) ).Single();
        var action = Lambda.Of( () => sut.GetDependencyRange( type ) );
        action.Should().ThrowExactly<InvalidTypeRegistrationException>().AndMatch( e => e.Type == type );
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
        sut.Add<IFoo>().FromSharedImplementor<Implementor>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingKeyedSharedImplementorAndNonCachedKeyType()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromSharedImplementor<Implementor>( o => o.Keyed( 1 ) );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingKeyedSharedImplementorAndCachedKeyType()
    {
        var sut = new DependencyContainerBuilder();
        _ = sut.GetKeyedLocator( 1 );
        sut.Add<IFoo>().FromSharedImplementor<Implementor>( o => o.Keyed( 2 ) );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCtorParamDependencyCannotBeResolved()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IWithText>().FromConstructor( ctor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenMemberDependencyCannotBeResolved()
    {
        var ctor = typeof( FieldImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IWithText>().FromConstructor( ctor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnResultWithWarnings_WhenExplicitResolutionsAreUnused()
    {
        var ctor = typeof( CtorAndRefMemberImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<int>().FromFactory( _ => 0 );
        sut.Add<string>().FromFactory( _ => string.Empty );
        sut.Add<IWithText>()
            .FromConstructor(
                ctor,
                o => o.ResolveParameter( _ => false, typeof( int ) )
                    .ResolveMember( _ => false, _ => new object() ) );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeTrue();
            result.Container.Should().NotBeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            message.Warnings.Should().HaveCount( 2 );
            message.Errors.Should().BeEmpty();
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypedResolutionsHaveInvalidType()
    {
        var ctor = typeof( CtorAndRefMemberImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<int>().FromFactory( _ => 0 );
        sut.Add<string>().FromFactory( _ => string.Empty );
        sut.Add<IWithText>()
            .FromConstructor(
                ctor,
                o => o.ResolveParameter( _ => true, typeof( string ) )
                    .ResolveMember( _ => true, typeof( int ) ) );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 2 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitCtorDoesNotCreateInstancesOfCorrectType()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromConstructor( ctor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitCtorCreatesAbstractInstances()
    {
        var ctor = typeof( AbstractFoo ).GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromConstructor( ctor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeIsNotOfCorrectType()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType<ExplicitCtorImplementor>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeIsAbstract()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType<AbstractFoo>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCtorForTypeCouldNotBeFound()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<ChainableFoo>().FromConstructor();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( ChainableFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCtorForExplicitTypeCouldNotBeFound()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType<ChainableFoo>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Theory]
    [InlineData( typeof( IFoo ) )]
    [InlineData( typeof( AbstractFoo ) )]
    public void TryBuild_ShouldReturnFailureResult_WhenImplicitCtorCannotBeFoundBecauseDependencyIsAbstract(Type dependencyType)
    {
        var sut = new DependencyContainerBuilder();
        sut.Add( dependencyType ).FromConstructor();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( dependencyType ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Theory]
    [InlineData( typeof( IFoo ) )]
    [InlineData( typeof( AbstractFoo ) )]
    public void TryBuild_ShouldReturnFailureResult_WhenImplicitCtorCannotBeFoundBecauseExplicitTypeIsAbstract(Type type)
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType( type );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WithOnlyOneEntryWhenTwoDependenciesUseTheSameInvalidSharedImplementor()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.AddSharedImplementor<Implementor>().FromConstructor( ctor );
        sut.Add<IFoo>().FromSharedImplementor<Implementor>();
        sut.Add<IBar>().FromSharedImplementor<Implementor>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.CreateShared( new DependencyKey( typeof( Implementor ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyUsesSharedImplementorWithInvalidType()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<string>().FromFactory( _ => "foo" );
        sut.AddSharedImplementor<ExplicitCtorImplementor>();
        sut.Add<IFoo>().FromSharedImplementor<ExplicitCtorImplementor>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyThroughCtorParamsIsDetected()
    {
        var fooCtor = typeof( ChainableFoo ).GetConstructors().First();
        var barCtor = typeof( ChainableBar ).GetConstructors().First();
        var quxCtor = typeof( ChainableQux ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromConstructor( fooCtor );
        sut.Add<IBar>().FromConstructor( barCtor );
        sut.Add<IQux>().FromConstructor( quxCtor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyThroughFieldsIsDetected()
    {
        var fooCtor = typeof( ChainableFieldFoo ).GetConstructors().First();
        var barCtor = typeof( ChainableFieldBar ).GetConstructors().First();
        var quxCtor = typeof( ChainableFieldQux ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromConstructor( fooCtor );
        sut.Add<IBar>().FromConstructor( barCtor );
        sut.Add<IQux>().FromConstructor( quxCtor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyThroughPropertiesIsDetected()
    {
        var fooCtor = typeof( ChainablePropertyFoo ).GetConstructors().First();
        var barCtor = typeof( ChainablePropertyBar ).GetConstructors().First();
        var quxCtor = typeof( ChainablePropertyQux ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromConstructor( fooCtor );
        sut.Add<IBar>().FromConstructor( barCtor );
        sut.Add<IQux>().FromConstructor( quxCtor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenSelfDependencyIsDetected()
    {
        var ctor = typeof( DecoratorWithText ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IWithText>().FromConstructor( ctor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenMultipleCircularDependenciesAreDetected()
    {
        var fooCtor = typeof( ComplexFoo ).GetConstructors().First();
        var barCtor = typeof( ChainableBar ).GetConstructors().First();
        var otherBarCtor = typeof( ComplexBar ).GetConstructors().First();
        var quxCtor = typeof( ComplexQux ).GetConstructors().First();
        var textCtor = typeof( DecoratorWithText ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>()
            .FromConstructor(
                fooCtor,
                o => o.ResolveParameter( p => p.Name == "otherBar", typeof( IBar ), c => c.Keyed( 1 ) ) );

        sut.Add<IBar>().FromConstructor( barCtor );
        sut.Add<IQux>().FromConstructor( quxCtor );
        sut.Add<IWithText>().FromConstructor( textCtor );

        sut.GetKeyedLocator( 1 )
            .Add<IBar>()
            .FromConstructor(
                otherBarCtor,
                o => o.ResolveParameter( p => p.ParameterType == typeof( IFoo ), typeof( IFoo ), c => c.NotKeyed() ) );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 2 );
            if ( result.Messages.Count < 2 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 2 );

            message = result.Messages.ElementAt( 1 );
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Theory]
    [InlineData( DependencyLifetime.Transient, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Transient, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Transient, DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Transient, DependencyLifetime.Singleton )]
    [InlineData( DependencyLifetime.Scoped, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Scoped, DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.Scoped, DependencyLifetime.Singleton )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.ScopedSingleton )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Singleton )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Singleton )]
    public void TryBuild_ShouldReturnResultWithoutWarnings_WhenThereAreNotCaptiveDependencies(
        DependencyLifetime parentLifetime,
        DependencyLifetime dependencyLifetime)
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<string>().SetLifetime( dependencyLifetime ).FromFactory( _ => string.Empty );
        sut.Add<IWithText>().SetLifetime( parentLifetime ).FromConstructor( ctor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeTrue();
            result.Container.Should().NotBeNull();
            result.Messages.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( DependencyLifetime.Scoped, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.ScopedSingleton )]
    public void TryBuild_ShouldReturnResultWithWarnings_WhenCaptiveDependencyIsDetectedAndTheyAreTreatedAsWarnings(
        DependencyLifetime parentLifetime,
        DependencyLifetime dependencyLifetime)
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors( false );
        sut.Add<string>().SetLifetime( dependencyLifetime ).FromFactory( _ => string.Empty );
        sut.Add<IWithText>().SetLifetime( parentLifetime ).FromConstructor( ctor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeTrue();
            result.Container.Should().NotBeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            message.Warnings.Should().HaveCount( 1 );
            message.Errors.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( DependencyLifetime.Scoped, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.ScopedSingleton )]
    public void TryBuild_ShouldReturnFailureResult_WhenCaptiveDependencyIsDetectedAndTheyAreTreatedAsErrors(
        DependencyLifetime parentLifetime,
        DependencyLifetime dependencyLifetime)
    {
        var ctor = typeof( FieldImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors();
        sut.Add<string>().SetLifetime( dependencyLifetime ).FromFactory( _ => string.Empty );
        sut.Add<IWithText>().SetLifetime( parentLifetime ).FromConstructor( ctor );

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenFirstRangeDependencyElementIsInvalid()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IBar>().FromType<Implementor>();
        sut.Add<IFoo>().FromSharedImplementor<ChainableBar>();
        sut.Add<IFoo>().FromType<Implementor>();
        sut.Add<IFoo>().FromType<ChainableFoo>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ), 0 ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 2 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenSecondRangeDependencyElementIsInvalid()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType<Implementor>();
        sut.Add<IFoo>().FromType<ChainableFoo>();
        sut.Add<IFoo>().FromType<MultiCtorImplementor>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ), 1 ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Theory]
    [InlineData( DependencyLifetime.Scoped, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.ScopedSingleton )]
    public void TryBuild_ShouldReturnResultWithWarnings_WhenCaptiveDependencyIsDetectedInRangeElementAndTheyAreTreatedAsWarnings(
        DependencyLifetime parentLifetime,
        DependencyLifetime dependencyLifetime)
    {
        var sut = new DependencyContainerBuilder();
        sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors( false );
        sut.Add<string>().SetLifetime( parentLifetime ).FromFactory( _ => string.Empty );
        sut.Add<string>().SetLifetime( dependencyLifetime ).FromFactory( _ => string.Empty );
        sut.Add<string>().SetLifetime( parentLifetime ).FromFactory( _ => string.Empty );
        sut.Add<IFoo>().SetLifetime( parentLifetime ).FromType<RangeFoo>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeTrue();
            result.Container.Should().NotBeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().HaveCount( 1 );
            message.Errors.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( DependencyLifetime.Scoped, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.ScopedSingleton, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Transient )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.Scoped )]
    [InlineData( DependencyLifetime.Singleton, DependencyLifetime.ScopedSingleton )]
    public void TryBuild_ShouldReturnFailureResult_WhenCaptiveDependencyIsDetectedInRangeElementAndTheyAreTreatedAsErrors(
        DependencyLifetime parentLifetime,
        DependencyLifetime dependencyLifetime)
    {
        var sut = new DependencyContainerBuilder();
        sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors();
        sut.Add<string>().SetLifetime( parentLifetime ).FromFactory( _ => string.Empty );
        sut.Add<string>().SetLifetime( dependencyLifetime ).FromFactory( _ => string.Empty );
        sut.Add<string>().SetLifetime( parentLifetime ).FromFactory( _ => string.Empty );
        sut.Add<IFoo>().SetLifetime( parentLifetime ).FromType<RangeFoo>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 1 );
            if ( result.Messages.Count < 1 )
                return;

            var message = result.Messages.First();
            message.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            message.Warnings.Should().BeEmpty();
            message.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyForSelfRangeIsDetected()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IWithText>().FromType<RangeDecorator>();
        sut.Add<IWithText>().FromType<RangeDecorator>();
        sut.Add<IWithText>().FromType<RangeDecorator>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 3 );
            if ( result.Messages.Count < 3 )
                return;

            var firstMessage = result.Messages.First();
            firstMessage.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) );
            firstMessage.Warnings.Should().BeEmpty();
            firstMessage.Errors.Should().HaveCount( 3 );

            var secondMessage = result.Messages.ElementAt( 1 );
            secondMessage.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ), 0 ) );
            secondMessage.Warnings.Should().BeEmpty();
            secondMessage.Errors.Should().HaveCount( 2 );

            var thirdMessage = result.Messages.Last();
            thirdMessage.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ), 1 ) );
            thirdMessage.Warnings.Should().BeEmpty();
            thirdMessage.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyForNestedRangeIsDetected()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<string>().FromFactory( _ => string.Empty );
        sut.Add<IFoo>().FromType<TextFoo>();
        sut.Add<IWithText>().FromType<RangeDecorator>();
        sut.Add<IWithText>().FromType<ChainableRange>();
        sut.Add<IWithText>().FromType<ExplicitCtorImplementor>();

        var result = sut.TryBuild();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeFalse();
            result.Container.Should().BeNull();
            result.Messages.Should().HaveCount( 2 );
            if ( result.Messages.Count < 2 )
                return;

            var firstMessage = result.Messages.First();
            firstMessage.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) );
            firstMessage.Warnings.Should().BeEmpty();
            firstMessage.Errors.Should().HaveCount( 1 );

            var secondMessage = result.Messages.Last();
            secondMessage.ImplementorKey.Should().Be( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ), 0 ) );
            secondMessage.Warnings.Should().BeEmpty();
            secondMessage.Errors.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void Build_ShouldThrowDependencyContainerBuildException_WhenDependencyContainerCouldNotBeBuilt()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromSharedImplementor<Implementor>();
        sut.Add<IBar>().FromSharedImplementor<Implementor>();

        var action = Lambda.Of( () => sut.Build() );

        action.Should()
            .ThrowExactly<DependencyContainerBuildException>()
            .AndMatch( e => e.Messages.SelectMany( m => m.Errors ).Any() );
    }

    [Fact]
    public void Build_ShouldThrowDependencyContainerBuildException_WhenThereAreWarningsAndErrors()
    {
        var ctor = typeof( FieldImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors( false );
        sut.Add<string>().SetLifetime( DependencyLifetime.Transient ).FromFactory( _ => string.Empty );
        sut.Add<IWithText>().SetLifetime( DependencyLifetime.Singleton ).FromConstructor( ctor );
        sut.Add<IFoo>().FromSharedImplementor<Implementor>();

        var action = Lambda.Of( () => sut.Build() );

        action.Should()
            .ThrowExactly<DependencyContainerBuildException>()
            .AndMatch( e => e.Messages.SelectMany( m => m.Errors ).Any() );
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

[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property )]
public class OptionalAttribute : Attribute { }

public class ExtendedOptionalAttribute : OptionalAttribute { }

[AttributeUsage( AttributeTargets.Class )]
public class NonParameterAttribute : Attribute { }
