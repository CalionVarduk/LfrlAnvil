using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal struct MathExpressionTokenizerSubStream
{
    private readonly int _length;
    private readonly int _endIndex;
    private StringSlice _remaining;
    private StringSlice _skippedSymbols;
    private StringSlice _remainingSymbols;
    private IntermediateToken? _bufferedToken;
    private State _state;

    internal MathExpressionTokenizerSubStream(string input, int index, MathExpressionTokenReader.Params @params)
    {
        Debug.Assert( index < input.Length, "index < input.Length" );

        var startIndex = index++;
        _state = State.Started;

        while ( index < input.Length )
        {
            var c = input[index];

            if ( char.IsWhiteSpace( c ) )
                break;

            if ( c is TokenConstants.OpenParenthesis or
                    TokenConstants.CloseParenthesis or
                    TokenConstants.InlineFunctionSeparator ||
                c == @params.StringDelimiter )
                break;

            ++index;
        }

        _endIndex = index;
        _length = _endIndex - startIndex;
        _remaining = StringSlice.Create( input, startIndex, _length );
        _skippedSymbols = _remaining.Slice( 0, 0 );
        _remainingSymbols = _skippedSymbols;
        _bufferedToken = null;
    }

    public bool IsFinished => _state == State.Finished;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IntermediateToken ReadNext(MathExpressionTokenReader.Params @params)
    {
        Debug.Assert( _state != State.Finished, "state is not Finished" );

        var result = _state switch
        {
            State.ReadingNonSymbols => HandleReadingNonSymbolsState( @params ),
            State.StartReadingSymbols => HandleStartReadingSymbolsState( @params ),
            State.SearchingForTokenSymbols => HandleSearchingForTokenSymbolsState( @params ),
            State.StoringBufferedToken => HandleStoringBufferedTokenState(),
            _ => HandleStartedState( @params )
        };

        return result;
    }

    private IntermediateToken HandleStartedState(MathExpressionTokenReader.Params @params)
    {
        Debug.Assert( _state == State.Started, "state is Started" );

        var result = MathExpressionTokenReader.TryReadTokenGroup( _remaining, @params );
        if ( result is not null )
        {
            Finish();
            return result.Value;
        }

        _state = State.ReadingNonSymbols;
        return HandleReadingNonSymbolsState( @params );
    }

    private IntermediateToken HandleReadingNonSymbolsState(MathExpressionTokenReader.Params @params)
    {
        Debug.Assert( _state == State.ReadingNonSymbols, "state is ReadingNonSymbols" );
        Debug.Assert( _bufferedToken is null, "bufferedToken is null" );

        var input = _remaining.Source;
        var index = _remaining.StartIndex;

        Debug.Assert( index < _endIndex, "index < endIndex" );

        var c = input[index];

        if ( char.IsDigit( c ) )
        {
            var number = MathExpressionTokenReader.ReadNumber( input, index, @params );
            _remaining = _remaining.Slice( number.Symbol.Length );
            SetStateOrFinish( State.ReadingNonSymbols );
            return number;
        }

        if ( TokenConstants.IsValidSymbol( c ) )
        {
            _state = State.StartReadingSymbols;
            return HandleStartReadingSymbolsState( @params );
        }

        ++index;
        while ( index < _endIndex )
        {
            c = input[index];
            if ( TokenConstants.IsValidSymbol( c ) )
                break;

            ++index;
        }

        var readCount = index - _remaining.StartIndex;
        var slice = _remaining.Slice( 0, readCount );
        _remaining = _remaining.Slice( readCount );
        SetStateOrFinish( State.StartReadingSymbols );

        var boolean = MathExpressionTokenReader.TryReadBoolean( slice );
        if ( boolean is not null )
            return boolean.Value;

        if ( slice.Length != _length )
        {
            var tokenGroup = MathExpressionTokenReader.TryReadTokenGroup( slice, @params );
            if ( tokenGroup is not null )
                return tokenGroup.Value;
        }

        return IntermediateToken.CreateArgument( slice );
    }

    private IntermediateToken HandleStartReadingSymbolsState(MathExpressionTokenReader.Params @params)
    {
        Debug.Assert( _state == State.StartReadingSymbols, "state is StartReadingSymbols" );
        Debug.Assert( _bufferedToken is null, "bufferedToken is null" );

        var input = _remaining.Source;
        var index = _remaining.StartIndex + 1;

        Debug.Assert( _remaining.StartIndex < _endIndex, "startIndex < endIndex" );
        Debug.Assert( TokenConstants.IsValidSymbol( _remaining[0] ), "remaining[0] is a valid symbol" );
        Debug.Assert( _remainingSymbols.Length == 0, "remainingSymbols is empty" );
        Debug.Assert( _skippedSymbols.Length == 0, "skippedSymbols is empty" );

        while ( index < _endIndex )
        {
            var c = input[index];
            if ( ! TokenConstants.IsValidSymbol( c ) )
                break;

            ++index;
        }

        var readCount = index - _remaining.StartIndex;
        _remainingSymbols = _remaining.Slice( 0, readCount );
        _remaining = _remaining.Slice( readCount );
        _state = State.SearchingForTokenSymbols;
        return HandleSearchingForTokenSymbolsState( @params );
    }

    private IntermediateToken HandleSearchingForTokenSymbolsState(MathExpressionTokenReader.Params @params)
    {
        Debug.Assert( _state == State.SearchingForTokenSymbols, "state is SearchingForTokenSymbols" );
        Debug.Assert( _remainingSymbols.Length > 0, "remainingSymbols is not empty" );
        Debug.Assert( _skippedSymbols.Length == 0, "skippedSymbols is empty" );
        Debug.Assert( _bufferedToken is null, "bufferedToken is null" );

        var token = ScanForTokenWithinRemainingSymbols( @params );
        if ( token is not null )
        {
            if ( _remainingSymbols.Length == 0 )
                SetStateOrFinish( State.ReadingNonSymbols );

            return token.Value;
        }

        _skippedSymbols = _remainingSymbols.Slice( 0, 1 );
        _remainingSymbols = _remainingSymbols.Slice( 1 );

        if ( _remainingSymbols.Length != 0 )
            return HandleSearchingForTokenSymbolsWithSkippedState( @params );

        SetStateOrFinish( State.ReadingNonSymbols );
        return ReadSkippedSymbols();
    }

    private IntermediateToken HandleSearchingForTokenSymbolsWithSkippedState(MathExpressionTokenReader.Params @params)
    {
        Debug.Assert( _state == State.SearchingForTokenSymbols, "state is SearchingForTokenSymbols" );
        Debug.Assert( _skippedSymbols.Length > 0, "skippedSymbols is not empty" );
        Debug.Assert( _bufferedToken is null, "bufferedToken is null" );

        do
        {
            _bufferedToken = ScanForTokenWithinRemainingSymbols( @params );
            if ( _bufferedToken is not null )
            {
                _state = State.StoringBufferedToken;
                return ReadSkippedSymbols();
            }

            _skippedSymbols = _remainingSymbols.Slice( -_skippedSymbols.Length, _skippedSymbols.Length + 1 );
            _remainingSymbols = _remainingSymbols.Slice( 1 );
        }
        while ( _remainingSymbols.Length > 0 );

        SetStateOrFinish( State.ReadingNonSymbols );
        return ReadSkippedSymbols();
    }

    private IntermediateToken HandleStoringBufferedTokenState()
    {
        Debug.Assert( _state == State.StoringBufferedToken, "state is StoringBufferedToken" );
        Debug.Assert( _skippedSymbols.Length == 0, "skippedSymbols is empty" );
        Debug.Assert( _bufferedToken is not null, "bufferedToken is not null" );

        var result = _bufferedToken.Value;
        _bufferedToken = null;

        if ( _remainingSymbols.Length == 0 )
            SetStateOrFinish( State.ReadingNonSymbols );
        else
            _state = State.SearchingForTokenSymbols;

        return result;
    }

    private IntermediateToken? ScanForTokenWithinRemainingSymbols(MathExpressionTokenReader.Params @params)
    {
        Debug.Assert( _state is State.SearchingForTokenSymbols, "state is SearchingForTokenSymbols" );
        Debug.Assert( _remainingSymbols.Length > 0, "remainingSymbols is not empty" );

        for ( var length = _remainingSymbols.Length; length > 0; --length )
        {
            var slice = _remainingSymbols.Slice( 0, length );
            var tokenGroup = MathExpressionTokenReader.TryReadTokenGroup( slice, @params );
            if ( tokenGroup is null )
                continue;

            _remainingSymbols = _remainingSymbols.Slice( length );
            return tokenGroup.Value;
        }

        var first = _remainingSymbols[0];
        switch ( first )
        {
            case TokenConstants.FunctionParameterSeparator:
            {
                var startIndex = _remainingSymbols.StartIndex;
                _remainingSymbols = _remainingSymbols.Slice( 1 );
                return MathExpressionTokenReader.ReadFunctionParameterSeparator( _remainingSymbols.Source, startIndex );
            }

            case TokenConstants.MemberAccess:
            {
                var startIndex = _remainingSymbols.StartIndex;
                _remainingSymbols = _remainingSymbols.Slice( 1 );
                return MathExpressionTokenReader.ReadMemberAccess( _remainingSymbols.Source, startIndex );
            }
        }

        return null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private IntermediateToken ReadSkippedSymbols()
    {
        Debug.Assert( _skippedSymbols.Length > 0, "skippedSymbols is not empty" );

        var slice = _skippedSymbols;
        _skippedSymbols = _skippedSymbols.Slice( 0, 0 );
        return IntermediateToken.CreateArgument( slice );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Finish()
    {
        _remaining = StringSlice.Create( _remaining.Source, _endIndex, length: 0 );
        _state = State.Finished;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetStateOrFinish(State state)
    {
        _state = _remaining.Length == 0 ? State.Finished : state;
    }

    private enum State : byte
    {
        Finished = 0,
        Started = 1,
        ReadingNonSymbols = 2,
        StartReadingSymbols = 3,
        SearchingForTokenSymbols = 4,
        StoringBufferedToken = 5
    }
}