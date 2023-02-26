using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Dependencies.Tests" )]

// TODO:
// dedicated generic builder types for generic extension methods:
// ^ this would allow to provide a factory that returns T instead of object
// ^ also, this would possibly allow to set callback for disposal strategy that accepts T instead of object
// ^ simple QoL & DevExp improvements
//
// container builder modules - allow to 'add' a builder to another builder, so that they can be modularized
//
// automatic IEnumerable<> resolvers:
// ^ along with 'normal' dependency registrations, a transient 'IEnumerable' version of them will be generated automatically
// ^ this may still lead to captive or circular dependencies, so keep that in mind
// ^ dependency type can be registered multiple times, resolving that type returns the last registered version of it,
// ^ however, resolving IEnumerable<> of that type returns all versions, in order of registration
// ^ single dependency type can be configured to not be included in the IEnumerable<>
//
// generic dependency types:
// ^ add support for open generic constructable dependency types
// ^ can use only auto-discovered ctor or explicit ctor from that open generic type or shared open generic implementor
//
