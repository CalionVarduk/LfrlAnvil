namespace LfrlAnvil.Tests.ComparerFactoryTests;

public class ComparerFactoryTests : TestsBase
{
    [Fact]
    public void CreateBy_ShouldReturnCorrectResult()
    {
        var values = new[] { 1, 9, 10, 19, 20 };
        var sut = ComparerFactory<int>.CreateBy( x => x.ToString() );
        Array.Sort( values, sut );

        values.TestSequence( [ 1, 10, 19, 20, 9 ] ).Go();
    }
}
