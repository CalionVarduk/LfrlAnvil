using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionByteArray : SqliteColumnTypeDefinition<byte[]>
{
    private const char Delimiter = '\'';
    private const char Marker = 'X';
    private static readonly string Empty = $"{Marker}{Delimiter}{Delimiter}";

    internal SqliteColumnTypeDefinitionByteArray()
        : base( SqliteDataType.Blob, Array.Empty<byte>() ) { }

    [Pure]
    public override string ToDbLiteral(byte[] value)
    {
        const int stackallocThreshold = 64;

        if ( value.Length == 0 )
            return Empty;

        var length = checked( (value.Length << 1) + 3 );
        var data = length <= stackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = Marker;
        data[1] = Delimiter;
        var index = 2;

        for ( var i = 0; i < value.Length; ++i )
        {
            var b = value[i];
            data[index++] = ToHexChar( b >> 4 );
            data[index++] = ToHexChar( b & 0xF );
        }

        data[^1] = Delimiter;
        return new string( data );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static char ToHexChar(int value)
    {
        Assume.IsInRange( value, 0, 15, nameof( value ) );
        return (char)(value < 10 ? '0' + value : 'A' + value - 10);
    }
}
