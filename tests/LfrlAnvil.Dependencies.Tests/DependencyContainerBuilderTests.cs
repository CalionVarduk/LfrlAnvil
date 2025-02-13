using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Dependencies.Tests;

public class DependencyContainerBuilderTests : DependencyTestsBase
{
    [Fact]
    public void Ctor_ShouldCreateWithDefaultTransientLifetimeAndUseDisposableInterfaceStrategy()
    {
        var sut = new DependencyContainerBuilder();
        sut.DefaultLifetime.TestEquals( DependencyLifetime.Transient ).Go();
        sut.DefaultDisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ).Go();
        sut.DefaultDisposalStrategy.Callback.TestNull().Go();
        sut.Configuration.InjectablePropertyType.TestRefEquals( typeof( Injected<> ) ).Go();
        sut.Configuration.OptionalDependencyAttributeType.TestRefEquals( typeof( OptionalDependencyAttribute ) ).Go();
        sut.Configuration.TreatCaptiveDependenciesAsErrors.TestFalse().Go();
        (( IDependencyLocatorBuilder )sut).KeyType.TestNull().Go();
        (( IDependencyLocatorBuilder )sut).Key.TestNull().Go();
        (( IDependencyLocatorBuilder )sut).IsKeyed.TestFalse().Go();
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

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultLifetime.TestEquals( lifetime ),
                dependency.Lifetime.TestEquals( sut.DefaultLifetime ) )
            .Go();
    }

    [Fact]
    public void SetDefaultLifetime_ShouldThrowArgumentException_WhenValueIsNotValid()
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => { sut.SetDefaultLifetime( ( DependencyLifetime )4 ); } );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void
        SetDefaultDisposalStrategy_ShouldUpdateDisposalStrategyToUseDisposableInterfaceAndCauseNewImplementorsToStartWithThatStrategy()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultDisposalStrategy( DependencyImplementorDisposalStrategy.UseDisposableInterface() );
        var implementor = sut.Add<Implementor>().FromFactory( _ => new object() );
        var sharedImplementor = sut.AddSharedImplementor<Implementor>();

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultDisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                sut.DefaultDisposalStrategy.Callback.TestNull(),
                implementor.DisposalStrategy.TestEquals( sut.DefaultDisposalStrategy ),
                sharedImplementor.DisposalStrategy.TestEquals( sut.DefaultDisposalStrategy ) )
            .Go();
    }

    [Fact]
    public void SetDefaultDisposalStrategy_ShouldUpdateDisposalStrategyToRenounceOwnershipAndCauseNewImplementorsToStartWithThatStrategy()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );
        var implementor = sut.Add<Implementor>().FromFactory( _ => new object() );
        var sharedImplementor = sut.AddSharedImplementor<Implementor>();

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultDisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.RenounceOwnership ),
                sut.DefaultDisposalStrategy.Callback.TestNull(),
                implementor.DisposalStrategy.TestEquals( sut.DefaultDisposalStrategy ),
                sharedImplementor.DisposalStrategy.TestEquals( sut.DefaultDisposalStrategy ) )
            .Go();
    }

    [Fact]
    public void SetDefaultDisposalStrategy_ShouldUpdateDisposalStrategyToUseCallbackAndCauseNewImplementorsToStartWithThatStrategy()
    {
        var callback = Substitute.For<Action<object>>();
        var sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultDisposalStrategy( DependencyImplementorDisposalStrategy.UseCallback( callback ) );
        var implementor = sut.Add<Implementor>().FromFactory( _ => new object() );
        var sharedImplementor = sut.AddSharedImplementor<Implementor>();

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultDisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseCallback ),
                sut.DefaultDisposalStrategy.Callback.TestRefEquals( callback ),
                implementor.DisposalStrategy.TestEquals( sut.DefaultDisposalStrategy ),
                sharedImplementor.DisposalStrategy.TestEquals( sut.DefaultDisposalStrategy ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( Injected<> ) )]
    [InlineData( typeof( InjectedWithPublicCtor<> ) )]
    [InlineData( typeof( InjectedWithPrivateCtor<> ) )]
    public void Configuration_SetInjectablePropertyType_ShouldUpdateTheTypeIfItIsValid(Type type)
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();

        var result = sut.Configuration.SetInjectablePropertyType( type );

        Assertion.All(
                result.TestRefEquals( sut.Configuration ),
                sut.Configuration.InjectablePropertyType.TestRefEquals( type ) )
            .Go();
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
        action.Test( exc => exc.TestType().Exact<DependencyContainerBuilderConfigurationException>() ).Go();
    }

    [Theory]
    [InlineData( typeof( OptionalDependencyAttribute ) )]
    [InlineData( typeof( OptionalAttribute ) )]
    [InlineData( typeof( ExtendedOptionalAttribute ) )]
    public void Configuration_SetOptionalDependencyAttributeType_ShouldUpdateTheTypeIfItIsValid(Type type)
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();

        var result = sut.Configuration.SetOptionalDependencyAttributeType( type );

        Assertion.All(
                result.TestRefEquals( sut.Configuration ),
                sut.Configuration.OptionalDependencyAttributeType.TestRefEquals( type ) )
            .Go();
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
        action.Test( exc => exc.TestType().Exact<DependencyContainerBuilderConfigurationException>() ).Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Configuration_SetInjectablePropertyType_ShouldUpdateTheOptions(bool enabled)
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();

        var result = sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors( enabled );

        Assertion.All(
                result.TestRefEquals( sut.Configuration ),
                sut.Configuration.TreatCaptiveDependenciesAsErrors.TestEquals( enabled ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void GetKeyedLocator_ShouldReturnLocatorBuilderWithCorrectKey(int key)
    {
        var sut = new DependencyContainerBuilder();
        var result = sut.GetKeyedLocator( key );

        Assertion.All(
                result.Key.TestEquals( key ),
                (( IDependencyLocatorBuilder )result).Key.TestEquals( key ),
                result.KeyType.TestEquals( typeof( int ) ),
                result.IsKeyed.TestTrue() )
            .Go();
    }

    [Fact]
    public void GetKeyedLocator_ShouldReturnCorrectCachedLocatorBuilder_WhenCalledMoreThanOnceWithTheSameKey()
    {
        var sut = new DependencyContainerBuilder();

        var result1 = sut.GetKeyedLocator( 1 );
        var result2 = sut.GetKeyedLocator( 1 );

        result1.TestEquals( result2 ).Go();
    }

    [Fact]
    public void Add_ShouldAddNewDependencyWithoutImplementationDetailsAndWithDefaultLifetime()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.Add( typeof( IFoo ) );

        Assertion.All(
                result.DependencyType.TestEquals( typeof( IFoo ) ),
                result.Lifetime.TestEquals( sut.DefaultLifetime ),
                result.Implementor.TestNull(),
                result.SharedImplementorKey.TestNull(),
                result.IsIncludedInRange.TestTrue() )
            .Go();
    }

    [Fact]
    public void Add_FromFactory_ShouldSetProvidedFactoryAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromFactory( factory );

        Assertion.All(
                result.TestRefEquals( builder.Implementor ),
                builder.SharedImplementorKey.TestNull(),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Constructor.TestNull(),
                result.Factory.TestRefEquals( factory ),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull() )
            .Go();
    }

    [Fact]
    public void Add_FromConstructor_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor();

        Assertion.All(
                result.TestRefEquals( builder.Implementor ),
                builder.SharedImplementorKey.TestNull(),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void Add_FromConstructor_WithConfiguration_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
    {
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( o => o.SetOnCreatedCallback( onCreatedCallback ) );

        Assertion.All(
                result.TestRefEquals( builder.Implementor ),
                builder.SharedImplementorKey.TestNull(),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void Add_FromConstructor_WithInfo_ShouldSetConstructorAsCreationDetail()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( ctor );

        Assertion.All(
                result.TestRefEquals( builder.Implementor ),
                builder.SharedImplementorKey.TestNull(),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( builder.Implementor ),
                builder.SharedImplementorKey.TestNull(),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void Add_FromConstructor_ShouldUpdateConstructorInfoAsCreationDetail_WhenCalledMultipleTimes()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromConstructor();

        var result = builder.FromConstructor( ctor );

        Assertion.All(
                result.TestRefEquals( builder.Implementor ),
                builder.SharedImplementorKey.TestNull(),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void Add_FromType_ShouldSetTypeAsCreationDetail()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var result = builder.FromType( typeof( Implementor ) );

        Assertion.All(
                result.TestRefEquals( builder.Implementor ),
                builder.SharedImplementorKey.TestNull(),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestRefEquals( typeof( Implementor ) ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void Add_FromType_WithConfiguration_ShouldSetTypeAsCreationDetail()
    {
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var result = builder.FromType( typeof( Implementor ), o => o.SetOnCreatedCallback( onCreatedCallback ) );

        Assertion.All(
                result.TestRefEquals( builder.Implementor ),
                builder.SharedImplementorKey.TestNull(),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestRefEquals( typeof( Implementor ) ),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( builder ),
                result.Lifetime.TestEquals( lifetime ) )
            .Go();
    }

    [Fact]
    public void Add_SetLifetime_ShouldThrowArgumentException_WhenValueIsNotValid()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var action = Lambda.Of( () => { builder.SetLifetime( ( DependencyLifetime )4 ); } );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Add_IncludeInRange_ShouldUpdateIsIncludedInRangeCorrectly(bool included)
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );

        var result = builder.IncludeInRange( included );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.IsIncludedInRange.TestEquals( included ) )
            .Go();
    }

    [Fact]
    public void Add_FromSharedImplementor_ShouldSetProvidedSharedImplementorTypeAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromFactory( factory );

        var result = builder.FromSharedImplementor( typeof( Implementor ) );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.Implementor.TestNull(),
                result.SharedImplementorKey.TestNotNull(
                    k => Assertion.All(
                        k.Type.TestEquals( typeof( Implementor ) ),
                        k.Key.TestNull(),
                        k.KeyType.TestNull(),
                        k.IsKeyed.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void Add_FromSharedImplementor_Keyed_ShouldSetProvidedSharedImplementorTypeAndKeyAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.Add( typeof( IFoo ) );
        builder.FromFactory( factory );

        var result = builder.FromSharedImplementor<Implementor>( o => o.Keyed( 1 ) );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.Implementor.TestNull(),
                result.SharedImplementorKey.TestNotNull(
                    k => Assertion.All(
                        k.Type.TestEquals( typeof( Implementor ) ),
                        k.Key.TestEquals( 1 ),
                        k.KeyType.TestEquals( typeof( int ) ),
                        k.IsKeyed.TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void Add_FromSharedImplementor_ShouldSetProvidedSharedImplementorTypeAndLocatorKeyAsCreationDetail_WhenLocatorIsKeyed()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.GetKeyedLocator( 1 ).Add( typeof( IFoo ) );
        builder.FromFactory( factory );

        var result = builder.FromSharedImplementor( typeof( Implementor ) );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.Implementor.TestNull(),
                result.SharedImplementorKey.TestNotNull(
                    k => Assertion.All(
                        k.Type.TestEquals( typeof( Implementor ) ),
                        k.Key.TestEquals( 1 ),
                        k.KeyType.TestEquals( typeof( int ) ),
                        k.IsKeyed.TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void Add_FromSharedImplementor_NotKeyed_ShouldSetProvidedSharedImplementorTypeWithoutKeyAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, IFoo>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.GetKeyedLocator( 1 ).Add( typeof( IFoo ) );
        builder.FromFactory( factory );

        var result = builder.FromSharedImplementor<Implementor>( o => o.NotKeyed() );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.Implementor.TestNull(),
                result.SharedImplementorKey.TestNotNull(
                    k => Assertion.All(
                        k.Type.TestEquals( typeof( Implementor ) ),
                        k.Key.TestNull(),
                        k.KeyType.TestNull(),
                        k.IsKeyed.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_ShouldAddNewSharedImplementorWithoutCreationDetails()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.AddSharedImplementor( typeof( Implementor ) );

        Assertion.All(
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull(),
                result.OnResolvingCallback.TestNull() )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_ShouldDoNothingAndReturnPreviousBuilder_WhenAddingExistingSharedImplementorType()
    {
        var sut = new DependencyContainerBuilder();
        var expected = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = sut.AddSharedImplementor( typeof( Implementor ) );

        result.TestRefEquals( expected ).Go();
    }

    [Fact]
    public void AddSharedImplementor_FromFactory_ShouldSetProvidedFactoryAsCreationDetail()
    {
        var factory = Substitute.For<Func<IDependencyScope, Implementor>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromFactory( factory );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestRefEquals( factory ),
                result.Constructor.TestNull() )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor();

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithConfiguration_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
    {
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( o => o.SetOnCreatedCallback( onCreatedCallback ) );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithInfo_ShouldSetConstructorAsCreationDetail()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( ctor );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithInfoAndConfiguration_ShouldSetConstructorAsCreationDetail()
    {
        var ctor = typeof( Implementor ).GetConstructor( Type.EmptyTypes )!;
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor( ctor, o => o.SetOnCreatedCallback( onCreatedCallback ) );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.MemberResolutions.TestEmpty(),
                        c.InvocationOptions.ParameterResolutions.TestSequence(
                        [
                            InjectableDependencyResolution<ParameterInfo>.FromFactory( predicate1, factory ),
                            InjectableDependencyResolution<ParameterInfo>.FromImplementorKey(
                                predicate2,
                                new DependencyKey<int>( typeof( IFoo ), 1 ) )
                        ] ) ) ) )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestSequence(
                        [
                            InjectableDependencyResolution<MemberInfo>.FromFactory( predicate1, factory ),
                            InjectableDependencyResolution<MemberInfo>.FromImplementorKey(
                                predicate2,
                                new DependencyKey<int>( typeof( IFoo ), 1 ) )
                        ] ) ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithClearedParameterResolutions_ShouldClearConstructorParameterResolutions()
    {
        var predicate = Substitute.For<Func<ParameterInfo, bool>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor(
            o => o.ResolveParameter( predicate, typeof( IFoo ) ).ResolveMember( _ => true, typeof( IFoo ) ).ClearParameterResolutions() );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_FromConstructor_WithClearedMemberResolutions_ShouldClearConstructorMemberResolutions()
    {
        var predicate = Substitute.For<Func<MemberInfo, bool>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.FromConstructor(
            o => o.ResolveParameter( _ => true, typeof( IFoo ) ).ResolveMember( predicate, typeof( IFoo ) ).ClearMemberResolutions() );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( Implementor ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.Count.TestEquals( 1 ),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_FromType_ShouldSetTypeAsCreationDetail()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( IFoo ) );

        var result = builder.FromType( typeof( Implementor ) );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestRefEquals( typeof( Implementor ) ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_FromType_WithConfiguration_ShouldSetTypeAsCreationDetail()
    {
        var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( IFoo ) );

        var result = builder.FromType( typeof( Implementor ), o => o.SetOnCreatedCallback( onCreatedCallback ) );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.ImplementorType.TestEquals( typeof( IFoo ) ),
                result.Factory.TestNull(),
                result.Constructor.TestNotNull(
                    c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestRefEquals( typeof( Implementor ) ),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToUseDisposableInterfaceCorrectly()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.UseDisposableInterface() );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                result.DisposalStrategy.Callback.TestNull() )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToRenounceOwnershipCorrectly()
    {
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.RenounceOwnership ),
                result.DisposalStrategy.Callback.TestNull() )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToUseCallbackCorrectly()
    {
        var callback = Substitute.For<Action<object>>();
        var sut = new DependencyContainerBuilder();
        var builder = sut.AddSharedImplementor( typeof( Implementor ) );

        var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.UseCallback( callback ) );

        Assertion.All(
                result.TestRefEquals( builder ),
                result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseCallback ),
                result.DisposalStrategy.Callback.TestRefEquals( callback ) )
            .Go();
    }

    [Fact]
    public void AddSharedImplementor_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsOpenGeneric()
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.AddSharedImplementor( typeof( IList<> ) ) );
        action.Test( exc => exc.TestType().Exact<InvalidTypeRegistrationException>( e => e.Type.TestEquals( typeof( IList<> ) ) ) ).Go();
    }

    [Fact]
    public void AddSharedImplementor_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsClosedGenericWithGenericParameters()
    {
        var sut = new DependencyContainerBuilder();
        var type = typeof( List<> ).GetOpenGenericImplementations( typeof( IList<> ) ).Single();
        var action = Lambda.Of( () => sut.AddSharedImplementor( type ) );
        action.Test( exc => exc.TestType().Exact<InvalidTypeRegistrationException>( e => e.Type.TestEquals( type ) ) ).Go();
    }

    [Fact]
    public void GetDependencyRange_ShouldReturnNewlyCreatedEmptyRange_WhenDependencyTypeWasNotRegistered()
    {
        var sut = new DependencyContainerBuilder();

        var result = sut.GetDependencyRange( typeof( IFoo ) );

        Assertion.All(
                result.DependencyType.TestEquals( typeof( IFoo ) ),
                result.Elements.TestEmpty(),
                result.TryGetLast().TestNull() )
            .Go();
    }

    [Fact]
    public void GetDependencyRange_ShouldReturnCorrectBuilder_WhenDependencyTypeWasRegistered()
    {
        var sut = new DependencyContainerBuilder();
        var expected1 = sut.Add<IFoo>();
        var expected2 = sut.Add<IFoo>();

        var result = sut.GetDependencyRange( typeof( IFoo ) );

        Assertion.All(
                result.DependencyType.TestEquals( typeof( IFoo ) ),
                result.Elements.TestSequence( [ expected1, expected2 ] ),
                result.TryGetLast().TestRefEquals( expected2 ),
                expected1.RangeBuilder.TestRefEquals( result ),
                expected2.RangeBuilder.TestRefEquals( result ) )
            .Go();
    }

    [Fact]
    public void GetDependencyRange_SetOnResolvingCallback_ShouldUpdateCallback()
    {
        var callback = Substitute.For<Action<Type, IDependencyScope>>();
        var sut = new DependencyContainerBuilder();

        var result = sut.GetDependencyRange<IFoo>().SetOnResolvingCallback( callback );

        result.OnResolvingCallback.TestRefEquals( callback ).Go();
    }

    [Fact]
    public void GetDependencyRange_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsOpenGeneric()
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.GetDependencyRange( typeof( IList<> ) ) );
        action.Test( exc => exc.TestType().Exact<InvalidTypeRegistrationException>( e => e.Type.TestEquals( typeof( IList<> ) ) ) ).Go();
    }

    [Fact]
    public void GetDependencyRange_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsClosedGenericWithGenericParameters()
    {
        var sut = new DependencyContainerBuilder();
        var type = typeof( List<> ).GetOpenGenericImplementations( typeof( IList<> ) ).Single();
        var action = Lambda.Of( () => sut.GetDependencyRange( type ) );
        action.Test( exc => exc.TestType().Exact<InvalidTypeRegistrationException>( e => e.Type.TestEquals( type ) ) ).Go();
    }

    [Fact]
    public void TryGetSharedImplementor_ShouldReturnNull_WhenSharedImplementorTypeWasNotRegistered()
    {
        var sut = new DependencyContainerBuilder();
        var result = sut.TryGetSharedImplementor( typeof( Implementor ) );
        result.TestNull().Go();
    }

    [Fact]
    public void TryGetSharedImplementor_ShouldReturnCorrectBuilder_WhenSharedImplementorTypeWasRegistered()
    {
        var sut = new DependencyContainerBuilder();
        var expected = sut.AddSharedImplementor<Implementor>();

        var result = sut.TryGetSharedImplementor( typeof( Implementor ) );

        result.TestRefEquals( expected ).Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingSharedImplementor()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromSharedImplementor<Implementor>();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingKeyedSharedImplementorAndNonCachedKeyType()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromSharedImplementor<Implementor>( o => o.Keyed( 1 ) );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingKeyedSharedImplementorAndCachedKeyType()
    {
        var sut = new DependencyContainerBuilder();
        _ = sut.GetKeyedLocator( 1 );
        sut.Add<IFoo>().FromSharedImplementor<Implementor>( o => o.Keyed( 2 ) );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCtorParamDependencyCannotBeResolved()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IWithText>().FromConstructor( ctor );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenMemberDependencyCannotBeResolved()
    {
        var ctor = typeof( FieldImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IWithText>().FromConstructor( ctor );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestTrue(),
                result.Container.TestNotNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                            messages[0].Warnings.Count.TestEquals( 2 ),
                            messages[0].Errors.TestEmpty() ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypedResolutionsHaveInvalidType()
    {
        var ctor = typeof( CtorAndRefMemberImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<int>().FromFactory( _ => 0 );
        sut.Add<string>().FromFactory( _ => string.Empty );
        sut.Add<IWithText>()
            .FromConstructor( ctor, o => o.ResolveParameter( _ => true, typeof( string ) ).ResolveMember( _ => true, typeof( int ) ) );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 2 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitCtorDoesNotCreateInstancesOfCorrectType()
    {
        var ctor = typeof( ExplicitCtorImplementor ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromConstructor( ctor );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitCtorCreatesAbstractInstances()
    {
        var ctor = typeof( AbstractFoo ).GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromConstructor( ctor );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeIsNotOfCorrectType()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType<ExplicitCtorImplementor>();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeIsAbstract()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType<AbstractFoo>();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCtorForTypeCouldNotBeFound()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<ChainableFoo>().FromConstructor();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( ChainableFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCtorForExplicitTypeCouldNotBeFound()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType<ChainableFoo>();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( IFoo ) )]
    [InlineData( typeof( AbstractFoo ) )]
    public void TryBuild_ShouldReturnFailureResult_WhenImplicitCtorCannotBeFoundBecauseDependencyIsAbstract(Type dependencyType)
    {
        var sut = new DependencyContainerBuilder();
        sut.Add( dependencyType ).FromConstructor();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( dependencyType ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( typeof( IFoo ) )]
    [InlineData( typeof( AbstractFoo ) )]
    public void TryBuild_ShouldReturnFailureResult_WhenImplicitCtorCannotBeFoundBecauseExplicitTypeIsAbstract(Type type)
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType( type );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals( ImplementorKey.CreateShared( new DependencyKey( typeof( Implementor ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenDependencyUsesSharedImplementorWithInvalidType()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<string>().FromFactory( _ => "foo" );
        sut.AddSharedImplementor<ExplicitCtorImplementor>();
        sut.Add<IFoo>().FromSharedImplementor<ExplicitCtorImplementor>();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenSelfDependencyIsDetected()
    {
        var ctor = typeof( DecoratorWithText ).GetConstructors().First();

        var sut = new DependencyContainerBuilder();
        sut.Add<IWithText>().FromConstructor( ctor );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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
        sut.Add<IFoo>().FromConstructor( fooCtor, o => o.ResolveParameter( p => p.Name == "otherBar", typeof( IBar ), c => c.Keyed( 1 ) ) );

        sut.Add<IBar>().FromConstructor( barCtor );
        sut.Add<IQux>().FromConstructor( quxCtor );
        sut.Add<IWithText>().FromConstructor( textCtor );

        sut.GetKeyedLocator( 1 )
            .Add<IBar>()
            .FromConstructor(
                otherBarCtor,
                o => o.ResolveParameter( p => p.ParameterType == typeof( IFoo ), typeof( IFoo ), c => c.NotKeyed() ) );

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 2 ) )
                    .Then(
                        messages =>
                        {
                            var first = messages[0];
                            var second = messages[1];
                            return Assertion.All(
                                "messages",
                                first.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                                first.Warnings.TestEmpty(),
                                first.Errors.Count.TestEquals( 2 ),
                                second.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                                second.Warnings.TestEmpty(),
                                second.Errors.Count.TestEquals( 1 ) );
                        } ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestTrue(),
                result.Container.TestNotNull(),
                result.Messages.TestEmpty() )
            .Go();
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

        Assertion.All(
                result.IsOk.TestTrue(),
                result.Container.TestNotNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                            messages[0].Warnings.Count.TestEquals( 1 ),
                            messages[0].Errors.TestEmpty() ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ), 0 ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 2 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenSecondRangeDependencyElementIsInvalid()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromType<Implementor>();
        sut.Add<IFoo>().FromType<ChainableFoo>();
        sut.Add<IFoo>().FromType<MultiCtorImplementor>();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ), 1 ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestTrue(),
                result.Container.TestNotNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.Count.TestEquals( 1 ),
                            messages[0].Errors.TestEmpty() ) ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyForSelfRangeIsDetected()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IWithText>().FromType<RangeDecorator>();
        sut.Add<IWithText>().FromType<RangeDecorator>();
        sut.Add<IWithText>().FromType<RangeDecorator>();

        var result = sut.TryBuild();

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 3 ) )
                    .Then(
                        messages =>
                        {
                            var first = messages[0];
                            var second = messages[1];
                            var third = messages[2];
                            return Assertion.All(
                                "messages",
                                first.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ) ) ),
                                first.Warnings.TestEmpty(),
                                first.Errors.Count.TestEquals( 3 ),
                                second.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ), 0 ) ),
                                second.Warnings.TestEmpty(),
                                second.Errors.Count.TestEquals( 2 ),
                                third.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ), 1 ) ),
                                third.Warnings.TestEmpty(),
                                third.Errors.Count.TestEquals( 1 ) );
                        } ) )
            .Go();
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

        Assertion.All(
                result.IsOk.TestFalse(),
                result.Container.TestNull(),
                result.Messages.TestCount( count => count.TestEquals( 2 ) )
                    .Then(
                        messages =>
                        {
                            var first = messages[0];
                            var second = messages[1];
                            return Assertion.All(
                                "messages",
                                first.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IFoo ) ) ) ),
                                first.Warnings.TestEmpty(),
                                first.Errors.Count.TestEquals( 1 ),
                                second.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IWithText ) ), 0 ) ),
                                second.Warnings.TestEmpty(),
                                second.Errors.Count.TestEquals( 1 ) );
                        } ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldThrowDependencyContainerBuildException_WhenDependencyContainerCouldNotBeBuilt()
    {
        var sut = new DependencyContainerBuilder();
        sut.Add<IFoo>().FromSharedImplementor<Implementor>();
        sut.Add<IBar>().FromSharedImplementor<Implementor>();

        var action = Lambda.Of( () => sut.Build() );

        action.Test(
                exc => exc.TestType()
                    .Exact<DependencyContainerBuildException>( e => e.Messages.SelectMany( m => m.Errors ).Count().TestGreaterThan( 0 ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<DependencyContainerBuildException>( e => e.Messages.SelectMany( m => m.Errors ).Count().TestGreaterThan( 0 ) ) )
            .Go();
    }

    [Fact]
    public void IDependencyContainerBuilder_Build_ShouldBeEquivalentToBuild()
    {
        IDependencyContainerBuilder sut = new DependencyContainerBuilder();
        var result = sut.Build();
        result.TestType().AssignableTo<DependencyContainer>().Go();
    }

    [Fact]
    public void IDependencyLocatorBuilder_SetDefaultLifetime_ShouldBeEquivalentToSetDefaultLifetime()
    {
        IDependencyLocatorBuilder sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultLifetime( DependencyLifetime.Singleton );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.DefaultLifetime.TestEquals( DependencyLifetime.Singleton ) )
            .Go();
    }

    [Fact]
    public void IDependencyLocatorBuilder_SetDefaultDisposalStrategy_ShouldBeEquivalentToSetDefaultDisposalStrategy()
    {
        IDependencyLocatorBuilder sut = new DependencyContainerBuilder();

        var result = sut.SetDefaultDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.DefaultDisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.RenounceOwnership ),
                result.DefaultDisposalStrategy.Callback.TestNull() )
            .Go();
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
