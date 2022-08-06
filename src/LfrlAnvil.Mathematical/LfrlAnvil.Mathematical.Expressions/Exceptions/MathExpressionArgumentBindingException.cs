using System;

namespace LfrlAnvil.Mathematical.Expressions.Exceptions;

public class MathExpressionArgumentBindingException : InvalidOperationException
{
    public MathExpressionArgumentBindingException()
        : base( Resources.CannotBindValueToArgumentThatDoesNotExist ) { }
}
