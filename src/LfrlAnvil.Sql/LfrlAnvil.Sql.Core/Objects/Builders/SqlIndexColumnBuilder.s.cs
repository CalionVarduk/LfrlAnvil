using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public static class SqlIndexColumnBuilder
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexColumnBuilder<T> Create<T>(T column, OrderBy ordering)
        where T : class, ISqlColumnBuilder
    {
        return new SqlIndexColumnBuilder<T>( column, ordering );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexColumnBuilder<T> CreateAsc<T>(T column)
        where T : class, ISqlColumnBuilder
    {
        return Create( column, OrderBy.Asc );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexColumnBuilder<T> CreateDesc<T>(T column)
        where T : class, ISqlColumnBuilder
    {
        return Create( column, OrderBy.Desc );
    }
}
