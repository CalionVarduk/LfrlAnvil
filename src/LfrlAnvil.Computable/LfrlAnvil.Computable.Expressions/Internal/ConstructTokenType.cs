using System;

namespace LfrlAnvil.Computable.Expressions.Internal;

[Flags]
internal enum ConstructTokenType : ushort
{
    None = 0,
    BinaryOperator = 1,
    PrefixUnaryOperator = 2,
    PostfixUnaryOperator = 4,
    PrefixTypeConverter = 8,
    PostfixTypeConverter = 16,
    Function = 32,
    Constant = 64,
    TypeDeclaration = 128,
    Operator = BinaryOperator | PrefixUnaryOperator | PostfixUnaryOperator,
    TypeConverter = PrefixTypeConverter | PostfixTypeConverter,
    PrefixUnaryConstruct = PrefixUnaryOperator | PrefixTypeConverter,
    PostfixUnaryConstruct = PostfixUnaryOperator | PostfixTypeConverter
}
