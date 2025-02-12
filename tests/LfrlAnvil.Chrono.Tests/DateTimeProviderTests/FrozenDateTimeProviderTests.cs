using System.Threading.Tasks;

namespace LfrlAnvil.Chrono.Tests.DateTimeProviderTests;

public class FrozenDateTimeProviderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<DateTime>();
        var sut = new FrozenDateTimeProvider( value );
        sut.Kind.TestEquals( value.Kind ).Go();
    }

    [Fact]
    public async Task GetNow_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<DateTime>();
        var sut = new FrozenDateTimeProvider( value );

        var firstResult = sut.GetNow();
        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );
        var secondResult = sut.GetNow();

        Assertion.All(
                firstResult.TestEquals( value ),
                secondResult.TestEquals( value ) )
            .Go();
    }
}
