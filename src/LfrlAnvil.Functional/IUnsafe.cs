using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Functional;

public interface IUnsafe
{
    bool HasError { get; }
    bool IsOk { get; }

    [Pure]
    object GetValue();

    [Pure]
    object? GetValueOrDefault();

    [Pure]
    Exception GetError();

    [Pure]
    Exception? GetErrorOrDefault();
}
