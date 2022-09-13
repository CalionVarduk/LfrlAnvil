using System;

namespace LfrlAnvil.Computable.Automata;

[Flags]
public enum StateMachineNodeType : byte
{
    Default = 0,
    Initial = 1,
    Accept = 2
}
