namespace LfrlAnvil.Sql.Tests;

public class ReferenceBehaviorTests : TestsBase
{
    [Fact]
    public void Restrict_ShouldHaveCorrectProperties()
    {
        var sut = ReferenceBehavior.Restrict;

        Assertion.All(
                sut.Name.TestEquals( "RESTRICT" ),
                sut.Value.TestEquals( ReferenceBehavior.Values.Restrict ) )
            .Go();
    }

    [Fact]
    public void Cascade_ShouldHaveCorrectProperties()
    {
        var sut = ReferenceBehavior.Cascade;

        Assertion.All(
                sut.Name.TestEquals( "CASCADE" ),
                sut.Value.TestEquals( ReferenceBehavior.Values.Cascade ) )
            .Go();
    }

    [Fact]
    public void SetNull_ShouldHaveCorrectProperties()
    {
        var sut = ReferenceBehavior.SetNull;

        Assertion.All(
                sut.Name.TestEquals( "SET NULL" ),
                sut.Value.TestEquals( ReferenceBehavior.Values.SetNull ) )
            .Go();
    }

    [Fact]
    public void NoAction_ShouldHaveCorrectProperties()
    {
        var sut = ReferenceBehavior.NoAction;

        Assertion.All(
                sut.Name.TestEquals( "NO ACTION" ),
                sut.Value.TestEquals( ReferenceBehavior.Values.NoAction ) )
            .Go();
    }

    [Fact]
    public void GetBehavior_ShouldReturnRestrict()
    {
        var sut = ReferenceBehavior.GetBehavior( ReferenceBehavior.Values.Restrict );
        sut.TestRefEquals( ReferenceBehavior.Restrict ).Go();
    }

    [Fact]
    public void GetBehavior_ShouldReturnCascade()
    {
        var sut = ReferenceBehavior.GetBehavior( ReferenceBehavior.Values.Cascade );
        sut.TestRefEquals( ReferenceBehavior.Cascade ).Go();
    }

    [Fact]
    public void GetBehavior_ShouldReturnSetNull()
    {
        var sut = ReferenceBehavior.GetBehavior( ReferenceBehavior.Values.SetNull );
        sut.TestRefEquals( ReferenceBehavior.SetNull ).Go();
    }

    [Fact]
    public void GetBehavior_ShouldReturnNoAction()
    {
        var sut = ReferenceBehavior.GetBehavior( ReferenceBehavior.Values.NoAction );
        sut.TestRefEquals( ReferenceBehavior.NoAction ).Go();
    }
}
