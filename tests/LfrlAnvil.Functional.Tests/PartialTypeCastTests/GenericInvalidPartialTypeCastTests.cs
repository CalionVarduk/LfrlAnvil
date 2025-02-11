namespace LfrlAnvil.Functional.Tests.PartialTypeCastTests;

public abstract class GenericInvalidPartialTypeCastTests<TSource, TDestination> : GenericPartialTypeCastTests<TSource>
{
    [Fact]
    public void To_ShouldReturnCorrectTypeCast()
    {
        var value = Fixture.Create<TSource>();

        var sut = new PartialTypeCast<TSource>( value );

        var result = sut.To<TDestination>();

        Assertion.All(
                result.IsValid.TestFalse(),
                result.IsInvalid.TestTrue(),
                result.Source.TestEquals( value ),
                result.Result.TestEquals( default ) )
            .Go();
    }
}
