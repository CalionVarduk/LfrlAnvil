using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectChangeDescriptorTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 6 )]
    [InlineData( 7 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    [InlineData( 10 )]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenKeyIsReserved(int key)
    {
        var action = Lambda.Of( () => SqlObjectChangeDescriptor<string>.Create( "foo", key ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 11 )]
    public void Create_ShouldReturnCorrectChangeDescriptor(int key)
    {
        var result = SqlObjectChangeDescriptor<string>.Create( "foo", key );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be<string>();
            result.Key.Should().Be( key );
            result.Description.Should().Be( "foo" );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = SqlObjectChangeDescriptor.IsNullable;
        var result = sut.ToString();
        result.Should().Be( "[2] : 'IsNullable' (System.Boolean)" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = SqlObjectChangeDescriptor.IsNullable;
        var result = sut.GetHashCode();
        result.Should().Be( sut.Key.GetHashCode() );
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenChangeDescriptorsAreTheSame()
    {
        var a = SqlObjectChangeDescriptor.Name;
        var b = SqlObjectChangeDescriptor.Name;

        var result = a == b;

        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenFirstChangeDescriptorIsNull()
    {
        var a = SqlObjectChangeDescriptor.Name;
        SqlObjectChangeDescriptor? b = null;

        var result = a == b;

        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenSecondChangeDescriptorIsNull()
    {
        SqlObjectChangeDescriptor? a = null;
        var b = SqlObjectChangeDescriptor.Name;

        var result = a == b;

        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenBothChangeDescriptorsAreNull()
    {
        SqlObjectChangeDescriptor? a = null;
        SqlObjectChangeDescriptor? b = null;

        var result = a == b;

        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenChangeDescriptorsAreTheSame()
    {
        var a = SqlObjectChangeDescriptor.Name;
        var b = SqlObjectChangeDescriptor.Name;

        var result = a != b;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenFirstChangeDescriptorIsNull()
    {
        var a = SqlObjectChangeDescriptor.Name;
        SqlObjectChangeDescriptor? b = null;

        var result = a != b;

        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenSecondChangeDescriptorIsNull()
    {
        SqlObjectChangeDescriptor? a = null;
        var b = SqlObjectChangeDescriptor.Name;

        var result = a != b;

        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenBothChangeDescriptorsAreNull()
    {
        SqlObjectChangeDescriptor? a = null;
        SqlObjectChangeDescriptor? b = null;

        var result = a != b;

        result.Should().BeFalse();
    }
}
