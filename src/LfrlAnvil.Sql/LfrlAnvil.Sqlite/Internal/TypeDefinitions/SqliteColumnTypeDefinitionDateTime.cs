using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateTime : SqliteColumnTypeDefinition<DateTime, string>
{
    internal SqliteColumnTypeDefinitionDateTime(SqliteColumnTypeDefinitionString @base)
        : base( @base, DateTime.UnixEpoch ) { }

    [Pure]
    protected override string MapToBaseType(DateTime value)
    {
        return value.ToString( "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
    }
}
