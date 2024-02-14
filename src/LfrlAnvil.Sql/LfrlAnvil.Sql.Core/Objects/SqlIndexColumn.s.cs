using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects;

public static class SqlIndexColumn
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexColumn<T> Create<T>(T column, OrderBy ordering)
        where T : class, ISqlColumn
    {
        return new SqlIndexColumn<T>( column, ordering );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexColumn<T> CreateAsc<T>(T column)
        where T : class, ISqlColumn
    {
        return Create( column, OrderBy.Asc );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexColumn<T> CreateDesc<T>(T column)
        where T : class, ISqlColumn
    {
        return Create( column, OrderBy.Desc );
    }
}
