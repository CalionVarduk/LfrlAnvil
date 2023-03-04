using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

[Flags]
internal enum DependencyResolverFactoryState : byte
{
    Created = 0,
    Validatable = 1,
    ValidatedRequiredDependencies = 2,
    ValidatingCircularDependencies = 4,
    Validated = 8,
    Finished = 16,
    Invalid = 32,
    CircularDependenciesDetected = 64,
    CanRegisterCircularDependency = ValidatingCircularDependencies | CircularDependenciesDetected
}
