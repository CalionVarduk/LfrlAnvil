using System.Linq;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.AsyncTests;

public class InterlockedBooleanTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeFalse()
    {
        var sut = default( InterlockedBoolean );
        sut.Value.Should().BeFalse();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Ctor_ShouldCreateWithCorrectValue(bool value)
    {
        var sut = new InterlockedBoolean( value );
        sut.Value.Should().Be( value );
    }

    [Theory]
    [InlineData( true, "True" )]
    [InlineData( false, "False" )]
    public void ToString_ShouldReturnCorrectResult(bool value, string expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void GetHashCode_ShouldReturnCorrectResult(bool value)
    {
        var a = new InterlockedBoolean( value );
        var b = new InterlockedBoolean( value );
        b.Toggle();
        b.Toggle();
        var expected = value.GetHashCode();

        var result1 = a.GetHashCode();
        var result2 = b.GetHashCode();

        using ( new AssertionScope() )
        {
            result1.Should().Be( expected );
            result2.Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( false, true )]
    [InlineData( true, false )]
    public void WriteTrue_ShouldUpdateValueToTrueAndReturnTrueOnlyWhenValueWasChanged(bool value, bool expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.WriteTrue();

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            sut.Value.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( false, false )]
    [InlineData( true, true )]
    public void WriteFalse_ShouldUpdateValueToFalseAndReturnTrueOnlyWhenValueWasChanged(bool value, bool expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.WriteFalse();

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            sut.Value.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( true, false )]
    [InlineData( false, true )]
    public void Toggle_ShouldNegateValueAndReturnNewValue(bool value, bool expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.Toggle();

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            sut.Value.Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Toggle_CalledMultipleTimes_ShouldBehaveCorrectly(bool value)
    {
        var values = new bool[100];
        var sut = new InterlockedBoolean( value );

        for ( var i = 0; i < values.Length; ++i )
            values[i] = sut.Toggle();

        using ( new AssertionScope() )
        {
            values.Where( (_, i) => i.IsEven() ).Should().AllBeEquivalentTo( ! value );
            values.Where( (_, i) => i.IsOdd() ).Should().AllBeEquivalentTo( value );
            sut.Value.Should().Be( values[^1] );
        }
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( true, false, false )]
    [InlineData( false, true, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(bool a, bool b, bool expected)
    {
        var left = new InterlockedBoolean( a );
        var right = new InterlockedBoolean( b );

        var result = left == right;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( true, false, true )]
    [InlineData( false, true, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(bool a, bool b, bool expected)
    {
        var left = new InterlockedBoolean( a );
        var right = new InterlockedBoolean( b );

        var result = left != right;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( true, false, true )]
    [InlineData( false, true, false )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(bool a, bool b, bool expected)
    {
        var left = new InterlockedBoolean( a );
        var right = new InterlockedBoolean( b );

        var result = left >= right;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( true, false, false )]
    [InlineData( false, true, true )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(bool a, bool b, bool expected)
    {
        var left = new InterlockedBoolean( a );
        var right = new InterlockedBoolean( b );

        var result = left <= right;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( true, false, true )]
    [InlineData( false, true, false )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(bool a, bool b, bool expected)
    {
        var left = new InterlockedBoolean( a );
        var right = new InterlockedBoolean( b );

        var result = left > right;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( true, false, false )]
    [InlineData( false, true, true )]
    public void LessThanOperator_ShouldReturnCorrectResult(bool a, bool b, bool expected)
    {
        var left = new InterlockedBoolean( a );
        var right = new InterlockedBoolean( b );

        var result = left < right;

        result.Should().Be( expected );
    }
}
