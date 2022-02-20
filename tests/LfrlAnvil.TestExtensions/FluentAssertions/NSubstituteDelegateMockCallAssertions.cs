using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Primitives;
using NSubstitute;
using NSubstitute.Core;

namespace LfrlAnvil.TestExtensions.FluentAssertions
{
    public class NSubstituteDelegateMockCallAssertions<T> : ObjectAssertions
        where T : Delegate
    {
        internal NSubstituteDelegateMockCallAssertions(NSubstituteDelegateMockAssertions<T> target, int callIndex)
            : base( target.Subject.ReceivedCalls().ElementAtOrDefault( callIndex ) )
        {
            Target = target;
            CallIndex = callIndex;
        }

        public NSubstituteDelegateMockAssertions<T> Target { get; }
        public int CallIndex { get; }
        public new ICall? Subject => (ICall?)base.Subject;
        public object?[] Arguments => Subject?.GetArguments() ?? Array.Empty<object>();

        public AndConstraint<NSubstituteDelegateMockCallAssertions<T>> Exists()
        {
            Target.CallCount.Should().BeGreaterThan( CallIndex, "delegate must have been called an appropriate amount of times" );
            return new AndConstraint<NSubstituteDelegateMockCallAssertions<T>>( this );
        }

        public object? ArgAt(int argumentIndex)
        {
            return Subject?.GetArguments().ElementAtOrDefault( argumentIndex );
        }
    }
}
