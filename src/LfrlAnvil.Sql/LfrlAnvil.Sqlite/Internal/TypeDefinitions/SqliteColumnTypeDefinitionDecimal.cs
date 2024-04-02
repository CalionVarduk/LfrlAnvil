using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDecimal : SqliteColumnTypeDefinition<decimal>
{
    internal SqliteColumnTypeDefinitionDecimal()
        : base(
            SqliteDataType.Text,
            0m,
            static (reader, ordinal) => decimal.Parse(
                reader.GetString( ordinal ),
                NumberStyles.Number | NumberStyles.AllowExponent,
                CultureInfo.InvariantCulture ) ) { }

    [Pure]
    public override string ToDbLiteral(decimal value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(decimal value)
    {
        return value >= 0
            ? value.ToString( SqlHelpers.DecimalFormat, CultureInfo.InvariantCulture )
            : (-value).ToString( SqliteHelpers.DecimalFormatNegative, CultureInfo.InvariantCulture );
    }
}
