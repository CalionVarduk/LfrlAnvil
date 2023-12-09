using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionTimeOnly : MySqlColumnTypeDefinition<TimeOnly>
{
    internal MySqlColumnTypeDefinitionTimeOnly()
        : base( MySqlDataType.Time, TimeOnly.MinValue, static (reader, ordinal) => reader.GetTimeOnly( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(TimeOnly value)
    {
        return value.ToString( "TI\\ME\\'HH:mm:ss.ffffff\\'", CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(TimeOnly value)
    {
        return value;
    }
}
