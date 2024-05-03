using System;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a builder of dependency container configuration.
/// </summary>
public interface IDependencyContainerConfigurationBuilder
{
    /// <summary>
    /// Open generic type that describes the injectable member type.
    /// </summary>
    Type InjectablePropertyType { get; }

    /// <summary>
    /// Type of an attribute that describes an optional dependency.
    /// </summary>
    Type OptionalDependencyAttributeType { get; }

    /// <summary>
    /// Specifies whether or not captive dependencies should be treated as errors instead of warnings.
    /// </summary>
    bool TreatCaptiveDependenciesAsErrors { get; }

    /// <summary>
    /// Sets the <see cref="InjectablePropertyType"/> of this instance.
    /// </summary>
    /// <param name="openGenericType">Type to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="DependencyContainerBuilderConfigurationException">
    /// When the provided type is not an open generic type or contains more than one generic argument or does not have a constructor
    /// that accepts a single parameter of the generic argument type.
    /// </exception>
    IDependencyContainerConfigurationBuilder SetInjectablePropertyType(Type openGenericType);

    /// <summary>
    /// Sets the <see cref="OptionalDependencyAttributeType"/> of this instance.
    /// </summary>
    /// <param name="attributeType">Type to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="DependencyContainerBuilderConfigurationException">
    /// When the provided type is a generic type definition or does not extend the <see cref="Attribute"/> type
    /// or is not valid on parameters and fields and properties.
    /// </exception>
    IDependencyContainerConfigurationBuilder SetOptionalDependencyAttributeType(Type attributeType);

    /// <summary>
    /// Sets the <see cref="TreatCaptiveDependenciesAsErrors"/> of this instance.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyContainerConfigurationBuilder EnableTreatingCaptiveDependenciesAsErrors(bool enabled = true);
}
