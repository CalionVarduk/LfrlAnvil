using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal struct ExpressionTokenizerSubStream
{
    private readonly int _length;
    private readonly int _endIndex;
    private StringSlice _remaining;
    private StringSlice _skippedSymbols;
    private StringSlice _remainingSymbols;
    private IntermediateToken? _bufferedToken;
    private State _state;

    internal ExpressionTokenizerSubStream(string input, int index, ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.IsLessThan( index, input.Length, nameof( index ) );

        var startIndex = index++;
        _state = State.Started;

        while ( index < input.Length )
        {
            var c = input[index];

            if ( char.IsWhiteSpace( c ) )
                break;

            if ( c is TokenConstants.OpenedParenthesis or
                    TokenConstants.ClosedParenthesis or
                    TokenConstants.InlineFunctionSeparator ||
                c == configuration.StringDelimiter )
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
    internal IntermediateToken ReadNext(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.NotEquals( _state, State.Finished, nameof( _state ) );

        var result = _state switch
        {
            State.ReadingNonSymbols => HandleReadingNonSymbolsState( configuration ),
            State.StartReadingSymbols => HandleStartReadingSymbolsState( configuration ),
            State.SearchingForTokenSymbols => HandleSearchingForTokenSymbolsState( configuration ),
            State.StoringBufferedToken => HandleStoringBufferedTokenState(),
            _ => HandleStartedState( configuration )
        };

        return result;
    }

    private IntermediateToken HandleStartedState(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.Equals( _state, State.Started, nameof( _state ) );

        var result = ExpressionTokenReader.TryReadConstructs( _remaining, configuration );
        if ( result is not null )
        {
            Finish();
            return result.Value;
        }

        _state = State.ReadingNonSymbols;
        return HandleReadingNonSymbolsState( configuration );
    }

    private IntermediateToken HandleReadingNonSymbolsState(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.Equals( _state, State.ReadingNonSymbols, nameof( _state ) );
        Assume.IsNull( _bufferedToken, nameof( _bufferedToken ) );

        var input = _remaining.Source;
        var index = _remaining.StartIndex;

        Assume.IsLessThan( index, _endIndex, nameof( index ) );

        var c = input[index];

        if ( char.IsDigit( c ) )
        {
            var number = ExpressionTokenReader.ReadNumber( input, index, configuration );
            _remaining = _remaining.Slice( number.Symbol.Length );
            SetStateOrFinish( State.ReadingNonSymbols );
            return number;
        }

        if ( TokenConstants.InterpretAsSymbol( c ) )
        {
            _state = State.StartReadingSymbols;
            return HandleStartReadingSymbolsState( configuration );
        }

        ++index;
        while ( index < _endIndex )
        {
            c = input[index];
            if ( TokenConstants.InterpretAsSymbol( c ) )
                break;

            ++index;
        }

        var readCount = index - _remaining.StartIndex;
        var slice = _remaining.Slice( 0, readCount );
        _remaining = _remaining.Slice( readCount );
        SetStateOrFinish( State.StartReadingSymbols );

        var boolean = ExpressionTokenReader.TryReadBoolean( slice );
        if ( boolean is not null )
            return boolean.Value;

        if ( slice.Length != _length )
        {
            var tokenGroup = ExpressionTokenReader.TryReadConstructs( slice, configuration );
            if ( tokenGroup is not null )
                return tokenGroup.Value;
        }

        return IntermediateToken.CreateArgument( slice );
    }

    private IntermediateToken HandleStartReadingSymbolsState(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.Equals( _state, State.StartReadingSymbols, nameof( _state ) );
        Assume.IsNull( _bufferedToken, nameof( _bufferedToken ) );

        var input = _remaining.Source;
        var index = _remaining.StartIndex + 1;

        Assume.IsLessThan( _remaining.StartIndex, _endIndex, nameof( _remaining.StartIndex ) );
        Assume.True( TokenConstants.InterpretAsSymbol( _remaining[0] ), "Assumed first remaining character to be a valid symbol." );
        Assume.IsEmpty( _remainingSymbols, nameof( _remainingSymbols ) );
        Assume.IsEmpty( _skippedSymbols, nameof( _skippedSymbols ) );

        while ( index < _endIndex )
        {
            var c = input[index];
            if ( ! TokenConstants.InterpretAsSymbol( c ) )
                break;

            ++index;
        }

        var readCount = index - _remaining.StartIndex;
        _remainingSymbols = _remaining.Slice( 0, readCount );
        _remaining = _remaining.Slice( readCount );
        _state = State.SearchingForTokenSymbols;
        return HandleSearchingForTokenSymbolsState( configuration );
    }

    private IntermediateToken HandleSearchingForTokenSymbolsState(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.Equals( _state, State.SearchingForTokenSymbols, nameof( _state ) );
        Assume.IsNotEmpty( _remainingSymbols, nameof( _remainingSymbols ) );
        Assume.IsEmpty( _skippedSymbols, nameof( _skippedSymbols ) );
        Assume.IsNull( _bufferedToken, nameof( _bufferedToken ) );

        var token = ScanForTokenWithinRemainingSymbols( configuration );
        if ( token is not null )
        {
            if ( _remainingSymbols.Length == 0 )
                SetStateOrFinish( State.ReadingNonSymbols );

            return token.Value;
        }

        _skippedSymbols = _remainingSymbols.Slice( 0, 1 );
        _remainingSymbols = _remainingSymbols.Slice( 1 );

        if ( _remainingSymbols.Length != 0 )
            return HandleSearchingForTokenSymbolsWithSkippedState( configuration );

        SetStateOrFinish( State.ReadingNonSymbols );
        return ReadSkippedSymbols();
    }

    private IntermediateToken HandleSearchingForTokenSymbolsWithSkippedState(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.Equals( _state, State.SearchingForTokenSymbols, nameof( _state ) );
        Assume.IsNotEmpty( _skippedSymbols, nameof( _skippedSymbols ) );
        Assume.IsNull( _bufferedToken, nameof( _bufferedToken ) );

        do
        {
            _bufferedToken = ScanForTokenWithinRemainingSymbols( configuration );
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
        Assume.Equals( _state, State.StoringBufferedToken, nameof( _state ) );
        Assume.IsEmpty( _skippedSymbols, nameof( _skippedSymbols ) );
        Assume.IsNotNull( _bufferedToken, nameof( _bufferedToken ) );

        var result = _bufferedToken.Value;
        _bufferedToken = null;

        if ( _remainingSymbols.Length == 0 )
            SetStateOrFinish( State.ReadingNonSymbols );
        else
            _state = State.SearchingForTokenSymbols;

        return result;
    }

    private IntermediateToken? ScanForTokenWithinRemainingSymbols(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        Assume.Equals( _state, State.SearchingForTokenSymbols, nameof( _state ) );
        Assume.IsNotEmpty( _remainingSymbols, nameof( _remainingSymbols ) );

        for ( var length = _remainingSymbols.Length; length > 0; --length )
        {
            var slice = _remainingSymbols.Slice( 0, length );
            var constructs = ExpressionTokenReader.TryReadConstructs( slice, configuration );
            if ( constructs is null )
                continue;

            _remainingSymbols = _remainingSymbols.Slice( length );
            return constructs.Value;
        }

        var first = _remainingSymbols[0];
        switch ( first )
        {
            case TokenConstants.ElementSeparator:
            {
                var startIndex = _remainingSymbols.StartIndex;
                _remainingSymbols = _remainingSymbols.Slice( 1 );
                return ExpressionTokenReader.ReadElementSeparator( _remainingSymbols.Source, startIndex );
            }

            case TokenConstants.MemberAccess:
            {
                var startIndex = _remainingSymbols.StartIndex;
                _remainingSymbols = _remainingSymbols.Slice( 1 );
                return ExpressionTokenReader.ReadMemberAccess( _remainingSymbols.Source, startIndex );
            }
        }

        return null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private IntermediateToken ReadSkippedSymbols()
    {
        Assume.IsNotEmpty( _skippedSymbols, nameof( _skippedSymbols ) );

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
