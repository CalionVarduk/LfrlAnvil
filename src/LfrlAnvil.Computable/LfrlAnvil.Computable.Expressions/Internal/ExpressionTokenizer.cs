// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal struct ExpressionTokenizer
{
    private readonly string _input;
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;
    private ExpressionTokenizerSubStream _subStream;
    private int _index;

    internal ExpressionTokenizer(string input, ParsedExpressionFactoryInternalConfiguration configuration)
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
        Assume.IsLessThan( _index, _input.Length );

        if ( ! _subStream.IsFinished )
            return ReadNextTokenFromSubStream();

        var c = _input[_index];

        switch ( c )
        {
            case TokenConstants.OpenedParenthesis:
                return ExpressionTokenReader.ReadOpenedParenthesis( _input, _index );
            case TokenConstants.ClosedParenthesis:
                return ExpressionTokenReader.ReadClosedParenthesis( _input, _index );
            case TokenConstants.ElementSeparator:
                return ExpressionTokenReader.ReadElementSeparator( _input, _index );
            case TokenConstants.LineSeparator:
                return ExpressionTokenReader.ReadLineSeparator( _input, _index );
            case TokenConstants.MemberAccess:
                return ExpressionTokenReader.ReadMemberAccess( _input, _index );
        }

        if ( char.IsDigit( c ) )
            return ExpressionTokenReader.ReadNumber( _input, _index, _configuration );

        if ( c == _configuration.StringDelimiter )
            return ExpressionTokenReader.ReadString( _input, _index, _configuration );

        _subStream = new ExpressionTokenizerSubStream( _input, _index, _configuration );
        return ReadNextTokenFromSubStream();
    }

    private IntermediateToken ReadNextTokenFromSubStream()
    {
        Assume.False( _subStream.IsFinished );
        var result = _subStream.ReadNext( _configuration );
        return result;
    }
}
