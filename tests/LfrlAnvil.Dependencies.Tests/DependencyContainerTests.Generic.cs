using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Dependencies.Extensions;

namespace LfrlAnvil.Dependencies.Tests;

public partial class DependencyContainerTests
{
    public class Generic : DependencyTestsBase
    {
        [Fact]
        public void ResolvingDependency_WithOpenGenericDependencies_ShouldCloseDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableGenericFoo<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType( typeof( ChainableGenericQux<> ), opt => opt.ResolveParameter( _ => true, typeof( GenericImplementor<> ) ) );

            builder.Add<Foo>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Foo>();

            result.Inner.TestType().Exact<ChainableGenericFoo<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithOpenGenericDependencies_ShouldCloseDependencies_ViaMembers()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainablePropertyGenericFoo<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainablePropertyGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainablePropertyGenericQux<> ),
                    opt => opt.ResolveMember( _ => true, typeof( GenericImplementor<> ) ) );

            builder.Add<Foo>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Foo>();

            result.Inner.TestType().Exact<ChainablePropertyGenericFoo<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_GenericClosedDuringBuild()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableGenericFoo<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType( typeof( ChainableGenericQux<> ), opt => opt.ResolveParameter( _ => true, typeof( GenericImplementor<> ) ) );

            builder.Add<Foo>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<ChainableGenericFoo<string>>().Go();
        }

        [Fact]
        public void ResolvingTransientDependency_WithSharedImplementor_ShouldReturnNewInstanceEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Transient );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Transient );

            builder.Add<Parameterized<IGenericFoo<string>>>().SetLifetime( DependencyLifetime.Transient );
            builder.Add<Parameterized<IGenericBar<string>>>().SetLifetime( DependencyLifetime.Transient );
            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = scope.Locator.Resolve<Parameterized<IGenericBar<string>>>();

            new object[] { result1.Inner, result2.Inner }.Distinct().Count().TestEquals( 2 ).Go();
        }

        [Fact]
        public void ResolvingSingletonDependency_WithSharedImplementor_ShouldReturnSameInstanceEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.Add<Parameterized<IGenericFoo<string>>>().SetLifetime( DependencyLifetime.Singleton );
            builder.Add<Parameterized<IGenericBar<string>>>().SetLifetime( DependencyLifetime.Singleton );
            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = scope.Locator.Resolve<Parameterized<IGenericBar<string>>>();

            result1.Inner.TestRefEquals( result2.Inner ).Go();
        }

        [Fact]
        public void
            ResolvingScopedSingletonDependency_WithSharedImplementor_ShouldReturnNewInstancePerScopeAndItsChildrenForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.ScopedSingleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.ScopedSingleton );

            builder.Add<Parameterized<IGenericFoo<string>>>().SetLifetime( DependencyLifetime.ScopedSingleton );
            builder.Add<Parameterized<IGenericBar<string>>>().SetLifetime( DependencyLifetime.ScopedSingleton );
            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = scope.BeginScope().Locator.Resolve<Parameterized<IGenericBar<string>>>();
            var result3 = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            Assertion.All(
                    result1.Inner.TestRefEquals( result2.Inner ),
                    result1.Inner.TestRefEquals( result3.Inner ) )
                .Go();
        }

        [Fact]
        public void ResolvingScopedDependency_WithSharedImplementor_ShouldReturnSameInstancePerScopeEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Scoped );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Scoped );

            builder.Add<Parameterized<IGenericFoo<string>>>().SetLifetime( DependencyLifetime.Scoped );
            builder.Add<Parameterized<IGenericBar<string>>>().SetLifetime( DependencyLifetime.Scoped );
            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = scope.Locator.Resolve<Parameterized<IGenericBar<string>>>();

            result1.Inner.TestRefEquals( result2.Inner ).Go();
        }

        [Theory]
        [InlineData( DependencyLifetime.Transient )]
        [InlineData( DependencyLifetime.Scoped )]
        [InlineData( DependencyLifetime.ScopedSingleton )]
        [InlineData( DependencyLifetime.Singleton )]
        public void ResolvingDependency_WithSharedImplementor_ShouldReturnNewInstanceEachTimeForDifferentGenericArgs(
            DependencyLifetime lifetime)
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ) ).SetLifetime( lifetime );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ) ).SetLifetime( lifetime );
            builder.Add<Parameterized<IGenericFoo<string>>>().SetLifetime( lifetime );
            builder.Add<Parameterized<IGenericBar<int>>>().SetLifetime( lifetime );
            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = scope.Locator.Resolve<Parameterized<IGenericBar<int>>>();

            new object[] { result1.Inner, result2.Inner }.Distinct().Count().TestEquals( 2 ).Go();
        }

        [Theory]
        [InlineData( DependencyLifetime.Transient )]
        [InlineData( DependencyLifetime.Scoped )]
        [InlineData( DependencyLifetime.ScopedSingleton )]
        [InlineData( DependencyLifetime.Singleton )]
        public void ResolvingDependency_ShouldInvokeOnResolvingCallbackEveryTime(DependencyLifetime lifetime)
        {
            var onResolvingCallback = Substitute.For<Action<Type, IDependencyScope>>();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( lifetime )
                .FromType( typeof( GenericImplementor<> ) )
                .SetOnResolvingCallback( onResolvingCallback );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            builder.Add<Parameterized<IGenericFoo<int>>>();
            var sut = builder.Build();
            var scope = sut.RootScope.BeginScope();

            _ = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            _ = scope.Locator.Resolve<Parameterized<IGenericFoo<int>>>();

            Assertion.All(
                    onResolvingCallback.CallCount().TestEquals( 2 ),
                    onResolvingCallback.CallAt( 0 ).Arguments.TestSequence( [ typeof( IGenericFoo<string> ), scope ] ),
                    onResolvingCallback.CallAt( 1 ).Arguments.TestSequence( [ typeof( IGenericFoo<int> ), scope ] ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithSharedKeyedImplementor_ShouldReturnCorrectInstances()
        {
            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 )
                .AddSharedGenericImplementor( typeof( GenericImplementor<> ) )
                .FromType( typeof( GenericImplementor<> ) );

            builder.GetKeyedLocator( 1 )
                .AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ), o => o.Keyed( 1 ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.GetKeyedLocator( "foo" )
                .AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ), o => o.Keyed( 1 ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.Add<Parameterized<IGenericBar<string>>>();
            builder.GetKeyedLocator( 1 ).Add<Parameterized<IGenericFoo<string>>>();
            builder.GetKeyedLocator( "foo" ).Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericBar<string>>>();
            var result2 = sut.RootScope.GetKeyedLocator( 1 ).Resolve<Parameterized<IGenericFoo<string>>>();
            var result3 = sut.RootScope.GetKeyedLocator( "foo" ).Resolve<Parameterized<IGenericQux<string>>>();

            Assertion.All(
                    result1.Inner.TestRefEquals( result2.Inner ),
                    result2.Inner.TestRefEquals( result3.Inner ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtorAndDefaultParameter_ShouldReturnCorrectInstance_WhenParameterIsNotResolvable()
        {
            var ctor = typeof( DefaultCtorParamGenericImplementor<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<DefaultCtorParamGenericImplementor<string>>( e => e.Bar.TestNull() ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtorAndDefaultParameter_ShouldReturnCorrectInstance_WhenParameterIsResolvable()
        {
            var ctor = typeof( DefaultCtorParamGenericImplementor<> ).GetConstructors().First();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<DefaultCtorParamGenericImplementor<string>>( e => e.Bar.TestNotNull() ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtorAndOptionalParameter_ShouldReturnCorrectInstance_WhenParameterIsNotResolvable()
        {
            var ctor = typeof( OptionalCtorParamGenericImplementor<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<OptionalCtorParamGenericImplementor<string>>( e => e.Bar.TestNull() ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtorAndOptionalParameter_ShouldReturnCorrectInstance_WhenParameterIsResolvable()
        {
            var ctor = typeof( OptionalCtorParamGenericImplementor<> ).GetConstructors().First();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<OptionalCtorParamGenericImplementor<string>>( e => e.Bar.TestNotNull() ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromImplementorType()
        {
            var ctor = typeof( ChainableGenericFoo<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor( ctor, o => o.ResolveParameter( p => p.Name == "bar", typeof( GenericImplementor<> ) ) );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<ChainableGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromKeyedImplementorType()
        {
            var ctor = typeof( ChainableGenericFoo<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveParameter( p => p.Name == "bar", typeof( GenericImplementor<> ), c => c.Keyed( 1 ) ) );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<ChainableGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtorAndOptionalMember_ShouldReturnCorrectInstance_WhenMemberIsNotResolvable()
        {
            var ctor = typeof( OptionalMemberGenericImplementor<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<ParameterizedMember<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ParameterizedMember<IGenericFoo<string>>>();

            result.Inner.Instance.TestType().Exact<OptionalMemberGenericImplementor<string>>( e => e.Bar.TestNull() ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtorAndOptionalMember_ShouldReturnCorrectInstance_WhenMemberIsResolvable()
        {
            var ctor = typeof( OptionalMemberGenericImplementor<> ).GetConstructors().First();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<ParameterizedMember<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ParameterizedMember<IGenericFoo<string>>>();

            result.Inner.Instance.TestType().Exact<OptionalMemberGenericImplementor<string>>( e => e.Bar.TestNotNull() ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromImplementorType()
        {
            var ctor = typeof( ChainablePropertyGenericFoo<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor( ctor, o => o.ResolveMember( m => m.Name.Contains( "_bar" ), typeof( GenericImplementor<> ) ) );

            builder.Add<ParameterizedMember<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ParameterizedMember<IGenericFoo<string>>>();

            result.Inner.Instance.TestType()
                .Exact<ChainablePropertyGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromKeyedImplementorType()
        {
            var ctor = typeof( ChainablePropertyGenericFoo<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveMember( m => m.Name.Contains( "_bar" ), typeof( GenericImplementor<> ), c => c.Keyed( 1 ) ) );

            builder.Add<ParameterizedMember<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ParameterizedMember<IGenericFoo<string>>>();

            result.Inner.Instance.TestType()
                .Exact<ChainablePropertyGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldCallOnCreatedCallback()
        {
            var callback = Substitute.For<Action<object, Type, IDependencyScope>>();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ), o => o.SetOnCreatedCallback( callback ) );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            builder.Add<Parameterized<IGenericFoo<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            _ = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<int>>>();
            _ = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<int>>>();

            Assertion.All(
                    callback.CallCount().TestEquals( 2 ),
                    callback.CallAt( 0 ).Exists.TestTrue(),
                    callback.CallAt( 1 ).Exists.TestTrue(),
                    callback.CallAt( 0 ).Arguments.TestSequence( [ result1.Inner, typeof( IGenericFoo<string> ), sut.RootScope ] ),
                    callback.CallAt( 1 ).Arguments.TestSequence( [ result2.Inner, typeof( IGenericFoo<int> ), sut.RootScope ] ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithSpecializedOptionalCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.Add<IGenericBar<string>>().FromType<GenericImplementor<string>>();
            builder.Add<Parameterized<IGenericFoo<string>>>();
            builder.Add<Parameterized<IGenericFoo<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<int>>>();

            Assertion.All(
                    result1.Inner.TestType().Exact<DefaultCtorParamGenericImplementor<string>>( e => e.Bar.TestNotNull() ),
                    result2.Inner.TestType().Exact<DefaultCtorParamGenericImplementor<int>>( e => e.Bar.TestNull() ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithSpecializedCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableGenericFoo<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IGenericBar<string>>().FromType<GenericImplementor<string>>();
            builder.Add<Parameterized<IGenericFoo<string>>>();
            builder.Add<Parameterized<IGenericFoo<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<int>>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<ChainableGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() ),
                    result2.Inner.TestType().Exact<ChainableGenericFoo<int>>( e => e.Bar.TestType().Exact<ChainableGenericBar<int>>() ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithOptionalNonGenericCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParameterGenericImplementor<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<OptionalCtorParameterGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithNonGenericCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ExplicitCtorGenericImplementor<> ) );
            builder.Add<string>().FromFactory( _ => "foo" );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<ExplicitCtorGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithClosedGenericCtorParameterViaOpenGenericRegistration()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericCtorParameterGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<GenericCtorParameterGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithSpecializedOptionalMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalMemberGenericImplementor<> ) );
            builder.Add<IGenericBar<string>>().FromType<GenericImplementor<string>>();
            builder.Add<Parameterized<IGenericFoo<string>>>();
            builder.Add<Parameterized<IGenericFoo<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<int>>>();

            Assertion.All(
                    result1.Inner.TestType().Exact<OptionalMemberGenericImplementor<string>>( e => e.Bar.TestNotNull() ),
                    result2.Inner.TestType().Exact<OptionalMemberGenericImplementor<int>>( e => e.Bar.TestNull() ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithSpecializedMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableFieldGenericFoo<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableFieldGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IGenericBar<string>>().FromType<GenericImplementor<string>>();
            builder.Add<Parameterized<IGenericFoo<string>>>();
            builder.Add<Parameterized<IGenericFoo<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<int>>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<ChainableFieldGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() ),
                    result2.Inner.TestType()
                        .Exact<ChainableFieldGenericFoo<int>>( e => e.Bar.TestType().Exact<ChainableFieldGenericBar<int>>() ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithOptionalNonGenericMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalMemberGenericImplementor<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<OptionalMemberGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithNonGenericMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( FieldGenericImplementor<> ) );
            builder.Add<string>().FromFactory( _ => "foo" );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<FieldGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithClosedGenericMemberViaOpenGenericRegistration()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericMemberGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<GenericMemberGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithComplexDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ComplexGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( List<> ) );
            builder.AddGeneric( typeof( Dictionary<,> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<ComplexGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_ForClosedTypeWithSharedOpenGenericImplementor()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.Add<IGenericFoo<int>>().FromSharedImplementor<GenericImplementor<int>>().SetLifetime( DependencyLifetime.Singleton );
            builder.Add<IGenericBar<int>>().FromSharedImplementor<GenericImplementor<int>>().SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.Add<Parameterized<IGenericQux<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<int>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericBar<int>>();
            var result3 = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<int>>>();

            Assertion.All(
                    result1.TestRefEquals( result2 ),
                    result2.TestRefEquals( result3.Inner ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithValidConstraints()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericConstrained<> ) ).FromType( typeof( GenericConstrainedImplementor<> ) );
            builder.Add<Parameterized<IGenericConstrained<ConstrainedArg>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericConstrained<ConstrainedArg>>>();

            result.Inner.TestType().Exact<GenericConstrainedImplementor<ConstrainedArg>>().Go();
        }
    }
}

public class Foo
{
    public Foo(DependencyTestsBase.IGenericFoo<string> inner)
    {
        Inner = inner;
    }

    public DependencyTestsBase.IGenericFoo<string> Inner { get; }
}

public class OptionalCtorParameterGenericImplementor<T> : DependencyTestsBase.IGenericFoo<T>
{
    public OptionalCtorParameterGenericImplementor(string? text = null)
    {
        Text = text;
    }

    public string? Text { get; }
}

public class OptionalMemberGenericImplementor<T> : DependencyTestsBase.IGenericFoo<T>
{
    [OptionalDependency]
    public Injected<string?> Text { get; }
}

public class GenericCtorParameterGenericImplementor<T> : DependencyTestsBase.IGenericFoo<T>
{
    public GenericCtorParameterGenericImplementor(DependencyTestsBase.IGenericBar<string> bar)
    {
        Bar = bar;
    }

    public DependencyTestsBase.IGenericBar<string> Bar { get; }
}

public class GenericMemberGenericImplementor<T> : DependencyTestsBase.IGenericFoo<T>
{
    [OptionalDependency]
    public Injected<DependencyTestsBase.IGenericBar<string>> Text { get; }
}

public class ComplexGenericImplementor<T> : DependencyTestsBase.IGenericFoo<T>
{
    public ComplexGenericImplementor(DependencyTestsBase.IGenericBar<T> bar, List<T> list, Dictionary<string, List<T>> dict)
    {
        Bar = bar;
        List = list;
        Dict = dict;
    }

    public DependencyTestsBase.IGenericBar<T> Bar { get; }
    public List<T> List { get; }
    public Dictionary<string, List<T>> Dict { get; }
}

public interface IGenericConstrained<T>
    where T : IDisposable, IEnumerable<int>;

public class GenericConstrainedImplementor<T> : IGenericConstrained<T>
    where T : IDisposable, IEnumerable<int>;

public class ConstrainedArg : IDisposable, IEnumerable<int>
{
    public void Dispose() { }

    public IEnumerator<int> GetEnumerator()
    {
        return Enumerable.Empty<int>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
