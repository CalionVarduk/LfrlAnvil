using System;

namespace LfrlAnvil.Dependencies;

[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property )]
public sealed class OptionalDependencyAttribute : Attribute { }
