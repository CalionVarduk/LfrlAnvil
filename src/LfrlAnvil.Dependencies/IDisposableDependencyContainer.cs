using System;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a disposable dependency container.
/// </summary>
public interface IDisposableDependencyContainer : IDependencyContainer, IDisposable { }
