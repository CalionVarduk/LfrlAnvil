using System;

namespace LfrlAnvil.TestExtensions.FluentAssertions
{
    public static class DelegateExtensions
    {
        public static NSubstituteDelegateMockAssertions<T> Verify<T>(this T source)
            where T : Delegate
        {
            return new NSubstituteDelegateMockAssertions<T>( source );
        }
    }
}
