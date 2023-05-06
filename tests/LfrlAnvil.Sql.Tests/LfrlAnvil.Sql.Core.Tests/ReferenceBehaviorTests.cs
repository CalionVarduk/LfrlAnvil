namespace LfrlAnvil.Sql.Tests;

public class ReferenceBehaviorTests : TestsBase
{
    [Fact]
    public void Restrict_ShouldHaveCorrectProperties()
    {
        var sut = ReferenceBehavior.Restrict;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "RESTRICT" );
            sut.Value.Should().Be( ReferenceBehavior.Values.Restrict );
        }
    }

    [Fact]
    public void Cascade_ShouldHaveCorrectProperties()
    {
        var sut = ReferenceBehavior.Cascade;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "CASCADE" );
            sut.Value.Should().Be( ReferenceBehavior.Values.Cascade );
        }
    }
}
