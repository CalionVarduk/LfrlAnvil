using System.Globalization;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a set of <see cref="Thread"/> parameters.
/// </summary>
/// <param name="Culture">Thread's culture.</param>
/// <param name="UICulture">Thread's UI culture.</param>
/// <param name="Name">Thread's name.</param>
/// <param name="Priority">Thread's priority.</param>
public readonly record struct ThreadParams(
    CultureInfo? Culture = null,
    CultureInfo? UICulture = null,
    string? Name = null,
    ThreadPriority? Priority = null
);
