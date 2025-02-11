namespace LfrlAnvil.Functional.Tests.PartialTypeCastTests;

public abstract class GenericValidPartialTypeCastTests<TSource, TDestination> : GenericPartialTypeCastTests<TSource>
    where TSource : TDestination
{
    [Fact]
    public void To_ShouldReturnCorrectTypeCast()
    {
        var value = Fixture.Create<TSource>();

        var sut = new PartialTypeCast<TSource>( value );

        var result = sut.To<TDestination>();

        Assertion.All(
                result.IsValid.TestTrue(),
                result.IsInvalid.TestFalse(),
                result.Source.TestEquals( value ),
                result.Result.TestEquals( value ) )
            .Go();
    }
}
