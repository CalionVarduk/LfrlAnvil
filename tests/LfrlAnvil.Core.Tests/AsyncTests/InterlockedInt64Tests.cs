using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class InterlockedInt64Tests : TestsBase
{
    [Fact]
    public void Default_ShouldBeZero()
    {
        var sut = default( InterlockedInt64 );
        sut.Value.TestEquals( 0 ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( -10 )]
    [InlineData( 123 )]
    public void Ctor_ShouldCreateWithCorrectValue(long value)
    {
        var sut = new InterlockedInt64( value );
        sut.Value.TestEquals( value ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new InterlockedInt64( 123 );
        var result = sut.ToString();
        result.TestEquals( "123" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = new InterlockedInt64( 123 );
        var expected = 123.GetHashCode();

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123 )]
    [InlineData( 123, 456 )]
    [InlineData( 123, 789 )]
    [InlineData( 789, 456 )]
    public void Exchange_ShouldUpdateValueAndReturnOldValue(long value, long newValue)
    {
        var sut = new InterlockedInt64( value );
        var result = sut.Exchange( newValue );

        Assertion.All(
                result.TestEquals( value ),
                sut.Value.TestEquals( newValue ) )
            .Go();
    }

    [Theory]
    [InlineData( 123, 123, 123, 123 )]
    [InlineData( 123, 456, 123, 456 )]
    [InlineData( 123, 456, 789, 123 )]
    [InlineData( 789, 456, 123, 789 )]
    [InlineData( 456, 789, 456, 789 )]
    public void CompareExchange_ShouldUpdateValueWhenOldValueEqualsComparandAndReturnOldValue(
        long value,
        long newValue,
        long comparand,
        long expected)
    {
        var sut = new InterlockedInt64( value );
        var result = sut.CompareExchange( newValue, comparand );

        Assertion.All(
                result.TestEquals( value ),
                sut.Value.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 456, true )]
    [InlineData( 123, 789, true )]
    [InlineData( 789, 456, true )]
    public void Write_ShouldReturnTrueWhenValueWasChanged(long value, long newValue, bool expected)
    {
        var sut = new InterlockedInt64( value );
        var result = sut.Write( newValue );

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestEquals( newValue ) )
            .Go();
    }

    [Theory]
    [InlineData( 123, 123, 123, 123, false )]
    [InlineData( 123, 456, 123, 456, true )]
    [InlineData( 123, 456, 789, 123, false )]
    [InlineData( 789, 456, 123, 789, false )]
    [InlineData( 456, 789, 456, 789, true )]
    public void Write_WithComparand_ShouldReturnTrueWhenValueWasChangedDueToOldValueBeingEqualToComparand(
        long value,
        long newValue,
        long comparand,
        long expectedValue,
        bool expected)
    {
        var sut = new InterlockedInt64( value );
        var result = sut.Write( newValue, comparand );

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestEquals( expectedValue ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    public void Increment_ShouldAddOneToValueAndReturnNewValue(long oldValue)
    {
        var sut = new InterlockedInt64( oldValue );
        var result = sut.Increment();

        Assertion.All(
                result.TestEquals( oldValue + 1 ),
                sut.Value.TestEquals( oldValue + 1 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    public void Decrement_ShouldSubtractOneFromValueAndReturnNewValue(long oldValue)
    {
        var sut = new InterlockedInt64( oldValue );
        var result = sut.Decrement();

        Assertion.All(
                result.TestEquals( oldValue - 1 ),
                sut.Value.TestEquals( oldValue - 1 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    public void Add_ShouldAddToValueAndReturnNewValue(long oldValue)
    {
        var sut = new InterlockedInt64( oldValue );
        var result = sut.Add( 123 );

        Assertion.All(
                result.TestEquals( oldValue + 123 ),
                sut.Value.TestEquals( oldValue + 123 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    public void Subtract_ShouldSubtractFromValueAndReturnNewValue(long oldValue)
    {
        var sut = new InterlockedInt64( oldValue );
        var result = sut.Subtract( 123 );

        Assertion.All(
                result.TestEquals( oldValue - 123 ),
                sut.Value.TestEquals( oldValue - 123 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 12 )]
    [InlineData( 123 )]
    public void And_ShouldBitwiseAndValueAndReturnOldValue(long oldValue)
    {
        var sut = new InterlockedInt64( oldValue );
        var result = sut.And( 123 );

        Assertion.All(
                result.TestEquals( oldValue ),
                sut.Value.TestEquals( oldValue & 123 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 12 )]
    [InlineData( 123 )]
    public void Or_ShouldBitwiseOrValueAndReturnOldValue(long oldValue)
    {
        var sut = new InterlockedInt64( oldValue );
        var result = sut.Or( 123 );

        Assertion.All(
                result.TestEquals( oldValue ),
                sut.Value.TestEquals( oldValue | 123 ) )
            .Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 456, false )]
    [InlineData( 789, 456, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var left = new InterlockedInt64( a );
        var right = new InterlockedInt64( b );

        var result = left == right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 456, true )]
    [InlineData( 789, 456, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var left = new InterlockedInt64( a );
        var right = new InterlockedInt64( b );

        var result = left != right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 456, false )]
    [InlineData( 789, 456, true )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var left = new InterlockedInt64( a );
        var right = new InterlockedInt64( b );

        var result = left >= right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 456, true )]
    [InlineData( 789, 456, false )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var left = new InterlockedInt64( a );
        var right = new InterlockedInt64( b );

        var result = left <= right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 456, false )]
    [InlineData( 789, 456, true )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var left = new InterlockedInt64( a );
        var right = new InterlockedInt64( b );

        var result = left > right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 456, true )]
    [InlineData( 789, 456, false )]
    public void LessThanOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var left = new InterlockedInt64( a );
        var right = new InterlockedInt64( b );

        var result = left < right;

        result.TestEquals( expected ).Go();
    }
}
