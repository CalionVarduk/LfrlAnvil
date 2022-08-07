using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionArgumentBindingException : InvalidOperationException
{
    public ParsedExpressionArgumentBindingException()
        : base( Resources.CannotBindValueToArgumentThatDoesNotExist ) { }
}
