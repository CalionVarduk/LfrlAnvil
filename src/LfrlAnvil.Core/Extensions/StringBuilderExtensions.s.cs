using System;
using System.Text;

namespace LfrlAnvil.Extensions;

public static class StringBuilderExtensions
{
    public static StringBuilder Reverse(this StringBuilder builder, int startIndex = 0, int length = int.MaxValue)
    {
        var endIndex = (int)Math.Max( Math.Min( startIndex + (long)length, builder.Length ) - 1, 0 );
        startIndex = Math.Max( startIndex, 0 );

        while ( startIndex < endIndex )
        {
            var temp = builder[startIndex];
            builder[startIndex++] = builder[endIndex];
            builder[endIndex--] = temp;
        }

        return builder;
    }
}
