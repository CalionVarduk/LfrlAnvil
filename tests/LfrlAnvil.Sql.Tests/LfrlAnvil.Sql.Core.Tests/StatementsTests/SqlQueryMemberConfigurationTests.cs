using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryMemberConfigurationTests : TestsBase
{
    [Fact]
    public void Ignore_ShouldCreateConfigurationWithoutSourceFieldName()
    {
        var sut = SqlQueryMemberConfiguration.Ignore( "foo" );

        Assertion.All(
                sut.MemberName.TestEquals( "foo" ),
                sut.SourceFieldName.TestNull(),
                sut.CustomMapping.TestNull(),
                sut.CustomMappingDataReaderType.TestNull(),
                sut.CustomMappingMemberType.TestNull(),
                sut.IsIgnored.TestTrue() )
            .Go();
    }

    [Fact]
    public void From_WithSourceFieldName_ShouldCreateConfigurationWithDifferentMemberAndSourceFieldNames()
    {
        var sut = SqlQueryMemberConfiguration.From( "foo", "bar" );

        Assertion.All(
                sut.MemberName.TestEquals( "foo" ),
                sut.SourceFieldName.TestEquals( "bar" ),
                sut.CustomMapping.TestNull(),
                sut.CustomMappingDataReaderType.TestNull(),
                sut.CustomMappingMemberType.TestNull(),
                sut.IsIgnored.TestFalse() )
            .Go();
    }

    [Fact]
    public void From_WithMapping_ShouldCreateConfigurationWithCustomMapping()
    {
        var mapping = Lambda.ExpressionOf( (ISqlDataRecordFacade<IDataRecord> facade) => facade.Get<int>( "lorem" ) );
        var sut = SqlQueryMemberConfiguration.From( "foo", mapping );

        Assertion.All(
                sut.MemberName.TestEquals( "foo" ),
                sut.SourceFieldName.TestNull(),
                sut.CustomMapping.TestRefEquals( mapping ),
                sut.CustomMappingDataReaderType.TestEquals( typeof( IDataRecord ) ),
                sut.CustomMappingMemberType.TestEquals( typeof( int ) ),
                sut.IsIgnored.TestFalse() )
            .Go();
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

        Assertion.All(
                sut.MemberName.TestEquals( "foo" ),
                sut.SourceFieldName.TestNull(),
                sut.CustomMapping.TestNotRefEquals( mapping ),
                (sut.CustomMapping?.ToString()).TestEquals( expectedMapping.ToString() ),
                sut.CustomMappingDataReaderType.TestEquals( typeof( IDataReader ) ),
                sut.CustomMappingMemberType.TestEquals( typeof( int ) ),
                sut.IsIgnored.TestFalse() )
            .Go();
    }

    [Fact]
    public void From_WithMapping_ShouldThrowSqlCompilerConfigurationException_WhenMappingContainsUnresolvableNameParameters()
    {
        var mapping = Lambda.ExpressionOf(
            (ISqlDataRecordFacade<IDataRecord> facade) =>
                facade.Get<int>( facade.Record.GetString( 0 ) ) + facade.Get<int>( facade.Record.GetString( 1 ) ) );

        var action = Lambda.Of( () => SqlQueryMemberConfiguration.From( "foo", mapping ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerConfigurationException>( e => e.Errors.Count.TestEquals( 2 ) ) ).Go();
    }
}
