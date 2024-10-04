// Copyright 2024 Łukasz Furlepa
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using LfrlAnvil.Internal;

namespace LfrlAnvil.Tests.NullableIndexTests;

public class NullableIndexTests : TestsBase
{
    [Fact]
    public void Null_ShouldNotHaveValue()
    {
        var sut = NullableIndex.Null;

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.ToString().Should().Be( "NULL" );
        }
    }

    [Theory]
    [InlineData( int.MinValue )]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    [InlineData( int.MaxValue - 1 )]
    public void Create_ShouldCreateIndex(int value)
    {
        var sut = NullableIndex.Create( value );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeTrue();
            sut.Value.Should().Be( value );
            sut.ToString().Should().Be( value.ToString() );
        }
    }

    [Theory]
    [InlineData( int.MinValue )]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    [InlineData( int.MaxValue - 1 )]
    public void Create_FromNullableInt_ShouldCreateIndex(int? value)
    {
        var sut = NullableIndex.Create( value );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeTrue();
            sut.Value.Should().Be( value );
            sut.ToString().Should().Be( value.ToString() );
        }
    }

    [Fact]
    public void Create_FromNullableInt_ShouldAllowToCreateNull()
    {
        var sut = NullableIndex.Create( null );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.ToString().Should().Be( "NULL" );
        }
    }

    [Theory]
    [InlineData( int.MinValue )]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    [InlineData( int.MaxValue - 1 )]
    public void CreateUnsafe_ShouldCreateIndex(int value)
    {
        var sut = NullableIndex.CreateUnsafe( value );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeTrue();
            sut.Value.Should().Be( value );
            sut.ToString().Should().Be( value.ToString() );
        }
    }

    [Fact]
    public void CreateUnsafe_ShouldAllowToCreateNull()
    {
        var sut = NullableIndex.CreateUnsafe( NullableIndex.NullValue );

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeFalse();
            sut.ToString().Should().Be( "NULL" );
        }
    }

    [Theory]
    [InlineData( 123 )]
    [InlineData( NullableIndex.NullValue )]
    public void GetHashCode_ShouldReturnCorrectResult(int value)
    {
        var sut = NullableIndex.CreateUnsafe( value );
        var result = sut.GetHashCode();
        result.Should().Be( value.GetHashCode() );
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 124, false )]
    [InlineData( 124, 123, false )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, true )]
    [InlineData( NullableIndex.NullValue, 123, false )]
    [InlineData( 123, NullableIndex.NullValue, false )]
    public void Equals_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 123, 0 )]
    [InlineData( 123, 124, -1 )]
    [InlineData( 124, 123, 1 )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, 0 )]
    [InlineData( NullableIndex.NullValue, 123, 1 )]
    [InlineData( 123, NullableIndex.NullValue, -1 )]
    public void CompareTo_ShouldReturnCorrectResult(int val1, int val2, int expectedSign)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Theory]
    [InlineData( 123, 123 )]
    [InlineData( NullableIndex.NullValue, null )]
    public void NullableIntConversionOperator_ShouldReturnCorrectResult(int value, int? expected)
    {
        var sut = NullableIndex.CreateUnsafe( value );
        var result = ( int? )sut;
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 124 )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue )]
    public void IncrementOperator_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = NullableIndex.CreateUnsafe( value );
        sut++;
        sut.Should().Be( NullableIndex.CreateUnsafe( expected ) );
    }

    [Theory]
    [InlineData( 123, 122 )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue )]
    public void DecrementOperator_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = NullableIndex.CreateUnsafe( value );
        sut--;
        sut.Should().Be( NullableIndex.CreateUnsafe( expected ) );
    }

    [Theory]
    [InlineData( 123, 17, 140 )]
    [InlineData( 123, NullableIndex.NullValue, NullableIndex.NullValue )]
    [InlineData( NullableIndex.NullValue, 123, NullableIndex.NullValue )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, NullableIndex.NullValue )]
    public void AddOperator_ShouldReturnCorrectResult(int val1, int val2, int expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a + b;

        result.Should().Be( NullableIndex.CreateUnsafe( expected ) );
    }

    [Theory]
    [InlineData( 123, 13, 110 )]
    [InlineData( 123, NullableIndex.NullValue, NullableIndex.NullValue )]
    [InlineData( NullableIndex.NullValue, 123, NullableIndex.NullValue )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, NullableIndex.NullValue )]
    public void SubtractOperator_ShouldReturnCorrectResult(int val1, int val2, int expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a - b;

        result.Should().Be( NullableIndex.CreateUnsafe( expected ) );
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 124, false )]
    [InlineData( 124, 123, false )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, true )]
    [InlineData( NullableIndex.NullValue, 123, false )]
    [InlineData( 123, NullableIndex.NullValue, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 124, true )]
    [InlineData( 124, 123, true )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, false )]
    [InlineData( NullableIndex.NullValue, 123, true )]
    [InlineData( 123, NullableIndex.NullValue, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 124, false )]
    [InlineData( 124, 123, true )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, false )]
    [InlineData( NullableIndex.NullValue, 123, true )]
    [InlineData( 123, NullableIndex.NullValue, false )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 124, true )]
    [InlineData( 124, 123, false )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, true )]
    [InlineData( NullableIndex.NullValue, 123, false )]
    [InlineData( 123, NullableIndex.NullValue, true )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 124, true )]
    [InlineData( 124, 123, false )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, false )]
    [InlineData( NullableIndex.NullValue, 123, false )]
    [InlineData( 123, NullableIndex.NullValue, true )]
    public void LessThanOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 124, false )]
    [InlineData( 124, 123, true )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, true )]
    [InlineData( NullableIndex.NullValue, 123, true )]
    [InlineData( 123, NullableIndex.NullValue, false )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.CreateUnsafe( val1 );
        var b = NullableIndex.CreateUnsafe( val2 );

        var result = a >= b;

        result.Should().Be( expected );
    }
}
