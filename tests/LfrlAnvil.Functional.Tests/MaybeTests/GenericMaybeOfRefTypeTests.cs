namespace LfrlAnvil.Functional.Tests.MaybeTests;

public abstract class GenericMaybeOfRefTypeTests<T> : GenericMaybeTests<T>
    where T : class
{
    [Fact]
    public void Some_ShouldThrowArgumentNullException_WhenParameterIsNull()
    {
        var action = Lambda.Of( () => Maybe.Some<T>( null ) );
        action.Test( exc => exc.TestType().Exact<ArgumentNullException>() ).Go();
    }

    [Fact]
    public void MaybeConversionOperator_FromT_ShouldReturnNone_WhenParameterIsNull()
    {
        var sut = ( Maybe<T> )null;
        sut.HasValue.TestFalse().Go();
    }
}
