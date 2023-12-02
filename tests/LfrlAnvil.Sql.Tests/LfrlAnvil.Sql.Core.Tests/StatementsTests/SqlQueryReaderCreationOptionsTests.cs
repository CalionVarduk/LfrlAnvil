using System.Data;
using System.Data.Common;
using System.Reflection;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryReaderCreationOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldRepresentDefaultOptions()
    {
        var sut = SqlQueryReaderCreationOptions.Default;

        using ( new AssertionScope() )
        {
            sut.ResultSetFieldsPersistenceMode.Should().Be( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore );
            sut.AlwaysTestForNull.Should().BeFalse();
            sut.RowTypeConstructorPredicate.Should().BeNull();
            sut.RowTypeMemberPredicate.Should().BeNull();
            sut.MemberConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore )]
    [InlineData( SqlQueryReaderResultSetFieldsPersistenceMode.Persist )]
    [InlineData( SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes )]
    public void SetResultSetFieldsPersistenceMode_ShouldCreateOptionsWithNewMode(SqlQueryReaderResultSetFieldsPersistenceMode mode)
    {
        var sut = SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode( mode );

        using ( new AssertionScope() )
        {
            sut.ResultSetFieldsPersistenceMode.Should().Be( mode );
            sut.AlwaysTestForNull.Should().BeFalse();
            sut.RowTypeConstructorPredicate.Should().BeNull();
            sut.RowTypeMemberPredicate.Should().BeNull();
            sut.MemberConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableAlwaysTestingForNull_ShouldCreateOptionsWithNewOption(bool enabled)
    {
        var sut = SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull( enabled );

        using ( new AssertionScope() )
        {
            sut.ResultSetFieldsPersistenceMode.Should().Be( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore );
            sut.AlwaysTestForNull.Should().Be( enabled );
            sut.RowTypeConstructorPredicate.Should().BeNull();
            sut.RowTypeMemberPredicate.Should().BeNull();
            sut.MemberConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void SetRowTypeConstructorPredicate_ShouldCreateOptionsWithNewPredicate()
    {
        var predicate = Lambda.Of( (ConstructorInfo ctor) => ctor.IsPublic );
        var sut = SqlQueryReaderCreationOptions.Default.SetRowTypeConstructorPredicate( predicate );

        using ( new AssertionScope() )
        {
            sut.ResultSetFieldsPersistenceMode.Should().Be( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore );
            sut.AlwaysTestForNull.Should().BeFalse();
            sut.RowTypeConstructorPredicate.Should().BeSameAs( predicate );
            sut.RowTypeMemberPredicate.Should().BeNull();
            sut.MemberConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void SetRowTypeMemberPredicate_ShouldCreateOptionsWithNewPredicate()
    {
        var predicate = Lambda.Of( (MemberInfo member) => member.MemberType == MemberTypes.Property );
        var sut = SqlQueryReaderCreationOptions.Default.SetRowTypeMemberPredicate( predicate );

        using ( new AssertionScope() )
        {
            sut.ResultSetFieldsPersistenceMode.Should().Be( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore );
            sut.AlwaysTestForNull.Should().BeFalse();
            sut.RowTypeConstructorPredicate.Should().BeNull();
            sut.RowTypeMemberPredicate.Should().BeSameAs( predicate );
            sut.MemberConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void With_ShouldCreateOptionsWithAddedMemberConfiguration_WhenCurrentMemberConfigurationsAreEmpty()
    {
        var cfg = SqlQueryMemberConfiguration.Ignore( "foo" );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg );

        using ( new AssertionScope() )
        {
            SqlQueryReaderCreationOptions.Default.MemberConfigurations.ToArray().Should().BeEmpty();
            sut.ResultSetFieldsPersistenceMode.Should().Be( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore );
            sut.AlwaysTestForNull.Should().BeFalse();
            sut.RowTypeConstructorPredicate.Should().BeNull();
            sut.RowTypeMemberPredicate.Should().BeNull();
            sut.MemberConfigurations.ToArray().Should().BeSequentiallyEqualTo( cfg );
        }
    }

    [Fact]
    public void With_ShouldCreateOptionsWithAddedMemberConfiguration_WhenCurrentMemberConfigurationsAreNotEmpty()
    {
        var cfg1 = SqlQueryMemberConfiguration.Ignore( "foo" );
        var cfg2 = SqlQueryMemberConfiguration.Ignore( "bar" );
        var prev = SqlQueryReaderCreationOptions.Default.With( cfg1 );
        var sut = prev.With( cfg2 );

        using ( new AssertionScope() )
        {
            prev.MemberConfigurations.ToArray().Should().BeSequentiallyEqualTo( cfg1 );
            sut.ResultSetFieldsPersistenceMode.Should().Be( SqlQueryReaderResultSetFieldsPersistenceMode.Ignore );
            sut.AlwaysTestForNull.Should().BeFalse();
            sut.RowTypeConstructorPredicate.Should().BeNull();
            sut.RowTypeMemberPredicate.Should().BeNull();
            sut.MemberConfigurations.ToArray().Should().BeSequentiallyEqualTo( cfg1, cfg2 );
        }
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnNull_WhenMemberConfigurationsAreEmpty()
    {
        var sut = SqlQueryReaderCreationOptions.Default;
        var result = sut.CreateMemberConfigurationByNameLookup( typeof( IDataReader ) );
        result.Should().BeNull();
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnCorrectDictionary_WhenEachConfigurationHasDistinctMemberName()
    {
        var cfg1 = SqlQueryMemberConfiguration.Ignore( "foo" );
        var cfg2 = SqlQueryMemberConfiguration.From( "bar", "lorem" );
        var cfg3 = SqlQueryMemberConfiguration.From( "qux", f => f.Get<int>( "baz" ) );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg1 ).With( cfg2 ).With( cfg3 );

        var result = sut.CreateMemberConfigurationByNameLookup( typeof( IDataReader ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            result.Should().HaveCount( 3 );
            (result?.Keys).Should().BeEquivalentTo( "foo", "bar", "qux" );
            (result?.Values).Should().BeEquivalentTo( cfg1, cfg2, cfg3 );
        }
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnCorrectDictionary_WhenMemberNameIsDuplicated()
    {
        var cfg1 = SqlQueryMemberConfiguration.Ignore( "foo" );
        var cfg2 = SqlQueryMemberConfiguration.From( "bar", "lorem" );
        var cfg3 = SqlQueryMemberConfiguration.From( "bar", f => f.Get<int>( "baz" ) );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg1 ).With( cfg2 ).With( cfg3 );

        var result = sut.CreateMemberConfigurationByNameLookup( typeof( IDataReader ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            result.Should().HaveCount( 2 );
            (result?.Keys).Should().BeEquivalentTo( "foo", "bar" );
            (result?.Values).Should().BeEquivalentTo( cfg1, cfg3 );
        }
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnCorrectDictionary_WhenCustomMappingIsIncompatibleWithDataReaderType()
    {
        var cfg = SqlQueryMemberConfiguration.From( "bar", (ISqlDataRecordFacade<DbDataReader> f) => f.Get<int>( "baz" ) );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg );

        var result = sut.CreateMemberConfigurationByNameLookup( typeof( IDataReader ) );

        result.Should().BeNull();
    }

    [Fact]
    public void CreateMemberConfigurationByNameLookup_ShouldReturnCorrectDictionary_WhenCustomMappingIsCompatibleWithDataReaderType()
    {
        var cfg = SqlQueryMemberConfiguration.From( "foo", f => f.Get<int>( "bar" ) );
        var sut = SqlQueryReaderCreationOptions.Default.With( cfg );

        var result = sut.CreateMemberConfigurationByNameLookup( typeof( DbDataReader ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            result.Should().HaveCount( 1 );
            (result?.Keys).Should().BeEquivalentTo( "foo" );
            (result?.Values).Should().BeEquivalentTo( cfg );
        }
    }
}
