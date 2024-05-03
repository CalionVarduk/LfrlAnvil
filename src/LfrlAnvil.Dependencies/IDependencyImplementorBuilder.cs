using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a dependency implementor resolution builder.
/// </summary>
public interface IDependencyImplementorBuilder
{
    /// <summary>
    /// Implementor's type.
    /// </summary>
    Type ImplementorType { get; }

    /// <summary>
    /// Explicit constructor definition of this implementor's instances.
    /// </summary>
    IDependencyConstructor? Constructor { get; }

    /// <summary>
    /// Explicit creator of this implementor's instances.
    /// </summary>
    Func<IDependencyScope, object>? Factory { get; }

    /// <summary>
    /// Disposal strategy of this implementor's instances. See <see cref="DependencyImplementorDisposalStrategy"/> for more information.
    /// </summary>
    DependencyImplementorDisposalStrategy DisposalStrategy { get; }

    /// <summary>
    /// Specifies an optional callback that gets invoked right before the dependency instance is resolved.
    /// The first argument denotes the type of a dependency to resolve and
    /// the second argument is the scope that is resolving the dependency.
    /// </summary>
    Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    /// <summary>
    /// Specifies that this implementor's instances should be created by the best suited constructor of this implementor's type.
    /// </summary>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyImplementorBuilder FromConstructor(Action<IDependencyConstructorInvocationOptions>? configuration = null);

    /// <summary>
    /// Specifies that this implementor's instances should be created by the provided constructor.
    /// </summary>
    /// <param name="info">Constructor to use for creating implementor instances.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IDependencyConstructorInvocationOptions>? configuration = null);

    /// <summary>
    /// Specifies that this implementor's instances should be created
    /// by the best suited constructor of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Implementor's type.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyImplementorBuilder FromType(Type type, Action<IDependencyConstructorInvocationOptions>? configuration = null);

    /// <summary>
    /// Specifies that this implementor's instances should be created by the provided explicit <paramref name="factory"/>.
    /// </summary>
    /// <param name="factory">Explicit creator of implementor instances.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory);

    /// <summary>
    /// Sets the <see cref="DisposalStrategy"/> of this instance.
    /// </summary>
    /// <param name="strategy">Strategy to set.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyImplementorBuilder SetDisposalStrategy(DependencyImplementorDisposalStrategy strategy);

    /// <summary>
    /// Sets the <see cref="OnResolvingCallback"/> for this instance.
    /// </summary>
    /// <param name="callback">Delegate to set.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyImplementorBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback);
}
