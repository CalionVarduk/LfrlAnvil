using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateOnly : SqliteColumnTypeDefinition<DateOnly, string>
{
    internal SqliteColumnTypeDefinitionDateOnly(SqliteColumnTypeDefinitionString @base)
        : base( @base, DateOnly.FromDateTime( DateTime.UnixEpoch ) ) { }

    [Pure]
    protected override string MapToBaseType(DateOnly value)
    {
        return value.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture );
    }
}
