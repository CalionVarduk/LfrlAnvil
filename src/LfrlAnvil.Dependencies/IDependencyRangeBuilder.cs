using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a dependency range resolution builder.
/// </summary>
public interface IDependencyRangeBuilder
{
    /// <summary>
    /// Element type.
    /// </summary>
    Type DependencyType { get; }

    /// <summary>
    /// Specifies an optional callback that gets invoked right before the dependency instance is resolved.
    /// The first argument denotes the type of a dependency to resolve and
    /// the second argument is the scope that is resolving the dependency.
    /// </summary>
    Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    /// <summary>
    /// Collection of all <see cref="IDependencyBuilder"/> instances associated with this range.
    /// </summary>
    IReadOnlyList<IDependencyBuilder> Elements { get; }

    /// <summary>
    /// Creates a new <see cref="IDependencyBuilder"/> instance and registers it in this range.
    /// </summary>
    /// <returns>New <see cref="IDependencyBuilder"/> instance.</returns>
    IDependencyBuilder Add();

    /// <summary>
    /// Gets the last <see cref="IDependencyBuilder"/> instance registered in this range.
    /// </summary>
    /// <returns><see cref="IDependencyBuilder"/> instance of the last registered element or null when this range is empty.</returns>
    [Pure]
    IDependencyBuilder? TryGetLast();

    /// <summary>
    /// Sets the <see cref="OnResolvingCallback"/> for this instance.
    /// </summary>
    /// <param name="callback">Delegate to set.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyRangeBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback);
}
