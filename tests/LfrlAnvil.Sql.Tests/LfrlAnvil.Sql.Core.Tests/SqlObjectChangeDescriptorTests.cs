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
    [InlineData( 11 )]
    [InlineData( 12 )]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenKeyIsReserved(int key)
    {
        var action = Lambda.Of( () => SqlObjectChangeDescriptor<string>.Create( "foo", key ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 13 )]
    public void Create_ShouldReturnCorrectChangeDescriptor(int key)
    {
        var result = SqlObjectChangeDescriptor<string>.Create( "foo", key );

        Assertion.All(
                result.Type.TestEquals( typeof( string ) ),
                result.Key.TestEquals( key ),
                result.Description.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = SqlObjectChangeDescriptor.IsNullable;
        var result = sut.ToString();
        result.TestEquals( "[2] : 'IsNullable' (System.Boolean)" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = SqlObjectChangeDescriptor.IsNullable;
        var result = sut.GetHashCode();
        result.TestEquals( sut.Key.GetHashCode() ).Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenChangeDescriptorsAreTheSame()
    {
        var a = SqlObjectChangeDescriptor.Name;
        var b = SqlObjectChangeDescriptor.Name;

        var result = a == b;

        result.TestTrue().Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenFirstChangeDescriptorIsNull()
    {
        var a = SqlObjectChangeDescriptor.Name;
        SqlObjectChangeDescriptor? b = null;

        var result = a == b;

        result.TestFalse().Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenSecondChangeDescriptorIsNull()
    {
        SqlObjectChangeDescriptor? a = null;
        var b = SqlObjectChangeDescriptor.Name;

        var result = a == b;

        result.TestFalse().Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenBothChangeDescriptorsAreNull()
    {
        SqlObjectChangeDescriptor? a = null;
        SqlObjectChangeDescriptor? b = null;

        var result = a == b;

        result.TestTrue().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenChangeDescriptorsAreTheSame()
    {
        var a = SqlObjectChangeDescriptor.Name;
        var b = SqlObjectChangeDescriptor.Name;

        var result = a != b;

        result.TestFalse().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenFirstChangeDescriptorIsNull()
    {
        var a = SqlObjectChangeDescriptor.Name;
        SqlObjectChangeDescriptor? b = null;

        var result = a != b;

        result.TestTrue().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenSecondChangeDescriptorIsNull()
    {
        SqlObjectChangeDescriptor? a = null;
        var b = SqlObjectChangeDescriptor.Name;

        var result = a != b;

        result.TestTrue().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenBothChangeDescriptorsAreNull()
    {
        SqlObjectChangeDescriptor? a = null;
        SqlObjectChangeDescriptor? b = null;

        var result = a != b;

        result.TestFalse().Go();
    }
}
