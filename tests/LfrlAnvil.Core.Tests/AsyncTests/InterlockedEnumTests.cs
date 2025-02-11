using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class InterlockedEnumTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeDefaultEnum()
    {
        var sut = default( InterlockedEnum<Foo> );
        sut.Value.TestEquals( Foo.A ).Go();
    }

    [Theory]
    [InlineData( Foo.A )]
    [InlineData( Foo.B )]
    [InlineData( Foo.C )]
    public void Ctor_ShouldCreateWithCorrectValue(Foo value)
    {
        var sut = new InterlockedEnum<Foo>( value );
        sut.Value.TestEquals( value ).Go();
    }

    [Theory]
    [InlineData( Foo.A, "A" )]
    [InlineData( Foo.B, "B" )]
    [InlineData( Foo.C, "C" )]
    public void ToString_ShouldReturnCorrectResult(Foo value, string expected)
    {
        var sut = new InterlockedEnum<Foo>( value );
        var result = sut.ToString();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = new InterlockedEnum<Foo>( Foo.B );
        var expected = Foo.B.GetHashCode();

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A )]
    [InlineData( Foo.A, Foo.B )]
    [InlineData( Foo.A, Foo.C )]
    [InlineData( Foo.C, Foo.B )]
    public void Exchange_ShouldUpdateValueAndReturnOldValue(Foo value, Foo newValue)
    {
        var sut = new InterlockedEnum<Foo>( value );
        var result = sut.Exchange( newValue );

        Assertion.All(
                result.TestEquals( value ),
                sut.Value.TestEquals( newValue ) )
            .Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, Foo.A, Foo.A )]
    [InlineData( Foo.A, Foo.B, Foo.A, Foo.B )]
    [InlineData( Foo.A, Foo.B, Foo.C, Foo.A )]
    [InlineData( Foo.C, Foo.B, Foo.A, Foo.C )]
    [InlineData( Foo.B, Foo.C, Foo.B, Foo.C )]
    public void CompareExchange_ShouldUpdateValueWhenOldValueEqualsComparandAndReturnOldValue(
        Foo value,
        Foo newValue,
        Foo comparand,
        Foo expected)
    {
        var sut = new InterlockedEnum<Foo>( value );
        var result = sut.CompareExchange( newValue, comparand );

        Assertion.All(
                result.TestEquals( value ),
                sut.Value.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, false )]
    [InlineData( Foo.A, Foo.B, true )]
    [InlineData( Foo.A, Foo.C, true )]
    [InlineData( Foo.C, Foo.B, true )]
    public void Write_ShouldReturnTrueWhenValueWasChanged(Foo value, Foo newValue, bool expected)
    {
        var sut = new InterlockedEnum<Foo>( value );
        var result = sut.Write( newValue );

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestEquals( newValue ) )
            .Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, Foo.A, Foo.A, true )]
    [InlineData( Foo.A, Foo.B, Foo.A, Foo.B, true )]
    [InlineData( Foo.A, Foo.B, Foo.C, Foo.A, false )]
    [InlineData( Foo.C, Foo.B, Foo.A, Foo.C, false )]
    [InlineData( Foo.B, Foo.C, Foo.B, Foo.C, true )]
    public void Write_WithComparand_ShouldReturnTrueWhenValueWasChangedDueToOldValueBeingEqualToComparand(
        Foo value,
        Foo newValue,
        Foo comparand,
        Foo expectedValue,
        bool expected)
    {
        var sut = new InterlockedEnum<Foo>( value );
        var result = sut.Write( newValue, comparand );

        Assertion.All(
                result.TestEquals( expected ),
                sut.Value.TestEquals( expectedValue ) )
            .Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, true )]
    [InlineData( Foo.A, Foo.B, false )]
    [InlineData( Foo.C, Foo.B, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(Foo a, Foo b, bool expected)
    {
        var left = new InterlockedEnum<Foo>( a );
        var right = new InterlockedEnum<Foo>( b );

        var result = left == right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, false )]
    [InlineData( Foo.A, Foo.B, true )]
    [InlineData( Foo.C, Foo.B, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(Foo a, Foo b, bool expected)
    {
        var left = new InterlockedEnum<Foo>( a );
        var right = new InterlockedEnum<Foo>( b );

        var result = left != right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, true )]
    [InlineData( Foo.A, Foo.B, false )]
    [InlineData( Foo.C, Foo.B, true )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(Foo a, Foo b, bool expected)
    {
        var left = new InterlockedEnum<Foo>( a );
        var right = new InterlockedEnum<Foo>( b );

        var result = left >= right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, true )]
    [InlineData( Foo.A, Foo.B, true )]
    [InlineData( Foo.C, Foo.B, false )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(Foo a, Foo b, bool expected)
    {
        var left = new InterlockedEnum<Foo>( a );
        var right = new InterlockedEnum<Foo>( b );

        var result = left <= right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, false )]
    [InlineData( Foo.A, Foo.B, false )]
    [InlineData( Foo.C, Foo.B, true )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(Foo a, Foo b, bool expected)
    {
        var left = new InterlockedEnum<Foo>( a );
        var right = new InterlockedEnum<Foo>( b );

        var result = left > right;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( Foo.A, Foo.A, false )]
    [InlineData( Foo.A, Foo.B, true )]
    [InlineData( Foo.C, Foo.B, false )]
    public void LessThanOperator_ShouldReturnCorrectResult(Foo a, Foo b, bool expected)
    {
        var left = new InterlockedEnum<Foo>( a );
        var right = new InterlockedEnum<Foo>( b );

        var result = left < right;

        result.TestEquals( expected ).Go();
    }

    public enum Foo
    {
        A = 0,
        B = 1,
        C = 2
    }
}
