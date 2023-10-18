using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class MemorySizeTests : TestsBase
{
    [Fact]
    public void Zero_ShouldReturnZeroBytes()
    {
        var sut = MemorySize.Zero;
        sut.Bytes.Should().Be( 0 );
    }

    [Theory]
    [InlineData( -1073741824 )]
    [InlineData( 1073741824 )]
    public void Ctor_ShouldCreateCorrectResult(long value)
    {
        var sut = new MemorySize( value );
        sut.Bytes.Should().Be( value );
    }

    [Theory]
    [InlineData( -1073741824 )]
    [InlineData( 1073741824 )]
    public void FromBytes_WithInt64_ShouldCreateCorrectResult(long value)
    {
        var sut = MemorySize.FromBytes( value );
        sut.Bytes.Should().Be( value );
    }

    [Theory]
    [InlineData( -1073741824.0, -1073741824 )]
    [InlineData( -1073741824.49, -1073741824 )]
    [InlineData( -1073741824.5, -1073741825 )]
    [InlineData( 1073741824.0, 1073741824 )]
    [InlineData( 1073741824.49, 1073741824 )]
    [InlineData( 1073741824.5, 1073741825 )]
    public void FromBytes_WithDouble_ShouldCreateRoundedResult(double value, long expected)
    {
        var sut = MemorySize.FromBytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1048576, -1073741824 )]
    [InlineData( 1048576, 1073741824 )]
    public void FromKilobytes_WithInt64_ShouldCreateCorrectResult(long value, long expected)
    {
        var sut = MemorySize.FromKilobytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1048576.0, -1073741824 )]
    [InlineData( -1048576.00047, -1073741824 )]
    [InlineData( -1048576.00049, -1073741825 )]
    [InlineData( 1048576.0, 1073741824 )]
    [InlineData( 1048576.00047, 1073741824 )]
    [InlineData( 1048576.00049, 1073741825 )]
    public void FromKilobytes_WithDouble_ShouldCreateRoundedResult(double value, long expected)
    {
        var sut = MemorySize.FromKilobytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1024, -1073741824 )]
    [InlineData( 1024, 1073741824 )]
    public void FromMegabytes_WithInt64_ShouldCreateCorrectResult(long value, long expected)
    {
        var sut = MemorySize.FromMegabytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1024.0, -1073741824 )]
    [InlineData( -1024.00000046, -1073741824 )]
    [InlineData( -1024.00000048, -1073741825 )]
    [InlineData( 1024.0, 1073741824 )]
    [InlineData( 1024.00000046, 1073741824 )]
    [InlineData( 1024.00000048, 1073741825 )]
    public void FromMegabytes_WithDouble_ShouldCreateRoundedResult(double value, long expected)
    {
        var sut = MemorySize.FromMegabytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Theory]
    [InlineData( -16, -17179869184 )]
    [InlineData( 16, 17179869184 )]
    public void FromGigabytes_WithInt64_ShouldCreateCorrectResult(long value, long expected)
    {
        var sut = MemorySize.FromGigabytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Theory]
    [InlineData( -16.0, -17179869184 )]
    [InlineData( -16.00000000045, -17179869184 )]
    [InlineData( -16.00000000047, -17179869185 )]
    [InlineData( 16.0, 17179869184 )]
    [InlineData( 16.00000000045, 17179869184 )]
    [InlineData( 16.00000000047, 17179869185 )]
    public void FromGigabytes_WithDouble_ShouldCreateRoundedResult(double value, long expected)
    {
        var sut = MemorySize.FromGigabytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Theory]
    [InlineData( -4, -4398046511104 )]
    [InlineData( 4, 4398046511104 )]
    public void FromTerabytes_WithInt64_ShouldCreateCorrectResult(long value, long expected)
    {
        var sut = MemorySize.FromTerabytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Theory]
    [InlineData( -4.0, -4398046511104 )]
    [InlineData( -4.00000000000044, -4398046511104 )]
    [InlineData( -4.00000000000046, -4398046511105 )]
    [InlineData( 4.0, 4398046511104 )]
    [InlineData( 4.00000000000044, 4398046511104 )]
    [InlineData( 4.00000000000046, 4398046511105 )]
    public void FromTerabytes_WithDouble_ShouldCreateRoundedResult(double value, long expected)
    {
        var sut = MemorySize.FromTerabytes( value );
        sut.Bytes.Should().Be( expected );
    }

    [Fact]
    public void Properties_ShouldConvertBytesCorrectly()
    {
        var sut = MemorySize.FromBytes( 4673730511616 );

        using ( new AssertionScope() )
        {
            sut.Bytes.Should().Be( 4673730511616 );
            sut.TotalKilobytes.Should().Be( 4564189952.75 );
            sut.TotalMegabytes.Should().Be( 4457216.750732421875 );
            sut.TotalGigabytes.Should().Be( 4352.7507331371307373046875 );
            sut.TotalTerabytes.Should().Be( 4.2507331378292292356491088867188 );
            sut.FullKilobytes.Should().Be( 4564189952 );
            sut.FullMegabytes.Should().Be( 4457216 );
            sut.FullGigabytes.Should().Be( 4352 );
            sut.FullTerabytes.Should().Be( 4 );
            sut.BytesInKilobyte.Should().Be( 768 );
            sut.BytesInMegabyte.Should().Be( 787200 );
            sut.BytesInGigabyte.Should().Be( 806093568 );
            sut.BytesInTerabyte.Should().Be( 275684000512 );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = MemorySize.FromBytes( 1234 );
        var result = sut.ToString();
        result.Should().Be( "1234 B" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = MemorySize.FromBytes( 1234 );
        var result = sut.GetHashCode();
        result.Should().Be( sut.Bytes.GetHashCode() );
    }

    [Theory]
    [InlineData( 110, 111, false )]
    [InlineData( 111, 110, false )]
    [InlineData( 110, 110, true )]
    public void Equals_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var result = MemorySize.FromBytes( a ).Equals( MemorySize.FromBytes( b ) );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 110, 111, -1 )]
    [InlineData( 111, 110, 1 )]
    [InlineData( 110, 110, 0 )]
    public void CompareTo_ShouldReturnCorrectResult(long a, long b, int expectedSign)
    {
        var result = MemorySize.FromBytes( a ).CompareTo( MemorySize.FromBytes( b ) );
        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Fact]
    public void Add_ShouldReturnCorrectResult()
    {
        var sut = MemorySize.FromBytes( 123 );
        var other = MemorySize.FromBytes( 456 );

        var result = sut.Add( other );

        result.Bytes.Should().Be( 579 );
    }

    [Fact]
    public void Subtract_ShouldReturnCorrectResult()
    {
        var sut = MemorySize.FromBytes( 123 );
        var other = MemorySize.FromBytes( 456 );

        var result = sut.Subtract( other );

        result.Bytes.Should().Be( -333 );
    }

    [Fact]
    public void AddOperator_ShouldReturnCorrectResult()
    {
        var sut = MemorySize.FromBytes( 123 );
        var other = MemorySize.FromBytes( 456 );

        var result = sut + other;

        result.Bytes.Should().Be( 579 );
    }

    [Fact]
    public void SubtractOperator_ShouldReturnCorrectResult()
    {
        var sut = MemorySize.FromBytes( 123 );
        var other = MemorySize.FromBytes( 456 );

        var result = sut - other;

        result.Bytes.Should().Be( -333 );
    }

    [Theory]
    [InlineData( 110, 111, false )]
    [InlineData( 111, 110, false )]
    [InlineData( 110, 110, true )]
    public void EqualityOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var result = MemorySize.FromBytes( a ) == MemorySize.FromBytes( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 110, 111, true )]
    [InlineData( 111, 110, true )]
    [InlineData( 110, 110, false )]
    public void InequalityOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var result = MemorySize.FromBytes( a ) != MemorySize.FromBytes( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 110, 111, false )]
    [InlineData( 111, 110, true )]
    [InlineData( 110, 110, true )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var result = MemorySize.FromBytes( a ) >= MemorySize.FromBytes( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 110, 111, true )]
    [InlineData( 111, 110, false )]
    [InlineData( 110, 110, true )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var result = MemorySize.FromBytes( a ) <= MemorySize.FromBytes( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 110, 111, false )]
    [InlineData( 111, 110, true )]
    [InlineData( 110, 110, false )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var result = MemorySize.FromBytes( a ) > MemorySize.FromBytes( b );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 110, 111, true )]
    [InlineData( 111, 110, false )]
    [InlineData( 110, 110, false )]
    public void LessThanOperator_ShouldReturnCorrectResult(long a, long b, bool expected)
    {
        var result = MemorySize.FromBytes( a ) < MemorySize.FromBytes( b );
        result.Should().Be( expected );
    }
}
