namespace LfrlAnvil.Tests.BoxedTests;

public class BoxedTests : TestsBase
{
    [Fact]
    public void True_ShouldBeEquivalentToTrue()
    {
        var sut = Boxed.True;
        var result = sut.Equals( true );

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( bool ) );
            result.Should().BeTrue();
        }
    }

    [Fact]
    public void False_ShouldBeEquivalentToFalse()
    {
        var sut = Boxed.False;
        var result = sut.Equals( false );

        using ( new AssertionScope() )
        {
            result.GetType().Should().Be( typeof( bool ) );
            result.Should().BeTrue();
        }
    }

    [Fact]
    public void GetBool_ShouldReturnTrue_WhenParameterEqualsTrue()
    {
        var result = Boxed.GetBool( true );
        result.Should().BeSameAs( Boxed.True );
    }

    [Fact]
    public void GetBool_ShouldReturnFalse_WhenParameterEqualsFalse()
    {
        var result = Boxed.GetBool( false );
        result.Should().BeSameAs( Boxed.False );
    }
}
