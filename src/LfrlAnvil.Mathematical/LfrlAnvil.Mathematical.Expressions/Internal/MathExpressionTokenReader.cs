using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal static class MathExpressionTokenReader
{
    internal sealed class Params
    {
        internal Params(IReadOnlyDictionary<StringSlice, MathExpressionTokenSet> tokenSets, ITokenizerConfiguration configuration)
        {
            TokenSets = tokenSets;
            DecimalPoint = configuration.DecimalPoint;
            IntegerDigitSeparator = configuration.IntegerDigitSeparator;
            ScientificNotationExponents = configuration.ScientificNotationExponents;
            AllowScientificNotation = configuration.AllowScientificNotation;
            AllowNonIntegerValues = configuration.AllowNonIntegerValues;
            StringDelimiter = configuration.StringDelimiter;
        }

        internal IReadOnlyDictionary<StringSlice, MathExpressionTokenSet> TokenSets { get; }
        internal char DecimalPoint { get; }
        internal char IntegerDigitSeparator { get; }
        internal string ScientificNotationExponents { get; }
        internal bool AllowScientificNotation { get; }
        internal bool AllowNonIntegerValues { get; }
        internal char StringDelimiter { get; }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadOpenParenthesis(string input, int index)
    {
        Debug.Assert( index < input.Length, "index < input.Length" );
        var result = IntermediateToken.CreateOpenParenthesis( StringSlice.Create( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadCloseParenthesis(string input, int index)
    {
        Debug.Assert( index < input.Length, "index < input.Length" );
        var result = IntermediateToken.CreateCloseParenthesis( StringSlice.Create( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadFunctionParameterSeparator(string input, int index)
    {
        Debug.Assert( index < input.Length, "index < input.Length" );
        var result = IntermediateToken.CreateFunctionParameterSeparator( StringSlice.Create( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadInlineFunctionSeparator(string input, int index)
    {
        Debug.Assert( index < input.Length, "index < input.Length" );
        var result = IntermediateToken.CreateInlineFunctionSeparator( StringSlice.Create( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadMemberAccess(string input, int index)
    {
        Debug.Assert( index < input.Length, "index < input.Length" );
        var result = IntermediateToken.CreateMemberAccess( StringSlice.Create( input, index, length: 1 ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadString(string input, int index, Params @params)
    {
        Debug.Assert( index < input.Length, "index < input.Length" );
        Debug.Assert( input[index] == @params.StringDelimiter, "input[index] is StringDelimiter" );

        var startIndex = index++;
        var isPrevCharacterDelimiter = false;

        while ( index < input.Length )
        {
            var c = input[index];

            if ( c == @params.StringDelimiter )
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
            StringSlice.Create( input, startIndex, length: index - startIndex ) );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken? TryReadBoolean(StringSlice input)
    {
        if ( TokenConstants.IsBooleanTrue( input ) || TokenConstants.IsBooleanFalse( input ) )
            return IntermediateToken.CreateBooleanConstant( input );

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken? TryReadTokenGroup(StringSlice input, Params @params)
    {
        if ( @params.TokenSets.TryGetValue( input, out var group ) )
            return IntermediateToken.CreateTokenSet( input, group );

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken ReadNumber(string input, int index, Params @params)
    {
        Debug.Assert( index < input.Length, "index < input.Length" );
        Debug.Assert( char.IsDigit( input[index] ), "input[index] is a digit" );

        var startIndex = index++;
        var state = NumberReadingState.BeforeDecimalPoint;

        while ( index < input.Length )
        {
            var c = input[index];
            var @continue = state switch
            {
                NumberReadingState.AfterDecimalPoint =>
                    HandleAfterDecimalPointNumberReadingState( c, input, @params, ref index, ref state ),
                NumberReadingState.AfterScientificNotationSymbol =>
                    HandleAfterScientificNotationSymbolNumberReadingState( c, ref index, ref state ),
                NumberReadingState.AfterScientificNotationOperator =>
                    HandlerAfterScientificNotationOperatorNumberReadingState( c, input, ref index ),
                _ => HandleBeforeDecimalPointNumberReadingState( c, input, @params, ref index, ref state )
            };

            if ( ! @continue )
                break;

            ++index;
        }

        var result = IntermediateToken.CreateNumberConstant(
            StringSlice.Create( input, startIndex, length: index - startIndex ) );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool HandleBeforeDecimalPointNumberReadingState(
        char c,
        string input,
        Params @params,
        ref int index,
        ref NumberReadingState state)
    {
        Debug.Assert( state == NumberReadingState.BeforeDecimalPoint, "state is BeforeDecimalPoint" );

        if ( char.IsDigit( c ) )
            return true;

        var prev = input[index - 1];
        if ( prev == @params.IntegerDigitSeparator )
        {
            --index;
            return false;
        }

        if ( c == @params.IntegerDigitSeparator )
            return true;

        if ( c == @params.DecimalPoint )
        {
            if ( ! @params.AllowNonIntegerValues )
                return false;

            state = NumberReadingState.AfterDecimalPoint;
            return true;
        }

        if ( @params.AllowScientificNotation && @params.ScientificNotationExponents.Contains( c ) )
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
        Params @params,
        ref int index,
        ref NumberReadingState state)
    {
        Debug.Assert( state == NumberReadingState.AfterDecimalPoint, "state is AfterDecimalPoint" );

        if ( char.IsDigit( c ) )
            return true;

        var prev = input[index - 1];
        if ( prev == @params.DecimalPoint )
        {
            --index;
            return false;
        }

        if ( @params.AllowScientificNotation && @params.ScientificNotationExponents.Contains( c ) )
        {
            state = NumberReadingState.AfterScientificNotationSymbol;
            return true;
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool HandleAfterScientificNotationSymbolNumberReadingState(char c, ref int index, ref NumberReadingState state)
    {
        Debug.Assert( state == NumberReadingState.AfterScientificNotationSymbol, "state is AfterScientificNotationSymbol" );

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
