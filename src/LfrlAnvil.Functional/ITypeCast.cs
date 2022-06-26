using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Functional;

public interface ITypeCast<out TDestination> : IReadOnlyCollection<TDestination>
{
    object? Source { get; }
    bool IsValid { get; }
    bool IsInvalid { get; }

    [Pure]
    TDestination GetResult();

    [Pure]
    TDestination? GetResultOrDefault();
}
