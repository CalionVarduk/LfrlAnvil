using System.Data;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests;

public class SqlColumnTypeDefinitionTests : TestsBase
{
    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void TrySetNullableParameter_ShouldCallTrySetParameter_WhenValueIsNotNull(bool expected)
    {
        var value = new object();
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = Substitute.For<ISqlColumnTypeDefinition>();
        sut.TrySetParameter( Arg.Any<IDbDataParameter>(), Arg.Any<object>() ).Returns( expected );

        var result = sut.TrySetNullableParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            sut.VerifyCalls().Received( o => o.TrySetParameter( parameter, value ), 1 );
            sut.VerifyCalls().DidNotReceive( o => o.SetNullParameter( Arg.Any<IDbDataParameter>() ) );
        }
    }

    [Fact]
    public void TrySetNullableParameter_ShouldCallSetNullParameter_WhenValueIsNull()
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = Substitute.For<ISqlColumnTypeDefinition>();

        var result = sut.TrySetNullableParameter( parameter, null );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.VerifyCalls().DidNotReceive( o => o.TrySetParameter( Arg.Any<IDbDataParameter>(), Arg.Any<object>() ) );
            sut.VerifyCalls().Received( o => o.SetNullParameter( parameter ), 1 );
        }
    }

    [Fact]
    public void SetNullableParameter_WithRefType_ShouldCallSetParameter_WhenValueIsNotNull()
    {
        var value = Fixture.Create<string>();
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = Substitute.For<ISqlColumnTypeDefinition<string>>();

        sut.SetNullableParameter( parameter, value );

        using ( new AssertionScope() )
        {
            sut.VerifyCalls().Received( o => o.SetParameter( parameter, value ), 1 );
            sut.VerifyCalls().DidNotReceive( o => o.SetNullParameter( Arg.Any<IDbDataParameter>() ) );
        }
    }

    [Fact]
    public void SetNullableParameter_WithRefType_ShouldCallSetNullParameter_WhenValueIsNull()
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = Substitute.For<ISqlColumnTypeDefinition<string>>();

        sut.SetNullableParameter( parameter, null );

        using ( new AssertionScope() )
        {
            sut.VerifyCalls().DidNotReceive( o => o.SetParameter( Arg.Any<IDbDataParameter>(), Arg.Any<string>() ) );
            sut.VerifyCalls().Received( o => o.SetNullParameter( parameter ), 1 );
        }
    }

    [Fact]
    public void SetNullableParameter_WithValueType_ShouldCallSetParameter_WhenValueIsNotNull()
    {
        var value = Fixture.Create<int>();
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = Substitute.For<ISqlColumnTypeDefinition<int>>();

        sut.SetNullableParameter( parameter, value );

        using ( new AssertionScope() )
        {
            sut.VerifyCalls().Received( o => o.SetParameter( parameter, value ), 1 );
            sut.VerifyCalls().DidNotReceive( o => o.SetNullParameter( Arg.Any<IDbDataParameter>() ) );
        }
    }

    [Fact]
    public void SetNullableParameter_WithValueType_ShouldCallSetNullParameter_WhenValueIsNull()
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = Substitute.For<ISqlColumnTypeDefinition<int>>();

        sut.SetNullableParameter( parameter, null );

        using ( new AssertionScope() )
        {
            sut.VerifyCalls().DidNotReceive( o => o.SetParameter( Arg.Any<IDbDataParameter>(), Arg.Any<int>() ) );
            sut.VerifyCalls().Received( o => o.SetNullParameter( parameter ), 1 );
        }
    }
}
