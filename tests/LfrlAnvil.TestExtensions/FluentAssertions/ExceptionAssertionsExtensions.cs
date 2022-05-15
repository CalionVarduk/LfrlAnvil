using System;
using FluentAssertions;
using FluentAssertions.Primitives;
using FluentAssertions.Specialized;

namespace LfrlAnvil.TestExtensions.FluentAssertions
{
    public static class ExceptionAssertionsExtensions
    {
        public static AndConstraint<ObjectAssertions> AndMatch<TException>(
            this ExceptionAssertions<TException> source,
            Func<TException, bool> predicate,
            string because = "",
            params object[] becauseArgs)
            where TException : Exception
        {
            return source.And.Should().Match( s => predicate( (TException)s ), because, becauseArgs );
        }
    }
}
