using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal static class ExpressionTokenReader
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadOpenedParenthesis(string input, int index)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        var result = IntermediateToken.CreateOpenedParenthesis( new StringSegment( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadClosedParenthesis(string input, int index)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        var result = IntermediateToken.CreateClosedParenthesis( new StringSegment( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadOpenedSquareBracket(string input, int index)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        var result = IntermediateToken.CreateOpenedSquareBracket( new StringSegment( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadClosedSquareBracket(string input, int index)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        var result = IntermediateToken.CreateClosedSquareBracket( new StringSegment( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadAssignment(string input, int index)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        var result = IntermediateToken.CreateAssignment( new StringSegment( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadElementSeparator(string input, int index)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        var result = IntermediateToken.CreateElementSeparator( new StringSegment( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadLineSeparator(string input, int index)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        var result = IntermediateToken.CreateLineSeparator( new StringSegment( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadMemberAccess(string input, int index)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        var result = IntermediateToken.CreateMemberAccess( new StringSegment( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadString(string input, int index, ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        Assume.Equals( input[index], configuration.StringDelimiter, nameof( input ) + '[' + nameof( index ) + ']' );

        var startIndex = index++;
        var isPrevCharacterDelimiter = false;

        while ( index < input.Length )
        {
            var c = input[index];

            if ( c == configuration.StringDelimiter )
            {
                isPrevCharacterDelimiter = ! isPrevCharacterDelimiter;
                ++index;
                continue;
            }

            if ( isPrevCharacterDelimiter )
                break;

            ++index;
        }

        var result = IntermediateToken.CreateStringConstant(
            new StringSegment( input, startIndex, length: index - startIndex ) );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken? TryReadBoolean(StringSegment input)
    {
        if ( TokenConstants.IsBooleanTrue( input ) || TokenConstants.IsBooleanFalse( input ) )
            return IntermediateToken.CreateBooleanConstant( input );

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken? TryReadVariableDeclaration(StringSegment input)
    {
        if ( TokenConstants.IsVariableDeclaration( input ) )
            return IntermediateToken.CreateVariableDeclaration( input );

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken? TryReadMacroDeclaration(StringSegment input)
    {
        if ( TokenConstants.IsMacroDeclaration( input ) )
            return IntermediateToken.CreateMacroDeclaration( input );

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken? TryReadConstructs(StringSegment input, ParsedExpressionFactoryInternalConfiguration configuration)
    {
        if ( ! configuration.Constructs.TryGetValue( input, out var constructs ) )
            return null;

        return TokenConstants.AreEqual( input, TokenConstants.Assignment )
            ? IntermediateToken.CreateAssignmentWithConstructs( input, constructs )
            : IntermediateToken.CreateConstructs( input, constructs );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadNumber(string input, int index, ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );
        Assume.True( char.IsDigit( input[index] ), "Assumed input[index] to be a digit." );

        var startIndex = index++;
        var state = NumberReadingState.BeforeDecimalPoint;

        while ( index < input.Length )
        {
            var c = input[index];
            var @continue = state switch
            {
                NumberReadingState.AfterDecimalPoint =>
                    HandleAfterDecimalPointNumberReadingState( c, input, configuration, ref index, ref state ),
                NumberReadingState.AfterScientificNotationSymbol =>
                    HandleAfterScientificNotationSymbolNumberReadingState( c, ref index, ref state ),
                NumberReadingState.AfterScientificNotationOperator =>
                    HandlerAfterScientificNotationOperatorNumberReadingState( c, input, ref index ),
                _ => HandleBeforeDecimalPointNumberReadingState( c, input, configuration, ref index, ref state )
            };

            if ( ! @continue )
                break;

            ++index;
        }

        var result = IntermediateToken.CreateNumberConstant(
            new StringSegment( input, startIndex, length: index - startIndex ) );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool HandleBeforeDecimalPointNumberReadingState(
        char c,
        string input,
        ParsedExpressionFactoryInternalConfiguration configuration,
        ref int index,
        ref NumberReadingState state)
    {
        Assume.Equals( state, NumberReadingState.BeforeDecimalPoint, nameof( state ) );

        if ( char.IsDigit( c ) )
            return true;

        var prev = input[index - 1];
        if ( prev == configuration.IntegerDigitSeparator )
        {
            --index;
            return false;
        }

        if ( c == configuration.IntegerDigitSeparator )
            return true;

        if ( c == configuration.DecimalPoint )
        {
            if ( ! configuration.AllowNonIntegerNumbers )
                return false;

            state = NumberReadingState.AfterDecimalPoint;
            return true;
        }

        if ( configuration.AllowScientificNotation && configuration.ScientificNotationExponents.Contains( c ) )
        {
            state = NumberReadingState.AfterScientificNotationSymbol;
            return true;
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool HandleAfterDecimalPointNumberReadingState(
        char c,
        string input,
        ParsedExpressionFactoryInternalConfiguration configuration,
        ref int index,
        ref NumberReadingState state)
    {
        Assume.Equals( state, NumberReadingState.AfterDecimalPoint, nameof( state ) );

        if ( char.IsDigit( c ) )
            return true;

        var prev = input[index - 1];
        if ( prev == configuration.DecimalPoint )
        {
            --index;
            return false;
        }

        if ( configuration.AllowScientificNotation && configuration.ScientificNotationExponents.Contains( c ) )
        {
            state = NumberReadingState.AfterScientificNotationSymbol;
            return true;
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool HandleAfterScientificNotationSymbolNumberReadingState(char c, ref int index, ref NumberReadingState state)
    {
        Assume.Equals( state, NumberReadingState.AfterScientificNotationSymbol, nameof( state ) );

        if ( c is TokenConstants.ScientificNotationPositiveExponentOperator
                or TokenConstants.ScientificNotationNegativeExponentOperator ||
            char.IsDigit( c ) )
        {
            state = NumberReadingState.AfterScientificNotationOperator;
            return true;
        }

        --index;
        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool HandlerAfterScientificNotationOperatorNumberReadingState(char c, string input, ref int index)
    {
        if ( char.IsDigit( c ) )
            return true;

        if ( c is TokenConstants.ScientificNotationPositiveExponentOperator
            or TokenConstants.ScientificNotationNegativeExponentOperator )
        {
            index -= 2;
            return false;
        }

        var prev = input[index - 1];
        if ( prev is TokenConstants.ScientificNotationPositiveExponentOperator
            or TokenConstants.ScientificNotationNegativeExponentOperator )
        {
            index -= 2;
            return false;
        }

        return false;
    }

    private enum NumberReadingState : byte
    {
        BeforeDecimalPoint = 0,
        AfterDecimalPoint = 1,
        AfterScientificNotationSymbol = 2,
        AfterScientificNotationOperator = 3
    }
}
