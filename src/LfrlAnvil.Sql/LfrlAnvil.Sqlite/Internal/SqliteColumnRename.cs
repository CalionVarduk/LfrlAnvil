using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal readonly record struct SqliteColumnRename(SqliteColumnBuilder Column, string OriginalName, string Name, bool IsPending)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnRename Create(SqliteColumnBuilder column, string originalName)
    {
        return new SqliteColumnRename( column, originalName, column.Name, IsPending: true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnRename CreateTemporary(SqliteColumnRename @base, string temporaryName)
    {
        return new SqliteColumnRename( @base.Column, temporaryName, @base.Name, IsPending: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteColumnRename Complete()
    {
        return new SqliteColumnRename( Column, OriginalName, Name, IsPending: false );
    }
}
