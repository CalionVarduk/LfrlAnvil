using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateTimeOffset : SqliteColumnTypeDefinition<DateTimeOffset, string>
{
    internal SqliteColumnTypeDefinitionDateTimeOffset(SqliteColumnTypeDefinitionString @base)
        : base( @base, DateTimeOffset.UnixEpoch ) { }

    [Pure]
    protected override string MapToBaseType(DateTimeOffset value)
    {
        return value.ToString( "yyyy-MM-dd HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture );
    }
}
