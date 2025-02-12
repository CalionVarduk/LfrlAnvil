namespace LfrlAnvil.Chrono.Tests.DateTimeProviderTests;

public class DateTimeProviderTests : TestsBase
{
    [Fact]
    public void Utc_ShouldReturnCorrectResult()
    {
        var sut = DateTimeProvider.Utc;
        sut.Kind.TestEquals( DateTimeKind.Utc ).Go();
    }

    [Fact]
    public void Local_ShouldReturnCorrectResult()
    {
        var sut = DateTimeProvider.Local;
        sut.Kind.TestEquals( DateTimeKind.Local ).Go();
    }

    [Fact]
    public void GetNow_ForUtc_ShouldReturnCorrectResult()
    {
        var sut = new UtcDateTimeProvider();

        var expectedMin = DateTime.UtcNow;
        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        Assertion.All(
                result.Kind.TestEquals( DateTimeKind.Utc ),
                result.TestInRange( expectedMin, expectedMax ) )
            .Go();
    }

    [Fact]
    public void GetNow_ForLocal_ShouldReturnCorrectResult()
    {
        var sut = new LocalDateTimeProvider();

        var expectedMin = DateTime.Now;
        var result = sut.GetNow();
        var expectedMax = DateTime.Now;

        Assertion.All(
                result.Kind.TestEquals( DateTimeKind.Local ),
                result.TestInRange( expectedMin, expectedMax ) )
            .Go();
    }
}
