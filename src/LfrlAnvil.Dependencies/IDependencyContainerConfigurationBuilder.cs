using System;

namespace LfrlAnvil.Dependencies;

public interface IDependencyContainerConfigurationBuilder
{
    Type InjectablePropertyType { get; }
    Type OptionalDependencyAttributeType { get; }
    bool TreatCaptiveDependenciesAsErrors { get; }

    IDependencyContainerConfigurationBuilder SetInjectablePropertyType(Type openGenericType);
    IDependencyContainerConfigurationBuilder SetOptionalDependencyAttributeType(Type attributeType);
    IDependencyContainerConfigurationBuilder EnableTreatingCaptiveDependenciesAsErrors(bool enabled = true);
}
