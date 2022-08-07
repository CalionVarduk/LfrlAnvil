using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class MathExpressionArgumentBindingException : InvalidOperationException
{
    public MathExpressionArgumentBindingException()
        : base( Resources.CannotBindValueToArgumentThatDoesNotExist ) { }
}
