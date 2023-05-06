using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionTimeOnly : SqliteColumnTypeDefinition<TimeOnly, string>
{
    internal SqliteColumnTypeDefinitionTimeOnly(SqliteColumnTypeDefinitionString @base)
        : base( @base, TimeOnly.MinValue ) { }

    [Pure]
    protected override string MapToBaseType(TimeOnly value)
    {
        return value.ToString( "HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
    }
}
