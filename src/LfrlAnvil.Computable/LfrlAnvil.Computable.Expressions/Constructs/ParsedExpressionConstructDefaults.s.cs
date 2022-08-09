using LfrlAnvil.Computable.Expressions.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public static class ParsedExpressionConstructDefaults
{
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

    public const int TypeConverterPrecedence = 1;
    public const int NegatePrecedence = 1;
    public const int NotPrecedence = 1;
    public const int BitwiseNotPrecedence = 1;
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

    public static readonly ParsedExpressionTypeDefinitionSymbols BooleanTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "boolean" );

    public static readonly ParsedExpressionTypeDefinitionSymbols DecimalTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "decimal" )
        .SetPostfixTypeConverter( "M" );

    public static readonly ParsedExpressionTypeDefinitionSymbols DoubleTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "double" );

    public static readonly ParsedExpressionTypeDefinitionSymbols FloatTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "float" )
        .SetPostfixTypeConverter( "F" );

    public static readonly ParsedExpressionTypeDefinitionSymbols Int32TypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "int32" );

    public static readonly ParsedExpressionTypeDefinitionSymbols Int64TypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "int64" )
        .SetPostfixTypeConverter( "L" );

    public static readonly ParsedExpressionTypeDefinitionSymbols BigIntTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "bigint" );

    public static readonly ParsedExpressionTypeDefinitionSymbols StringTypeSymbols = new ParsedExpressionTypeDefinitionSymbols()
        .SetName( "string" );
}
