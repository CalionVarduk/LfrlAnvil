using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlScalarQueryResultTests : TestsBase
{
    [Fact]
    public void Default_TypeErased_ShouldNotHaveValue()
    {
        var sut = default( SqlScalarQueryResult );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.Value.Should().BeNull();
        }
    }

    [Fact]
    public void Empty_TypeErased_ShouldNotHaveValue()
    {
        var sut = SqlScalarQueryResult.Empty;

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.Value.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void Ctor_TypeErased_ShouldCreateWithValue(object? value)
    {
        var sut = new SqlScalarQueryResult( value );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeTrue();
            sut.Value.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ToString_TypeErased_ShouldReturnCorrectResult_ForEmpty()
    {
        var sut = SqlScalarQueryResult.Empty;
        var result = sut.ToString();
        result.Should().Be( "Empty()" );
    }

    [Fact]
    public void ToString_TypeErased_ShouldReturnCorrectResult_ForNonEmpty()
    {
        var sut = new SqlScalarQueryResult( "foo" );
        var result = sut.ToString();
        result.Should().Be( "Value(foo)" );
    }

    [Fact]
    public void GetHashCode_TypeErased_ShouldReturnCorrectResult()
    {
        var sut = new SqlScalarQueryResult( "foo" );
        var expected = HashCode.Combine( true, "foo" );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetValue_TypeErased_ShouldReturnValue_WhenItExists()
    {
        var expected = "foo";
        var sut = new SqlScalarQueryResult( expected );

        var result = sut.GetValue();

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void GetValue_TypeErased_ShouldThrowInvalidOperationException_WhenValueDoesNotExist()
    {
        var sut = SqlScalarQueryResult.Empty;
        var action = Lambda.Of( () => sut.GetValue() );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetValueOrDefault_TypeErased_ShouldReturnValue_WhenItExists()
    {
        var expected = "foo";
        var sut = new SqlScalarQueryResult( expected );

        var result = sut.GetValueOrDefault( "bar" );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void GetValueOrDefault_TypeErased_ShouldReturnDefaultValue_WhenValueDoesNotExist()
    {
        var expected = "foo";
        var sut = SqlScalarQueryResult.Empty;

        var result = sut.GetValueOrDefault( expected );

        result.Should().BeSameAs( expected );
    }

    [Theory]
    [InlineData( "foo", "foo", true )]
    [InlineData( "foo", "bar", false )]
    [InlineData( 42, "foo", false )]
    public void EqualityOperator_TypeErased_ShouldReturnCorrectResult_ForNonEmpty(object? a, object? b, bool expected)
    {
        var result = new SqlScalarQueryResult( a ) == new SqlScalarQueryResult( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( false, true, false )]
    [InlineData( true, false, false )]
    public void EqualityOperator_TypeErased_ShouldReturnCorrectResult_ForEmpty(bool isFirstEmpty, bool isSecondEmpty, bool expected)
    {
        var a = isFirstEmpty ? SqlScalarQueryResult.Empty : new SqlScalarQueryResult( null );
        var b = isSecondEmpty ? SqlScalarQueryResult.Empty : new SqlScalarQueryResult( null );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "foo", false )]
    [InlineData( "foo", "bar", true )]
    [InlineData( 42, "foo", true )]
    public void InequalityOperator_TypeErased_ShouldReturnCorrectResult_ForNonEmpty(object? a, object? b, bool expected)
    {
        var result = new SqlScalarQueryResult( a ) != new SqlScalarQueryResult( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( false, true, true )]
    [InlineData( true, false, true )]
    public void InequalityOperator_TypeErased_ShouldReturnCorrectResult_ForEmpty(bool isFirstEmpty, bool isSecondEmpty, bool expected)
    {
        var a = isFirstEmpty ? SqlScalarQueryResult.Empty : new SqlScalarQueryResult( null );
        var b = isSecondEmpty ? SqlScalarQueryResult.Empty : new SqlScalarQueryResult( null );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Fact]
    public void Default_Generic_ShouldNotHaveValue()
    {
        var sut = default( SqlScalarQueryResult<string> );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.Value.Should().BeNull();
        }
    }

    [Fact]
    public void Empty_Generic_ShouldNotHaveValue()
    {
        var sut = SqlScalarQueryResult<string>.Empty;

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.Value.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void Ctor_Generic_ShouldCreateWithValue(string? value)
    {
        var sut = new SqlScalarQueryResult<string>( value );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeTrue();
            sut.Value.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ToString_Generic_ShouldReturnCorrectResult_ForEmpty()
    {
        var sut = SqlScalarQueryResult<string>.Empty;
        var result = sut.ToString();
        result.Should().Be( "Empty<System.String>()" );
    }

    [Fact]
    public void ToString_Generic_ShouldReturnCorrectResult_ForNonEmpty()
    {
        var sut = new SqlScalarQueryResult<string>( "foo" );
        var result = sut.ToString();
        result.Should().Be( "Value<System.String>(foo)" );
    }

    [Fact]
    public void GetHashCode_Generic_ShouldReturnCorrectResult()
    {
        var sut = new SqlScalarQueryResult<string>( "foo" );
        var expected = HashCode.Combine( true, "foo" );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetValue_Generic_ShouldReturnValue_WhenItExists()
    {
        var expected = "foo";
        var sut = new SqlScalarQueryResult<string>( expected );

        var result = sut.GetValue();

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void GetValue_Generic_ShouldThrowInvalidOperationException_WhenValueDoesNotExist()
    {
        var sut = SqlScalarQueryResult<string>.Empty;
        var action = Lambda.Of( () => sut.GetValue() );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetValueOrDefault_Generic_ShouldReturnValue_WhenItExists()
    {
        var expected = "foo";
        var sut = new SqlScalarQueryResult<string>( expected );

        var result = sut.GetValueOrDefault( "bar" );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void GetValueOrDefault_Generic_ShouldReturnDefaultValue_WhenValueDoesNotExist()
    {
        var expected = "foo";
        var sut = SqlScalarQueryResult<string>.Empty;

        var result = sut.GetValueOrDefault( expected );

        result.Should().BeSameAs( expected );
    }

    [Theory]
    [InlineData( "foo", "foo", true )]
    [InlineData( "foo", "bar", false )]
    public void EqualityOperator_Generic_ShouldReturnCorrectResult_ForNonEmpty(string? a, string? b, bool expected)
    {
        var result = new SqlScalarQueryResult<string>( a ) == new SqlScalarQueryResult<string>( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( false, true, false )]
    [InlineData( true, false, false )]
    public void EqualityOperator_Generic_ShouldReturnCorrectResult_ForEmpty(bool isFirstEmpty, bool isSecondEmpty, bool expected)
    {
        var a = isFirstEmpty ? SqlScalarQueryResult<string>.Empty : new SqlScalarQueryResult<string>( null );
        var b = isSecondEmpty ? SqlScalarQueryResult<string>.Empty : new SqlScalarQueryResult<string>( null );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "foo", false )]
    [InlineData( "foo", "bar", true )]
    public void InequalityOperator_Generic_ShouldReturnCorrectResult_ForNonEmpty(string? a, string? b, bool expected)
    {
        var result = new SqlScalarQueryResult<string>( a ) != new SqlScalarQueryResult<string>( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( false, true, true )]
    [InlineData( true, false, true )]
    public void InequalityOperator_Generic_ShouldReturnCorrectResult_ForEmpty(bool isFirstEmpty, bool isSecondEmpty, bool expected)
    {
        var a = isFirstEmpty ? SqlScalarQueryResult<string>.Empty : new SqlScalarQueryResult<string>( null );
        var b = isSecondEmpty ? SqlScalarQueryResult<string>.Empty : new SqlScalarQueryResult<string>( null );

        var result = a != b;

        result.Should().Be( expected );
    }
}
