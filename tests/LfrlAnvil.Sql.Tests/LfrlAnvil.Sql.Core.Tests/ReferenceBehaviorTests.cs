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

    [Fact]
    public void SetNull_ShouldHaveCorrectProperties()
    {
        var sut = ReferenceBehavior.SetNull;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "SET NULL" );
            sut.Value.Should().Be( ReferenceBehavior.Values.SetNull );
        }
    }

    [Fact]
    public void NoAction_ShouldHaveCorrectProperties()
    {
        var sut = ReferenceBehavior.NoAction;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "NO ACTION" );
            sut.Value.Should().Be( ReferenceBehavior.Values.NoAction );
        }
    }

    [Fact]
    public void GetBehavior_ShouldReturnRestrict()
    {
        var sut = ReferenceBehavior.GetBehavior( ReferenceBehavior.Values.Restrict );
        sut.Should().BeSameAs( ReferenceBehavior.Restrict );
    }

    [Fact]
    public void GetBehavior_ShouldReturnCascade()
    {
        var sut = ReferenceBehavior.GetBehavior( ReferenceBehavior.Values.Cascade );
        sut.Should().BeSameAs( ReferenceBehavior.Cascade );
    }

    [Fact]
    public void GetBehavior_ShouldReturnSetNull()
    {
        var sut = ReferenceBehavior.GetBehavior( ReferenceBehavior.Values.SetNull );
        sut.Should().BeSameAs( ReferenceBehavior.SetNull );
    }

    [Fact]
    public void GetBehavior_ShouldReturnNoAction()
    {
        var sut = ReferenceBehavior.GetBehavior( ReferenceBehavior.Values.NoAction );
        sut.Should().BeSameAs( ReferenceBehavior.NoAction );
    }
}
