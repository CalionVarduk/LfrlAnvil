using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sql.Tests;

public class SqlColumnTypeDefinitionTests : TestsBase
{
    [Fact]
    public void TryToNullableParameterValue_ShouldReturnTryToParameterValueResult_WhenValueIsNotNull()
    {
        var sut = Substitute.For<ISqlColumnTypeDefinition>();
        sut.TryToParameterValue( Arg.Any<object>() ).Returns( "foo" );

        var result = sut.TryToNullableParameterValue( "bar" );

        result.Should().Be( "foo" );
    }

    [Fact]
    public void TryToNullableParameterValue_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = Substitute.For<ISqlColumnTypeDefinition>();
        sut.TryToParameterValue( Arg.Any<object>() ).Returns( "foo" );

        var result = sut.TryToNullableParameterValue( null );

        result.Should().BeSameAs( DBNull.Value );
    }

    [Fact]
    public void ToNullableParameterValue_ForRefType_ShouldReturnToParameterValueResult_WhenValueIsNotNull()
    {
        var sut = Substitute.For<ISqlColumnTypeDefinition<string>>();
        sut.ToParameterValue( Arg.Any<string>() ).Returns( "foo" );

        var result = sut.ToNullableParameterValue( "bar" );

        result.Should().Be( "foo" );
    }

    [Fact]
    public void ToNullableParameterValue_ForRefType_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = Substitute.For<ISqlColumnTypeDefinition<string>>();
        sut.ToParameterValue( Arg.Any<string>() ).Returns( "foo" );

        var result = sut.ToNullableParameterValue( null );

        result.Should().BeSameAs( DBNull.Value );
    }

    [Fact]
    public void ToNullableParameterValue_ForValueType_ShouldReturnToParameterValueResult_WhenValueIsNotNull()
    {
        var sut = Substitute.For<ISqlColumnTypeDefinition<int>>();
        sut.ToParameterValue( Arg.Any<int>() ).Returns( 1 );

        var result = sut.ToNullableParameterValue( 0 );

        result.Should().Be( 1 );
    }

    [Fact]
    public void ToNullableParameterValue_ForValueType_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = Substitute.For<ISqlColumnTypeDefinition<int>>();
        sut.ToParameterValue( Arg.Any<int>() ).Returns( 123 );

        var result = sut.ToNullableParameterValue( null );

        result.Should().BeSameAs( DBNull.Value );
    }
}
