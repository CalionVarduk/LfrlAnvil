﻿using System.Diagnostics.Contracts;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionInt32 : MySqlColumnTypeDefinition<int>
{
    internal MySqlColumnTypeDefinitionInt32()
        : base( MySqlDataType.Int, 0, static (reader, ordinal) => reader.GetInt32( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(int value)
    {
        return MySqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(int value)
    {
        return value;
    }
}