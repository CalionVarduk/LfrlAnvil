using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Internal;

internal readonly record struct PostgreSqlObjectRename(SqlObjectBuilder Object, string OriginalName, string Name, bool IsPending)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlObjectRename Create(SqlObjectBuilder obj, string originalName)
    {
        return new PostgreSqlObjectRename( obj, originalName, obj.Name, IsPending: true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlObjectRename CreateTemporary(PostgreSqlObjectRename @base, string temporaryName)
    {
        return new PostgreSqlObjectRename( @base.Object, temporaryName, @base.Name, IsPending: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlObjectRename Complete()
    {
        return new PostgreSqlObjectRename( Object, OriginalName, Name, IsPending: false );
    }
}
