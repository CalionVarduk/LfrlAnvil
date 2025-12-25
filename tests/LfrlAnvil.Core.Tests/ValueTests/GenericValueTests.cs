namespace LfrlAnvil.Tests.ValueTests;

public abstract class GenericValueTests<T> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateCorrectValue()
    {
        var item = Fixture.Create<T>();
        var result = new Value<T>( item );
        result.Item.TestEquals( item ).Go();
    }

    [Fact]
    public void ConversionOperator_ToValue_ShouldReturnCorrectResult()
    {
        var item = Fixture.Create<T>();
        var result = ( Value<T> )item;
        result.Item.TestEquals( item ).Go();
    }

    [Fact]
    public void ConversionOperator_ToItem_ShouldReturnCorrectResult()
    {
        var sut = new Value<T>( Fixture.Create<T>() );
        var result = ( T )sut;
        result.TestEquals( sut.Item ).Go();
    }
}
