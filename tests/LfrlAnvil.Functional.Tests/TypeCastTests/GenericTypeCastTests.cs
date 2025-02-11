namespace LfrlAnvil.Functional.Tests.TypeCastTests;

public abstract class GenericTypeCastTests<TSource, TDestination> : TestsBase
{
    [Fact]
    public void Empty_ShouldBeInvalid()
    {
        var sut = TypeCast<TSource, TDestination>.Empty;

        Assertion.All(
                sut.IsValid.TestFalse(),
                sut.IsInvalid.TestTrue(),
                sut.Source.TestEquals( default ),
                sut.Result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;
        var expected = Hash.Default.Add( value ).Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void TypeCastConversionOperator_FromNil_ShouldReturnCorrectResult()
    {
        var result = ( TypeCast<TSource, TDestination> )Nil.Instance;

        Assertion.All(
                result.IsValid.TestFalse(),
                result.IsInvalid.TestTrue(),
                result.Source.TestEquals( default ),
                result.Result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void ITypeCastSource_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();

        ITypeCast<TDestination> sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.Source;

        result.TestEquals( value ).Go();
    }
}
