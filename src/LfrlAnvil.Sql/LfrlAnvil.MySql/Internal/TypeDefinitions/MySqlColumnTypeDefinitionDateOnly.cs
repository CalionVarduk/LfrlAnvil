﻿using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionDateOnly : MySqlColumnTypeDefinition<DateOnly>
{
    internal MySqlColumnTypeDefinitionDateOnly()
        : base(
            MySqlDataType.Date,
            DateOnly.FromDateTime( DateTime.UnixEpoch ),
            static (reader, ordinal) => reader.GetDateOnly( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(DateOnly value)
    {
        const string format = $@"DATE\'{SqlHelpers.DateFormat}\'";
        return value.ToString( format, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateOnly value)
    {
        return value;
    }
}
