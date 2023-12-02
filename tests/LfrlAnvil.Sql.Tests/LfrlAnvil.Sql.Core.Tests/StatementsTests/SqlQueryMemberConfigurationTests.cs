using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryMemberConfigurationTests : TestsBase
{
    [Fact]
    public void Ignore_ShouldCreateConfigurationWithoutSourceFieldName()
    {
        var sut = SqlQueryMemberConfiguration.Ignore( "foo" );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "foo" );
            sut.SourceFieldName.Should().BeNull();
            sut.CustomMapping.Should().BeNull();
            sut.CustomMappingDataReaderType.Should().BeNull();
            sut.CustomMappingMemberType.Should().BeNull();
            sut.IsIgnored.Should().BeTrue();
        }
    }

    [Fact]
    public void From_WithSourceFieldName_ShouldCreateConfigurationWithDifferentMemberAndSourceFieldNames()
    {
        var sut = SqlQueryMemberConfiguration.From( "foo", "bar" );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "foo" );
            sut.SourceFieldName.Should().Be( "bar" );
            sut.CustomMapping.Should().BeNull();
            sut.CustomMappingDataReaderType.Should().BeNull();
            sut.CustomMappingMemberType.Should().BeNull();
            sut.IsIgnored.Should().BeFalse();
        }
    }

    [Fact]
    public void From_WithMapping_ShouldCreateConfigurationWithCustomMapping()
    {
        var mapping = Lambda.ExpressionOf( (ISqlDataRecordFacade<IDataRecord> facade) => facade.Get<int>( "lorem" ) );
        var sut = SqlQueryMemberConfiguration.From( "foo", mapping );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "foo" );
            sut.SourceFieldName.Should().BeNull();
            sut.CustomMapping.Should().BeSameAs( mapping );
            sut.CustomMappingDataReaderType.Should().Be( typeof( IDataRecord ) );
            sut.CustomMappingMemberType.Should().Be( typeof( int ) );
            sut.IsIgnored.Should().BeFalse();
        }
    }

    [Fact]
    public void From_WithMapping_ShouldCreateConfigurationWithCustomMappingWithConstexprNameParameter()
    {
        var nameSource = static (string n) => "lorem" + n;
        var mapping = Lambda.ExpressionOf(
            (ISqlDataRecordFacade<IDataReader> facade) => facade.GetNullable( nameSource( "ipsum" ), 5 ) + int.Parse( "5" ) );

        var expectedMapping = Lambda.ExpressionOf(
            (ISqlDataRecordFacade<IDataReader> facade) => facade.GetNullable( "loremipsum", 5 ) + int.Parse( "5" ) );

        var sut = SqlQueryMemberConfiguration.From( "foo", mapping );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "foo" );
            sut.SourceFieldName.Should().BeNull();
            sut.CustomMapping.Should().NotBeSameAs( mapping );
            sut.CustomMapping.Should().BeEquivalentTo( expectedMapping );
            sut.CustomMappingDataReaderType.Should().Be( typeof( IDataReader ) );
            sut.CustomMappingMemberType.Should().Be( typeof( int ) );
            sut.IsIgnored.Should().BeFalse();
        }
    }

    [Fact]
    public void From_WithMapping_ShouldThrowSqlCompilerConfigurationException_WhenMappingContainsUnresolvableNameParameters()
    {
        var mapping = Lambda.ExpressionOf(
            (ISqlDataRecordFacade<IDataRecord> facade) =>
                facade.Get<int>( facade.Record.GetString( 0 ) ) + facade.Get<int>( facade.Record.GetString( 1 ) ) );

        var action = Lambda.Of( () => SqlQueryMemberConfiguration.From( "foo", mapping ) );

        action.Should().ThrowExactly<SqlCompilerConfigurationException>().AndMatch( e => e.Errors.Count == 2 );
    }
}
