([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://img.shields.io/nuget/v/LfrlAnvil.Dependencies.svg)](https://www.nuget.org/packages/LfrlAnvil.Dependencies/)

# [<img src="../../../assets/logo.png" alt="logo" height="80"/>](../../../assets/logo.png) [LfrlAnvil.Dependencies](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Dependencies)

This project contains an implementation of an IoC container.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Dependencies/LfrlAnvil.Dependencies.html).

### Examples

Following are a few examples of how to create a container and how to resolve dependencies:
```csharp
public interface IFoo { }

public interface IBar { }

public interface IQux { }

public interface IGenericFoo<T> { }

public interface IGenericBar<T> { }

public interface IGenericQux<T> { }

public class FooBar : IFoo, IBar { }

public class Qux : IQux
{
    public Qux(IFoo foo) { }
}

public class GenericFooBar<T> : IGenericFoo<T>, IGenericBar<T> { }

public class GenericQux<T1, T2> : IGenericQux<T1>
{
    public GenericQux(IGenericFoo<T2> foo) { }
}

// creates a new empty IoC container builder
var builder = new DependencyContainerBuilder();

// registers a shared implementor type
// shared implementors can be used to configure multiple dependency types
// that use the same dependency resolver, as long as those dependency types have the same lifetime
builder.AddSharedImplementor<FooBar>();

// registers an open generic shared implementor type
builder.AddSharedGenericImplementor( typeof( GenericFooBar<> ) );

// registers an IFoo interface as a resolvable dependency with Scoped lifetime
// that should be resolved by using the previously registered shared implementor
builder.Add<IFoo>().SetLifetime( DependencyLifetime.Scoped ).FromSharedImplementor<FooBar>();

// registers an IBar interface as a resolvable dependency with Scoped lifetime
// that should be resolved by using the previously registered shared implementor
// this means that IFoo and IBar resolutions from the same scope will return the same FooBar instance
builder.Add<IBar>().SetLifetime( DependencyLifetime.Scoped ).FromSharedImplementor<FooBar>();

// registers an open generic IGenericFoo<> interface
// using previously registered open generic shared implementor
builder
    .AddGeneric( typeof( IGenericFoo<> ) )
    .SetLifetime( DependencyLifetime.Scoped )
    .FromSharedImplementor( typeof( GenericFooBar<> ) );

// registers an open generic IGenericBar<> interface
// using previously registered open generic shared implementor
builder
    .AddGeneric( typeof( IGenericBar<> ) )
    .SetLifetime( DependencyLifetime.Scoped )
    .FromSharedImplementor( typeof( GenericFooBar<> ) );

// defines a keyed locator builder
var keyed = builder.GetKeyedLocator( 42 );

// registers a keyed (with key equal to 42) IQux interface as a resolvable dependency with Singleton lifetime
// that should be implemented by Qux type
var quxBuilder = keyed.Add<IQux>().SetLifetime( DependencyLifetime.Singleton ).FromType<Qux>();

// the container builder will automatically attempt to find the best-suited implementor constructor
// but it is also possible to specify constructors explicitly
// also, since keyed locator doesn't have IFoo registered
// a custom resolution can be used to e.g. point at the non-keyed IFoo registration instead
quxBuilder.FromConstructor(
    typeof( Qux ).GetConstructors().First(),
    opt => opt.ResolveParameter( p => p.Name == "foo", typeof( IFoo ), o => o.NotKeyed() ) );

// registeres an open generic keyed IGenericQux<> interface
// that should be implemented via partially closed type GenericQux<T1, int>
keyed
    .AddGeneric( typeof( IGenericQux<> ) )
    .SetLifetime( DependencyLifetime.Singleton )
    .FromType(
        typeof( GenericQux<,> ).SubstituteGenericArguments( null, typeof( int ) ),
        opt => opt.ResolveParameter( p => p.Name == "foo", typeof( IGenericFoo<int> ), o => o.NotKeyed() ) );

// builds the IoC container
var container = builder.Build();

// begins a new scope
// it's generally not a good practice to resolve dependencies directly from the root scope
// since it may lead to memory-leak-like behavior
// also, make sure to dispose began scopes once you're done with them
using var scope = container.RootScope.BeginScope();

// resolves IFoo instance
var foo = scope.Locator.Resolve<IFoo>();

// resolves IBar instance, which should return the same object
var bar = scope.Locator.Resolve<IBar>();

// resolves keyed IQux instance
var qux = scope.GetKeyedLocator( 42 ).Resolve<IQux>();

// resolves a range of IBar instances
// result will contain only one element, since IBar dependency has only been registered once
var barRange = scope.Locator.Resolve<IEnumerable<IBar>>();

// resolves open generic IGenericFoo<> instance with string argument
var genericFoo = scope.Locator.Resolve<IGenericFoo<string>>();

// resolves open generic IGenericBar<> instance with string argument
// which should return the same object
var genericBar = scope.Locator.Resolve<IGenericBar<string>>();

// resolves keyed IGenericQux<> instance with string argument
var genericQux = scope.GetKeyedLocator( 42 ).Resolve<IGenericQux<string>>();

// resolves an open generic range of IGenericBar<> instances with string argument
var genericBarRange = scope.Locator.Resolve<IEnumerable<IGenericBar<string>>>();

// it is also possible to resolve the container itself
var c = scope.Locator.Resolve<IDependencyContainer>();

// as well as the current scope
var s = scope.Locator.Resolve<IDependencyScope>();

// or a scope factory
var f = scope.Locator.Resolve<IDependencyScopeFactory>();
```
