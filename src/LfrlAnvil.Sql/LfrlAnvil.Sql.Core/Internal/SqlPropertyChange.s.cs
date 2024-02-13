using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Internal;

public static class SqlPropertyChange
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPropertyChange<T> Create<T>(T newValue, object? state = null)
    {
        return new SqlPropertyChange<T>( isActive: true, newValue, state );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPropertyChange<T> Cancel<T>()
    {
        return new SqlPropertyChange<T>( isActive: false, default!, state: null );
    }
}
