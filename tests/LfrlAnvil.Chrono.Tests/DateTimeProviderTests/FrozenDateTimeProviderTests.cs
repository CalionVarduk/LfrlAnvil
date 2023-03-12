using System.Threading.Tasks;

namespace LfrlAnvil.Chrono.Tests.DateTimeProviderTests;

public class FrozenDateTimeProviderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<DateTime>();
        var sut = new FrozenDateTimeProvider( value );
        sut.Kind.Should().Be( value.Kind );
    }

    [Fact]
    public void GetNow_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<DateTime>();
        var sut = new FrozenDateTimeProvider( value );

        var firstResult = sut.GetNow();
        Task.Delay( TimeSpan.FromMilliseconds( 1 ) ).Wait();
        var secondResult = sut.GetNow();

        using ( new AssertionScope() )
        {
            firstResult.Should().Be( value );
            secondResult.Should().Be( value );
        }
    }
}
