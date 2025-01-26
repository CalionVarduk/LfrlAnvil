using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ResultTests;

public class ResultTests : TestsBase
{
    [Fact]
    public void Valid_ShouldReturnResultWithoutException()
    {
        var sut = Result.Valid;
        sut.Exception.Should().BeNull();
    }

    [Fact]
    public void Error_ShouldCreateResultWithException()
    {
        var exception = new Exception( "foo" );
        var result = Result.Error( exception );
        result.Exception.Should().BeSameAs( exception );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenExceptionIsNull()
    {
        var sut = Result.Valid;
        var result = sut.ToString();
        result.Should().Be( "<VALID>" );
    }

    [Fact]
    public void ToString_ShouldReturnExceptionToString_WhenExceptionIsNotNull()
    {
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception );
        var result = sut.ToString();
        result.Should().Be( exception.ToString() );
    }

    [Fact]
    public void ThrowIfError_ShouldDoNothing_WhenExceptionIsNull()
    {
        var sut = Result.Valid;
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfError_ShouldThrow_WhenExceptionIsNotNull()
    {
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Should().Throw<Exception>().And.Should().BeSameAs( exception );
    }

    [Fact]
    public void Create_ShouldCreateGenericResultWithoutException()
    {
        var value = Fixture.Create<string>();
        var result = Result.Create( value );

        using ( new AssertionScope() )
        {
            result.Value.Should().BeSameAs( value );
            result.Exception.Should().BeNull();
        }
    }

    [Fact]
    public void Error_ShouldCreateGenericResultWithException()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var result = Result.Error( exception, value );

        using ( new AssertionScope() )
        {
            result.Value.Should().BeSameAs( value );
            result.Exception.Should().BeSameAs( exception );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenGenericExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var result = sut.ToString();

        result.Should()
            .Be(
                $"""
                 Value: {value}
                 <VALID>
                 """ );
    }

    [Fact]
    public void ToString_ShouldReturnExceptionToString_WhenGenericExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var result = sut.ToString();

        result.Should()
            .Be(
                $"""
                 Value: {value}
                 {exception}
                 """ );
    }

    [Fact]
    public void ThrowIfError_ShouldDoNothing_WhenGenericExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfError_ShouldThrow_WhenGenericExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Should().Throw<Exception>().And.Should().BeSameAs( exception );
    }

    [Fact]
    public void GetValueOrThrow_ShouldReturnValue_WhenGenericExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var result = sut.GetValueOrThrow();
        result.Should().BeSameAs( sut.Value );
    }

    [Fact]
    public void GetValueOrThrow_ShouldThrow_WhenGenericExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var action = Lambda.Of( () => sut.GetValueOrThrow() );
        action.Should().Throw<Exception>().And.Should().BeSameAs( exception );
    }

    [Fact]
    public void ResultConversionOperator_FromException_ShouldReturnError()
    {
        var exception = new Exception( "foo" );
        var result = ( Result )exception;
        result.Exception.Should().BeSameAs( exception );
    }

    [Fact]
    public void ResultConversionOperator_FromException_ShouldReturnErrorForGeneric()
    {
        var exception = new Exception( "foo" );
        var result = ( Result<string> )exception;
        result.Exception.Should().BeSameAs( exception );
    }

    [Fact]
    public void ResultConversionOperator_ShouldReturnValid_WhenExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var result = ( Result )sut;
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void ResultConversionOperator_ShouldReturnError_WhenExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var result = ( Result )sut;
        result.Exception.Should().BeSameAs( exception );
    }

    [Fact]
    public void Deconstruct_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );

        var (outValue, outException) = sut;

        using ( new AssertionScope() )
        {
            outValue.Should().BeSameAs( value );
            outException.Should().BeSameAs( exception );
        }
    }
}
