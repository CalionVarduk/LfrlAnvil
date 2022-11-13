using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Dependencies.Tests" )]

// TODO:
// Four different ways to configure dependency builders:
// ^ auto-discovered ctor => provide the type, the builder will attempt to discover a most valid ctor for it automatically (default)
// ^ explicit ctor => provide the ctor to use directly
// ^ explicit factory => provide a factory delegate that returns dependency instances
// ^ shared implementor key => dependency only, uses shared dependency resolver associated with the provided implementor key
// ctor versions will validate circular dependencies and dependency resolutions during container building
// ctor versions will allow to specify injections per parameter, by default will try to resolve through scope, can be overriden with a delegate
// ctor versions will accept an optional OnCreated callback
// ctor versions respect InjectablePropertyType & OptionalDependencyAttributeType configurations
// ctor versions treat Nullable<> as optional by default
//
// give up on AggregateExceptions, their messages are not verbose enough
//
// dedicated generic builder types for generic extension methods:
// ^ this would allow to provide a factory that returns T instead of object
// ^ also, this would possibly allow to set callback for disposal strategy that accepts T instead of object
// ^ simple QoL & DevExp improvements
//
// configuration:
// ^ treat captive dependencies as errors => by default its a warning (works only for ctor based dependency builders)
//
// add query methods:
// is type registered? what is its lifetime? etc.
//
// keyed dependencies:
// ^ add possibility to register dependencies under custom keys
//
// automatic IEnumerable<> resolvers:
// ^ along with 'normal' dependency registrations, a transient 'IEnumerable' version of them will be generated automatically
// ^ this may still lead to captive dependencies, so keep that in mind
// ^ dependency type can be registered multiple times, resolving that type returns the last registered version of it,
// ^ however, resolving IEnumerable<> of that type returns all versions, in order of registration
//
// generic dependency types:
// ^ add support for open generic constructable dependency types
// ^ can use only auto-discovered ctor or explicit ctor from that open generic type or shared open generic implementor
//
