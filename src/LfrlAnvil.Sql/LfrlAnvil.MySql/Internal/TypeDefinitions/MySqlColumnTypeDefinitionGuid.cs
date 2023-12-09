using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionGuid : MySqlColumnTypeDefinition<Guid>
{
    internal MySqlColumnTypeDefinitionGuid(MySqlDataTypeProvider provider)
        : base( provider.GetGuid(), Guid.Empty, static (reader, ordinal) => new Guid( (byte[])reader.GetValue( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(Guid value)
    {
        return MySqlHelpers.GetDbLiteral( value.ToByteArray() );
    }

    [Pure]
    public override object ToParameterValue(Guid value)
    {
        return value.ToByteArray();
    }
}
