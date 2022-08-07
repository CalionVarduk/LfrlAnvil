using FluentAssertions.Execution;
using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.MutationTests;

public abstract class GenericMutationExtensionsTests<T> : TestsBase
{
    [Fact]
    public void Reduce_ShouldReturnCorrectResult()
    {
        var (oldestValue, oldValue, newValue, newestValue) = Fixture.CreateDistinctCollection<T>( 4 );

        var oldMutation = new Mutation<T>( oldestValue, oldValue );
        var newMutation = new Mutation<T>( newValue, newestValue );

        var sut = new Mutation<Mutation<T>>( oldMutation, newMutation );

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( oldestValue );
            result.Value.Should().Be( newestValue );
            result.HasChanged.Should().BeTrue();
        }
    }
}
