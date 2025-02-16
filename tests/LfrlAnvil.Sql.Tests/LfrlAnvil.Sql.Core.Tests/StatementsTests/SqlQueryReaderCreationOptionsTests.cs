using System.Data;
using System.Data.Common;
using System.Reflection;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryReaderCreationOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldRepresentDefaultOptions()
    {
        var sut = SqlQueryReaderCreationOptions.Default;

        Assertion.All(
                sut.ResultSetFieldsPersistenceMode.TestEquals( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore ),
                sut.AlwaysTestForNull.TestFalse(),
                sut.RowTypeConstructorPredicate.TestNull(),
                sut.RowTypeMemberPredicate.TestNull(),
                sut.MemberConfigurations.TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore )]
    [InlineData( SqlQueryReaderResultSetFieldsPersistenceMode.Persist )]
    [InlineData( SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes )]
    public void SetResultSetFieldsPersistenceMode_ShouldCreateOptionsWithNewMode(SqlQueryReaderResultSetFieldsPersistenceMode mode)
    {
        var sut = SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode( mode );

        Assertion.All(
                sut.ResultSetFieldsPersistenceMode.TestEquals( mode ),
                sut.AlwaysTestForNull.TestFalse(),
                sut.RowTypeConstructorPredicate.TestNull(),
                sut.RowTypeMemberPredicate.TestNull(),
                sut.MemberConfigurations.TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableAlwaysTestingForNull_ShouldCreateOptionsWithNewOption(bool enabled)
    {
        var sut = SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull( enabled );

        Assertion.All(
                sut.ResultSetFieldsPersistenceMode.TestEquals( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore ),
                sut.AlwaysTestForNull.TestEquals( enabled ),
                sut.RowTypeConstructorPredicate.TestNull(),
                sut.RowTypeMemberPredicate.TestNull(),
                sut.MemberConfigurations.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetRowTypeConstructorPredicate_ShouldCreateOptionsWithNewPredicate()
    {
        var predicate = Lambda.Of( (ConstructorInfo ctor) => ctor.IsPublic );
        var sut = SqlQueryReaderCreationOptions.Default.SetRowTypeConstructorPredicate( predicate );

        Assertion.All(
                sut.ResultSetFieldsPersistenceMode.TestEquals( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore ),
                sut.AlwaysTestForNull.TestFalse(),
                sut.RowTypeConstructorPredicate.TestRefEquals( predicate ),
                sut.RowTypeMemberPredicate.TestNull(),
                sut.MemberConfigurations.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetRowTypeMemberPredicate_ShouldCreateOptionsWithNewPredicate()
    {
        var predicate = Lambda.Of( (MemberInfo member) => member.MemberType == MemberTypes.Property );
        var sut = SqlQueryReaderCreationOptions.Default.SetRowTypeMemberPredicate( predicate );

        Assertion.All(
                sut.ResultSetFieldsPersistenceMode.TestEquals( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore ),
                sut.AlwaysTestForNull.TestFalse(),
                sut.RowTypeConstructorPredicate.TestNull(),
                sut.RowTypeMemberPredicate.TestRefEquals( predicate ),
                sut.MemberConfigurations.TestEmpty() )
            .Go();
    }

    [Fact]
    public void With_ShouldCreateOptionsWithAddedMemberConfiguration_WhenCurrentMemberConfigurationsAreEmpty()
    {
        var cfg = SqlQueryMemberConfiguration.Ignore( "foo" );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg );

        Assertion.All(
                SqlQueryReaderCreationOptions.Default.MemberConfigurations.ToArray().TestEmpty(),
                sut.ResultSetFieldsPersistenceMode.TestEquals( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore ),
                sut.AlwaysTestForNull.TestFalse(),
                sut.RowTypeConstructorPredicate.TestNull(),
                sut.RowTypeMemberPredicate.TestNull(),
                sut.MemberConfigurations.TestSequence( [ cfg ] ) )
            .Go();
    }

    [Fact]
    public void With_ShouldCreateOptionsWithAddedMemberConfiguration_WhenCurrentMemberConfigurationsAreNotEmpty()
    {
        var cfg1 = SqlQueryMemberConfiguration.Ignore( "foo" );
        var cfg2 = SqlQueryMemberConfiguration.Ignore( "bar" );
        var prev = SqlQueryReaderCreationOptions.Default.With( cfg1 );
        var sut = prev.With( cfg2 );

        Assertion.All(
                prev.MemberConfigurations.TestSequence( [ cfg1 ] ),
                sut.ResultSetFieldsPersistenceMode.TestEquals( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore ),
                sut.AlwaysTestForNull.TestFalse(),
                sut.RowTypeConstructorPredicate.TestNull(),
                sut.RowTypeMemberPredicate.TestNull(),
                sut.MemberConfigurations.TestSequence( [ cfg1, cfg2 ] ) )
            .Go();
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnNull_WhenMemberConfigurationsAreEmpty()
    {
        var sut = SqlQueryReaderCreationOptions.Default;
        var result = sut.CreateMemberConfigurationByNameLookup( typeof( IDataReader ) );
        result.TestNull().Go();
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnCorrectDictionary_WhenEachConfigurationHasDistinctMemberName()
    {
        var cfg1 = SqlQueryMemberConfiguration.Ignore( "foo" );
        var cfg2 = SqlQueryMemberConfiguration.From( "bar", "lorem" );
        var cfg3 = SqlQueryMemberConfiguration.From( "qux", f => f.Get<int>( "baz" ) );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg1 ).With( cfg2 ).With( cfg3 );

        var result = sut.CreateMemberConfigurationByNameLookup( typeof( IDataReader ) );

        result.TestNotNull(
                r => Assertion.All( r.Keys.TestSetEqual( [ "foo", "bar", "qux" ] ), r.Values.TestSetEqual( [ cfg1, cfg2, cfg3 ] ) ) )
            .Go();
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnCorrectDictionary_WhenMemberNameIsDuplicated()
    {
        var cfg1 = SqlQueryMemberConfiguration.Ignore( "foo" );
        var cfg2 = SqlQueryMemberConfiguration.From( "bar", "lorem" );
        var cfg3 = SqlQueryMemberConfiguration.From( "bar", f => f.Get<int>( "baz" ) );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg1 ).With( cfg2 ).With( cfg3 );

        var result = sut.CreateMemberConfigurationByNameLookup( typeof( IDataReader ) );

        result.TestNotNull( r => Assertion.All( r.Keys.TestSetEqual( [ "foo", "bar" ] ), r.Values.TestSetEqual( [ cfg1, cfg3 ] ) ) )
            .Go();
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnCorrectDictionary_WhenCustomMappingIsIncompatibleWithDataReaderType()
    {
        var cfg = SqlQueryMemberConfiguration.From( "bar", (ISqlDataRecordFacade<DbDataReader> f) => f.Get<int>( "baz" ) );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg );

        var result = sut.CreateMemberConfigurationByNameLookup( typeof( IDataReader ) );

        result.TestNull().Go();
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnCorrectDictionary_WhenCustomMappingIsCompatibleWithDataReaderType()
    {
        var cfg = SqlQueryMemberConfiguration.From( "foo", f => f.Get<int>( "bar" ) );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg );

        var result = sut.CreateMemberConfigurationByNameLookup( typeof( DbDataReader ) );

        result.TestNotNull( r => Assertion.All( r.Keys.TestSetEqual( [ "foo" ] ), r.Values.TestSetEqual( [ cfg ] ) ) )
            .Go();
    }
}
