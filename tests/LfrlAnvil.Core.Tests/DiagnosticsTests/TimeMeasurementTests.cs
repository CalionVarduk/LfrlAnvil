using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class TimeMeasurementTests : TestsBase
{
    [Fact]
    public void Zero_ShouldReturnCorrectResult()
    {
        var sut = TimeMeasurement.Zero;

        Assertion.All(
                sut.Preparation.TestEquals( TimeSpan.Zero ),
                sut.Invocation.TestEquals( TimeSpan.Zero ),
                sut.Teardown.TestEquals( TimeSpan.Zero ),
                sut.Total.TestEquals( TimeSpan.Zero ) )
            .Go();
    }

    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = default( TimeMeasurement );

        Assertion.All(
                sut.Preparation.TestEquals( TimeSpan.Zero ),
                sut.Invocation.TestEquals( TimeSpan.Zero ),
                sut.Teardown.TestEquals( TimeSpan.Zero ),
                sut.Total.TestEquals( TimeSpan.Zero ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var preparation = Fixture.Create<TimeSpan>();
        var invocation = Fixture.Create<TimeSpan>();
        var teardown = Fixture.Create<TimeSpan>();
        var sut = new TimeMeasurement( preparation, invocation, teardown );

        Assertion.All(
                sut.Preparation.TestEquals( preparation ),
                sut.Invocation.TestEquals( invocation ),
                sut.Teardown.TestEquals( teardown ),
                sut.Total.TestEquals( preparation + invocation + teardown ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var preparation = TimeSpan.FromSeconds( 0.1234567 );
        var invocation = TimeSpan.FromSeconds( 1.2345678 );
        var teardown = TimeSpan.FromSeconds( 123.456789 );
        var sut = new TimeMeasurement( preparation, invocation, teardown );

        var result = sut.ToString();

        result.TestEquals( "Preparation: 0.1234567s, Invocation: 1.2345678s, Teardown: 123.4567890s (Total: 124.8148135s)" ).Go();
    }

    [Fact]
    public void SetPreparation_ShouldUpdatePreparationPropertyOnly()
    {
        var (oldPreparation, newPreparation, invocation, teardown) = Fixture.CreateManyDistinct<TimeSpan>( count: 4 );
        var sut = new TimeMeasurement( oldPreparation, invocation, teardown );

        var result = sut.SetPreparation( newPreparation );

        Assertion.All(
                result.Preparation.TestEquals( newPreparation ),
                result.Invocation.TestEquals( invocation ),
                result.Teardown.TestEquals( teardown ) )
            .Go();
    }

    [Fact]
    public void SetInvocation_ShouldUpdateInvocationPropertyOnly()
    {
        var (preparation, oldInvocation, newInvocation, teardown) = Fixture.CreateManyDistinct<TimeSpan>( count: 4 );
        var sut = new TimeMeasurement( preparation, oldInvocation, teardown );

        var result = sut.SetInvocation( newInvocation );

        Assertion.All(
                result.Preparation.TestEquals( preparation ),
                result.Invocation.TestEquals( newInvocation ),
                result.Teardown.TestEquals( teardown ) )
            .Go();
    }

    [Fact]
    public void SetTeardown_ShouldUpdateTeardownPropertyOnly()
    {
        var (preparation, invocation, oldTeardown, newTeardown) = Fixture.CreateManyDistinct<TimeSpan>( count: 4 );
        var sut = new TimeMeasurement( preparation, invocation, oldTeardown );

        var result = sut.SetTeardown( newTeardown );

        Assertion.All(
                result.Preparation.TestEquals( preparation ),
                result.Invocation.TestEquals( invocation ),
                result.Teardown.TestEquals( newTeardown ) )
            .Go();
    }
}
