namespace LfrlAnvil.Tests.BoxedTests;

public class BoxedTests : TestsBase
{
    [Fact]
    public void True_ShouldBeEquivalentToTrue()
    {
        var sut = Boxed.True;
        var result = sut.Equals( true );

        Assertion.All(
                sut.TestType().Exact<bool>(),
                result.TestTrue() )
            .Go();
    }

    [Fact]
    public void False_ShouldBeEquivalentToFalse()
    {
        var sut = Boxed.False;
        var result = sut.Equals( false );

        Assertion.All(
                sut.TestType().Exact<bool>(),
                result.TestTrue() )
            .Go();
    }

    [Fact]
    public void GetBool_ShouldReturnTrue_WhenParameterEqualsTrue()
    {
        var result = Boxed.GetBool( true );
        result.TestRefEquals( Boxed.True ).Go();
    }

    [Fact]
    public void GetBool_ShouldReturnFalse_WhenParameterEqualsFalse()
    {
        var result = Boxed.GetBool( false );
        result.TestRefEquals( Boxed.False ).Go();
    }
}
