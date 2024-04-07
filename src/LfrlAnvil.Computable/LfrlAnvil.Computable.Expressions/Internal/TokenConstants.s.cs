using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal static class TokenConstants
{
    internal const char Underscore = '_';
    internal const char OpenedParenthesis = '(';
    internal const char ClosedParenthesis = ')';
    internal const char OpenedSquareBracket = '[';
    internal const char ClosedSquareBracket = ']';
    internal const char LineSeparator = ';';
    internal const char ElementSeparator = ',';
    internal const char MemberAccess = '.';
    internal const char ScientificNotationPositiveExponentOperator = '+';
    internal const char ScientificNotationNegativeExponentOperator = '-';
    internal const char Assignment = '=';

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsBooleanTrue(StringSegment text)
    {
        if ( text.Length != 4 )
            return false;

        var source = text.Source;

        if ( source[text.StartIndex] == 't' )
            return source[text.StartIndex + 1] == 'r' && source[text.StartIndex + 2] == 'u' && source[text.StartIndex + 3] == 'e';

        if ( source[text.StartIndex] == 'T' )
        {
            if ( source[text.StartIndex + 1] == 'R' )
                return source[text.StartIndex + 2] == 'U' && source[text.StartIndex + 3] == 'E';

            if ( source[text.StartIndex + 1] == 'r' )
                return source[text.StartIndex + 2] == 'u' && source[text.StartIndex + 3] == 'e';
        }

        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsBooleanFalse(StringSegment text)
    {
        if ( text.Length != 5 )
            return false;

        var source = text.Source;

        if ( source[text.StartIndex] == 'f' )
        {
            return source[text.StartIndex + 1] == 'a'
                && source[text.StartIndex + 2] == 'l'
                && source[text.StartIndex + 3] == 's'
                && source[text.StartIndex + 4] == 'e';
        }

        if ( source[text.StartIndex] == 'F' )
        {
            if ( source[text.StartIndex + 1] == 'A' )
            {
                return source[text.StartIndex + 2] == 'L' && source[text.StartIndex + 3] == 'S' && source[text.StartIndex + 4] == 'E';
            }

            if ( source[text.StartIndex + 1] == 'a' )
            {
                return source[text.StartIndex + 2] == 'l' && source[text.StartIndex + 3] == 's' && source[text.StartIndex + 4] == 'e';
            }
        }

        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsSquareBrackets(StringSegment text)
    {
        if ( text.Length != 2 )
            return false;

        var source = text.Source;

        return source[text.StartIndex] == OpenedSquareBracket && source[text.StartIndex + 1] == ClosedSquareBracket;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsVariableDeclaration(StringSegment text)
    {
        if ( text.Length != 3 )
            return false;

        var source = text.Source;

        if ( source[text.StartIndex] == 'l' )
            return source[text.StartIndex + 1] == 'e' && source[text.StartIndex + 2] == 't';

        if ( source[text.StartIndex] == 'L' )
        {
            if ( source[text.StartIndex + 1] == 'E' )
                return source[text.StartIndex + 2] == 'T';

            if ( source[text.StartIndex + 1] == 'e' )
                return source[text.StartIndex + 2] == 't';
        }

        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsMacroDeclaration(StringSegment text)
    {
        if ( text.Length != 5 )
            return false;

        var source = text.Source;

        if ( source[text.StartIndex] == 'm' )
        {
            return source[text.StartIndex + 1] == 'a'
                && source[text.StartIndex + 2] == 'c'
                && source[text.StartIndex + 3] == 'r'
                && source[text.StartIndex + 4] == 'o';
        }

        if ( source[text.StartIndex] == 'M' )
        {
            if ( source[text.StartIndex + 1] == 'A' )
                return source[text.StartIndex + 2] == 'C' && source[text.StartIndex + 3] == 'R' && source[text.StartIndex + 4] == 'O';

            if ( source[text.StartIndex + 1] == 'a' )
                return source[text.StartIndex + 2] == 'c' && source[text.StartIndex + 3] == 'r' && source[text.StartIndex + 4] == 'o';
        }

        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool InterpretAsSymbol(char c)
    {
        return c != Underscore && (char.IsSymbol( c ) || char.IsPunctuation( c ));
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsReservedSymbol(char c)
    {
        return c is OpenedParenthesis or ClosedParenthesis or LineSeparator or ElementSeparator or MemberAccess;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool AreEqual(StringSegment a, char b)
    {
        return a.Length == 1 && a.Source[a.StartIndex] == b;
    }
}
