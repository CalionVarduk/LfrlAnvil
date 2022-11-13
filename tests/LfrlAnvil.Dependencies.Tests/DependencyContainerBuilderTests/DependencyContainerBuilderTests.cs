﻿using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
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
    public void SetInjectablePropertyType_ShouldThrowInvalidInjectablePropertyTypeException_WhenTypeIsNotValid(Type type)
    {
        var sut = new DependencyContainerBuilder();
        var action = Lambda.Of( () => sut.SetInjectablePropertyType( type ) );
        action.Should().ThrowExactly<InvalidInjectablePropertyTypeException>();
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
            result.SharedImplementorType.Should().BeNull();
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
            builder.SharedImplementorType.Should().BeNull();
            result.ImplementorType.Should().Be( typeof( IFoo ) );
            result.Factory.Should().BeSameAs( factory );
            result.DisposalStrategy.Type.Should().Be( DependencyImplementorDisposalStrategyType.UseDisposableInterface );
            result.DisposalStrategy.Callback.Should().BeNull();
            result.OnResolvingCallback.Should().BeNull();
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
            result.SharedImplementorType.Should().Be( typeof( Implementor ) );
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
            message.DependencyType.Should().Be( typeof( IFoo ) );
            message.ImplementorType.Should().Be( typeof( Implementor ) );
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
