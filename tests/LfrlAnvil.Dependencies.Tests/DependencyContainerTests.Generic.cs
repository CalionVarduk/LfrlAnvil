using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Functional;

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
        public void ResolvingDependency_WithNestedMembers()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( GenericNestedMember<> ) );
            builder.Add<Parameterized<GenericNestedMember<string>>>();

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<GenericNestedMember<string>>>();

            Assertion.All(
                    result.Inner.Foo.TestType().AssignableTo<GenericImplementor<string>>(),
                    result.Inner.Bar.TestType().AssignableTo<ChainableGenericBar<string>>() )
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

        [Fact]
        public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitInjectionIsInvalidAndOtherCtorIsChosen()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( MultiCtorGenericImplementor<> ) )
                .FromConstructor( o => o.ResolveParameter(
                    p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof( IGenericBar<> ),
                    typeof( ChainableGenericFoo<> ) ) );

            builder.Add<Parameterized<MultiCtorGenericImplementor<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<MultiCtorGenericImplementor<string>>>();

            Assertion.All(
                    result.Inner.Bar.TestNull(),
                    result.Inner.Qux.TestType().AssignableTo<GenericImplementor<string>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterAndMemberAreInjectable()
        {
            var ctor = typeof( CtorAndRefMemberGenericImplementor<> ).GetConstructors().First();
            var stringValue = Fixture.Create<string>();
            var intValue = Fixture.Create<int>();

            var builder = new DependencyContainerBuilder();
            builder.Add<string>().FromFactory( _ => stringValue );
            builder.Add<int>().FromFactory( _ => intValue );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType()
                .AssignableTo<CtorAndRefMemberGenericImplementor<string>>( e => e.Text.TestEquals( $"{stringValue}{intValue}" ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterAndMemberAreInjectableWithNullableType()
        {
            var ctor = typeof( CtorAndValueMemberGenericImplementor<> ).GetConstructors().First();
            var byteValue = Fixture.Create<byte>();
            var intValue = Fixture.Create<int>();

            var builder = new DependencyContainerBuilder();
            builder.Add<byte?>().FromFactory( _ => byteValue );
            builder.Add<int>().FromFactory( _ => intValue );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType()
                .AssignableTo<CtorAndValueMemberGenericImplementor<string>>( e => e.Text.TestEquals( $"{intValue}{byteValue}" ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependencyDirectly_ShouldThrowOpenGenericDependencyException()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            var sut = builder.Build();

            var action = Lambda.Of( () => sut.RootScope.Locator.Resolve( typeof( GenericImplementor<> ) ) );

            action.Test( exc => exc.TestType().Exact<OpenGenericDependencyException>() ).Go();
        }

        [Fact]
        public void ResolvingTransientOpenDependency_WithSharedImplementor_ShouldReturnNewInstanceEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Transient );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Transient );

            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.Locator.Resolve<IGenericBar<string>>();

            new object[] { result1, result2 }.Distinct().Count().TestEquals( 2 ).Go();
        }

        [Fact]
        public void ResolvingSingletonOpenDependency_WithSharedImplementor_ShouldReturnSameInstanceEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.Locator.Resolve<IGenericBar<string>>();

            result1.TestRefEquals( result2 ).Go();
        }

        [Fact]
        public void
            ResolvingScopedSingletonOpenDependency_WithSharedImplementor_ShouldReturnNewInstancePerScopeAndItsChildrenForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.ScopedSingleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.ScopedSingleton );

            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.BeginScope().Locator.Resolve<IGenericBar<string>>();
            var result3 = scope.Locator.Resolve<IGenericFoo<string>>();

            Assertion.All(
                    result1.TestRefEquals( result2 ),
                    result1.TestRefEquals( result3 ) )
                .Go();
        }

        [Fact]
        public void ResolvingScopedOpenDependency_WithSharedImplementor_ShouldReturnSameInstancePerScopeEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Scoped );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Scoped );

            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.Locator.Resolve<IGenericBar<string>>();

            result1.TestRefEquals( result2 ).Go();
        }

        [Theory]
        [InlineData( DependencyLifetime.Transient )]
        [InlineData( DependencyLifetime.Scoped )]
        [InlineData( DependencyLifetime.ScopedSingleton )]
        [InlineData( DependencyLifetime.Singleton )]
        public void ResolvingOpenDependency_WithSharedImplementor_ShouldReturnNewInstanceEachTimeForDifferentGenericArgs(
            DependencyLifetime lifetime)
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ) ).SetLifetime( lifetime );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromSharedImplementor( typeof( GenericImplementor<> ) ).SetLifetime( lifetime );
            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.Locator.Resolve<IGenericBar<int>>();

            new object[] { result1, result2 }.Distinct().Count().TestEquals( 2 ).Go();
        }

        [Theory]
        [InlineData( DependencyLifetime.Transient )]
        [InlineData( DependencyLifetime.Scoped )]
        [InlineData( DependencyLifetime.ScopedSingleton )]
        [InlineData( DependencyLifetime.Singleton )]
        public void ResolvingOpenDependency_ShouldInvokeOnResolvingCallbackEveryTime(DependencyLifetime lifetime)
        {
            var onResolvingCallback = Substitute.For<Action<Type, IDependencyScope>>();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( lifetime )
                .FromType( typeof( GenericImplementor<> ) )
                .SetOnResolvingCallback( onResolvingCallback );

            var sut = builder.Build();
            var scope = sut.RootScope.BeginScope();

            _ = scope.Locator.Resolve<IGenericFoo<string>>();
            _ = scope.Locator.Resolve<IGenericFoo<int>>();

            Assertion.All(
                    onResolvingCallback.CallCount().TestEquals( 2 ),
                    onResolvingCallback.CallAt( 0 ).Arguments.TestSequence( [ typeof( IGenericFoo<string> ), scope ] ),
                    onResolvingCallback.CallAt( 1 ).Arguments.TestSequence( [ typeof( IGenericFoo<int> ), scope ] ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithSharedKeyedImplementor_ShouldReturnCorrectInstances()
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

            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericBar<string>>();
            var result2 = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IGenericFoo<string>>();
            var result3 = sut.RootScope.GetKeyedLocator( "foo" ).Resolve<IGenericQux<string>>();

            Assertion.All(
                    result1.TestRefEquals( result2 ),
                    result2.TestRefEquals( result3 ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtorAndDefaultParameter_ShouldReturnCorrectInstance_WhenParameterIsNotResolvable()
        {
            var ctor = typeof( DefaultCtorParamGenericImplementor<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<DefaultCtorParamGenericImplementor<string>>( e => e.Bar.TestNull() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtorAndDefaultParameter_ShouldReturnCorrectInstance_WhenParameterIsResolvable()
        {
            var ctor = typeof( DefaultCtorParamGenericImplementor<> ).GetConstructors().First();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<DefaultCtorParamGenericImplementor<string>>( e => e.Bar.TestNotNull() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtorAndOptionalParameter_ShouldReturnCorrectInstance_WhenParameterIsNotResolvable()
        {
            var ctor = typeof( OptionalCtorParamGenericImplementor<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<OptionalCtorParamGenericImplementor<string>>( e => e.Bar.TestNull() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtorAndOptionalParameter_ShouldReturnCorrectInstance_WhenParameterIsResolvable()
        {
            var ctor = typeof( OptionalCtorParamGenericImplementor<> ).GetConstructors().First();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<OptionalCtorParamGenericImplementor<string>>( e => e.Bar.TestNotNull() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromImplementorType()
        {
            var ctor = typeof( ChainableGenericFoo<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor( ctor, o => o.ResolveParameter( p => p.Name == "bar", typeof( GenericImplementor<> ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<ChainableGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() ).Go();
        }

        [Fact]
        public void
            ResolvingOpenDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromKeyedImplementorType()
        {
            var ctor = typeof( ChainableGenericFoo<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveParameter( p => p.Name == "bar", typeof( GenericImplementor<> ), c => c.Keyed( 1 ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<ChainableGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtorAndOptionalMember_ShouldReturnCorrectInstance_WhenMemberIsNotResolvable()
        {
            var ctor = typeof( OptionalMemberGenericImplementor<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<OptionalMemberGenericImplementor<string>>( e => e.Bar.TestNull() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtorAndOptionalMember_ShouldReturnCorrectInstance_WhenMemberIsResolvable()
        {
            var ctor = typeof( OptionalMemberGenericImplementor<> ).GetConstructors().First();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<OptionalMemberGenericImplementor<string>>( e => e.Bar.TestNotNull() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtor_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromImplementorType()
        {
            var ctor = typeof( ChainablePropertyGenericFoo<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor( ctor, o => o.ResolveMember( m => m.Name.Contains( "_bar" ), typeof( GenericImplementor<> ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType()
                .Exact<ChainablePropertyGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtor_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromKeyedImplementorType()
        {
            var ctor = typeof( ChainablePropertyGenericFoo<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveMember( m => m.Name.Contains( "_bar" ), typeof( GenericImplementor<> ), c => c.Keyed( 1 ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType()
                .Exact<ChainablePropertyGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithNestedMembers()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( GenericNestedMember<> ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<GenericNestedMember<string>>();

            Assertion.All(
                    result.Foo.TestType().AssignableTo<GenericImplementor<string>>(),
                    result.Bar.TestType().AssignableTo<ChainableGenericBar<string>>() )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldCallOnCreatedCallback()
        {
            var callback = Substitute.For<Action<object, Type, IDependencyScope>>();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ), o => o.SetOnCreatedCallback( callback ) );

            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();
            _ = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<int>>();
            _ = sut.RootScope.Locator.Resolve<IGenericFoo<int>>();

            Assertion.All(
                    callback.CallCount().TestEquals( 2 ),
                    callback.CallAt( 0 ).Exists.TestTrue(),
                    callback.CallAt( 1 ).Exists.TestTrue(),
                    callback.CallAt( 0 ).Arguments.TestSequence( [ result1, typeof( IGenericFoo<string> ), sut.RootScope ] ),
                    callback.CallAt( 1 ).Arguments.TestSequence( [ result2, typeof( IGenericFoo<int> ), sut.RootScope ] ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithSpecializedOptionalCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.Add<IGenericBar<string>>().FromType<GenericImplementor<string>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<int>>();

            Assertion.All(
                    result1.TestType().Exact<DefaultCtorParamGenericImplementor<string>>( e => e.Bar.TestNotNull() ),
                    result2.TestType().Exact<DefaultCtorParamGenericImplementor<int>>( e => e.Bar.TestNull() ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithSpecializedCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableGenericFoo<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IGenericBar<string>>().FromType<GenericImplementor<string>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<int>>();

            Assertion.All(
                    result1.TestType().Exact<ChainableGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() ),
                    result2.TestType().Exact<ChainableGenericFoo<int>>( e => e.Bar.TestType().Exact<ChainableGenericBar<int>>() ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithOptionalNonGenericCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParameterGenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<OptionalCtorParameterGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithNonGenericCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ExplicitCtorGenericImplementor<> ) );
            builder.Add<string>().FromFactory( _ => "foo" );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<ExplicitCtorGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithClosedGenericCtorParameterViaOpenGenericRegistration()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericCtorParameterGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<GenericCtorParameterGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithSpecializedOptionalMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalMemberGenericImplementor<> ) );
            builder.Add<IGenericBar<string>>().FromType<GenericImplementor<string>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<int>>();

            Assertion.All(
                    result1.TestType().Exact<OptionalMemberGenericImplementor<string>>( e => e.Bar.TestNotNull() ),
                    result2.TestType().Exact<OptionalMemberGenericImplementor<int>>( e => e.Bar.TestNull() ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithSpecializedMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ChainableFieldGenericFoo<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( ChainableFieldGenericBar<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IGenericBar<string>>().FromType<GenericImplementor<string>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<int>>();

            Assertion.All(
                    result1.TestType().Exact<ChainableFieldGenericFoo<string>>( e => e.Bar.TestType().Exact<GenericImplementor<string>>() ),
                    result2.TestType()
                        .Exact<ChainableFieldGenericFoo<int>>( e => e.Bar.TestType().Exact<ChainableFieldGenericBar<int>>() ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithOptionalNonGenericMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalMemberGenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<OptionalMemberGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithNonGenericMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( FieldGenericImplementor<> ) );
            builder.Add<string>().FromFactory( _ => "foo" );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<FieldGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithClosedGenericMemberViaOpenGenericRegistration()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericMemberGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<GenericMemberGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithComplexDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( ComplexGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( List<> ) );
            builder.AddGeneric( typeof( Dictionary<,> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<ComplexGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ForClosedTypeWithSharedOpenGenericImplementor()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.Add<IGenericFoo<int>>().FromSharedImplementor<GenericImplementor<int>>().SetLifetime( DependencyLifetime.Singleton );
            builder.Add<IGenericBar<int>>().FromSharedImplementor<GenericImplementor<int>>().SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<int>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericBar<int>>();
            var result3 = sut.RootScope.Locator.Resolve<IGenericQux<int>>();

            Assertion.All(
                    result1.TestRefEquals( result2 ),
                    result2.TestRefEquals( result3 ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithValidConstraints()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericConstrained<> ) ).FromType( typeof( GenericConstrainedImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericConstrained<ConstrainedArg>>();

            result.TestType().Exact<GenericConstrainedImplementor<ConstrainedArg>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldThrowMissingDependencyException_WhenDependencyTypeHasNotBeenRegistered()
        {
            var builder = new DependencyContainerBuilder();
            var sut = builder.Build();

            var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IGenericFoo<string>>() );

            action.Test( exc =>
                    exc.TestType().Exact<MissingDependencyException>( e => e.DependencyType.TestEquals( typeof( IGenericFoo<string> ) ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitInjectionIsInvalidAndOtherCtorIsChosen()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( MultiCtorGenericImplementor<> ) )
                .FromConstructor( o => o.ResolveParameter(
                    p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof( IGenericBar<> ),
                    typeof( ChainableGenericFoo<> ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<MultiCtorGenericImplementor<string>>();

            Assertion.All(
                    result.Bar.TestNull(),
                    result.Qux.TestType().AssignableTo<GenericImplementor<string>>() )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterAndMemberAreInjectable()
        {
            var ctor = typeof( CtorAndRefMemberGenericImplementor<> ).GetConstructors().First();
            var stringValue = Fixture.Create<string>();
            var intValue = Fixture.Create<int>();

            var builder = new DependencyContainerBuilder();
            builder.Add<string>().FromFactory( _ => stringValue );
            builder.Add<int>().FromFactory( _ => intValue );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType()
                .AssignableTo<CtorAndRefMemberGenericImplementor<string>>( e => e.Text.TestEquals( $"{stringValue}{intValue}" ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterAndMemberAreInjectableWithNullableType()
        {
            var ctor = typeof( CtorAndValueMemberGenericImplementor<> ).GetConstructors().First();
            var byteValue = Fixture.Create<byte>();
            var intValue = Fixture.Create<int>();

            var builder = new DependencyContainerBuilder();
            builder.Add<byte?>().FromFactory( _ => byteValue );
            builder.Add<int>().FromFactory( _ => intValue );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType()
                .AssignableTo<CtorAndValueMemberGenericImplementor<string>>( e => e.Text.TestEquals( $"{intValue}{byteValue}" ) )
                .Go();
        }

        [Fact]
        public void
            ResolvingOpenDependency_ShouldThrowCircularDependencyReferenceException_WhenCircularReferenceHasBeenDetectedDueToGenericSpecialization()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( ChainableGenericQux<> ) );
            builder.Add<IGenericBar<string>>()
                .FromFactory( s => new ChainableGenericBar<string>( s.Locator.Resolve<IGenericQux<string>>() ) );

            var sut = builder.Build();

            var action = Lambda.Of( () => sut.RootScope.Locator.Resolve<IGenericFoo<string>>() );

            action.Test( exc => exc.TestType()
                    .Exact<CircularDependencyReferenceException>( e => Assertion.All(
                        e.DependencyType.TestEquals( typeof( IGenericFoo<string> ) ),
                        e.ImplementorType.TestEquals( typeof( IGenericFoo<string> ) ),
                        e.InnerException.TestType()
                            .AssignableTo<CircularDependencyReferenceException>( inner1 => Assertion.All(
                                inner1.DependencyType.TestEquals( typeof( IGenericBar<string> ) ),
                                inner1.ImplementorType.TestEquals( typeof( IGenericBar<string> ) ),
                                inner1.InnerException.TestType()
                                    .AssignableTo<CircularDependencyReferenceException>( inner2 => Assertion.All(
                                        inner2.DependencyType.TestEquals( typeof( IGenericQux<string> ) ),
                                        inner2.ImplementorType.TestEquals( typeof( IGenericQux<string> ) ),
                                        inner2.InnerException.TestType()
                                            .AssignableTo<CircularDependencyReferenceException>( inner3 => Assertion.All(
                                                inner3.DependencyType.TestEquals( typeof( IGenericFoo<string> ) ),
                                                inner3.ImplementorType.TestEquals( typeof( IGenericFoo<string> ) ),
                                                inner3.InnerException.TestType()
                                                    .AssignableTo<CircularDependencyReferenceException>( inner4 => Assertion.All(
                                                        inner4.DependencyType.TestEquals( typeof( IGenericBar<string> ) ),
                                                        inner4.ImplementorType.TestEquals( typeof( IGenericBar<string> ) ),
                                                        inner4.InnerException.TestNull() ) ) ) ) ) ) ) ) ) ) )
                .Go();
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
