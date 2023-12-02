using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionGuid : SqliteColumnTypeDefinition<Guid>
{
    internal SqliteColumnTypeDefinitionGuid()
        : base( SqliteDataType.Blob, Guid.Empty, static (reader, ordinal) => new Guid( (byte[])reader.GetValue( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(Guid value)
    {
        return SqliteHelpers.GetDbLiteral( value.ToByteArray() );
    }

    [Pure]
    public override object ToParameterValue(Guid value)
    {
        return value.ToByteArray();
    }
}
