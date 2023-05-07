using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sqlite.Builders;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.BuildersTests;

public partial class SqliteDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = new SqliteDatabaseBuilder();

        using ( new AssertionScope() )
        {
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Should().BeEquivalentTo( sut.Schemas.Default );
            ((ISqlSchemaBuilderCollection)sut.Schemas).Default.Should().BeSameAs( sut.Schemas.Default );
            ((ISqlSchemaBuilderCollection)sut.Schemas).Database.Should().BeSameAs( sut.Schemas.Database );

            sut.Schemas.Default.Database.Should().BeSameAs( sut );
            sut.Schemas.Default.Name.Should().BeEmpty();
            sut.Schemas.Default.FullName.Should().BeEmpty();
            ((ISqlSchemaBuilder)sut.Schemas.Default).Database.Should().BeSameAs( sut.Schemas.Default.Database );
            ((ISqlSchemaBuilder)sut.Schemas.Default).Objects.Should().BeSameAs( sut.Schemas.Default.Objects );

            sut.Schemas.Default.Objects.Count.Should().Be( 0 );
            sut.Schemas.Default.Objects.Should().BeEmpty();
            sut.Schemas.Default.Objects.Schema.Should().BeSameAs( sut.Schemas.Default );
            ((ISqlObjectBuilderCollection)sut.Schemas.Default.Objects).Schema.Should().BeSameAs( sut.Schemas.Default.Objects.Schema );

            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
            sut.IsAttached.Should().BeTrue();
            sut.GetPendingStatements().ToArray().Should().BeEmpty();

            ((ISqlDatabaseBuilder)sut).DataTypes.Should().BeSameAs( sut.DataTypes );
            ((ISqlDatabaseBuilder)sut).TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            ((ISqlDatabaseBuilder)sut).Schemas.Should().BeSameAs( sut.Schemas );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void Ctor_ShouldSetCorrectMode(SqlDatabaseCreateMode mode)
    {
        var sut = new SqliteDatabaseBuilder( mode );

        using ( new AssertionScope() )
        {
            sut.Mode.Should().Be( mode );
            sut.IsAttached.Should().BeTrue();
        }
    }

    [Fact]
    public void AddRawStatement_ShouldAddNewStatement_WhenThereAreNoPendingChanges()
    {
        var statement = Fixture.Create<string>();
        var sut = new SqliteDatabaseBuilder();

        sut.AddRawStatement( statement );
        var result = sut.GetPendingStatements().ToArray();

        result.Should().BeSequentiallyEqualTo( statement );
    }

    [Fact]
    public void AddRawStatement_ShouldAddNewStatement_WhenThereAreSomePendingChanges()
    {
        var statement = Fixture.Create<string>();
        var sut = new SqliteDatabaseBuilder( SqlDatabaseCreateMode.Commit );

        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.SetPrimaryKey( column.Asc() );

        sut.AddRawStatement( statement );
        var result = sut.GetPendingStatements().ToArray();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 2 );
            result[^1].Should().Be( statement );
        }
    }

    [Fact]
    public void AddRawStatement_ShouldDoNothing_WhenBuilderIsDetached()
    {
        var statement = Fixture.Create<string>();
        var sut = new SqliteDatabaseBuilder().SetDetachedMode();

        sut.AddRawStatement( statement );

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void AddRawStatement_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
    {
        var statement = Fixture.Create<string>();
        var sut = new SqliteDatabaseBuilder( SqlDatabaseCreateMode.NoChanges );

        sut.AddRawStatement( statement );

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void ObjectChanges_ShouldDoNothing_WhenBuilderIsDetached()
    {
        var sut = new SqliteDatabaseBuilder().SetDetachedMode();

        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "D" ).Asc() ).MarkAsUnique();
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;
        var fk = table.ForeignKeys.Create( ix1, ix2 );
        fk.SetOnDeleteBehavior( ReferenceBehavior.Cascade ).SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var column = table.Columns.Create( "E" );
        column.SetName( "F" ).MarkAsNullable().SetType<int>().SetDefaultValue( 123 );
        table.SetName( "U" );
        column.Remove();

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void ObjectCreation_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
    {
        var sut = new SqliteDatabaseBuilder( SqlDatabaseCreateMode.NoChanges );

        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "D" ).Asc() ).MarkAsUnique();
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;
        var fk = table.ForeignKeys.Create( ix1, ix2 );
        fk.SetOnDeleteBehavior( ReferenceBehavior.Cascade ).SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var column = table.Columns.Create( "E" );
        column.SetName( "F" ).MarkAsNullable().SetType<int>().SetDefaultValue( 123 );
        table.SetName( "U" );
        column.Remove();

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetAttachedMode_ShouldUpdateIsAttached(bool enabled)
    {
        var sut = new SqliteDatabaseBuilder().SetAttachedMode( ! enabled );

        var result = ((ISqlDatabaseBuilder)sut).SetAttachedMode( enabled );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsAttached.Should().Be( enabled );
        }
    }

    [Fact]
    public void SetAttachedMode_ShouldDoNothing_WhenBuilderIsAlreadyAttached()
    {
        var sut = new SqliteDatabaseBuilder();

        var result = ((ISqlDatabaseBuilder)sut).SetAttachedMode();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsAttached.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetDetachedMode_ShouldUpdateIsAttached(bool enabled)
    {
        var sut = new SqliteDatabaseBuilder().SetDetachedMode( ! enabled );

        var result = ((ISqlDatabaseBuilder)sut).SetDetachedMode( enabled );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsAttached.Should().Be( ! enabled );
        }
    }

    [Fact]
    public void SetDetachedMode_ShouldDoNothing_WhenBuilderIsAlreadyDetached()
    {
        var sut = new SqliteDatabaseBuilder().SetDetachedMode();

        var result = ((ISqlDatabaseBuilder)sut).SetDetachedMode();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsAttached.Should().BeFalse();
        }
    }

    [Fact]
    public void DetachingBuilder_ShouldCompletePendingChanges()
    {
        var sut = new SqliteDatabaseBuilder();

        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.SetPrimaryKey( column.Asc() );

        sut.SetDetachedMode();
        var result = sut.GetPendingStatements().ToArray();

        result.Should().HaveCount( 1 );
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenDatabaseIsSqlite()
    {
        var action = Substitute.For<Action<SqliteDatabaseBuilder>>();
        var sut = new SqliteDatabaseBuilder();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenDatabaseIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteDatabaseBuilder>>();
        var sut = Substitute.For<ISqlDatabaseBuilder>();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
