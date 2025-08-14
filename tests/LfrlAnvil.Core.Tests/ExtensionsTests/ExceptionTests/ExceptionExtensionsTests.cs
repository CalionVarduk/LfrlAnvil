using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ExtensionsTests.ExceptionTests;

public class ExceptionExtensionsTests : TestsBase
{
    [Fact]
    public void Rethrow_ShouldThrowExceptionWithOriginalStackTrace()
    {
        var exception = Erratic.Try( () => throw new Exception() ).GetError();
        var originalStackTrace = exception.StackTrace!.Split( Environment.NewLine );

        var result = Erratic.Try( () => { exception.Rethrow(); } );

        Assertion.All(
                result.HasError.TestTrue(),
                result.GetError().TestRefEquals( exception ),
                result.GetError().StackTrace!.Split( Environment.NewLine ).TestContainsSequence( originalStackTrace ) )
            .Go();
    }

    [Fact]
    public void Consolidate_ShouldReturnNull_WhenCollectionIsEmpty()
    {
        var exceptions = Array.Empty<Exception>();
        var result = exceptions.Consolidate();
        result.TestNull().Go();
    }

    [Fact]
    public void Consolidate_ShouldReturnFirstException_WhenCollectionContainsOnlyOneElement()
    {
        var exceptions = new[] { new Exception( "foo" ) };
        var result = exceptions.Consolidate();
        result.TestRefEquals( exceptions[0] ).Go();
    }

    [Fact]
    public void Consolidate_ShouldReturnAggregateException_WhenCollectionContainsMoreThanOneElement()
    {
        var exceptions = new[] { new Exception( "foo" ), new Exception( "bar" ) };
        var result = exceptions.Consolidate();
        result.TestType().Exact<AggregateException>( exc => exc.InnerExceptions.TestSequence( exceptions ) ).Go();
    }
}
