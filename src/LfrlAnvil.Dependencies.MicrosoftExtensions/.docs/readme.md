([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Dependencies.MicrosoftExtensions)](https://www.nuget.org/packages/LfrlAnvil.Dependencies.MicrosoftExtensions/)

# [<img src="../../../assets/logo.png" alt="logo" height="80"/>](../../../assets/logo.png) [LfrlAnvil.Dependencies.MicrosoftExtensions](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Dependencies.MicrosoftExtensions)

This project contains an integration of LfrlAnvil IoC container with Microsoft's service provider.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Dependencies.MicrosoftExtensions/LfrlAnvil.Dependencies.MicrosoftExtensions.html).

### Examples

Following is an example on how to create an `IServiceProvider` from a `DependencyContainerBuilder`:
```csharp
// services to populate the builder with
IServiceCollection services = ...;

// creates a service provider factory
var factory = new DependencyContainerServiceProviderFactory(
    onBuild: b =>
    {
        // can be used to register dependencies directly via DependencyContainerBuilder instance
    } );

// populates the builder with services
var builder = factory.CreateBuilder( services );

// creates root service provider
var serviceProvider = factory.CreateServiceProvider( builder );
```
