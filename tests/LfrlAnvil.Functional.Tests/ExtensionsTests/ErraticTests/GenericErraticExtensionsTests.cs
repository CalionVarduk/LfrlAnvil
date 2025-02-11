using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.ErraticTests;

public abstract class GenericErraticExtensionsTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void ToMaybe_ShouldReturnWithValue_WhenHasNonNullValue()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = ( Erratic<T> )value;

        var result = sut.ToMaybe();

        Assertion.All(
                result.HasValue.TestTrue(),
                result.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void ToMaybe_ShouldReturnWithoutValue_WhenHasError()
    {
        var error = new Exception();

        var sut = ( Erratic<T> )error;

        var result = sut.ToMaybe();

        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void ToEither_ShouldReturnWithFirst_WhenHasValue()
    {
        var value = Fixture.Create<T>();

        var sut = ( Erratic<T> )value;

        var result = sut.ToEither();

        Assertion.All(
                result.HasFirst.TestTrue(),
                result.First.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void ToEither_ShouldReturnWithSecond_WhenHasError()
    {
        var error = new Exception();

        var sut = ( Erratic<T> )error;

        var result = sut.ToEither();

        Assertion.All(
                result.HasSecond.TestTrue(),
                result.Second.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenHasError()
    {
        var error = new Exception();

        var sut = ( Erratic<Erratic<T>> )error;

        var result = sut.Reduce();

        Assertion.All(
                result.HasError.TestTrue(),
                result.Error.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenHasUnderlyingError()
    {
        var error = new Exception();
        var underlying = ( Erratic<T> )error;

        var sut = ( Erratic<Erratic<T>> )underlying;

        var result = sut.Reduce();

        Assertion.All(
                result.HasError.TestTrue(),
                result.Error.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenHasUnderlyingValue()
    {
        var value = Fixture.Create<T>();
        var underlying = ( Erratic<T> )value;

        var sut = ( Erratic<Erratic<T>> )underlying;

        var result = sut.Reduce();

        Assertion.All(
                result.IsOk.TestTrue(),
                result.Value.TestEquals( value ) )
            .Go();
    }
}
