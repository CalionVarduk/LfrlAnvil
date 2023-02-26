using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

public interface IDependencyConstructor
{
    ConstructorInfo? Info { get; }
    Type? Type { get; }
    IDependencyConstructorInvocationOptions InvocationOptions { get; }
}
