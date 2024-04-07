namespace LfrlAnvil.Tests.RefTests;

public abstract class GenericRefTests<T> : TestsBase
{
    [Fact]
    public void Create_ShouldCreateCorrectRef()
    {
        var value = Fixture.Create<T>();
        var sut = Ref.Create( value );
        sut.Value.Should().Be( value );
    }

    [Fact]
    public void CtorWithValue_ShouldCreateWithCorrectValue()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );
        sut.Value.Should().Be( value );
    }

    [Fact]
    public void TConversionOperator_ShouldReturnUnderlyingValue()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );

        var result = ( T )sut;

        result.Should().Be( value );
    }

    [Fact]
    public void RefConversionOperator_ShouldCreateProperRef()
    {
        var value = Fixture.Create<T>();
        var result = ( Ref<T> )value;
        result.Value.Should().Be( value );
    }
}
