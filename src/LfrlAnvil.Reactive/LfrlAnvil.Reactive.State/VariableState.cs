using System;

namespace LfrlAnvil.Reactive.State;

[Flags]
public enum VariableState : byte
{
    Default = 0,
    Changed = 1,
    Invalid = 2,
    Warning = 4,
    ReadOnly = 8,
    Disposed = 16,
    Dirty = 32
}
