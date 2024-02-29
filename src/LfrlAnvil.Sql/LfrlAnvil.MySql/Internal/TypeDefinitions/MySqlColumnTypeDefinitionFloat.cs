﻿using System.Diagnostics.Contracts;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionFloat : MySqlColumnTypeDefinition<float>
{
    internal MySqlColumnTypeDefinitionFloat()
        : base( MySqlDataType.Float, 0.0F, static (reader, ordinal) => reader.GetFloat( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(float value)
    {
        return MySqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(float value)
    {
        return value;
    }
}