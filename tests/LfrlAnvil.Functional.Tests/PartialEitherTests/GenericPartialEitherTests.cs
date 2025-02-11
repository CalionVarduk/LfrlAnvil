namespace LfrlAnvil.Functional.Tests.PartialEitherTests;

public abstract class GenericPartialEitherTests<T1, T2> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateWithCorrectValue()
    {
        var value = Fixture.Create<T1>();
        var sut = new PartialEither<T1>( value );
        sut.Value.TestEquals( value ).Go();
    }

    [Fact]
    public void WithFirst_ShouldCreateCorrectEither()
    {
        var value = Fixture.Create<T1>();

        var sut = new PartialEither<T1>( value );
        var result = sut.WithFirst<T2>();

        Assertion.All(
                result.HasFirst.TestFalse(),
                result.Second.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void WithSecond_ShouldCreateCorrectEither()
    {
        var value = Fixture.Create<T1>();

        var sut = new PartialEither<T1>( value );
        var result = sut.WithSecond<T2>();

        Assertion.All(
                result.HasFirst.TestTrue(),
                result.First.TestEquals( value ) )
            .Go();
    }
}
