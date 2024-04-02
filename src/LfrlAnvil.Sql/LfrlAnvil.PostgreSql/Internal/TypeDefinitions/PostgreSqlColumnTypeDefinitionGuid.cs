using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionGuid : PostgreSqlColumnTypeDefinition<Guid>
{
    internal PostgreSqlColumnTypeDefinitionGuid()
        : base( PostgreSqlDataType.Uuid, Guid.Empty, static (reader, ordinal) => reader.GetGuid( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(Guid value)
    {
        return PostgreSqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(Guid value)
    {
        return value;
    }
}
