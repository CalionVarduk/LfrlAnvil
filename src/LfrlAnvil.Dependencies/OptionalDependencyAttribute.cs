using System;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a default attribute that can be used for marking parameters and members as optional dependencies.
/// </summary>
[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property )]
public sealed class OptionalDependencyAttribute : Attribute { }
