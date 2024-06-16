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

using LfrlAnvil.Computable.Expressions.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Contains default symbols and precedences of constructs.
/// </summary>
public static class ParsedExpressionConstructDefaults
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public const string AddSymbol = "+";
    public const string SubtractSymbol = "-";
    public const string MultiplySymbol = "*";
    public const string DivideSymbol = "/";
    public const string ModuloSymbol = "mod";
    public const string CoalesceSymbol = "??";
    public const string NegateSymbol = SubtractSymbol;
    public const string EqualToSymbol = "==";
    public const string NotEqualToSymbol = "!=";
    public const string GreaterThanSymbol = ">";
    public const string LessThanSymbol = "<";
    public const string GreaterThanOrEqualToSymbol = ">=";
    public const string LessThanOrEqualToSymbol = "<=";
    public const string CompareSymbol = "<=>";
    public const string AndSymbol = "and";
    public const string OrSymbol = "or";
    public const string NotSymbol = "not";
    public const string BitwiseNotSymbol = "~";
    public const string BitwiseAndSymbol = "&";
    public const string BitwiseOrSymbol = "|";
    public const string BitwiseXorSymbol = "^";
    public const string BitwiseLeftShiftSymbol = "<<";
    public const string BitwiseRightShiftSymbol = ">>";

    public const int DefaultUnaryPrecedence = 1;
    public const int TypeConverterPrecedence = DefaultUnaryPrecedence;
    public const int NegatePrecedence = DefaultUnaryPrecedence;
    public const int NotPrecedence = DefaultUnaryPrecedence;
    public const int BitwiseNotPrecedence = DefaultUnaryPrecedence;
    public const int MultiplyPrecedence = 2;
    public const int DividePrecedence = 2;
    public const int ModuloPrecedence = 2;
    public const int AddPrecedence = 3;
    public const int SubtractPrecedence = 3;
    public const int BitwiseLeftShiftPrecedence = 4;
    public const int BitwiseRightShiftPrecedence = 4;
    public const int ComparePrecedence = 5;
    public const int GreaterThanPrecedence = 6;
    public const int LessThanPrecedence = 6;
    public const int GreaterThanOrEqualToPrecedence = 6;
    public const int LessThanOrEqualToPrecedence = 6;
    public const int EqualToPrecedence = 7;
    public const int NotEqualToPrecedence = 7;
    public const int BitwiseAndPrecedence = 8;
    public const int BitwiseXorPrecedence = 9;
    public const int BitwiseOrPrecedence = 10;
    public const int AndPrecedence = 11;
    public const int OrPrecedence = 12;
    public const int CoalescePrecedence = 13;

    public const string IfSymbol = "if";
    public const string SwitchCaseSymbol = "case";
    public const string SwitchSymbol = "switch";
    public const string ThrowSymbol = "throw";

    public const string MemberAccessSymbol = "MEMBER_ACCESS";
    public const string IndexerCallSymbol = "INDEXER_CALL";
    public const string MethodCallSymbol = "METHOD_CALL";
    public const string CtorCallSymbol = "CTOR_CALL";
    public const string InvokeSymbol = "INVOKE";
    public const string MakeArraySymbol = "MAKE_ARRAY";

    public static readonly ParsedExpressionTypeDefinitionSymbols BooleanTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "boolean" );

    public static readonly ParsedExpressionTypeDefinitionSymbols DecimalTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "decimal" )
        .SetPostfixTypeConverter( "m" );

    public static readonly ParsedExpressionTypeDefinitionSymbols DoubleTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "double" );

    public static readonly ParsedExpressionTypeDefinitionSymbols FloatTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "float" )
        .SetPostfixTypeConverter( "f" );

    public static readonly ParsedExpressionTypeDefinitionSymbols Int32TypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "int32" )
        .SetPostfixTypeConverter( "i" );

    public static readonly ParsedExpressionTypeDefinitionSymbols Int64TypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "int64" )
        .SetPostfixTypeConverter( "l" );

    public static readonly ParsedExpressionTypeDefinitionSymbols BigIntTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "bigint" );

    public static readonly ParsedExpressionTypeDefinitionSymbols StringTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "string" );
}
