﻿namespace LfrlAnvil.Identifiers
{
    public enum LowValueExceededHandlingStrategy : byte
    {
        Forbidden = 0,
        AddHighValue = 1,
        BusyWait = 2,
        Sleep = 3
    }
}
