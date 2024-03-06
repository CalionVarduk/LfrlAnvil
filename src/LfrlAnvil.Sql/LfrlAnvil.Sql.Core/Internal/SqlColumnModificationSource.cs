using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public readonly record struct SqlColumnModificationSource<T>(T Column, T Source)
    where T : ISqlColumnBuilder
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnModificationSource<T> Self(T column)
    {
        return new SqlColumnModificationSource<T>( column, column );
    }
}
