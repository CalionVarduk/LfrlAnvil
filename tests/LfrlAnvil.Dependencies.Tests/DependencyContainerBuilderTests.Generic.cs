using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Dependencies.Tests;

public partial class DependencyContainerBuilderTests
{
    public class Generic : DependencyTestsBase
    {
        [Fact]
        public void AddGeneric_ShouldAddNewDependencyWithoutImplementationDetailsAndWithDefaultLifetime()
        {
            var sut = new DependencyContainerBuilder();

            var result = sut.AddGeneric( typeof( IGenericFoo<> ) );

            Assertion.All(
                    result.DependencyType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.Lifetime.TestEquals( sut.DefaultLifetime ),
                    result.Implementor.TestNull(),
                    result.SharedImplementorKey.TestNull(),
                    result.IsIncludedInRange.TestTrue() )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromConstructor_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromSharedImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor();

            Assertion.All(
                    result.TestRefEquals( builder.Implementor ),
                    builder.SharedImplementorKey.TestNull(),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull(),
                    result.OnResolvingCallback.TestNull(),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromConstructor_WithConfiguration_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
        {
            var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromSharedImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( o => o.SetOnCreatedCallback( onCreatedCallback ) );

            Assertion.All(
                    result.TestRefEquals( builder.Implementor ),
                    builder.SharedImplementorKey.TestNull(),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull(),
                    result.OnResolvingCallback.TestNull(),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromConstructor_WithInfo_ShouldSetConstructorAsCreationDetail()
        {
            var ctor = typeof( GenericImplementor<> ).GetConstructor( Type.EmptyTypes )!;
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromSharedImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( ctor );

            Assertion.All(
                    result.TestRefEquals( builder.Implementor ),
                    builder.SharedImplementorKey.TestNull(),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull(),
                    result.OnResolvingCallback.TestNull(),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromConstructor_WithInfoAndConfiguration_ShouldSetConstructorAsCreationDetail()
        {
            var ctor = typeof( GenericImplementor<> ).GetConstructor( Type.EmptyTypes )!;
            var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromSharedImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( ctor, o => o.SetOnCreatedCallback( onCreatedCallback ) );

            Assertion.All(
                    result.TestRefEquals( builder.Implementor ),
                    builder.SharedImplementorKey.TestNull(),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull(),
                    result.OnResolvingCallback.TestNull(),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromConstructor_ShouldUpdateConstructorInfoAsCreationDetail_WhenCalledMultipleTimes()
        {
            var ctor = typeof( GenericImplementor<> ).GetConstructor( Type.EmptyTypes )!;
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromConstructor();

            var result = builder.FromConstructor( ctor );

            Assertion.All(
                    result.TestRefEquals( builder.Implementor ),
                    builder.SharedImplementorKey.TestNull(),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull(),
                    result.OnResolvingCallback.TestNull(),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromType_ShouldSetTypeAsCreationDetail()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );

            var result = builder.FromType( typeof( GenericImplementor<> ) );

            Assertion.All(
                    result.TestRefEquals( builder.Implementor ),
                    builder.SharedImplementorKey.TestNull(),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull(),
                    result.OnResolvingCallback.TestNull(),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestRefEquals( typeof( GenericImplementor<> ) ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromType_WithConfiguration_ShouldSetTypeAsCreationDetail()
        {
            var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );

            var result = builder.FromType( typeof( GenericImplementor<> ), o => o.SetOnCreatedCallback( onCreatedCallback ) );

            Assertion.All(
                    result.TestRefEquals( builder.Implementor ),
                    builder.SharedImplementorKey.TestNull(),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull(),
                    result.OnResolvingCallback.TestNull(),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestRefEquals( typeof( GenericImplementor<> ) ),
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
        public void AddGeneric_SetLifetime_ShouldUpdateDependencyLifetime(DependencyLifetime lifetime)
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );

            var result = builder.SetLifetime( lifetime );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.Lifetime.TestEquals( lifetime ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_SetLifetime_ShouldThrowArgumentException_WhenValueIsNotValid()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );

            var action = Lambda.Of( () => { builder.SetLifetime( ( DependencyLifetime )4 ); } );

            action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void AddGeneric_IncludeInRange_ShouldUpdateIsIncludedInRangeCorrectly(bool included)
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );

            var result = builder.IncludeInRange( included );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.IsIncludedInRange.TestEquals( included ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromSharedImplementor_ShouldSetProvidedSharedImplementorTypeAsCreationDetail()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromConstructor();

            var result = builder.FromSharedImplementor( typeof( GenericImplementor<> ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.Implementor.TestNull(),
                    result.SharedImplementorKey.TestNotNull( k => Assertion.All(
                        k.Type.TestEquals( typeof( GenericImplementor<> ) ),
                        k.Key.TestNull(),
                        k.KeyType.TestNull(),
                        k.IsKeyed.TestFalse() ) ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromSharedImplementor_Keyed_ShouldSetProvidedSharedImplementorTypeAndKeyAsCreationDetail()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromConstructor();

            var result = builder.FromSharedImplementor( typeof( GenericImplementor<> ), o => o.Keyed( 1 ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.Implementor.TestNull(),
                    result.SharedImplementorKey.TestNotNull( k => Assertion.All(
                        k.Type.TestEquals( typeof( GenericImplementor<> ) ),
                        k.Key.TestEquals( 1 ),
                        k.KeyType.TestEquals( typeof( int ) ),
                        k.IsKeyed.TestTrue() ) ) )
                .Go();
        }

        [Fact]
        public void
            AddGeneric_FromSharedImplementor_ShouldSetProvidedSharedImplementorTypeAndLocatorKeyAsCreationDetail_WhenLocatorIsKeyed()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.GetKeyedLocator( 1 ).AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromConstructor();

            var result = builder.FromSharedImplementor( typeof( GenericImplementor<> ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.Implementor.TestNull(),
                    result.SharedImplementorKey.TestNotNull( k => Assertion.All(
                        k.Type.TestEquals( typeof( GenericImplementor<> ) ),
                        k.Key.TestEquals( 1 ),
                        k.KeyType.TestEquals( typeof( int ) ),
                        k.IsKeyed.TestTrue() ) ) )
                .Go();
        }

        [Fact]
        public void AddGeneric_FromSharedImplementor_NotKeyed_ShouldSetProvidedSharedImplementorTypeWithoutKeyAsCreationDetail()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.GetKeyedLocator( 1 ).AddGeneric( typeof( IGenericFoo<> ) );
            builder.FromConstructor();

            var result = builder.FromSharedImplementor( typeof( GenericImplementor<> ), o => o.NotKeyed() );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.Implementor.TestNull(),
                    result.SharedImplementorKey.TestNotNull( k => Assertion.All(
                        k.Type.TestEquals( typeof( GenericImplementor<> ) ),
                        k.Key.TestNull(),
                        k.KeyType.TestNull(),
                        k.IsKeyed.TestFalse() ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_ShouldAddNewSharedImplementorWithoutCreationDetails()
        {
            var sut = new DependencyContainerBuilder();

            var result = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            Assertion.All(
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull(),
                    result.OnResolvingCallback.TestNull() )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_ShouldDoNothingAndReturnPreviousBuilder_WhenAddingExistingSharedImplementorType()
        {
            var sut = new DependencyContainerBuilder();
            var expected = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_FromConstructor_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor();

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_FromConstructor_WithConfiguration_ShouldSetDefaultAutomaticConstructorAsCreationDetail()
        {
            var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( o => o.SetOnCreatedCallback( onCreatedCallback ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_FromConstructor_WithInfo_ShouldSetConstructorAsCreationDetail()
        {
            var ctor = typeof( GenericImplementor<> ).GetConstructor( Type.EmptyTypes )!;
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( ctor );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_FromConstructor_WithInfoAndConfiguration_ShouldSetConstructorAsCreationDetail()
        {
            var ctor = typeof( GenericImplementor<> ).GetConstructor( Type.EmptyTypes )!;
            var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( ctor, o => o.SetOnCreatedCallback( onCreatedCallback ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestRefEquals( ctor ),
                        c.Type.TestRefEquals( ctor.DeclaringType ),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void
            AddSharedGenericImplementor_FromConstructor_WithParameterResolutions_ShouldAddConstructorParameterResolutionsAsCreationDetail()
        {
            var predicate1 = Substitute.For<Func<ParameterInfo, bool>>();
            var predicate2 = Substitute.For<Func<ParameterInfo, bool>>();
            var factory = Lambda.ExpressionOf( (IDependencyScope s, ParameterInfo p) =>
                Substitute.For<Func<IDependencyScope, ParameterInfo, object>>()( s, p ) );

            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( o =>
                o.ResolveParameter( predicate1, factory ).ResolveParameter( predicate2, typeof( IFoo ), i => i.Keyed( 1 ) ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
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
        public void
            AddSharedGenericImplementor_FromConstructor_WithMemberResolutions_ShouldAddConstructorMemberResolutionsAsCreationDetail()
        {
            var predicate1 = Substitute.For<Func<MemberInfo, bool>>();
            var predicate2 = Substitute.For<Func<MemberInfo, bool>>();
            var factory = Lambda.ExpressionOf( (IDependencyScope s, MemberInfo m) =>
                Substitute.For<Func<IDependencyScope, MemberInfo, object>>()( s, m ) );

            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( o =>
                o.ResolveMember( predicate1, factory ).ResolveMember( predicate2, typeof( IFoo ), i => i.Keyed( 1 ) ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
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
        public void AddSharedGenericImplementor_FromConstructor_WithClearedParameterResolutions_ShouldClearConstructorParameterResolutions()
        {
            var predicate = Substitute.For<Func<ParameterInfo, bool>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( o =>
                o.ResolveParameter( predicate, typeof( IFoo ) ).ResolveMember( _ => true, typeof( IFoo ) ).ClearParameterResolutions() );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_FromConstructor_WithClearedMemberResolutions_ShouldClearConstructorMemberResolutions()
        {
            var predicate = Substitute.For<Func<MemberInfo, bool>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.FromConstructor( o =>
                o.ResolveParameter( _ => true, typeof( IFoo ) ).ResolveMember( predicate, typeof( IFoo ) ).ClearMemberResolutions() );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( GenericImplementor<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestNull(),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.Count.TestEquals( 1 ),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_FromType_ShouldSetTypeAsCreationDetail()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( IGenericFoo<> ) );

            var result = builder.FromType( typeof( GenericImplementor<> ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestRefEquals( typeof( GenericImplementor<> ) ),
                        c.InvocationOptions.OnCreatedCallback.TestNull(),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_FromType_WithConfiguration_ShouldSetTypeAsCreationDetail()
        {
            var onCreatedCallback = Substitute.For<Action<object, Type, IDependencyScope>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( IGenericFoo<> ) );

            var result = builder.FromType( typeof( GenericImplementor<> ), o => o.SetOnCreatedCallback( onCreatedCallback ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.ImplementorType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.Constructor.TestNotNull( c => Assertion.All(
                        c.Info.TestNull(),
                        c.Type.TestRefEquals( typeof( GenericImplementor<> ) ),
                        c.InvocationOptions.OnCreatedCallback.TestRefEquals( onCreatedCallback ),
                        c.InvocationOptions.ParameterResolutions.TestEmpty(),
                        c.InvocationOptions.MemberResolutions.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToUseDisposableInterfaceCorrectly()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.UseDisposableInterface() );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseDisposableInterface ),
                    result.DisposalStrategy.Callback.TestNull() )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToRenounceOwnershipCorrectly()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.RenounceOwnership ),
                    result.DisposalStrategy.Callback.TestNull() )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_SetDisposalStrategy_ShouldUpdateDisposalStrategyToUseCallbackCorrectly()
        {
            var callback = Substitute.For<Action<object>>();
            var sut = new DependencyContainerBuilder();
            var builder = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = builder.SetDisposalStrategy( DependencyImplementorDisposalStrategy.UseCallback( callback ) );

            Assertion.All(
                    result.TestRefEquals( builder ),
                    result.DisposalStrategy.Type.TestEquals( DependencyImplementorDisposalStrategyType.UseCallback ),
                    result.DisposalStrategy.Callback.TestRefEquals( callback ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsNotOpenGeneric()
        {
            var sut = new DependencyContainerBuilder();
            var action = Lambda.Of( () => sut.AddSharedGenericImplementor( typeof( IList<int> ) ) );
            action.Test( exc => exc.TestType().Exact<InvalidTypeRegistrationException>( e => e.Type.TestEquals( typeof( IList<int> ) ) ) )
                .Go();
        }

        [Fact]
        public void AddSharedGenericImplementor_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsOpenIEnumerable()
        {
            var sut = new DependencyContainerBuilder();
            var action = Lambda.Of( () => sut.AddSharedGenericImplementor( typeof( IEnumerable<> ) ) );
            action.Test( exc =>
                    exc.TestType().Exact<InvalidTypeRegistrationException>( e => e.Type.TestEquals( typeof( IEnumerable<> ) ) ) )
                .Go();
        }

        [Fact]
        public void GetGenericDependencyRange_ShouldReturnNewlyCreatedEmptyRange_WhenDependencyTypeWasNotRegistered()
        {
            var sut = new DependencyContainerBuilder();

            var result = sut.GetGenericDependencyRange( typeof( IGenericFoo<> ) );

            Assertion.All(
                    result.DependencyType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.Elements.TestEmpty(),
                    result.TryGetLast().TestNull() )
                .Go();
        }

        [Fact]
        public void GetGenericDependencyRange_ShouldReturnCorrectBuilder_WhenDependencyTypeWasRegistered()
        {
            var sut = new DependencyContainerBuilder();
            var expected1 = sut.AddGeneric( typeof( IGenericFoo<> ) );
            var expected2 = sut.AddGeneric( typeof( IGenericFoo<> ) );

            var result = sut.GetGenericDependencyRange( typeof( IGenericFoo<> ) );

            Assertion.All(
                    result.DependencyType.TestEquals( typeof( IGenericFoo<> ) ),
                    result.Elements.TestSequence( [ expected1, expected2 ] ),
                    result.TryGetLast().TestRefEquals( expected2 ),
                    expected1.RangeBuilder.TestRefEquals( result ),
                    expected2.RangeBuilder.TestRefEquals( result ) )
                .Go();
        }

        [Fact]
        public void GetGenericDependencyRange_SetOnResolvingCallback_ShouldUpdateCallback()
        {
            var callback = Substitute.For<Action<Type, IDependencyScope>>();
            var sut = new DependencyContainerBuilder();

            var result = sut.GetGenericDependencyRange( typeof( IGenericFoo<> ) ).SetOnResolvingCallback( callback );

            result.OnResolvingCallback.TestRefEquals( callback ).Go();
        }

        [Fact]
        public void GetGenericDependencyRange_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsNotOpenGeneric()
        {
            var sut = new DependencyContainerBuilder();
            var action = Lambda.Of( () => sut.GetGenericDependencyRange( typeof( IList<int> ) ) );
            action.Test( exc => exc.TestType().Exact<InvalidTypeRegistrationException>( e => e.Type.TestEquals( typeof( IList<int> ) ) ) )
                .Go();
        }

        [Fact]
        public void GetGenericDependencyRange_ShouldThrowInvalidTypeRegistrationException_WhenTypeIsOpenIEnumerable()
        {
            var sut = new DependencyContainerBuilder();
            var action = Lambda.Of( () => sut.GetGenericDependencyRange( typeof( IEnumerable<> ) ) );
            action.Test( exc =>
                    exc.TestType().Exact<InvalidTypeRegistrationException>( e => e.Type.TestEquals( typeof( IEnumerable<> ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetSharedGenericImplementor_ShouldReturnNull_WhenSharedImplementorTypeWasNotRegistered()
        {
            var sut = new DependencyContainerBuilder();
            var result = sut.TryGetSharedGenericImplementor( typeof( GenericImplementor<> ) );
            result.TestNull().Go();
        }

        [Fact]
        public void TryGetSharedGenericImplementor_ShouldReturnCorrectBuilder_WhenSharedImplementorTypeWasRegistered()
        {
            var sut = new DependencyContainerBuilder();
            var expected = sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );

            var result = sut.TryGetSharedGenericImplementor( typeof( GenericImplementor<> ) );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingSharedImplementor()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingKeyedSharedImplementorAndNonCachedKeyType()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ), o => o.Keyed( 1 ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenDependencyIsImplementedByNonExistingKeyedSharedImplementorAndCachedKeyType()
        {
            var sut = new DependencyContainerBuilder();
            _ = sut.GetKeyedLocator( 1 );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ), o => o.Keyed( 2 ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenCtorParamDependencyCannotBeResolved()
        {
            var ctor = typeof( ExplicitCtorGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenMemberDependencyCannotBeResolved()
        {
            var ctor = typeof( FieldGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnResultWithWarnings_WhenExplicitResolutionsAreUnused()
        {
            var ctor = typeof( GenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.Add<int>().FromFactory( _ => 0 );
            sut.Add<string>().FromFactory( _ => string.Empty );
            sut.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveParameter( _ => false, typeof( int ) )
                        .ResolveMember( _ => false, (_, _) => new object() ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestTrue(),
                    result.Container.TestNotNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.Count.TestEquals( 2 ),
                            messages[0].Errors.TestEmpty() ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypedResolutionsHaveInvalidType()
        {
            var ctor = typeof( CtorAndRefMemberGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.Add<int>().FromFactory( _ => 0 );
            sut.Add<string>().FromFactory( _ => string.Empty );
            sut.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor( ctor, o => o.ResolveParameter( _ => true, typeof( string ) ).ResolveMember( _ => true, typeof( int ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenClosedTypeIsExplicitlyResolvedWithOpenGenericType()
        {
            var ctor = typeof( CtorAndRefMemberGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.Add<string>().FromFactory( _ => string.Empty );
            sut.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveParameter( _ => true, typeof( List<> ) ).ResolveMember( _ => true, typeof( string ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenOpenGenericTypeIsExplicitlyResolvedWithClosedType()
        {
            var ctor = typeof( ChainableGenericFoo<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor( ctor, o => o.ResolveParameter( _ => true, typeof( IGenericBar<int> ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenOpenGenericTypeParameterIsExplicitlyResolvedWithTypeWithFreeGenericArgs()
        {
            var ctor = typeof( ChainableGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor( ctor, o => o.ResolveParameter( _ => true, typeof( GenericFreeFoo<,> ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericQux<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void
            TryBuild_ShouldReturnFailureResult_WhenOpenGenericTypeParameterIsExplicitlyResolvedWithTypeWithWronglySubstitutedGenericArgs()
        {
            var ctor = typeof( ChainableGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveParameter( _ => true, typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( typeof( string ) ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericQux<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenOpenGenericTypeParameterIsExplicitlyResolvedWithTypeThatIsNotFullyOpen()
        {
            var ctor = typeof( ChainableGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveParameter( _ => true, typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( int ) ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericQux<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenOpenGenericTypeMemberIsExplicitlyResolvedWithTypeWithFreeGenericArgs()
        {
            var ctor = typeof( ChainableFieldGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor( ctor, o => o.ResolveMember( _ => true, typeof( GenericFreeFoo<,> ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericQux<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void
            TryBuild_ShouldReturnFailureResult_WhenOpenGenericTypeMemberIsExplicitlyResolvedWithTypeWithWronglySubstitutedGenericArgs()
        {
            var ctor = typeof( ChainableFieldGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveMember( _ => true, typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( typeof( string ) ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericQux<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenOpenGenericTypeMemberIsExplicitlyResolvedWithTypeThatIsNotFullyOpen()
        {
            var ctor = typeof( ChainableFieldGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveMember( _ => true, typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( int ) ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericQux<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitCtorDoesNotCreateInstancesOfCorrectType()
        {
            var ctor = typeof( ExplicitCtorGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromConstructor( ctor );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericBar<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitCtorCreatesAbstractInstances()
        {
            var ctor = typeof( GenericAbstractFoo<> )
                .GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeIsNotOfCorrectType()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ExplicitCtorGenericImplementor<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericBar<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeIsNotOpenGeneric()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ExplicitCtorImplementor ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericBar<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeHasFreeGenericArgs()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericFreeFoo<,> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericBar<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeHasWronglySubstitutedGenericArgs()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericBar<> ) )
                .FromType( typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( typeof( string ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericBar<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeUsesSameGenericArgForMultipleDependencyArgs()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericMulti<,,> ) ).FromType( typeof( GenericCollapsedMulti<,,> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericMulti<,,> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeUsesClosedTypeForSomeDependencyGenericArgs()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericMulti<,,> ) ).FromType( typeof( GenericPartiallyClosedMulti<,,> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericMulti<,,> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( typeof( GenericRefTypeFoo<> ) )]
        [InlineData( typeof( GenericValueTypeFoo<> ) )]
        [InlineData( typeof( GenericDefaultCtorFoo<> ) )]
        [InlineData( typeof( GenericDisposableFoo<> ) )]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeAddsMoreGenericConstraints(Type type)
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( type );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenExplicitTypeIsAbstract()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericAbstractFoo<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenCtorForTypeCouldNotBeFound()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( ExplicitCtorGenericImplementor<> ) ).FromConstructor();

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals(
                                    ImplementorKey.Create( new DependencyKey( typeof( ExplicitCtorGenericImplementor<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenCtorForExplicitTypeCouldNotBeFound()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ExplicitCtorGenericImplementor<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( typeof( IGenericFoo<> ) )]
        [InlineData( typeof( GenericAbstractFoo<> ) )]
        public void TryBuild_ShouldReturnFailureResult_WhenImplicitCtorCannotBeFoundBecauseDependencyIsAbstract(Type dependencyType)
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( dependencyType ).FromConstructor();

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( dependencyType ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( typeof( IGenericFoo<> ) )]
        [InlineData( typeof( GenericAbstractFoo<> ) )]
        public void TryBuild_ShouldReturnFailureResult_WhenImplicitCtorCannotBeFoundBecauseExplicitTypeIsAbstract(Type type)
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( type );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WithOnlyOneEntryWhenTwoDependenciesUseTheSameInvalidSharedImplementor()
        {
            var ctor = typeof( ExplicitCtorGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddSharedGenericImplementor( typeof( GenericImplementor<> ) ).FromConstructor( ctor );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ) );
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals(
                                    ImplementorKey.CreateShared( new DependencyKey( typeof( GenericImplementor<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenDependencyUsesSharedImplementorWithInvalidType()
        {
            var sut = new DependencyContainerBuilder();
            sut.Add<string>().FromFactory( _ => "foo" );
            sut.AddSharedGenericImplementor( typeof( ExplicitCtorGenericImplementor<> ) );
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromSharedImplementor( typeof( ExplicitCtorGenericImplementor<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericBar<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenDependencyUsesSharedImplementorWithFreeGenericArgs()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddSharedGenericImplementor( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( GenericFreeFoo<,> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenDependencyUsesSharedImplementorWithWronglySubstitutedGenericArgs()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddSharedGenericImplementor( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( typeof( string ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenDependencyUsesSharedImplementorThatIsNotFullyOpen()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddSharedGenericImplementor( typeof( GenericFreeFoo<,> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( string ) ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenSharedImplementorIsNotGeneric()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddSharedImplementor<Implementor>();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( Implementor ) );
            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyThroughCtorParamsIsDetected()
        {
            var fooCtor = typeof( ChainableGenericFoo<> ).GetConstructors().First();
            var barCtor = typeof( ChainableGenericBar<> ).GetConstructors().First();
            var quxCtor = typeof( ChainableGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( fooCtor );
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromConstructor( barCtor );
            sut.AddGeneric( typeof( IGenericQux<> ) ).FromConstructor( quxCtor );
            sut.Add<Parameterized<IGenericFoo<string>>>();

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 2 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ),
                            messages[1]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<string> ) ) ) ),
                            messages[1].Warnings.TestEmpty(),
                            messages[1].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyThroughFieldsIsDetected()
        {
            var fooCtor = typeof( ChainableFieldGenericFoo<> ).GetConstructors().First();
            var barCtor = typeof( ChainableFieldGenericBar<> ).GetConstructors().First();
            var quxCtor = typeof( ChainableFieldGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( fooCtor );
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromConstructor( barCtor );
            sut.AddGeneric( typeof( IGenericQux<> ) ).FromConstructor( quxCtor );
            sut.Add<Parameterized<IGenericFoo<string>>>();

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 2 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ),
                            messages[1]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<string> ) ) ) ),
                            messages[1].Warnings.TestEmpty(),
                            messages[1].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyThroughPropertiesIsDetected()
        {
            var fooCtor = typeof( ChainablePropertyGenericFoo<> ).GetConstructors().First();
            var barCtor = typeof( ChainablePropertyGenericBar<> ).GetConstructors().First();
            var quxCtor = typeof( ChainablePropertyGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( fooCtor );
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromConstructor( barCtor );
            sut.AddGeneric( typeof( IGenericQux<> ) ).FromConstructor( quxCtor );
            sut.Add<Parameterized<IGenericFoo<string>>>();

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 2 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ),
                            messages[1]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<string> ) ) ) ),
                            messages[1].Warnings.TestEmpty(),
                            messages[1].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenSelfDependencyIsDetected()
        {
            var ctor = typeof( DecoratedGenericFoo<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            sut.Add<Parameterized<IGenericFoo<string>>>();

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 2 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ),
                            messages[1]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<string> ) ) ) ),
                            messages[1].Warnings.TestEmpty(),
                            messages[1].Errors.Count.TestEquals( 1 ) ) ) )
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
            var ctor = typeof( ExplicitCtorGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.Add<string>().SetLifetime( dependencyLifetime ).FromFactory( _ => string.Empty );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( parentLifetime ).FromConstructor( ctor );

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
            var ctor = typeof( ExplicitCtorGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors( false );
            sut.Add<string>().SetLifetime( dependencyLifetime ).FromFactory( _ => string.Empty );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( parentLifetime ).FromConstructor( ctor );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestTrue(),
                    result.Container.TestNotNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
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
            var ctor = typeof( ChainableFieldGenericQux<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( dependencyLifetime ).FromType( typeof( GenericImplementor<> ) );
            sut.AddGeneric( typeof( IGenericQux<> ) ).SetLifetime( parentLifetime ).FromConstructor( ctor );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericQux<> ) ) ) ),
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
        public void TryBuild_ShouldReturnResultWithWarnings_WhenCaptiveDependencyIsDetectedAndTheyAreTreatedAsWarnings_ForClosedType(
            DependencyLifetime parentLifetime,
            DependencyLifetime dependencyLifetime)
        {
            var ctor = typeof( DefaultCtorParamGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors( false );
            sut.AddGeneric( typeof( IGenericBar<> ) ).SetLifetime( parentLifetime ).FromType( typeof( GenericImplementor<> ) );
            sut.Add<IGenericBar<string>>().SetLifetime( dependencyLifetime ).FromFactory( _ => new GenericImplementor<string>() );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( parentLifetime ).FromConstructor( ctor );
            sut.Add<Parameterized<IGenericFoo<string>>>();

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestTrue(),
                    result.Container.TestNotNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<string> ) ) ) ),
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
        public void TryBuild_ShouldReturnFailureResult_WhenCaptiveDependencyIsDetectedAndTheyAreTreatedAsErrors_ForClosedType(
            DependencyLifetime parentLifetime,
            DependencyLifetime dependencyLifetime)
        {
            var ctor = typeof( DefaultCtorParamGenericImplementor<> ).GetConstructors().First();

            var sut = new DependencyContainerBuilder();
            sut.Configuration.EnableTreatingCaptiveDependenciesAsErrors();
            sut.AddGeneric( typeof( IGenericBar<> ) ).SetLifetime( parentLifetime ).FromType( typeof( GenericImplementor<> ) );
            sut.Add<IGenericBar<string>>().SetLifetime( dependencyLifetime ).FromFactory( _ => new GenericImplementor<string>() );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( parentLifetime ).FromConstructor( ctor );
            sut.Add<Parameterized<IGenericFoo<string>>>();

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<string> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenFirstRangeDependencyElementIsInvalid()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( ChainableGenericBar<> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableGenericFoo<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ), 0 ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenSecondRangeDependencyElementIsInvalid()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableGenericFoo<> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( MultiCtorGenericImplementor<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0]
                                .ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ), 1 ) ),
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
            sut.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( parentLifetime ).FromType( typeof( GenericRangeFoo<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestTrue(),
                    result.Container.TestNotNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
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
            sut.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( parentLifetime ).FromType( typeof( GenericRangeFoo<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 1 ) )
                        .Then( messages => Assertion.All(
                            messages[0].ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                            messages[0].Warnings.TestEmpty(),
                            messages[0].Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryBuild_ShouldReturnFailureResult_WhenCircularDependencyForSelfRangeIsDetected()
        {
            var sut = new DependencyContainerBuilder();
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericFooRangeDecorator<> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericFooRangeDecorator<> ) );
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericFooRangeDecorator<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 3 ) )
                        .Then( messages =>
                        {
                            var first = messages[0];
                            var second = messages[1];
                            var third = messages[2];
                            return Assertion.All(
                                "messages",
                                first.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                                first.Warnings.TestEmpty(),
                                first.Errors.Count.TestEquals( 3 ),
                                second.ImplementorKey.TestEquals(
                                    ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ), 0 ) ),
                                second.Warnings.TestEmpty(),
                                second.Errors.Count.TestEquals( 2 ),
                                third.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ), 1 ) ),
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
            sut.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableGenericFooRange<> ) );
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericBarRangeDecorator<> ) );
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableGenericRange<> ) );
            sut.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );

            var result = sut.TryBuild();

            Assertion.All(
                    result.IsOk.TestFalse(),
                    result.Container.TestNull(),
                    result.Messages.TestCount( count => count.TestEquals( 2 ) )
                        .Then( messages =>
                        {
                            var first = messages[0];
                            var second = messages[1];
                            return Assertion.All(
                                "messages",
                                first.ImplementorKey.TestEquals( ImplementorKey.Create( new DependencyKey( typeof( IGenericFoo<> ) ) ) ),
                                first.Warnings.TestEmpty(),
                                first.Errors.Count.TestEquals( 1 ),
                                second.ImplementorKey.TestEquals(
                                    ImplementorKey.Create( new DependencyKey( typeof( IGenericBar<> ) ), 0 ) ),
                                second.Warnings.TestEmpty(),
                                second.Errors.Count.TestEquals( 1 ) );
                        } ) )
                .Go();
        }

        [Fact]
        public void GetDependencyRange_ForGenericType_ShouldRegisterOpenGeneric()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.GetDependencyRange<IGenericFoo<string>>();
            var openBuilder = builder.OpenGenericBuilder;

            Assertion.All(
                    openBuilder.TestRefEquals( sut.GetGenericDependencyRange( typeof( IGenericFoo<> ) ) ),
                    openBuilder.TestNotNull( b => Assertion.All(
                        "openBuilder",
                        b.Elements.TestEmpty(),
                        b.DependencyType.TestEquals( typeof( IGenericFoo<> ) ),
                        b.ClosedBuilders.TestSequence( [ builder ] ) ) ) )
                .Go();
        }

        [Fact]
        public void GetDependencyRange_ForGenericType_ShouldUpdateOpenGeneric_WhenItExists()
        {
            var sut = new DependencyContainerBuilder();
            var stringBuilder = sut.GetDependencyRange<IGenericFoo<string>>();
            var intBuilder = sut.GetDependencyRange<IGenericFoo<int>>();

            Assertion.All(
                    stringBuilder.OpenGenericBuilder.TestRefEquals( sut.GetGenericDependencyRange( typeof( IGenericFoo<> ) ) ),
                    intBuilder.OpenGenericBuilder.TestRefEquals( stringBuilder.OpenGenericBuilder ),
                    stringBuilder.OpenGenericBuilder.TestNotNull( b => Assertion.All(
                        "openBuilder",
                        b.Elements.TestEmpty(),
                        b.DependencyType.TestEquals( typeof( IGenericFoo<> ) ),
                        b.ClosedBuilders.TestSequence( [ stringBuilder, intBuilder ] ) ) ) )
                .Go();
        }

        [Fact]
        public void GetDependencyRange_ForClosedIEnumerable_ShouldNotRegisterOpenGeneric()
        {
            var sut = new DependencyContainerBuilder();
            var builder = sut.GetDependencyRange<IEnumerable<IFoo>>();
            builder.OpenGenericBuilder.TestNull().Go();
        }

        [Fact]
        public void OpenGenericBuilders_ShouldBePropagatedToClosedBuildersInCorrectOrder()
        {
            var sut = new DependencyContainerBuilder();
            var bar = sut.Add<IGenericBar<string>>();
            var stringBuilder = sut.Add<IGenericFoo<string>>();
            var openBuilder = sut.AddGeneric( typeof( IGenericFoo<> ) );
            var intBuilder = sut.Add<IGenericFoo<int>>();

            Assertion.All(
                    stringBuilder.IsLastInRange.TestFalse(),
                    stringBuilder.RangeIndex.TestEquals( 0 ),
                    stringBuilder.RangeBuilder.TryGetLast().TestRefEquals( stringBuilder ),
                    stringBuilder.RangeBuilder.Elements.TestSequence( [ stringBuilder ] ),
                    intBuilder.IsLastInRange.TestTrue(),
                    intBuilder.RangeIndex.TestEquals( 1 ),
                    intBuilder.RangeBuilder.TryGetLast().TestRefEquals( intBuilder ),
                    intBuilder.RangeBuilder.Elements.TestSequence( [ intBuilder ] ),
                    openBuilder.IsLastInRange.TestTrue(),
                    openBuilder.RangeIndex.TestEquals( 0 ),
                    openBuilder.RangeBuilder.TryGetLast().TestRefEquals( openBuilder ),
                    openBuilder.RangeBuilder.Elements.TestSequence( [ openBuilder ] ),
                    openBuilder.IsLastInClosedRange( stringBuilder.RangeBuilder ).TestTrue(),
                    openBuilder.GetClosedRangeIndex( stringBuilder.RangeBuilder ).TestEquals( 1 ),
                    openBuilder.IsLastInClosedRange( intBuilder.RangeBuilder ).TestFalse(),
                    openBuilder.GetClosedRangeIndex( intBuilder.RangeBuilder ).TestEquals( 0 ),
                    openBuilder.IsLastInClosedRange( bar.RangeBuilder ).TestFalse(),
                    openBuilder.GetClosedRangeIndex( bar.RangeBuilder ).TestEquals( -1 ) )
                .Go();
        }
    }
}

public class GenericCollapsedMulti<T1, T2, T3> : DependencyTestsBase.IGenericMulti<T1, T2, T1> { }

public class GenericPartiallyClosedMulti<T1, T2, T3> : DependencyTestsBase.IGenericMulti<T1, T2, int> { }

public class GenericRefTypeFoo<T> : DependencyTestsBase.IGenericFoo<T>
    where T : class { }

public class GenericValueTypeFoo<T> : DependencyTestsBase.IGenericFoo<T>
    where T : struct { }

public class GenericDefaultCtorFoo<T> : DependencyTestsBase.IGenericFoo<T>
    where T : new() { }

public class GenericDisposableFoo<T> : DependencyTestsBase.IGenericFoo<T>
    where T : IDisposable { }
