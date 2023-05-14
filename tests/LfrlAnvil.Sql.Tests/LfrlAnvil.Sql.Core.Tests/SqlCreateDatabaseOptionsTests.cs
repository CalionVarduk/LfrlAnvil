namespace LfrlAnvil.Sql.Tests;

public class SqlCreateDatabaseOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = SqlCreateDatabaseOptions.Default;

        using ( new AssertionScope() )
        {
            sut.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            sut.VersionHistorySchemaName.Should().BeNull();
            sut.VersionHistoryTableName.Should().BeNull();
            sut.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void SetMode_ShouldUpdateModeCorrectly(SqlDatabaseCreateMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetMode( mode );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( mode );
            result.VersionHistorySchemaName.Should().BeNull();
            result.VersionHistoryTableName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
        }
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void SetVersionHistorySchemaName_ShouldUpdateNameCorrectly(string? name)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistorySchemaName( name );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistorySchemaName.Should().BeSameAs( name );
            result.VersionHistoryTableName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
        }
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void SetVersionHistoryTableName_ShouldUpdateNameCorrectly(string? name)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryTableName( name );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistorySchemaName.Should().BeNull();
            result.VersionHistoryTableName.Should().BeSameAs( name );
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryPersistenceMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryPersistenceMode.LastRecordOnly )]
    public void SetVersionHistoryPersistenceMode_ShouldUpdateModeCorrectly(SqlDatabaseVersionHistoryPersistenceMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryPersistenceMode( mode );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistorySchemaName.Should().BeNull();
            result.VersionHistoryTableName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( mode );
        }
    }
}
