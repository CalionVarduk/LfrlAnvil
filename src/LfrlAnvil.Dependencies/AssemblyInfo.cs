using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Dependencies.Tests" )]

// TODO:
// dedicated generic builder types for generic extension methods:
// ^ this would allow to provide a factory that returns T instead of object
// ^ also, this would possibly allow to set callback for disposal strategy that accepts T instead of object
// ^ simple QoL & DevExp improvements
// ^ LOW PRIORITY, needs a lot of annoying changes for little gain, setup can still go wrong
// ^ since, e.g. providing an explicit ctor should still be available, which can't really be done 'generically'
// ^ at least not without complete interface rework
// ^ e.g. FromCtor<T>(Func<ConstructorInfo, bool> selector), where selector is fed constructors from the provided T type
// ^ however, this would have its own drawbacks & annoyances
// ^ e.g. why need to filter at all if desired ctor info ref is already assigned to a variable? feels inefficient
//
// generic dependency types:
// ^ add support for open generic constructable dependency types
// ^ can use only auto-discovered ctor or explicit ctor from that open generic type or shared open generic implementor
// ^ each generic dependency could provide partial generic parameters resolution
// ^ e.g. Implementor<T, U> : IDependency<T>, T will be filled in automatically, but U cannot be resolved based on the interface alone
// ^ the functionality could allow to provide a concrete type to use as a substitution for the U type
//
