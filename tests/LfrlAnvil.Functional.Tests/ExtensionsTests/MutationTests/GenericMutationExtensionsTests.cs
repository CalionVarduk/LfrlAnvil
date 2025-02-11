using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.MutationTests;

public abstract class GenericMutationExtensionsTests<T> : TestsBase
{
    [Fact]
    public void Reduce_ShouldReturnCorrectResult()
    {
        var (oldestValue, oldValue, newValue, newestValue) = Fixture.CreateManyDistinct<T>( count: 4 );

        var oldMutation = new Mutation<T>( oldestValue, oldValue );
        var newMutation = new Mutation<T>( newValue, newestValue );

        var sut = new Mutation<Mutation<T>>( oldMutation, newMutation );

        var result = sut.Reduce();

        Assertion.All(
                result.OldValue.TestEquals( oldestValue ),
                result.Value.TestEquals( newestValue ),
                result.HasChanged.TestTrue() )
            .Go();
    }
}
