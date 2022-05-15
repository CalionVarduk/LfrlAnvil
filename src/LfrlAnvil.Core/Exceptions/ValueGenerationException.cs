using System;

namespace LfrlAnvil.Exceptions
{
    public class ValueGenerationException : InvalidOperationException
    {
        public ValueGenerationException()
            : base( ExceptionResources.FailedToGenerateNextValue ) { }
    }
}
