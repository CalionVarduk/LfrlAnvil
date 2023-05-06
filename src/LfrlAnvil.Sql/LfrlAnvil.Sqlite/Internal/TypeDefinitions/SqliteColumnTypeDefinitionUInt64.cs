using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt64 : SqliteColumnTypeDefinition<ulong, long>
{
    internal SqliteColumnTypeDefinitionUInt64(SqliteColumnTypeDefinitionInt64 @base)
        : base( @base, (ulong)@base.DefaultValue ) { }

    [Pure]
    public override string? TryToDbLiteral(object value)
    {
        return value is ulong v && v <= long.MaxValue ? Base.ToDbLiteral( unchecked( (long)v ) ) : null;
    }

    [Pure]
    protected override long MapToBaseType(ulong value)
    {
        return checked( (long)value );
    }
}
