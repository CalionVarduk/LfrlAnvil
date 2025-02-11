using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.RefTests;

public abstract class GenericRefTests<T> : TestsBase
{
    [Fact]
    public void Create_ShouldCreateCorrectRef()
    {
        var value = Fixture.Create<T>();
        var sut = Ref.Create( value );
        sut.Value.TestEquals( value ).Go();
    }

    [Fact]
    public void CtorWithValue_ShouldCreateWithCorrectValue()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );
        sut.Value.TestEquals( value ).Go();
    }

    [Fact]
    public void Count_ShouldReturnOne()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );
        sut.Count.TestEquals( 1 ).Go();
    }

    [Fact]
    public void GetIndexer_ShouldReturnValueWhenIndexIsEqualToZero()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );

        var result = sut[0];

        result.TestEquals( value ).Go();
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

        action.Test( exc => exc.TestType().Exact<IndexOutOfRangeException>() ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );
        sut.TestSequence( [ value ] ).Go();
    }

    [Fact]
    public void TConversionOperator_ShouldReturnUnderlyingValue()
    {
        var value = Fixture.Create<T>();
        var sut = new Ref<T>( value );

        var result = ( T )sut;

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void RefConversionOperator_ShouldCreateProperRef()
    {
        var value = Fixture.Create<T>();
        var result = ( Ref<T> )value;
        result.Value.TestEquals( value ).Go();
    }
}
