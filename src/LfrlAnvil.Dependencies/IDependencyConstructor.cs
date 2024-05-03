using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a constructor of dependency instances.
/// </summary>
public interface IDependencyConstructor
{
    /// <summary>
    /// <see cref="ConstructorInfo"/> to use for creating dependency instances.
    /// Null value means that an attempt will be made to find best suited constructor from <see cref="Type"/>.
    /// </summary>
    /// <remarks>
    /// Best suited constructor is found by scanning all constructors of a type and scoring them based on the following rules:
    /// <list type="bullet">
    /// <item><description>Public constructors start with 1 point, private constructors start with 0 points.</description></item>
    /// <item><description>
    /// If at least one parameter cannot be resolved or its resolver's type is not compatible,
    /// then the constructor is marked as not eligible.
    /// </description></item>
    /// <item><description>Each parameter with a custom factory resolution adds 3 points.</description></item>
    /// <item><description>
    /// Each parameter with a custom non-factory resolution that is not a captive dependency adds 3 points.
    /// </description></item>
    /// <item><description>Each automatically resolvable parameter that is not a captive dependency adds 2 points.</description></item>
    /// <item><description>
    /// Each unresolvable optional parameter (e.g. with a default value) with a custom non-factory resolution adds 2 points.
    /// </description></item>
    /// <item><description>Each unresolvable optional parameter (e.g. with a default value) adds 1 point.</description></item>
    /// <item><description>
    /// Each parameter with a custom non-factory resolution that is a captive dependency adds 1 point.
    /// </description></item>
    /// </list>
    /// Then, the constructor with the highest score is chosen. If multiple constructors exist with the highest score,
    /// then the constructor with the largest number of parameters is chosen.
    /// </remarks>
    ConstructorInfo? Info { get; }

    /// <summary>
    /// <see cref="System.Type"/> to use for resolving dependency instances.
    /// Null value means that the dependency's type itself will be used.
    /// </summary>
    Type? Type { get; }

    /// <summary>
    /// Specifies custom constructor invocation options.
    /// </summary>
    IDependencyConstructorInvocationOptions InvocationOptions { get; }
}
