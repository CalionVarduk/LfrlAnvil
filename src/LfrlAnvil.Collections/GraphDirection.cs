using System;

namespace LfrlAnvil.Collections;

[Flags]
public enum GraphDirection : byte
{
    None = 0,
    In = 1,
    Out = 2,
    Both = In | Out
}
