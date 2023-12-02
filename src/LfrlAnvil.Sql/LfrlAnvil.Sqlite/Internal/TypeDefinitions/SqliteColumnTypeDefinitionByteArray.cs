﻿using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionByteArray : SqliteColumnTypeDefinition<byte[]>
{
    internal SqliteColumnTypeDefinitionByteArray()
        : base( SqliteDataType.Blob, Array.Empty<byte>(), static (reader, ordinal) => (byte[])reader.GetValue( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(byte[] value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(byte[] value)
    {
        return value;
    }
}
