using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal struct MathExpressionTokenizer
{
    private readonly string _input;
    private readonly MathExpressionFactoryInternalConfiguration _configuration;
    private MathExpressionTokenizerSubStream _subStream;
    private int _index;

    internal MathExpressionTokenizer(string input, MathExpressionFactoryInternalConfiguration configuration)
    {
        _input = input;
        _configuration = configuration;
        _subStream = default;
        _index = 0;
        SkipWhiteSpaces();
    }

    public bool ReadNextToken(out IntermediateToken token)
    {
        if ( _index == _input.Length )
        {
            token = default;
            return false;
        }

        token = ReadNextToken();
        _index += token.Symbol.Length;
        SkipWhiteSpaces();
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SkipWhiteSpaces()
    {
        if ( ! _subStream.IsFinished )
            return;

        while ( _index < _input.Length )
        {
            var c = _input[_index];
            if ( ! char.IsWhiteSpace( c ) )
                return;

            ++_index;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private IntermediateToken ReadNextToken()
    {
        Debug.Assert( _index < _input.Length, "index < input.Length" );

        if ( ! _subStream.IsFinished )
            return ReadNextTokenFromSubStream();

        var c = _input[_index];

        switch ( c )
        {
            case TokenConstants.OpenedParenthesis:
                return MathExpressionTokenReader.ReadOpenedParenthesis( _input, _index );
            case TokenConstants.ClosedParenthesis:
                return MathExpressionTokenReader.ReadClosedParenthesis( _input, _index );
            case TokenConstants.FunctionParameterSeparator:
                return MathExpressionTokenReader.ReadFunctionParameterSeparator( _input, _index );
            case TokenConstants.InlineFunctionSeparator:
                return MathExpressionTokenReader.ReadInlineFunctionSeparator( _input, _index );
            case TokenConstants.MemberAccess:
                return MathExpressionTokenReader.ReadMemberAccess( _input, _index );
        }

        if ( char.IsDigit( c ) )
            return MathExpressionTokenReader.ReadNumber( _input, _index, _configuration );

        if ( c == _configuration.StringDelimiter )
            return MathExpressionTokenReader.ReadString( _input, _index, _configuration );

        _subStream = new MathExpressionTokenizerSubStream( _input, _index, _configuration );
        return ReadNextTokenFromSubStream();
    }

    private IntermediateToken ReadNextTokenFromSubStream()
    {
        Debug.Assert( ! _subStream.IsFinished, "subStream is not Finished" );

        var result = _subStream.ReadNext( _configuration );
        return result;
    }
}
