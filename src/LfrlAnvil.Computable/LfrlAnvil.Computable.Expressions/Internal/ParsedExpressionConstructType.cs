using System;

namespace LfrlAnvil.Computable.Expressions.Internal;

/// <summary>
/// Represents a type of a parsed expression construct.
/// </summary>
[Flags]
public enum ParsedExpressionConstructType : ushort
{
    /// <summary>
    /// Specifies the lack of type.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies a binary operator.
    /// </summary>
    BinaryOperator = 1,

    /// <summary>
    /// Specifies a prefix unary operator.
    /// </summary>
    PrefixUnaryOperator = 2,

    /// <summary>
    /// Specifies a postfix unary operator.
    /// </summary>
    PostfixUnaryOperator = 4,

    /// <summary>
    /// Specifies a prefix type converter.
    /// </summary>
    PrefixTypeConverter = 8,

    /// <summary>
    /// Specifies a postfix type converter.
    /// </summary>
    PostfixTypeConverter = 16,

    /// <summary>
    /// Specifies a function.
    /// </summary>
    Function = 32,

    /// <summary>
    /// Specifies a variadic function.
    /// </summary>
    VariadicFunction = 64,

    /// <summary>
    /// Specifies a constant value.
    /// </summary>
    Constant = 128,

    /// <summary>
    /// Specifies a type declaration.
    /// </summary>
    TypeDeclaration = 256,

    /// <summary>
    /// Specifies any type of an operator.
    /// </summary>
    Operator = BinaryOperator | PrefixUnaryOperator | PostfixUnaryOperator,

    /// <summary>
    /// Specifies any type of a type converter.
    /// </summary>
    TypeConverter = PrefixTypeConverter | PostfixTypeConverter,

    /// <summary>
    /// Specifies any type of a prefix unary construct.
    /// </summary>
    PrefixUnaryConstruct = PrefixUnaryOperator | PrefixTypeConverter,

    /// <summary>
    /// Specifies any type of a postfix unary construct.
    /// </summary>
    PostfixUnaryConstruct = PostfixUnaryOperator | PostfixTypeConverter,

    /// <summary>
    /// Specifies any type of a unary construct.
    /// </summary>
    UnaryConstruct = PrefixUnaryConstruct | PostfixUnaryConstruct
}
