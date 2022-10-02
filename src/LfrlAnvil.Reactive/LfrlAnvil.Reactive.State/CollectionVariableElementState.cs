using System;

namespace LfrlAnvil.Reactive.State;

[Flags]
public enum CollectionVariableElementState : byte
{
    Default = 0,
    Changed = 1,
    Invalid = 2,
    Warning = 4,
    Added = 8,
    Removed = 16,
    NotFound = 32
}
