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
}
