﻿using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionDateTimeOffset : PostgreSqlColumnTypeDefinition<DateTimeOffset>
{
    internal PostgreSqlColumnTypeDefinitionDateTimeOffset()
        : base(
            PostgreSqlDataType.CreateVarChar( 33 ),
            DateTimeOffset.UnixEpoch,
            static (reader, ordinal) => DateTimeOffset.Parse( reader.GetString( ordinal ), CultureInfo.InvariantCulture ) ) { }

    [Pure]
    public override string ToDbLiteral(DateTimeOffset value)
    {
        const string format = $@"\'{SqlHelpers.DateTimeFormat}zzz\'";
        return value.ToString( format, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTimeOffset value)
    {
        const string format = $"{SqlHelpers.DateTimeFormat}zzz";
        return value.ToString( format, CultureInfo.InvariantCulture );
    }
}