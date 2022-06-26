using System;

namespace LfrlAnvil.Exceptions;

public class LazyDisposableAssignmentException : InvalidOperationException
{
    public LazyDisposableAssignmentException()
        : base( ExceptionResources.LazyDisposableCannotAssign ) { }
}
