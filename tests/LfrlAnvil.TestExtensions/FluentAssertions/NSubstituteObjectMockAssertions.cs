using FluentAssertions.Primitives;
using NSubstitute.Exceptions;

namespace LfrlAnvil.TestExtensions.FluentAssertions;

public class NSubstituteObjectMockAssertions<T> : ObjectAssertions
    where T : class
{
    internal NSubstituteObjectMockAssertions(T obj)
        : base( obj ) { }

    public new T Subject => (T)base.Subject;

    public AndConstraint<NSubstituteObjectMockAssertions<T>> Received(Action<T> verification)
    {
        Action action = () => verification( Subject.Received() );
        action.Should().NotThrow<ReceivedCallsException>();
        return new AndConstraint<NSubstituteObjectMockAssertions<T>>( this );
    }

    public AndConstraint<NSubstituteObjectMockAssertions<T>> Received(Action<T> verification, int count)
    {
        Action action = () => verification( Subject.Received( count ) );
        action.Should().NotThrow<ReceivedCallsException>();
        return new AndConstraint<NSubstituteObjectMockAssertions<T>>( this );
    }

    public AndConstraint<NSubstituteObjectMockAssertions<T>> DidNotReceive(Action<T> verification)
    {
        Action action = () => verification( Subject.DidNotReceive() );
        action.Should().NotThrow<ReceivedCallsException>();
        return new AndConstraint<NSubstituteObjectMockAssertions<T>>( this );
    }
}
