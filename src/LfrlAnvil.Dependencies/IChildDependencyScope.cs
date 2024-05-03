using System;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a child dependency scope.
/// </summary>
public interface IChildDependencyScope : IDependencyScope, IDisposable { }
