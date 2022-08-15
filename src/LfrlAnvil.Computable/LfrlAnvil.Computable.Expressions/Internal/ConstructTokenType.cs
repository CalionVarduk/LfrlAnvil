using System;

namespace LfrlAnvil.Computable.Expressions.Internal;

// TODO: this could be made public
// it would make IParsedExpressionFactory interface a lot more compact & factory builder could also give access to it
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
    VariadicFunction = 64,
    Constant = 128,
    TypeDeclaration = 256,
    Operator = BinaryOperator | PrefixUnaryOperator | PostfixUnaryOperator,
    TypeConverter = PrefixTypeConverter | PostfixTypeConverter,
    PrefixUnaryConstruct = PrefixUnaryOperator | PrefixTypeConverter,
    PostfixUnaryConstruct = PostfixUnaryOperator | PostfixTypeConverter
}
