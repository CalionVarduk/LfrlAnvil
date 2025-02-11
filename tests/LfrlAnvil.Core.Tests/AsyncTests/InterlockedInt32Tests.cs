using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class InterlockedInt32Tests : TestsBase
{
    [Fact]
    public void Default_ShouldBeZero()
    {
        var sut = default( InterlockedInt32 );
        sut.Value.TestEquals( 0 ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( -10 )]
    [InlineData( 123 )]
    public void Ctor_ShouldCreateWithCorrectValue(int value)
    {
        var sut = new InterlockedInt32( value );
        sut.Value.TestEquals( value ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new InterlockedInt32( 123 );
        var result = sut.ToString();
        result.TestEquals( "123" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = new InterlockedInt32( 123 );
        var expected = 123.GetHashCode();

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123 )]
    [InlineData( 123, 456 )]
    [InlineData( 123, 789 )]
    [InlineData( 789, 456 )]
    public void Exchange_ShouldUpdateValueAndReturnOldValue(int value, int newValue)
    {
        var sut = new InterlockedInt32( value );
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
        int value,
        int newValue,
        int comparand,
        int expected)
    {
        var sut = new InterlockedInt32( value );
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
    public void Write_ShouldReturnTrueWhenValueWasChanged(int value, int newValue, bool expected)
    {
        var sut = new InterlockedInt32( value );
        var result = sut.Write( newValue );

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestEquals( newValue ) )
            .Go();
    }

    [Theory]
    [InlineData( 123, 123, 123, 123, true )]
    [InlineData( 123, 456, 123, 456, true )]
    [InlineData( 123, 456, 789, 123, false )]
    [InlineData( 789, 456, 123, 789, false )]
    [InlineData( 456, 789, 456, 789, true )]
    public void Write_WithComparand_ShouldReturnTrueWhenValueWasChangedDueToOldValueBeingEqualToComparand(
        int value,
        int newValue,
        int comparand,
        int expectedValue,
        bool expected)
    {
        var sut = new InterlockedInt32( value );
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
    public void Increment_ShouldAddOneToValueAndReturnNewValue(int oldValue)
    {
        var sut = new InterlockedInt32( oldValue );
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
    public void Decrement_ShouldSubtractOneFromValueAndReturnNewValue(int oldValue)
    {
        var sut = new InterlockedInt32( oldValue );
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
    public void Add_ShouldAddToValueAndReturnNewValue(int oldValue)
    {
        var sut = new InterlockedInt32( oldValue );
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
    public void Subtract_ShouldSubtractFromValueAndReturnNewValue(int oldValue)
    {
        var sut = new InterlockedInt32( oldValue );
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
    public void And_ShouldBitwiseAndValueAndReturnOldValue(int oldValue)
    {
        var sut = new InterlockedInt32( oldValue );
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
    public void Or_ShouldBitwiseOrValueAndReturnOldValue(int oldValue)
    {
        var sut = new InterlockedInt32( oldValue );
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
    public void EqualityOperator_ShouldReturnCorrectResult(int a, int b, bool expected)
    {
        var left = new InterlockedInt32( a );
        var right = new InterlockedInt32( b );

        var result = left == right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 456, true )]
    [InlineData( 789, 456, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(int a, int b, bool expected)
    {
        var left = new InterlockedInt32( a );
        var right = new InterlockedInt32( b );

        var result = left != right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 456, false )]
    [InlineData( 789, 456, true )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int a, int b, bool expected)
    {
        var left = new InterlockedInt32( a );
        var right = new InterlockedInt32( b );

        var result = left >= right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 456, true )]
    [InlineData( 789, 456, false )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int a, int b, bool expected)
    {
        var left = new InterlockedInt32( a );
        var right = new InterlockedInt32( b );

        var result = left <= right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 456, false )]
    [InlineData( 789, 456, true )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(int a, int b, bool expected)
    {
        var left = new InterlockedInt32( a );
        var right = new InterlockedInt32( b );

        var result = left > right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 456, true )]
    [InlineData( 789, 456, false )]
    public void LessThanOperator_ShouldReturnCorrectResult(int a, int b, bool expected)
    {
        var left = new InterlockedInt32( a );
        var right = new InterlockedInt32( b );

        var result = left < right;

        result.TestEquals( expected ).Go();
    }
}
