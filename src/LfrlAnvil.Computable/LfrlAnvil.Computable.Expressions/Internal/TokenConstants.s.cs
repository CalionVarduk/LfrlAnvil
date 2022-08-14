using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal static class TokenConstants
{
    internal const char Underscore = '_';
    internal const char OpenedParenthesis = '(';
    internal const char ClosedParenthesis = ')';
    internal const char InlineFunctionSeparator = ';';
    internal const char ElementSeparator = ',';
    internal const char MemberAccess = '.';
    internal const char ScientificNotationPositiveExponentOperator = '+';
    internal const char ScientificNotationNegativeExponentOperator = '-';

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsBooleanTrue(StringSlice text)
    {
        if ( text.Length != 4 )
            return false;

        if ( text.Source[text.StartIndex] == 't' )
        {
            return text.Source[text.StartIndex + 1] == 'r' &&
                text.Source[text.StartIndex + 2] == 'u' &&
                text.Source[text.StartIndex + 3] == 'e';
        }

        if ( text.Source[text.StartIndex] == 'T' )
        {
            if ( text.Source[text.StartIndex + 1] == 'R' )
                return text.Source[text.StartIndex + 2] == 'U' && text.Source[text.StartIndex + 3] == 'E';

            if ( text.Source[text.StartIndex + 1] == 'r' )
                return text.Source[text.StartIndex + 2] == 'u' && text.Source[text.StartIndex + 3] == 'e';
        }

        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsBooleanFalse(StringSlice text)
    {
        if ( text.Length != 5 )
            return false;

        if ( text.Source[text.StartIndex] == 'f' )
        {
            return text.Source[text.StartIndex + 1] == 'a' &&
                text.Source[text.StartIndex + 2] == 'l' &&
                text.Source[text.StartIndex + 3] == 's' &&
                text.Source[text.StartIndex + 4] == 'e';
        }

        if ( text.Source[text.StartIndex] == 'F' )
        {
            if ( text.Source[text.StartIndex + 1] == 'A' )
                return text.Source[text.StartIndex + 2] == 'L' &&
                    text.Source[text.StartIndex + 3] == 'S' &&
                    text.Source[text.StartIndex + 4] == 'E';

            if ( text.Source[text.StartIndex + 1] == 'a' )
                return text.Source[text.StartIndex + 2] == 'l' &&
                    text.Source[text.StartIndex + 3] == 's' &&
                    text.Source[text.StartIndex + 4] == 'e';
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
        return c is OpenedParenthesis or ClosedParenthesis or InlineFunctionSeparator or ElementSeparator or MemberAccess;
    }
}
