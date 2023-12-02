using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDouble : SqliteColumnTypeDefinition<double>
{
    internal SqliteColumnTypeDefinitionDouble()
        : base( SqliteDataType.Real, 0.0, static (reader, ordinal) => reader.GetDouble( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(double value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(double value)
    {
        return value;
    }
}
