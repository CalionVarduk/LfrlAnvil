using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="StringBuilder"/> extension methods.
/// </summary>
public static class StringBuilderExtensions
{
    /// <summary>
    /// Reverses a given segment in the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source string builder.</param>
    /// <param name="startIndex">Index of the first character to reverse. Equal to <b>0</b> by default.</param>
    /// <param name="length">Length of the segment to reverse. Equal to <see cref="Int32.MaxValue"/> by default.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static StringBuilder Reverse(this StringBuilder builder, int startIndex = 0, int length = int.MaxValue)
    {
        var endIndex = ( int )Math.Max( Math.Min( startIndex + ( long )length, builder.Length ) - 1, 0 );
        startIndex = Math.Max( startIndex, 0 );

        while ( startIndex < endIndex )
        {
            var temp = builder[startIndex];
            builder[startIndex++] = builder[endIndex];
            builder[endIndex--] = temp;
        }

        return builder;
    }

    /// <summary>
    /// Appends a new line followed by <paramref name="count"/> spaces.
    /// </summary>
    /// <param name="builder">Source string builder.</param>
    /// <param name="count">Number of spaces to append after new line.</param>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder Indent(this StringBuilder builder, int count)
    {
        return builder.AppendLine().Append( ' ', count );
    }

    /// <summary>
    /// Appends a <paramref name="symbol"/> followed by new line.
    /// </summary>
    /// <param name="builder">Source string builder.</param>
    /// <param name="symbol">Symbol to append before new line.</param>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder AppendLine(this StringBuilder builder, char symbol)
    {
        return builder.Append( symbol ).AppendLine();
    }

    /// <summary>
    /// Appends a single space character.
    /// </summary>
    /// <param name="builder">Source string builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder AppendSpace(this StringBuilder builder)
    {
        return builder.Append( ' ' );
    }

    /// <summary>
    /// Appends a single dot character.
    /// </summary>
    /// <param name="builder">Source string builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder AppendDot(this StringBuilder builder)
    {
        return builder.Append( '.' );
    }

    /// <summary>
    /// Appends a single comma character.
    /// </summary>
    /// <param name="builder">Source string builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder AppendComma(this StringBuilder builder)
    {
        return builder.Append( ',' );
    }

    /// <summary>
    /// Appends a single semicolon character.
    /// </summary>
    /// <param name="builder">Source string builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder AppendSemicolon(this StringBuilder builder)
    {
        return builder.Append( ';' );
    }

    /// <summary>
    /// Reduces the <see cref="StringBuilder.Length"/> of the <paramref name="builder"/> by the given <paramref name="length"/>.
    /// </summary>
    /// <param name="builder">Source string builder.</param>
    /// <param name="length">Number of characters to remove at the end.</param>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder ShrinkBy(this StringBuilder builder, int length)
    {
        Assume.IsGreaterThanOrEqualTo( length, 0 );
        builder.Length -= length;
        return builder;
    }
}
