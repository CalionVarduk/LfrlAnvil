using System;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt64 : SqliteColumnTypeDefinition<ulong>
{
    internal SqliteColumnTypeDefinitionUInt64()
        : base( SqliteDataType.Integer, 0 ) { }

    [Pure]
    public override string ToDbLiteral(ulong value)
    {
        return SqliteHelpers.GetDbLiteral( checked( (long)value ) );
    }

    [Pure]
    public override string? TryToDbLiteral(object value)
    {
        return value is ulong v && v <= long.MaxValue ? SqliteHelpers.GetDbLiteral( unchecked( (long)v ) ) : null;
    }

    public override void SetParameter(IDbDataParameter parameter, ulong value)
    {
        var v = checked( (long)value );
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = v;
    }

    public override void SetNullParameter(IDbDataParameter parameter)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = DBNull.Value;
    }

    public override bool TrySetParameter(IDbDataParameter parameter, object value)
    {
        if ( value is not ulong v || v > long.MaxValue )
            return false;

        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = unchecked( (long)v );
        return true;
    }
}
