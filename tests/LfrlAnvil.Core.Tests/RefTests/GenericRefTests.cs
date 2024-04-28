using LfrlAnvil.Functional;

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
    public void Count_ShouldReturnOne()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );
        sut.Count.Should().Be( 1 );
    }

    [Fact]
    public void GetIndexer_ShouldReturnValueWhenIndexIsEqualToZero()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );

        var result = sut[0];

        result.Should().Be( value );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void GetIndexer_ShouldThrowIndexOutOfRangeException_WhenIndexIsNotEqualToZero(int index)
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );
        sut.Should().BeEquivalentTo( new[] { value } );
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
