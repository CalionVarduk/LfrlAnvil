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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal static class TokenValidation
{
    [Pure]
    internal static bool IsValidLocalTermName(StringSegment token, char stringDelimiter)
    {
        if ( token.Length == 0 )
            return false;

        var source = token.Source;
        var index = token.StartIndex;
        var c = source[index];

        if ( c == stringDelimiter )
            return false;

        if ( c != TokenConstants.Underscore && ! char.IsLetter( c ) )
            return false;

        ++index;
        var endIndex = token.EndIndex;

        while ( index < endIndex )
        {
            c = source[index];

            if ( c == stringDelimiter )
                return false;

            if ( c != TokenConstants.Underscore && ! char.IsLetterOrDigit( c ) )
                return false;

            ++index;
        }

        return true;
    }

    [Pure]
    internal static bool IsValidConstructSymbol(StringSegment token, char stringDelimiter)
    {
        if ( token.Length == 0 )
            return false;

        if ( TokenConstants.IsBooleanTrue( token )
            || TokenConstants.IsBooleanFalse( token )
            || TokenConstants.IsVariableDeclaration( token )
            || TokenConstants.IsMacroDeclaration( token )
            || TokenConstants.AreEqual( token, TokenConstants.OpenedSquareBracket )
            || TokenConstants.AreEqual( token, TokenConstants.ClosedSquareBracket )
            || TokenConstants.IsSquareBrackets( token ) )
            return false;

        var source = token.Source;
        var index = token.StartIndex;
        var c = source[index];

        if ( c == stringDelimiter || TokenConstants.IsReservedSymbol( c ) )
            return false;

        if ( c == TokenConstants.Underscore )
        {
            if ( token.Length == 1 )
                return false;
        }
        else if ( ! char.IsLetter( c ) && ! char.IsSymbol( c ) && ! char.IsPunctuation( c ) )
            return false;

        ++index;
        var endIndex = token.EndIndex;

        while ( index < endIndex )
        {
            c = source[index];

            if ( c == stringDelimiter || TokenConstants.IsReservedSymbol( c ) )
                return false;

            if ( c != TokenConstants.Underscore && ! char.IsLetterOrDigit( c ) && ! char.IsSymbol( c ) && ! char.IsPunctuation( c ) )
                return false;

            ++index;
        }

        return true;
    }

    [Pure]
    internal static bool IsNumberSymbolValid(char symbol)
    {
        return symbol != TokenConstants.ScientificNotationPositiveExponentOperator
            && symbol != TokenConstants.ScientificNotationNegativeExponentOperator
            && symbol != TokenConstants.OpenedParenthesis
            && symbol != TokenConstants.ClosedParenthesis
            && symbol != TokenConstants.OpenedSquareBracket
            && symbol != TokenConstants.ClosedSquareBracket
            && symbol != TokenConstants.LineSeparator
            && symbol != TokenConstants.Assignment
            && (char.IsLetter( symbol ) || char.IsSymbol( symbol ) || char.IsPunctuation( symbol ));
    }

    [Pure]
    internal static bool IsStringDelimiterSymbolValid(char symbol)
    {
        return symbol != TokenConstants.Underscore
            && symbol != TokenConstants.OpenedSquareBracket
            && symbol != TokenConstants.ClosedSquareBracket
            && symbol != TokenConstants.ScientificNotationPositiveExponentOperator
            && symbol != TokenConstants.ScientificNotationNegativeExponentOperator
            && symbol != TokenConstants.Assignment
            && ! TokenConstants.IsReservedSymbol( symbol )
            && (char.IsLetter( symbol ) || char.IsSymbol( symbol ) || char.IsPunctuation( symbol ));
    }

    [Pure]
    internal static bool IsExponentSymbolValid(char symbol)
    {
        return char.IsLetter( symbol );
    }
}
