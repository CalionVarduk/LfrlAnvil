using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionString : SqliteColumnTypeDefinition<string>
{
    private const char Delimiter = '\'';
    private static readonly string Empty = $"{Delimiter}{Delimiter}";

    internal SqliteColumnTypeDefinitionString()
        : base( SqliteDataType.Text, string.Empty ) { }

    [Pure]
    public override string ToDbLiteral(string value)
    {
        const int stackallocThreshold = 64;

        var delimiterIndex = value.IndexOf( Delimiter );
        if ( delimiterIndex == -1 )
            return value.Length == 0 ? Empty : $"{Delimiter}{value}{Delimiter}";

        var delimiterCount = GetDelimiterCount( value.AsSpan( delimiterIndex + 1 ) ) + 1;

        var length = checked( value.Length + delimiterCount + 2 );
        var data = length <= stackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = Delimiter;

        var startIndex = 0;
        var buffer = data.Slice( 1, data.Length - 2 );

        do
        {
            var segment = value.AsSpan( startIndex, delimiterIndex - startIndex );
            segment.CopyTo( buffer );
            buffer[segment.Length] = Delimiter;
            buffer[segment.Length + 1] = Delimiter;
            buffer = buffer.Slice( segment.Length + 2 );

            startIndex = delimiterIndex + 1;
            delimiterIndex = value.IndexOf( Delimiter, startIndex );
        }
        while ( delimiterIndex != -1 );

        value.AsSpan( startIndex ).CopyTo( buffer );
        data[^1] = Delimiter;
        return new string( data );
    }

    [Pure]
    private static int GetDelimiterCount(ReadOnlySpan<char> text)
    {
        var count = 0;
        for ( var i = 0; i < text.Length; ++i )
        {
            if ( text[i] == Delimiter )
                ++count;
        }

        return count;
    }
}
