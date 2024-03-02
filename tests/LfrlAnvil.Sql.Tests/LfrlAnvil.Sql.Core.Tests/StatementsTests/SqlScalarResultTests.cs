using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlScalarResultTests : TestsBase
{
    [Fact]
    public void Default_TypeErased_ShouldNotHaveValue()
    {
        var sut = default( SqlScalarResult );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.Value.Should().BeNull();
        }
    }

    [Fact]
    public void Empty_TypeErased_ShouldNotHaveValue()
    {
        var sut = SqlScalarResult.Empty;

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
        var sut = new SqlScalarResult( value );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeTrue();
            sut.Value.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ToString_TypeErased_ShouldReturnCorrectResult_ForEmpty()
    {
        var sut = SqlScalarResult.Empty;
        var result = sut.ToString();
        result.Should().Be( "Empty()" );
    }

    [Fact]
    public void ToString_TypeErased_ShouldReturnCorrectResult_ForNonEmpty()
    {
        var sut = new SqlScalarResult( "foo" );
        var result = sut.ToString();
        result.Should().Be( "Value(foo)" );
    }

    [Fact]
    public void GetHashCode_TypeErased_ShouldReturnCorrectResult()
    {
        var sut = new SqlScalarResult( "foo" );
        var expected = HashCode.Combine( true, "foo" );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetValue_TypeErased_ShouldReturnValue_WhenItExists()
    {
        var expected = "foo";
        var sut = new SqlScalarResult( expected );

        var result = sut.GetValue();

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void GetValue_TypeErased_ShouldThrowInvalidOperationException_WhenValueDoesNotExist()
    {
        var sut = SqlScalarResult.Empty;
        var action = Lambda.Of( () => sut.GetValue() );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetValueOrDefault_TypeErased_ShouldReturnValue_WhenItExists()
    {
        var expected = "foo";
        var sut = new SqlScalarResult( expected );

        var result = sut.GetValueOrDefault( "bar" );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void GetValueOrDefault_TypeErased_ShouldReturnDefaultValue_WhenValueDoesNotExist()
    {
        var expected = "foo";
        var sut = SqlScalarResult.Empty;

        var result = sut.GetValueOrDefault( expected );

        result.Should().BeSameAs( expected );
    }

    [Theory]
    [InlineData( "foo", "foo", true )]
    [InlineData( "foo", "bar", false )]
    [InlineData( 42, "foo", false )]
    public void EqualityOperator_TypeErased_ShouldReturnCorrectResult_ForNonEmpty(object? a, object? b, bool expected)
    {
        var result = new SqlScalarResult( a ) == new SqlScalarResult( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( false, true, false )]
    [InlineData( true, false, false )]
    public void EqualityOperator_TypeErased_ShouldReturnCorrectResult_ForEmpty(bool isFirstEmpty, bool isSecondEmpty, bool expected)
    {
        var a = isFirstEmpty ? SqlScalarResult.Empty : new SqlScalarResult( null );
        var b = isSecondEmpty ? SqlScalarResult.Empty : new SqlScalarResult( null );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "foo", false )]
    [InlineData( "foo", "bar", true )]
    [InlineData( 42, "foo", true )]
    public void InequalityOperator_TypeErased_ShouldReturnCorrectResult_ForNonEmpty(object? a, object? b, bool expected)
    {
        var result = new SqlScalarResult( a ) != new SqlScalarResult( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( false, true, true )]
    [InlineData( true, false, true )]
    public void InequalityOperator_TypeErased_ShouldReturnCorrectResult_ForEmpty(bool isFirstEmpty, bool isSecondEmpty, bool expected)
    {
        var a = isFirstEmpty ? SqlScalarResult.Empty : new SqlScalarResult( null );
        var b = isSecondEmpty ? SqlScalarResult.Empty : new SqlScalarResult( null );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Fact]
    public void Default_Generic_ShouldNotHaveValue()
    {
        var sut = default( SqlScalarResult<string> );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.Value.Should().BeNull();
        }
    }

    [Fact]
    public void Empty_Generic_ShouldNotHaveValue()
    {
        var sut = SqlScalarResult<string>.Empty;

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
        var sut = new SqlScalarResult<string>( value );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeTrue();
            sut.Value.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ToString_Generic_ShouldReturnCorrectResult_ForEmpty()
    {
        var sut = SqlScalarResult<string>.Empty;
        var result = sut.ToString();
        result.Should().Be( "Empty<System.String>()" );
    }

    [Fact]
    public void ToString_Generic_ShouldReturnCorrectResult_ForNonEmpty()
    {
        var sut = new SqlScalarResult<string>( "foo" );
        var result = sut.ToString();
        result.Should().Be( "Value<System.String>(foo)" );
    }

    [Fact]
    public void GetHashCode_Generic_ShouldReturnCorrectResult()
    {
        var sut = new SqlScalarResult<string>( "foo" );
        var expected = HashCode.Combine( true, "foo" );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetValue_Generic_ShouldReturnValue_WhenItExists()
    {
        var expected = "foo";
        var sut = new SqlScalarResult<string>( expected );

        var result = sut.GetValue();

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void GetValue_Generic_ShouldThrowInvalidOperationException_WhenValueDoesNotExist()
    {
        var sut = SqlScalarResult<string>.Empty;
        var action = Lambda.Of( () => sut.GetValue() );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetValueOrDefault_Generic_ShouldReturnValue_WhenItExists()
    {
        var expected = "foo";
        var sut = new SqlScalarResult<string>( expected );

        var result = sut.GetValueOrDefault( "bar" );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void GetValueOrDefault_Generic_ShouldReturnDefaultValue_WhenValueDoesNotExist()
    {
        var expected = "foo";
        var sut = SqlScalarResult<string>.Empty;

        var result = sut.GetValueOrDefault( expected );

        result.Should().BeSameAs( expected );
    }

    [Theory]
    [InlineData( "foo", "foo", true )]
    [InlineData( "foo", "bar", false )]
    public void EqualityOperator_Generic_ShouldReturnCorrectResult_ForNonEmpty(string? a, string? b, bool expected)
    {
        var result = new SqlScalarResult<string>( a ) == new SqlScalarResult<string>( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( false, true, false )]
    [InlineData( true, false, false )]
    public void EqualityOperator_Generic_ShouldReturnCorrectResult_ForEmpty(bool isFirstEmpty, bool isSecondEmpty, bool expected)
    {
        var a = isFirstEmpty ? SqlScalarResult<string>.Empty : new SqlScalarResult<string>( null );
        var b = isSecondEmpty ? SqlScalarResult<string>.Empty : new SqlScalarResult<string>( null );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "foo", false )]
    [InlineData( "foo", "bar", true )]
    public void InequalityOperator_Generic_ShouldReturnCorrectResult_ForNonEmpty(string? a, string? b, bool expected)
    {
        var result = new SqlScalarResult<string>( a ) != new SqlScalarResult<string>( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( false, true, true )]
    [InlineData( true, false, true )]
    public void InequalityOperator_Generic_ShouldReturnCorrectResult_ForEmpty(bool isFirstEmpty, bool isSecondEmpty, bool expected)
    {
        var a = isFirstEmpty ? SqlScalarResult<string>.Empty : new SqlScalarResult<string>( null );
        var b = isSecondEmpty ? SqlScalarResult<string>.Empty : new SqlScalarResult<string>( null );

        var result = a != b;

        result.Should().Be( expected );
    }
}
