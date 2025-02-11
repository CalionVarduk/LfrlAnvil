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
        sut.Value.TestFalse().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Ctor_ShouldCreateWithCorrectValue(bool value)
    {
        var sut = new InterlockedBoolean( value );
        sut.Value.TestEquals( value ).Go();
    }

    [Theory]
    [InlineData( true, "True" )]
    [InlineData( false, "False" )]
    public void ToString_ShouldReturnCorrectResult(bool value, string expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.ToString();
        result.TestEquals( expected ).Go();
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

        Assertion.All(
                result1.TestEquals( expected ),
                result2.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [InlineData( false, true )]
    [InlineData( true, false )]
    public void WriteTrue_ShouldUpdateValueToTrueAndReturnTrueOnlyWhenValueWasChanged(bool value, bool expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.WriteTrue();

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( false, false )]
    [InlineData( true, true )]
    public void WriteFalse_ShouldUpdateValueToFalseAndReturnTrueOnlyWhenValueWasChanged(bool value, bool expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.WriteFalse();

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( false, true, true )]
    [InlineData( false, false, false )]
    [InlineData( true, false, true )]
    [InlineData( true, true, false )]
    public void Write_ShouldUpdateValueAndReturnTrueOnlyWhenValueWasChanged(bool value, bool newValue, bool expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.Write( newValue );

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestEquals( newValue ) )
            .Go();
    }

    [Theory]
    [InlineData( true, false )]
    [InlineData( false, true )]
    public void Toggle_ShouldNegateValueAndReturnNewValue(bool value, bool expected)
    {
        var sut = new InterlockedBoolean( value );
        var result = sut.Toggle();

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestEquals( expected ) )
            .Go();
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

        Assertion.All(
                values.Where( (_, i) => i.IsEven() ).TestAll( (e, _) => e.TestNotEquals( value ) ),
                values.Where( (_, i) => i.IsOdd() ).TestAll( (e, _) => e.TestEquals( value ) ),
                sut.Value.TestEquals( values[^1] ) )
            .Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
    }
}
