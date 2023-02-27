using System;
using System.Collections.Generic;

namespace LfrlAnvil.Dependencies;

public interface IDependencyRangeBuilder : IReadOnlyList<IDependencyBuilder>
{
    Type DependencyType { get; }
    IDependencyBuilder Add();
}
