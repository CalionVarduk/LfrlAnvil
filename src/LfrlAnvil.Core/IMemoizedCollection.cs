using System;
using System.Collections.Generic;

namespace LfrlAnvil
{
    public interface IMemoizedCollection<T> : IReadOnlyCollection<T>
    {
        Lazy<IReadOnlyCollection<T>> Source { get; }
        bool IsMaterialized { get; }
    }
}
