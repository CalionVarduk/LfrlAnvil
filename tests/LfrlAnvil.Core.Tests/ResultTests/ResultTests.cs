using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ResultTests;

public class ResultTests : TestsBase
{
    [Fact]
    public void Valid_ShouldReturnResultWithoutException()
    {
        var sut = Result.Valid;
        sut.Exception.TestNull().Go();
    }

    [Fact]
    public void Error_ShouldCreateResultWithException()
    {
        var exception = new Exception( "foo" );
        var result = Result.Error( exception );
        result.Exception.TestRefEquals( exception ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenExceptionIsNull()
    {
        var sut = Result.Valid;
        var result = sut.ToString();
        result.TestEquals( "<VALID>" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnExceptionToString_WhenExceptionIsNotNull()
    {
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception );
        var result = sut.ToString();
        result.TestEquals( exception.ToString() ).Go();
    }

    [Fact]
    public void ThrowIfError_ShouldDoNothing_WhenExceptionIsNull()
    {
        var sut = Result.Valid;
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void ThrowIfError_ShouldThrow_WhenExceptionIsNotNull()
    {
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Test( exc => exc.TestRefEquals( exception ) ).Go();
    }

    [Fact]
    public void WithValue_ShouldCreateGenericResultWithoutException()
    {
        var sut = Result.Valid;
        var result = sut.WithValue( true );
        Assertion.All( result.Value.TestTrue(), result.Exception.TestNull() ).Go();
    }

    [Fact]
    public void WithValue_ShouldCreateGenericResultWithException()
    {
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception );
        var result = sut.WithValue( true );
        Assertion.All( result.Value.TestTrue(), result.Exception.TestRefEquals( exception ) ).Go();
    }

    [Fact]
    public void Create_ShouldCreateGenericResultWithoutException()
    {
        var value = Fixture.Create<string>();
        var result = Result.Create( value );

        Assertion.All(
                result.Value.TestRefEquals( value ),
                result.Exception.TestNull() )
            .Go();
    }

    [Fact]
    public void Error_ShouldCreateGenericResultWithException()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var result = Result.Error( exception, value );

        Assertion.All(
                result.Value.TestRefEquals( value ),
                result.Exception.TestRefEquals( exception ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenGenericExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var result = sut.ToString();

        result.TestEquals(
                $"""
                 Value: {value}
                 <VALID>
                 """ )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnExceptionToString_WhenGenericExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var result = sut.ToString();

        result.TestEquals(
                $"""
                 Value: {value}
                 {exception}
                 """ )
            .Go();
    }

    [Fact]
    public void ThrowIfError_ShouldDoNothing_WhenGenericExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void ThrowIfError_ShouldThrow_WhenGenericExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Test( exc => exc.TestRefEquals( exception ) ).Go();
    }

    [Fact]
    public void GetValueOrThrow_ShouldReturnValue_WhenGenericExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var result = sut.GetValueOrThrow();
        result.TestRefEquals( sut.Value ).Go();
    }

    [Fact]
    public void GetValueOrThrow_ShouldThrow_WhenGenericExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var action = Lambda.Of( () => sut.GetValueOrThrow() );
        action.Test( exc => exc.TestRefEquals( exception ) ).Go();
    }

    [Fact]
    public void ResultConversionOperator_FromException_ShouldReturnError()
    {
        var exception = new Exception( "foo" );
        var result = ( Result )exception;
        result.Exception.TestRefEquals( exception ).Go();
    }

    [Fact]
    public void ResultConversionOperator_FromException_ShouldReturnErrorForGeneric()
    {
        var exception = new Exception( "foo" );
        var result = ( Result<string> )exception;
        result.Exception.TestRefEquals( exception ).Go();
    }

    [Fact]
    public void ResultConversionOperator_FromValue_ShouldReturnOkForGeneric()
    {
        var value = Fixture.Create<string>();
        var result = ( Result<string> )value;

        Assertion.All(
                result.Exception.TestNull(),
                result.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void ResultConversionOperator_ShouldReturnValid_WhenExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var result = ( Result )sut;
        result.Exception.TestNull().Go();
    }

    [Fact]
    public void ResultConversionOperator_ShouldReturnError_WhenExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var result = ( Result )sut;
        result.Exception.TestRefEquals( exception ).Go();
    }

    [Fact]
    public void Deconstruct_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );

        var (outValue, outException) = sut;

        Assertion.All(
                outValue.TestRefEquals( value ),
                outException.TestRefEquals( exception ) )
            .Go();
    }
}
