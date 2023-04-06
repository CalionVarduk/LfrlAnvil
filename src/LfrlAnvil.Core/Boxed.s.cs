using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

public static class Boxed
{
    public static readonly object True = true;
    public static readonly object False = false;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static object GetBool(bool value)
    {
        return value ? True : False;
    }
}
