using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDecimal : SqliteColumnTypeDefinition<decimal, string>
{
    internal SqliteColumnTypeDefinitionDecimal(SqliteColumnTypeDefinitionString @base)
        : base( @base, 0m ) { }

    [Pure]
    protected override string MapToBaseType(decimal value)
    {
        return value.ToString( "0.0###########################", CultureInfo.InvariantCulture );
    }
}
