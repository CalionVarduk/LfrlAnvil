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
            sut.GetPendingStatements().ToArray().Should().BeEmpty();

            ((ISqlDatabaseBuilder)sut).DataTypes.Should().BeSameAs( sut.DataTypes );
            ((ISqlDatabaseBuilder)sut).TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            ((ISqlDatabaseBuilder)sut).Schemas.Should().BeSameAs( sut.Schemas );
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
        var sut = new SqliteDatabaseBuilder();

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
