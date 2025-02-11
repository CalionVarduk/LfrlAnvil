using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.ObjectTests;

public abstract class GenericObjectExtensionsTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void ToMaybe_ShouldReturnCorrectResult_WhenNotNull()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = value.ToMaybe();

        Assertion.All(
                sut.HasValue.TestTrue(),
                sut.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void ToEither_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = value.ToEither();
        sut.Value.TestEquals( value ).Go();
    }

    [Fact]
    public void ToErratic_WithValue_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = value.ToErratic();

        Assertion.All(
                sut.IsOk.TestTrue(),
                sut.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void ToErratic_WithException_ShouldReturnCorrectResult()
    {
        var error = new Exception();

        var sut = error.ToErratic();

        Assertion.All(
                sut.HasError.TestTrue(),
                sut.Error.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void TypeCast_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();
        var sut = value.TypeCast();
        sut.Value.TestEquals( value ).Go();
    }

    [Fact]
    public void ToMutation_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();

        var sut = value.ToMutation();

        Assertion.All(
                sut.OldValue.TestEquals( value ),
                sut.Value.TestEquals( value ),
                sut.HasChanged.TestFalse() )
            .Go();
    }

    [Fact]
    public void Mutate_ShouldReturnCorrectResult()
    {
        var (value, newValue) = Fixture.CreateManyDistinct<T>( count: 2 );

        var sut = value.Mutate( newValue );

        Assertion.All(
                sut.OldValue.TestEquals( value ),
                sut.Value.TestEquals( newValue ),
                sut.HasChanged.TestTrue() )
            .Go();
    }
}
