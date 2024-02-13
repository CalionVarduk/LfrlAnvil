using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public static class SqlObjectBuilderReferenceSource
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderReferenceSource<SqlObjectBuilder> Create(SqlObjectBuilder @object, string? property = null)
    {
        return new SqlObjectBuilderReferenceSource<SqlObjectBuilder>( @object, property );
    }
}
