using System.Threading.Tasks;

namespace LfrlAnvil.Chrono.Tests.TimestampProviderTests;

public class FrozenTimestampProviderTests : TestsBase
{
    [Fact]
    public async Task GetNow_ShouldReturnCorrectResult()
    {
        var expected = new Timestamp( Fixture.Create<int>() );
        var sut = new FrozenTimestampProvider( expected );

        var firstResult = sut.GetNow();
        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );
        var secondResult = sut.GetNow();

        using ( new AssertionScope() )
        {
            firstResult.Should().Be( expected );
            secondResult.Should().Be( expected );
        }
    }
}
