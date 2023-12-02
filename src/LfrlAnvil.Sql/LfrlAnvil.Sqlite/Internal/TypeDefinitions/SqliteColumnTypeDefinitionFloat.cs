using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionFloat : SqliteColumnTypeDefinition<float>
{
    internal SqliteColumnTypeDefinitionFloat()
        : base( SqliteDataType.Real, 0.0F, static (reader, ordinal) => (float)reader.GetDouble( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(float value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(float value)
    {
        return (double)value;
    }
}
