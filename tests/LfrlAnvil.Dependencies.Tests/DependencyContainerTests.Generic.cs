using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Extensions;
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

        [Fact]
        public void
            ResolvingTransientDependency_WithPartiallyClosedSharedImplementor_ShouldReturnNewInstanceEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Transient );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
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
        public void
            ResolvingSingletonDependency_WithPartiallyClosedSharedImplementor_ShouldReturnSameInstanceEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
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
            ResolvingScopedSingletonDependency_WithPartiallyClosedSharedImplementor_ShouldReturnNewInstancePerScopeAndItsChildrenForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.ScopedSingleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
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
        public void
            ResolvingScopedDependency_WithPartiallyClosedSharedImplementor_ShouldReturnSameInstancePerScopeEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Scoped );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
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
        public void ResolvingDependency_WithPartiallyClosedSharedImplementor_ShouldReturnNewInstanceEachTimeForDifferentGenericArgs(
            DependencyLifetime lifetime)
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( lifetime );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( lifetime );

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
        public void ResolvingDependency_WithSharedImplementor_ShouldGroupSharedImplementorsByDependencyLifetimes()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Scoped );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            builder.Add<Parameterized<IGenericBar<string>>>();
            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();
            var scope = sut.RootScope.BeginScope();

            var result1 = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = scope.Locator.Resolve<Parameterized<IGenericBar<string>>>();
            var result3 = scope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            Assertion.All(
                    result1.Inner.TestRefEquals( result2.Inner ),
                    result1.Inner.TestNotRefEquals( result3.Inner ) )
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
        public void ResolvingDependency_WithPartiallyClosedSharedImplementor_ShouldGroupSharedImplementorsByDependencyLifetimes()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( string ) ) )
                .SetLifetime( DependencyLifetime.Scoped );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            builder.Add<Parameterized<IGenericBar<string>>>();
            builder.Add<Parameterized<IGenericQux<int>>>();
            var sut = builder.Build();
            var scope = sut.RootScope.BeginScope();

            var result1 = scope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = scope.Locator.Resolve<Parameterized<IGenericBar<string>>>();
            var result3 = scope.Locator.Resolve<Parameterized<IGenericQux<int>>>();

            Assertion.All(
                    result1.Inner.TestRefEquals( result2.Inner ),
                    result1.Inner.TestNotRefEquals( result3.Inner ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyClosedSharedKeyedImplementor_ShouldReturnCorrectInstances()
        {
            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 )
                .AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) )
                .FromType( typeof( GenericFreeImplementor<,> ) );

            builder.GetKeyedLocator( 1 )
                .AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor(
                    typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ),
                    o => o.Keyed( 1 ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.GetKeyedLocator( "foo" )
                .AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor(
                    typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( string ) ),
                    o => o.Keyed( 1 ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.Add<Parameterized<IGenericBar<string>>>();
            builder.GetKeyedLocator( 1 ).Add<Parameterized<IGenericFoo<string>>>();
            builder.GetKeyedLocator( "foo" ).Add<Parameterized<IGenericQux<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericBar<string>>>();
            var result2 = sut.RootScope.GetKeyedLocator( 1 ).Resolve<Parameterized<IGenericFoo<string>>>();
            var result3 = sut.RootScope.GetKeyedLocator( "foo" ).Resolve<Parameterized<IGenericQux<int>>>();

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
        public void ResolvingDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromFactory()
        {
            var ctor = typeof( ChainableGenericQux<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveParameter(
                        p => p.Name == "foo",
                        (_, p) => Activator.CreateInstance(
                            typeof( GenericImplementor<> ).MakeGenericType( p.ParameterType.GetGenericArguments()[0] ) )! ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result.Inner.TestType().Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericImplementor<string>>() ).Go();
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
        public void ResolvingDependency_WithCtorAndMember_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromFactory()
        {
            var ctor = typeof( ChainableFieldGenericQux<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveMember(
                        m => m.Name.Contains( "_foo" ),
                        (_, m) => Activator.CreateInstance(
                            typeof( GenericImplementor<> ).MakeGenericType(
                                (( FieldInfo )m).FieldType.GetGenericArguments()[0].GetGenericArguments()[0] ) )! ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result.Inner.TestType()
                .Exact<ChainableFieldGenericQux<string>>( e => e.Foo.TestType().Exact<GenericImplementor<string>>() )
                .Go();
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
        public void
            ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitFactoryIsChosenOverCtorWithNormallyInjectedDependency()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( MultiCtorGenericImplementor<> ) )
                .FromConstructor( o => o.ResolveParameter(
                    p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof( IGenericBar<> ),
                    (_, p) => Activator.CreateInstance(
                        typeof( GenericImplementor<> ).MakeGenericType( p.ParameterType.GetGenericArguments()[0] ) )! ) );

            builder.Add<ParameterizedMember<MultiCtorGenericImplementor<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ParameterizedMember<MultiCtorGenericImplementor<string>>>();

            result.Inner.Instance.TestType()
                .Exact<MultiCtorGenericImplementor<string>>( e => Assertion.All(
                    e.Bar.TestType().Exact<GenericImplementor<string>>(),
                    e.Qux.TestNull() ) )
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
        public void ResolvingDependency_WithSpecializedRangeCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IEnumerable<IGenericFoo<string>>>().FromFactory( _ => Array.Empty<IGenericFoo<string>>() );
            builder.Add<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            builder.Add<Parameterized<IEnumerable<IGenericFoo<int>>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            var result2 = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<int>>>>();

            Assertion.All(
                    result1.Inner.Count().TestEquals( 0 ),
                    result2.Inner.Count().TestEquals( 2 ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithSpecializedRangeMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IEnumerable<IGenericFoo<string>>>().FromFactory( _ => Array.Empty<IGenericFoo<string>>() );
            builder.Add<ParameterizedMember<IEnumerable<IGenericFoo<string>>>>();
            builder.Add<ParameterizedMember<IEnumerable<IGenericFoo<int>>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<ParameterizedMember<IEnumerable<IGenericFoo<string>>>>();
            var result2 = sut.RootScope.Locator.Resolve<ParameterizedMember<IEnumerable<IGenericFoo<int>>>>();

            Assertion.All(
                    result1.Inner.Instance.Count().TestEquals( 0 ),
                    result2.Inner.Instance.Count().TestEquals( 2 ) )
                .Go();
        }

        [Fact]
        public void ResolvingRangeDependency_ShouldInvokeOnResolvingCallbackEveryTime()
        {
            var onResolvingCallback = Substitute.For<Action<Type, IDependencyScope>>();

            var builder = new DependencyContainerBuilder();
            builder.GetGenericDependencyRange( typeof( IGenericFoo<> ) )
                .SetOnResolvingCallback( onResolvingCallback )
                .Add()
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ) );

            builder.Add<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            builder.Add<Parameterized<IEnumerable<IGenericFoo<int>>>>();
            var sut = builder.Build();

            _ = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            _ = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<int>>>>();

            Assertion.All(
                    onResolvingCallback.CallCount().TestEquals( 2 ),
                    onResolvingCallback.CallAt( 0 ).Arguments.TestSequence( [ typeof( IEnumerable<IGenericFoo<string>> ), sut.RootScope ] ),
                    onResolvingCallback.CallAt( 1 ).Arguments.TestSequence( [ typeof( IEnumerable<IGenericFoo<int>> ), sut.RootScope ] ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnCorrectInstance_WhenCtorRequiresEmptyRegisteredRange()
        {
            var builder = new DependencyContainerBuilder();
            _ = builder.GetGenericDependencyRange( typeof( IGenericFoo<> ) );
            builder.Add<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<string>>>>();

            result.Inner.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnCorrectInstance_WhenMemberRequiresEmptyRegisteredRange()
        {
            var builder = new DependencyContainerBuilder();
            _ = builder.GetGenericDependencyRange( typeof( IGenericFoo<> ) );
            builder.Add<ParameterizedMember<IEnumerable<IGenericFoo<string>>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ParameterizedMember<IEnumerable<IGenericFoo<string>>>>();

            result.Inner.Instance.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenRangeOnlyContainsElementsExcludedFromRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<string>>>>();

            result.Inner.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnLastRegisteredInstance_WhenMoreThanOneElementIsRegisteredInRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParamGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<DefaultCtorParamGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnCorrectInstance_WhenDependencyIsDecoratedRangeExcludingSelf()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParamGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericFooRangeDecorator<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<GenericFooRangeDecorator<string>>().Go();
        }

        [Fact]
        public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenFirstElementIsExcluded()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParamGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.Add<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<string>>>>();

            result.Inner.Select( i => i.GetType() )
                .TestSequence(
                    [ typeof( OptionalCtorParamGenericImplementor<string> ), typeof( DefaultCtorParamGenericImplementor<string> ) ] )
                .Go();
        }

        [Fact]
        public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenSecondElementIsExcluded()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .IncludeInRange( false )
                .FromType( typeof( OptionalCtorParamGenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.Add<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<string>>>>();

            result.Inner.Select( i => i.GetType() )
                .TestSequence( [ typeof( GenericImplementor<string> ), typeof( DefaultCtorParamGenericImplementor<string> ) ] )
                .Go();
        }

        [Fact]
        public void ResolvingRangeDependency_ShouldReturnCorrectInstance_WhenSomeElementsAreRegisteredAsKeyed()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParamGenericImplementor<> ) );
            builder.Add<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalMemberGenericImplementor<> ) );
            builder.GetKeyedLocator( 1 ).Add<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericFoo<string>>>>();
            var keyedResult = sut.RootScope.GetKeyedLocator( 1 ).Resolve<Parameterized<IEnumerable<IGenericFoo<string>>>>();

            Assertion.All(
                    result.Inner.Select( i => i.GetType() )
                        .TestSequence( [ typeof( GenericImplementor<string> ), typeof( OptionalCtorParamGenericImplementor<string> ) ] ),
                    keyedResult.Inner.Select( i => i.GetType() )
                        .TestSequence(
                            [ typeof( DefaultCtorParamGenericImplementor<string> ), typeof( OptionalMemberGenericImplementor<string> ) ] ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithSharedImplementor_ShouldWorkCorrectlyForRangeElements()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.Add<ParameterizedMember<IEnumerable<IGenericFoo<string>>>>();
            builder.Add<ParameterizedMember<IGenericBar<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<ParameterizedMember<IEnumerable<IGenericFoo<string>>>>();
            var result2 = sut.RootScope.Locator.Resolve<ParameterizedMember<IGenericBar<string>>>();

            result1.Inner.Instance.TestCount( count => count.TestEquals( 2 ) )
                .Then( foo => Assertion.All(
                    foo[0].TestRefEquals( result2.Inner.Instance ),
                    foo[1].TestNotRefEquals( result2.Inner.Instance ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyClosedSharedImplementor_ShouldWorkCorrectlyForRangeElements()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<ParameterizedMember<IEnumerable<IGenericFoo<string>>>>();
            builder.Add<ParameterizedMember<IGenericBar<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<ParameterizedMember<IEnumerable<IGenericFoo<string>>>>();
            var result2 = sut.RootScope.Locator.Resolve<ParameterizedMember<IGenericBar<string>>>();

            result1.Inner.Instance.TestCount( count => count.TestEquals( 2 ) )
                .Then( foo => Assertion.All(
                    foo[0].TestRefEquals( result2.Inner.Instance ),
                    foo[1].TestNotRefEquals( result2.Inner.Instance ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnCorrectInstance_WhenClosedGenericRangeIsResolvedViaCtor()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( ChainableGenericFooRange<> ) );
            builder.Add<Parameterized<ChainableGenericFooRange<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<ChainableGenericFooRange<string>>>();

            result.Inner.Bars.Count().TestEquals( 2 ).Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnCorrectInstance_WhenClosedGenericRangeIsResolvedViaMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( ChainableGenericFooMemberRange<> ) );
            builder.Add<Parameterized<ChainableGenericFooMemberRange<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<ChainableGenericFooMemberRange<string>>>();

            result.Inner.Bars.Count().TestEquals( 2 ).Go();
        }

        [Fact]
        public void ResolvingDependency_WithCustomKeyProviders()
        {
            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( ChainableGenericQux<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( ChainableFieldGenericQux<> ) );

            builder.Configuration
                .SetConstructorParameterKeyProvider( p =>
                    p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof( IGenericFoo<> ) ? 1 : null )
                .SetMemberKeyProvider( m =>
                    m is FieldInfo field
                    && field.FieldType.IsGenericType
                    && field.FieldType.GetGenericTypeDefinition() == typeof( Injected<> )
                    && field.FieldType.GetGenericArguments()[0].IsGenericType
                    && field.FieldType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof( IGenericFoo<> )
                        ? 1
                        : null );

            builder.Add<Parameterized<IEnumerable<IGenericQux<string>>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IEnumerable<IGenericQux<string>>>>();

            result.Inner.TestCount( count => count.TestEquals( 2 ) )
                .Then( qux => Assertion.All(
                    qux[0].TestType().Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericImplementor<string>>() ),
                    qux[1]
                        .TestType()
                        .Exact<ChainableFieldGenericQux<string>>( e => e.Foo.TestType().Exact<GenericImplementor<string>>() ) ) )
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
        public void ResolvingOpenRangeDependencyDirectly_ShouldThrowOpenGenericDependencyException()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericImplementor<> ) );
            var sut = builder.Build();

            var action = Lambda.Of( () =>
                sut.RootScope.Locator.Resolve( typeof( IEnumerable<> ).MakeGenericType( typeof( GenericImplementor<> ) ) ) );

            action.Test( exc => exc.TestType().Exact<OpenGenericDependencyException>() ).Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnOpenGenericInstance_WhenItIsLastInClosedRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.Add<IGenericFoo<string>>().FromType<GenericImplementor<string>>();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<DefaultCtorParamGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnClosedGenericInstance_WhenItIsLastInClosedRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.Add<IGenericFoo<string>>().FromType<GenericImplementor<string>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<GenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingRangeDependency_ShouldReturnOpenAndClosedGenericInstancesInCorrectOrder()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.Add<IGenericFoo<string>>().FromType<GenericImplementor<string>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();

            result.TestCount( count => count.TestEquals( 2 ) )
                .Then( foo => Assertion.All(
                    foo[0].TestType().Exact<DefaultCtorParamGenericImplementor<string>>(),
                    foo[1].TestType().Exact<GenericImplementor<string>>() ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnSharedOpenGenericInstance_WhenItIsLastInClosedRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.Add<IGenericFoo<string>>().FromType<DefaultCtorParamGenericImplementor<string>>();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericBar<string>>();
            var result3 = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            Assertion.All(
                    result1.TestType().Exact<GenericImplementor<string>>(),
                    result1.TestRefEquals( result2 ),
                    result2.TestRefEquals( result3.Inner ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_ShouldReturnPartiallyClosedSharedOpenGenericInstance_WhenItIsLastInClosedRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( string ) ) );

            builder.Add<IGenericFoo<string>>().FromType<DefaultCtorParamGenericImplementor<string>>();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<Parameterized<IGenericQux<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericBar<string>>();
            var result3 = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<int>>>();

            Assertion.All(
                    result1.TestType().Exact<GenericFreeImplementor<string, int>>(),
                    result1.TestRefEquals( result2 ),
                    result2.TestRefEquals( result3.Inner ) )
                .Go();
        }

        [Fact]
        public void ResolvingRangeDependency_ShouldIncludeSharedOpenGenericInstance_WhenItIsNotLastInClosedRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.Add<IGenericFoo<string>>().FromType<DefaultCtorParamGenericImplementor<string>>();
            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericBar<string>>();
            var result3 = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result1.TestCount( count => count.TestEquals( 2 ) )
                .Then( foo => Assertion.All(
                    foo[0].TestType().Exact<GenericImplementor<string>>(),
                    foo[1].TestType().Exact<DefaultCtorParamGenericImplementor<string>>(),
                    foo[0].TestRefEquals( result2 ),
                    result2.TestRefEquals( result3.Inner ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingRangeDependency_ShouldIncludePartiallyClosedSharedOpenGenericInstance_WhenItIsNotLastInClosedRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( string ) ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<IGenericFoo<string>>().FromType<DefaultCtorParamGenericImplementor<string>>();
            builder.Add<Parameterized<IGenericQux<int>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericBar<string>>();
            var result3 = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<int>>>();

            result1.TestCount( count => count.TestEquals( 2 ) )
                .Then( foo => Assertion.All(
                    foo[0].TestType().Exact<GenericFreeImplementor<string, int>>(),
                    foo[1].TestType().Exact<DefaultCtorParamGenericImplementor<string>>(),
                    foo[0].TestRefEquals( result2 ),
                    result2.TestRefEquals( result3.Inner ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyClosedImplementorCtor()
        {
            var ctor = typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( int ) ).GetConstructors().First();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<GenericFreeFoo<string, int>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyClosedImplementorType()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromType( typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();

            result.Inner.TestType().Exact<GenericFreeFoo<string, int>>().Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyOpenGenericResolverBasedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( GenericFreeFooWithClosedSources<,> ) );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( long ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( long ) ) );

            builder.Add<Parameterized<GenericFreeFooWithClosedSources<string, int>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<GenericFreeFooWithClosedSources<string, int>>>();

            Assertion.All(
                    result.Inner.Bar.TestType().Exact<GenericFreeImplementor<int, long>>(),
                    result.Inner.Qux.Instance.TestType().Exact<GenericFreeImplementor<long, int>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyOpenCustomCtorParameterResolution()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableGenericQux<> ),
                    o => o.ResolveParameter(
                        _ => true,
                        typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result.Inner.TestType()
                .Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericFreeImplementor<string, int>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyOpenCustomMemberResolution()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableFieldGenericQux<> ),
                    o => o.ResolveMember(
                        _ => true,
                        typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result.Inner.TestType()
                .Exact<ChainableFieldGenericQux<string>>( e => e.Foo.TestType().Exact<GenericFreeImplementor<string, int>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyOpenCustomCtorParameterResolution_FromOpenShared()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooDirect<,> ) );
            builder.AddGeneric( typeof( IGenericFreeFoo<,> ) )
                .FromSharedImplementor( typeof( GenericFreeFooDirect<,> ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableGenericQux<> ),
                    o => o.ResolveParameter(
                        _ => true,
                        typeof( IGenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( long ) ) ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result.Inner.TestType()
                .Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericFreeFooDirect<string, long>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyOpenCustomMemberResolution_FromOpenShared()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooDirect<,> ) );
            builder.AddGeneric( typeof( IGenericFreeFoo<,> ) )
                .FromSharedImplementor( typeof( GenericFreeFooDirect<,> ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableFieldGenericQux<> ),
                    o => o.ResolveMember(
                        _ => true,
                        typeof( IGenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( long ) ) ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result.Inner.TestType()
                .Exact<ChainableFieldGenericQux<string>>( e => e.Foo.TestType().Exact<GenericFreeFooDirect<string, long>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyOpenCustomCtorParameterResolution_FromPartiallyOpenShared()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericDistinctFreeImplementor<,,> ) );
            builder.AddGeneric( typeof( IGenericFreeFoo<,> ) )
                .FromSharedImplementor(
                    typeof( GenericDistinctFreeImplementor<,,> ).SubstituteGenericArguments( null, null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableGenericQux<> ),
                    o => o.ResolveParameter(
                        _ => true,
                        typeof( IGenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( double ) ) ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result.Inner.TestType()
                .Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericDistinctFreeImplementor<string, double, int>>() )
                .Go();
        }

        [Fact]
        public void ResolvingDependency_WithPartiallyOpenCustomMemberResolution_FromPartiallyOpenShared()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericDistinctFreeImplementor<,,> ) );
            builder.AddGeneric( typeof( IGenericFreeFoo<,> ) )
                .FromSharedImplementor(
                    typeof( GenericDistinctFreeImplementor<,,> ).SubstituteGenericArguments( null, null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableFieldGenericQux<> ),
                    o => o.ResolveMember(
                        _ => true,
                        typeof( IGenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( double ) ) ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();

            result.Inner.TestType()
                .Exact<ChainableFieldGenericQux<string>>( e =>
                    e.Foo.TestType().Exact<GenericDistinctFreeImplementor<string, double, int>>() )
                .Go();
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

        [Fact]
        public void
            ResolvingTransientOpenDependency_WithPartiallyClosedSharedImplementor_ShouldReturnNewInstanceEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Transient );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Transient );

            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.Locator.Resolve<IGenericBar<string>>();

            new object[] { result1, result2 }.Distinct().Count().TestEquals( 2 ).Go();
        }

        [Fact]
        public void
            ResolvingSingletonOpenDependency_WithPartiallyClosedSharedImplementor_ShouldReturnSameInstanceEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            var sut = builder.Build();
            var scope = sut.RootScope;

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.Locator.Resolve<IGenericBar<string>>();

            result1.TestRefEquals( result2 ).Go();
        }

        [Fact]
        public void
            ResolvingScopedSingletonOpenDependency_WithPartiallyClosedSharedImplementor_ShouldReturnNewInstancePerScopeAndItsChildrenForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.ScopedSingleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
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
        public void
            ResolvingScopedOpenDependency_WithPartiallyClosedSharedImplementor_ShouldReturnSameInstancePerScopeEachTimeForAllSharingDependencies()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Scoped );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
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
        public void ResolvingOpenDependency_WithPartiallyClosedSharedImplementor_ShouldReturnNewInstanceEachTimeForDifferentGenericArgs(
            DependencyLifetime lifetime)
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( lifetime );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( lifetime );

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
        public void ResolvingOpenDependency_WithSharedImplementor_ShouldGroupSharedImplementorsByDependencyLifetimes()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericImplementor<> ) )
                .SetLifetime( DependencyLifetime.Scoped );

            var sut = builder.Build();
            var scope = sut.RootScope.BeginScope();

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.Locator.Resolve<IGenericBar<string>>();
            var result3 = scope.Locator.Resolve<IGenericQux<string>>();

            Assertion.All(
                    result1.TestRefEquals( result2 ),
                    result1.TestNotRefEquals( result3 ) )
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
        public void ResolvingOpenDependency_WithPartiallyClosedSharedImplementor_ShouldGroupSharedImplementorsByDependencyLifetimes()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( string ) ) )
                .SetLifetime( DependencyLifetime.Scoped );

            var sut = builder.Build();
            var scope = sut.RootScope.BeginScope();

            var result1 = scope.Locator.Resolve<IGenericFoo<string>>();
            var result2 = scope.Locator.Resolve<IGenericBar<string>>();
            var result3 = scope.Locator.Resolve<IGenericQux<int>>();

            Assertion.All(
                    result1.TestRefEquals( result2 ),
                    result1.TestNotRefEquals( result3 ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyClosedSharedKeyedImplementor_ShouldReturnCorrectInstances()
        {
            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 )
                .AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) )
                .FromType( typeof( GenericFreeImplementor<,> ) );

            builder.GetKeyedLocator( 1 )
                .AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor(
                    typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ),
                    o => o.Keyed( 1 ) )
                .SetLifetime( DependencyLifetime.Singleton );

            builder.GetKeyedLocator( "foo" )
                .AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor(
                    typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( string ) ),
                    o => o.Keyed( 1 ) )
                .SetLifetime( DependencyLifetime.Singleton );

            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IGenericBar<string>>();
            var result2 = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IGenericFoo<string>>();
            var result3 = sut.RootScope.GetKeyedLocator( "foo" ).Resolve<IGenericQux<int>>();

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
        public void ResolvingOpenDependency_WithCtor_ShouldReturnCorrectInstance_WhenParameterHasExplicitResolutionFromFactory()
        {
            var ctor = typeof( ChainableGenericQux<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveParameter(
                        p => p.Name == "foo",
                        (_, p) => Activator.CreateInstance(
                            typeof( GenericImplementor<> ).MakeGenericType( p.ParameterType.GetGenericArguments()[0] ) )! ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericQux<string>>();

            result.TestType().Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericImplementor<string>>() ).Go();
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
        public void ResolvingOpenDependency_WithCtorAndMember_ShouldReturnCorrectInstance_WhenMemberHasExplicitResolutionFromFactory()
        {
            var ctor = typeof( ChainableFieldGenericQux<> ).GetConstructors().First();

            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromConstructor(
                    ctor,
                    o => o.ResolveMember(
                        m => m.Name.Contains( "_foo" ),
                        (_, m) => Activator.CreateInstance(
                            typeof( GenericImplementor<> ).MakeGenericType(
                                (( FieldInfo )m).FieldType.GetGenericArguments()[0].GetGenericArguments()[0] ) )! ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericQux<string>>();

            result.TestType().Exact<ChainableFieldGenericQux<string>>( e => e.Foo.TestType().Exact<GenericImplementor<string>>() ).Go();
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
        public void
            ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenCtorWithExplicitFactoryIsChosenOverCtorWithNormallyInjectedDependency()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( MultiCtorGenericImplementor<> ) )
                .FromConstructor( o => o.ResolveParameter(
                    p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof( IGenericBar<> ),
                    (_, p) => Activator.CreateInstance(
                        typeof( GenericImplementor<> ).MakeGenericType( p.ParameterType.GetGenericArguments()[0] ) )! ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<MultiCtorGenericImplementor<string>>();

            result.TestType()
                .Exact<MultiCtorGenericImplementor<string>>( e => Assertion.All(
                    e.Bar.TestType().Exact<GenericImplementor<string>>(),
                    e.Qux.TestNull() ) )
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
        public void ResolvingOpenDependency_WithSpecializedRangeCtorParameter()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IEnumerable<IGenericBar<string>>>().FromFactory( _ => Array.Empty<IGenericBar<string>>() );
            builder.AddGeneric( typeof( ChainableGenericFooRange<> ) );
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<ChainableGenericFooRange<string>>();
            var result2 = sut.RootScope.Locator.Resolve<ChainableGenericFooRange<int>>();

            Assertion.All(
                    result1.Bars.Count().TestEquals( 0 ),
                    result2.Bars.Count().TestEquals( 2 ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithSpecializedRangeMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IEnumerable<IGenericBar<string>>>().FromFactory( _ => Array.Empty<IGenericBar<string>>() );
            builder.AddGeneric( typeof( ChainableGenericFooMemberRange<> ) );
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<ChainableGenericFooMemberRange<string>>();
            var result2 = sut.RootScope.Locator.Resolve<ChainableGenericFooMemberRange<int>>();

            Assertion.All(
                    result1.Bars.Count().TestEquals( 0 ),
                    result2.Bars.Count().TestEquals( 2 ) )
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

        [Fact]
        public void ResolvingOpenRangeDependency_ShouldInvokeOnResolvingCallbackEveryTime()
        {
            var onResolvingCallback = Substitute.For<Action<Type, IDependencyScope>>();

            var builder = new DependencyContainerBuilder();
            builder.GetGenericDependencyRange( typeof( IGenericFoo<> ) )
                .SetOnResolvingCallback( onResolvingCallback )
                .Add()
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ) );

            var sut = builder.Build();

            _ = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();
            _ = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<int>>>();

            Assertion.All(
                    onResolvingCallback.CallCount().TestEquals( 2 ),
                    onResolvingCallback.CallAt( 0 ).Arguments.TestSequence( [ typeof( IEnumerable<IGenericFoo<string>> ), sut.RootScope ] ),
                    onResolvingCallback.CallAt( 1 ).Arguments.TestSequence( [ typeof( IEnumerable<IGenericFoo<int>> ), sut.RootScope ] ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenUnregisteredRangeDependency_ShouldReturnEmptyCollection_WhenDoingItForTheFirstTime()
        {
            var sut = new DependencyContainerBuilder().Build();
            var result = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();
            result.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenCtorRequiresUnregisteredRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( ChainableGenericFooRange<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ChainableGenericFooRange<string>>();

            result.Bars.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenCtorRequiresEmptyRegisteredRange()
        {
            var builder = new DependencyContainerBuilder();
            _ = builder.GetGenericDependencyRange( typeof( IGenericBar<> ) );
            builder.AddGeneric( typeof( ChainableGenericFooRange<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ChainableGenericFooRange<string>>();

            result.Bars.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenMemberRequiresEmptyRegisteredRange()
        {
            var builder = new DependencyContainerBuilder();
            _ = builder.GetGenericDependencyRange( typeof( IGenericBar<> ) );
            builder.AddGeneric( typeof( ChainableGenericFooMemberRange<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ChainableGenericFooMemberRange<string>>();

            result.Bars.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenMemberRequiresUnregisteredRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( ChainableGenericFooMemberRange<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ChainableGenericFooMemberRange<string>>();

            result.Bars.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingOpenRangeDependency_ShouldReturnCorrectInstance_WhenRangeDoesNotContainsAnyElements()
        {
            var builder = new DependencyContainerBuilder();
            _ = builder.GetGenericDependencyRange( typeof( IGenericFoo<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();

            result.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingOpenRangeDependency_ShouldReturnCorrectInstance_WhenRangeOnlyContainsElementsExcludedFromRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();

            result.TestEmpty().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnLastRegisteredInstance_WhenMoreThanOneElementIsRegisteredInRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParamGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<DefaultCtorParamGenericImplementor<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenDependencyIsDecoratedRangeExcludingSelf()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParamGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericFooRangeDecorator<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<GenericFooRangeDecorator<string>>().Go();
        }

        [Fact]
        public void ResolvingOpenRangeDependency_ShouldReturnCorrectInstance_WhenFirstElementIsExcluded()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).IncludeInRange( false ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParamGenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();

            result.Select( i => i.GetType() )
                .TestSequence(
                    [ typeof( OptionalCtorParamGenericImplementor<string> ), typeof( DefaultCtorParamGenericImplementor<string> ) ] )
                .Go();
        }

        [Fact]
        public void ResolvingOpenRangeDependency_ShouldReturnCorrectInstance_WhenSecondElementIsExcluded()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .IncludeInRange( false )
                .FromType( typeof( OptionalCtorParamGenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();

            result.Select( i => i.GetType() )
                .TestSequence( [ typeof( GenericImplementor<string> ), typeof( DefaultCtorParamGenericImplementor<string> ) ] )
                .Go();
        }

        [Fact]
        public void ResolvingOpenRangeDependency_ShouldReturnCorrectInstance_WhenSomeElementsAreRegisteredAsKeyed()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalCtorParamGenericImplementor<> ) );
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( DefaultCtorParamGenericImplementor<> ) );
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( OptionalMemberGenericImplementor<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();
            var keyedResult = sut.RootScope.GetKeyedLocator( 1 ).Resolve<IEnumerable<IGenericFoo<string>>>();

            Assertion.All(
                    result.Select( i => i.GetType() )
                        .TestSequence( [ typeof( GenericImplementor<string> ), typeof( OptionalCtorParamGenericImplementor<string> ) ] ),
                    keyedResult.Select( i => i.GetType() )
                        .TestSequence(
                            [ typeof( DefaultCtorParamGenericImplementor<string> ), typeof( OptionalMemberGenericImplementor<string> ) ] ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithSharedImplementor_ShouldWorkCorrectlyForRangeElements()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericImplementor<> ) );

            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericBar<string>>();

            result1.TestCount( count => count.TestEquals( 2 ) )
                .Then( foo => Assertion.All(
                    foo[0].TestRefEquals( result2 ),
                    foo[1].TestNotRefEquals( result2 ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyClosedSharedImplementor_ShouldWorkCorrectlyForRangeElements()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ) );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<IEnumerable<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericBar<string>>();

            result1.TestCount( count => count.TestEquals( 2 ) )
                .Then( foo => Assertion.All(
                    foo[0].TestRefEquals( result2 ),
                    foo[1].TestNotRefEquals( result2 ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenClosedGenericRangeIsResolvedViaCtor()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( ChainableGenericFooRange<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ChainableGenericFooRange<string>>();

            result.Bars.Count().TestEquals( 2 ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_ShouldReturnCorrectInstance_WhenClosedGenericRangeIsResolvedViaMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( ChainableGenericFooMemberRange<> ) );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<ChainableGenericFooMemberRange<string>>();

            result.Bars.Count().TestEquals( 2 ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithCustomKeyProviders()
        {
            var builder = new DependencyContainerBuilder();
            builder.GetKeyedLocator( 1 ).AddGeneric( typeof( IGenericFoo<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( ChainableGenericQux<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( ChainableFieldGenericQux<> ) );

            builder.Configuration
                .SetConstructorParameterKeyProvider( p =>
                    p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof( IGenericFoo<> ) ? 1 : null )
                .SetMemberKeyProvider( m =>
                    m is FieldInfo field
                    && field.FieldType.IsGenericType
                    && field.FieldType.GetGenericTypeDefinition() == typeof( Injected<> )
                    && field.FieldType.GetGenericArguments()[0].IsGenericType
                    && field.FieldType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof( IGenericFoo<> )
                        ? 1
                        : null );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IEnumerable<IGenericQux<string>>>();

            result.TestCount( count => count.TestEquals( 2 ) )
                .Then( qux => Assertion.All(
                    qux[0].TestType().Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericImplementor<string>>() ),
                    qux[1]
                        .TestType()
                        .Exact<ChainableFieldGenericQux<string>>( e => e.Foo.TestType().Exact<GenericImplementor<string>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyClosedImplementorCtor()
        {
            var ctor = typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( int ) ).GetConstructors().First();
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).FromConstructor( ctor );
            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<GenericFreeFoo<string, int>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyClosedImplementorType()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromType( typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericFoo<string>>();

            result.TestType().Exact<GenericFreeFoo<string, int>>().Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyOpenGenericResolverBasedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( GenericFreeFooWithClosedSources<,> ) );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( long ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( long ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<GenericFreeFooWithClosedSources<double, int>>();

            Assertion.All(
                    result.Bar.TestType().Exact<GenericFreeImplementor<int, long>>(),
                    result.Qux.Instance.TestType().Exact<GenericFreeImplementor<long, int>>() )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyOpenCustomCtorParameterResolution()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableGenericQux<> ),
                    o => o.ResolveParameter(
                        _ => true,
                        typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericQux<string>>();

            result.TestType().Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericFreeImplementor<string, int>>() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyOpenCustomMemberResolution()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableFieldGenericQux<> ),
                    o => o.ResolveMember(
                        _ => true,
                        typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( int ) ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericQux<string>>();

            result.TestType()
                .Exact<ChainableFieldGenericQux<string>>( e => e.Foo.TestType().Exact<GenericFreeImplementor<string, int>>() )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyOpenCustomCtorParameterResolution_FromOpenShared()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooDirect<,> ) );
            builder.AddGeneric( typeof( IGenericFreeFoo<,> ) )
                .FromSharedImplementor( typeof( GenericFreeFooDirect<,> ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableGenericQux<> ),
                    o => o.ResolveParameter(
                        _ => true,
                        typeof( IGenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( long ) ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericQux<string>>();

            result.TestType().Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericFreeFooDirect<string, long>>() ).Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyOpenCustomMemberResolution_FromOpenShared()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooDirect<,> ) );
            builder.AddGeneric( typeof( IGenericFreeFoo<,> ) )
                .FromSharedImplementor( typeof( GenericFreeFooDirect<,> ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableFieldGenericQux<> ),
                    o => o.ResolveMember(
                        _ => true,
                        typeof( IGenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( long ) ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericQux<string>>();

            result.TestType()
                .Exact<ChainableFieldGenericQux<string>>( e => e.Foo.TestType().Exact<GenericFreeFooDirect<string, long>>() )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyOpenCustomCtorParameterResolution_FromPartiallyOpenShared()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericDistinctFreeImplementor<,,> ) );
            builder.AddGeneric( typeof( IGenericFreeFoo<,> ) )
                .FromSharedImplementor(
                    typeof( GenericDistinctFreeImplementor<,,> ).SubstituteGenericArguments( null, null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableGenericQux<> ),
                    o => o.ResolveParameter(
                        _ => true,
                        typeof( IGenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( double ) ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericQux<string>>();

            result.TestType()
                .Exact<ChainableGenericQux<string>>( e => e.Foo.TestType().Exact<GenericDistinctFreeImplementor<string, double, int>>() )
                .Go();
        }

        [Fact]
        public void ResolvingOpenDependency_WithPartiallyOpenCustomMemberResolution_FromPartiallyOpenShared()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericDistinctFreeImplementor<,,> ) );
            builder.AddGeneric( typeof( IGenericFreeFoo<,> ) )
                .FromSharedImplementor(
                    typeof( GenericDistinctFreeImplementor<,,> ).SubstituteGenericArguments( null, null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromType(
                    typeof( ChainableFieldGenericQux<> ),
                    o => o.ResolveMember(
                        _ => true,
                        typeof( IGenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( double ) ) ) );

            var sut = builder.Build();

            var result = sut.RootScope.Locator.Resolve<IGenericQux<string>>();

            result.TestType()
                .Exact<ChainableFieldGenericQux<string>>( e =>
                    e.Foo.TestType().Exact<GenericDistinctFreeImplementor<string, double, int>>() )
                .Go();
        }

        [Theory]
        [InlineData( DependencyLifetime.Transient )]
        [InlineData( DependencyLifetime.Scoped )]
        [InlineData( DependencyLifetime.ScopedSingleton )]
        [InlineData( DependencyLifetime.Singleton )]
        public void DependencyLocator_TryGetLifetime_ShouldReturnCorrectResult(DependencyLifetime expected)
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( expected ).FromType( typeof( GenericImplementor<> ) );
            var container = builder.Build();

            var result = container.RootScope.Locator.TryGetLifetime( typeof( IGenericFoo<> ) );

            result.TestEquals( expected ).Go();
        }

        [Theory]
        [InlineData( DependencyLifetime.Transient )]
        [InlineData( DependencyLifetime.Scoped )]
        [InlineData( DependencyLifetime.ScopedSingleton )]
        [InlineData( DependencyLifetime.Singleton )]
        public void DependencyLocator_TryGetLifetime_ForClosedType_ShouldReturnCorrectResult(DependencyLifetime expected)
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) ).SetLifetime( expected ).FromType( typeof( GenericImplementor<> ) );
            var container = builder.Build();

            var result = container.RootScope.Locator.TryGetLifetime( typeof( IGenericFoo<string> ) );

            result.TestEquals( expected ).Go();
        }

        [Fact]
        public void DependencyLocator_TryGetLifetime_ShouldReturnCorrectResult_ForRange()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .SetLifetime( DependencyLifetime.Singleton )
                .FromType( typeof( GenericImplementor<> ) );

            var container = builder.Build();

            var result = container.RootScope.Locator.TryGetLifetime( typeof( IEnumerable<> ).MakeGenericType( typeof( IGenericFoo<> ) ) );

            result.TestEquals( DependencyLifetime.Transient ).Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithOpenGenericParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithOpenSources<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithOpenSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithOpenSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Bar.TestType().Exact<GenericImplementor<string>>(),
                            e.Qux.Instance.TestType().Exact<GenericImplementor<string>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithOpenSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Bar.TestType().Exact<GenericImplementor<double>>(),
                            e.Qux.Instance.TestType().Exact<GenericImplementor<double>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithPartiallyOpenGenericParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithPartiallyOpenSources<,> ) );
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithOpenSources<,> ) );
            builder.AddGeneric( typeof( GenericFreeFooWithOpenSources<,> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithOpenSources<,> ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor(
                    typeof( GenericFreeFooWithPartiallyOpenSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithPartiallyOpenSources<string, int>>( e =>
                            Assertion.All(
                                "result1",
                                e.CtorFoo.TestType().Exact<GenericFreeFooWithOpenSources<string, int>>(),
                                e.MemberFoo.Instance.TestType().Exact<GenericFreeFooWithOpenSources<int, string>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithPartiallyOpenSources<double, int>>( e =>
                            Assertion.All(
                                "result2",
                                e.CtorFoo.TestType().Exact<GenericFreeFooWithOpenSources<double, int>>(),
                                e.MemberFoo.Instance.TestType().Exact<GenericFreeFooWithOpenSources<int, double>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithOptionalClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithClosedSources<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithClosedSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithClosedSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Bar.TestNull(),
                            e.Qux.Instance.TestNull() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithClosedSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Bar.TestNull(),
                            e.Qux.Instance.TestNull() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithOptionalSpecializedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithClosedSources<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithClosedSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<IGenericBar<int>>().FromType<GenericImplementor<int>>();
            builder.Add<IGenericQux<int>>().FromType<GenericImplementor<int>>();
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithClosedSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Bar.TestType().Exact<GenericImplementor<int>>(),
                            e.Qux.Instance.TestType().Exact<GenericImplementor<int>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithClosedSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Bar.TestType().Exact<GenericImplementor<int>>(),
                            e.Qux.Instance.TestType().Exact<GenericImplementor<int>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithFactoryBasedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithClosedSources<,> ) )
                .FromConstructor( o => o.ResolveParameter(
                        _ => true,
                        (_, p) => Activator.CreateInstance(
                            typeof( GenericImplementor<> ).MakeGenericType( p.ParameterType.GetGenericArguments()[0] ) )! )
                    .ResolveMember(
                        _ => true,
                        (_, m) => Activator.CreateInstance(
                            typeof( GenericImplementor<> ).MakeGenericType(
                                (( PropertyInfo )m).PropertyType.GetGenericArguments()[0].GetGenericArguments()[0] ) )! ) );

            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithClosedSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithClosedSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Bar.TestType().Exact<GenericImplementor<int>>(),
                            e.Qux.Instance.TestType().Exact<GenericImplementor<int>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithClosedSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Bar.TestType().Exact<GenericImplementor<int>>(),
                            e.Qux.Instance.TestType().Exact<GenericImplementor<int>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithNonGenericResolverBasedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithNonGenericSources<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor(
                    typeof( GenericFreeFooWithNonGenericSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.Add<IFoo>().FromType<Implementor>();
            builder.Add<IBar>().FromType<Implementor>();
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithNonGenericSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Foo.TestType().Exact<Implementor>(),
                            e.Bar.Instance.TestType().Exact<Implementor>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithNonGenericSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Foo.TestType().Exact<Implementor>(),
                            e.Bar.Instance.TestType().Exact<Implementor>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithGenericRangeResolverBasedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithRangeSources<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithRangeSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithRangeSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Bar.TestAll( (x, _) => x.TestType().Exact<GenericImplementor<int>>() ),
                            e.Qux.Instance.TestAll( (x, _) => x.TestType().Exact<GenericImplementor<int>>() ) ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithRangeSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Bar.TestAll( (x, _) => x.TestType().Exact<GenericImplementor<int>>() ),
                            e.Qux.Instance.TestAll( (x, _) => x.TestType().Exact<GenericImplementor<int>>() ) ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithGenericResolverBasedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithClosedSources<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithClosedSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithClosedSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Bar.TestType().Exact<GenericImplementor<int>>(),
                            e.Qux.Instance.TestType().Exact<GenericImplementor<int>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithClosedSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Bar.TestType().Exact<GenericImplementor<int>>(),
                            e.Qux.Instance.TestType().Exact<GenericImplementor<int>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithSpecializedGenericResolverBasedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithClosedSources<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithClosedSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericBar<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.AddGeneric( typeof( IGenericQux<> ) ).FromType( typeof( GenericImplementor<> ) );
            builder.Add<IGenericBar<int>>().FromType<GenericFreeImplementor<int, string>>();
            builder.Add<IGenericQux<int>>().FromType<GenericFreeImplementor<long, int>>();
            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithClosedSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Bar.TestType().Exact<GenericFreeImplementor<int, string>>(),
                            e.Qux.Instance.TestType().Exact<GenericFreeImplementor<long, int>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithClosedSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Bar.TestType().Exact<GenericFreeImplementor<int, string>>(),
                            e.Qux.Instance.TestType().Exact<GenericFreeImplementor<long, int>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithPartiallyOpenGenericResolverBasedClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeFooWithClosedSources<,> ) );
            builder.AddSharedGenericImplementor( typeof( GenericFreeImplementor<,> ) );
            builder.AddGeneric( typeof( IGenericFoo<> ) )
                .FromSharedImplementor( typeof( GenericFreeFooWithClosedSources<,> ).SubstituteGenericArguments( null, typeof( int ) ) );

            builder.AddGeneric( typeof( IGenericBar<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( long ) ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor( typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( typeof( long ) ) );

            builder.Add<Parameterized<IGenericFoo<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericFoo<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericFoo<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeFooWithClosedSources<string, int>>( e => Assertion.All(
                            "result1",
                            e.Bar.TestType().Exact<GenericFreeImplementor<int, long>>(),
                            e.Qux.Instance.TestType().Exact<GenericFreeImplementor<long, int>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeFooWithClosedSources<double, int>>( e => Assertion.All(
                            "result2",
                            e.Bar.TestType().Exact<GenericFreeImplementor<int, long>>(),
                            e.Qux.Instance.TestType().Exact<GenericFreeImplementor<long, int>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithPartiallyOpenCustomResolutionsForParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeQuxWithResolvableSources<,> ) )
                .FromConstructor( o => o.ResolveParameter(
                        _ => true,
                        typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                    .ResolveMember(
                        _ => true,
                        typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( long ) ) ) );

            builder.AddGeneric( typeof( GenericFreeFoo<,> ) );
            builder.AddGeneric( typeof( GenericFreeImplementor<,> ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor(
                    typeof( GenericFreeQuxWithResolvableSources<,> ).SubstituteGenericArguments( null, typeof( bool ) ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericQux<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeQuxWithResolvableSources<string, bool>>( e => Assertion.All(
                            "result1",
                            e.CtorFoo.TestType().Exact<GenericFreeFoo<string, int>>(),
                            e.MemberFoo.Instance.TestType().Exact<GenericFreeImplementor<string, long>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeQuxWithResolvableSources<double, bool>>( e => Assertion.All(
                            "result2",
                            e.CtorFoo.TestType().Exact<GenericFreeFoo<double, int>>(),
                            e.MemberFoo.Instance.TestType().Exact<GenericFreeImplementor<double, long>>() ) ) )
                .Go();
        }

        [Fact]
        public void ResolvingPartiallyClosedDependency_WithPartiallyOpenCustomResolutionsForClosedParameterAndMember()
        {
            var builder = new DependencyContainerBuilder();
            builder.AddSharedGenericImplementor( typeof( GenericFreeQuxWithResolvableClosedSources<,> ) )
                .FromConstructor( o => o.ResolveParameter(
                        _ => true,
                        typeof( GenericFreeFoo<,> ).SubstituteGenericArguments( null, typeof( int ) ) )
                    .ResolveMember(
                        _ => true,
                        typeof( GenericFreeImplementor<,> ).SubstituteGenericArguments( null, typeof( long ) ) ) );

            builder.AddGeneric( typeof( GenericFreeFoo<,> ) );
            builder.AddGeneric( typeof( GenericFreeImplementor<,> ) );

            builder.AddGeneric( typeof( IGenericQux<> ) )
                .FromSharedImplementor(
                    typeof( GenericFreeQuxWithResolvableClosedSources<,> ).SubstituteGenericArguments( null, typeof( bool ) ) );

            builder.Add<Parameterized<IGenericQux<string>>>();
            var sut = builder.Build();

            var result1 = sut.RootScope.Locator.Resolve<Parameterized<IGenericQux<string>>>();
            var result2 = sut.RootScope.Locator.Resolve<IGenericQux<double>>();

            Assertion.All(
                    result1.Inner.TestType()
                        .Exact<GenericFreeQuxWithResolvableClosedSources<string, bool>>( e => Assertion.All(
                            "result1",
                            e.CtorFoo.TestType().Exact<GenericFreeFoo<bool, int>>(),
                            e.MemberFoo.Instance.TestType().Exact<GenericFreeImplementor<bool, long>>() ) ),
                    result2.TestType()
                        .Exact<GenericFreeQuxWithResolvableClosedSources<double, bool>>( e => Assertion.All(
                            "result2",
                            e.CtorFoo.TestType().Exact<GenericFreeFoo<bool, int>>(),
                            e.MemberFoo.Instance.TestType().Exact<GenericFreeImplementor<bool, long>>() ) ) )
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

public class GenericFreeFooWithOpenSources<T1, T2> : DependencyTestsBase.IGenericFoo<T1>
{
    public GenericFreeFooWithOpenSources(DependencyTestsBase.IGenericBar<T1>? bar = null)
    {
        Bar = bar;
    }

    public DependencyTestsBase.IGenericBar<T1>? Bar { get; }

    [OptionalDependency]
    public Injected<DependencyTestsBase.IGenericQux<T1>?> Qux { get; }
}

public class GenericFreeFooWithPartiallyOpenSources<T1, T2> : DependencyTestsBase.IGenericFoo<T1>
{
    public GenericFreeFooWithPartiallyOpenSources(GenericFreeFooWithOpenSources<T1, T2>? ctorFoo = null)
    {
        CtorFoo = ctorFoo;
    }

    public GenericFreeFooWithOpenSources<T1, T2>? CtorFoo { get; }

    [OptionalDependency]
    public Injected<GenericFreeFooWithOpenSources<T2, T1>?> MemberFoo { get; }
}

public class GenericFreeFooWithNonGenericSources<T1, T2> : DependencyTestsBase.IGenericFoo<T1>
{
    public GenericFreeFooWithNonGenericSources(DependencyTestsBase.IFoo? foo = null)
    {
        Foo = foo;
    }

    public DependencyTestsBase.IFoo? Foo { get; }

    [OptionalDependency]
    public Injected<DependencyTestsBase.IBar?> Bar { get; }
}

public class GenericFreeFooWithClosedSources<T1, T2> : DependencyTestsBase.IGenericFoo<T1>
{
    public GenericFreeFooWithClosedSources(DependencyTestsBase.IGenericBar<T2>? bar = null)
    {
        Bar = bar;
    }

    public DependencyTestsBase.IGenericBar<T2>? Bar { get; }

    [OptionalDependency]
    public Injected<DependencyTestsBase.IGenericQux<T2>?> Qux { get; }
}

public class GenericFreeFooWithRangeSources<T1, T2> : DependencyTestsBase.IGenericFoo<T1>
{
    public GenericFreeFooWithRangeSources(IEnumerable<DependencyTestsBase.IGenericBar<T2>> bar)
    {
        Bar = bar;
    }

    public IEnumerable<DependencyTestsBase.IGenericBar<T2>> Bar { get; }
    public Injected<IEnumerable<DependencyTestsBase.IGenericQux<T2>>> Qux { get; }
}

public class GenericFreeQuxWithResolvableSources<T1, T2> : DependencyTestsBase.IGenericQux<T1>
{
    public GenericFreeQuxWithResolvableSources(DependencyTestsBase.IGenericFoo<T1> ctorFoo)
    {
        CtorFoo = ctorFoo;
    }

    public DependencyTestsBase.IGenericFoo<T1> CtorFoo { get; }
    public Injected<DependencyTestsBase.IGenericFoo<T1>> MemberFoo { get; }
}

public class GenericFreeQuxWithResolvableClosedSources<T1, T2> : DependencyTestsBase.IGenericQux<T1>
{
    public GenericFreeQuxWithResolvableClosedSources(DependencyTestsBase.IGenericFoo<T2> ctorFoo)
    {
        CtorFoo = ctorFoo;
    }

    public DependencyTestsBase.IGenericFoo<T2> CtorFoo { get; }
    public Injected<DependencyTestsBase.IGenericFoo<T2>> MemberFoo { get; }
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
