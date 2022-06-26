using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Internal
{
    // TODO:
    // symbol token validation:
    // open parenthesis must be unique
    // close parenthesis must be unique
    // inline function delimiter must be unique
    // function parameter delimiter & (number integer digit separator | number decimal point) can be the same
    // member access & (number integer digit separator | number decimal point) can be the same
    // string delimiter must be unique
    // string delimiter escape doesn't have to be unique (must be different from string delimiter though)
    // number integer digit separator & (function parameter delimiter | member access) can be the same
    // number decimal point & (function parameter delimiter | member access) can be the same
    // number scientific notification exponents must be unique (string itself can contain duplicates, that will be internally trimmed)
    // number scientific notation positive exponent operator must be unique
    // number scientific notation negative exponent operator must be unique
    // none of the tokens can be digits
    // none of the tokens can be a '_' symbol
    // none of the tokens can be a white space symbol
    //
    // operator tokens must:
    // not contain open parenthesis
    // not contain close parenthesis
    // not contain inline function delimiter
    // not contain function parameter delimiter
    // not contain member access
    // not contain string delimiter
    // not contain white spaces
    // not start with a digit
    // not be equal to '_' symbol
    internal struct MathExpressionTokenizer
    {
        private readonly string _input;
        private readonly MathExpressionTokenReader.Params _params;
        private MathExpressionTokenizerSubStream _subStream;
        private int _index;

        internal MathExpressionTokenizer(
            string input,
            MathExpressionTokenReader.Params @params)
        {
            _input = input;
            _params = @params;
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
                case TokenConstants.OpenParenthesis:
                    return MathExpressionTokenReader.ReadOpenParenthesis( _input, _index );
                case TokenConstants.CloseParenthesis:
                    return MathExpressionTokenReader.ReadCloseParenthesis( _input, _index );
                case TokenConstants.FunctionParameterSeparator:
                    return MathExpressionTokenReader.ReadFunctionParameterSeparator( _input, _index );
                case TokenConstants.InlineFunctionSeparator:
                    return MathExpressionTokenReader.ReadInlineFunctionSeparator( _input, _index );
                case TokenConstants.MemberAccess:
                    return MathExpressionTokenReader.ReadMemberAccess( _input, _index );
            }

            if ( char.IsDigit( c ) )
                return MathExpressionTokenReader.ReadNumber( _input, _index, _params );

            if ( c == _params.StringDelimiter )
                return MathExpressionTokenReader.ReadString( _input, _index, _params );

            _subStream = new MathExpressionTokenizerSubStream( _input, _index, _params );
            return ReadNextTokenFromSubStream();
        }

        private IntermediateToken ReadNextTokenFromSubStream()
        {
            Debug.Assert( ! _subStream.IsFinished, "subStream is not Finished" );

            var result = _subStream.ReadNext( _params );
            return result;
        }
    }
}
