using System;

namespace LfrlAnvil.MySql.Internal;

[Flags]
internal enum MySqlObjectStatus : byte
{
    None = 0,
    Created = 1,
    Modified = 2,
    Removed = 4,
    Unused = Created | Removed
}
