([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Dependencies)](https://www.nuget.org/packages/LfrlAnvil.Dependencies/)

# [LfrlAnvil.Dependencies](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Dependencies)

This project contains an implementation of an IoC container.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Dependencies/LfrlAnvil.Dependencies.html).

### Examples

Following are a few examples of how to create a container and how to resolve dependencies:
```csharp
public interface IFoo { }

public interface IBar { }

public interface IQux { }

public class FooBar : IFoo, IBar { }

public class Qux : IQux
{
    public Qux(IFoo foo) { }
}

// creates a new empty IoC container builder
var builder = new DependencyContainerBuilder();

// registers a shared implementor type
// shared implementors can be used to configure multiple dependency types
// that use the same dependency resolver, as long as those dependency types have the same lifetime
builder.AddSharedImplementor<FooBar>();

// registers an IFoo interface as a resolvable dependency with Scoped lifetime
// that should be resolved by using the previously registered shared implementor
builder.Add<IFoo>().SetLifetime( DependencyLifetime.Scoped ).FromSharedImplementor<FooBar>();

// registers an IBar interface as a resolvable dependency with Scoped lifetime
// that should be resolved by using the previously registered shared implementor
// this means that IFoo and IBar resolutions from the same scope will returns the same FooBar instance
builder.Add<IBar>().SetLifetime( DependencyLifetime.Scoped ).FromSharedImplementor<FooBar>();

// defines a keyed locator builder
var keyed = builder.GetKeyedLocator( 42 );

// registers a keyed (with key equal to 42) IQux interface as a resolvable dependency with Singleton lifetime
// that should be implemented by Qux type
var quxBuilder = keyed.Add<IQux>().SetLifetime( DependencyLifetime.Singleton ).FromType<Qux>();

// the container builder will automatically attempt to find the best-suited implementor constructor
// but it is also possible to specify constructors explicitly
quxBuilder.FromConstructor( typeof( Qux ).GetConstructors().First() );

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

// it is also possible to resolve the container itself
var c = scope.Locator.Resolve<IDependencyContainer>();

// as well as the current scope
var s = scope.Locator.Resolve<IDependencyScope>();
```
