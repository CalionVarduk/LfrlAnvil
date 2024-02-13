using System.Linq;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = SqlDatabaseBuilderMock.Create();

        using ( new AssertionScope() )
        {
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Should().BeSequentiallyEqualTo( sut.Schemas.Default );

            sut.Schemas.Default.Database.Should().BeSameAs( sut );
            sut.Schemas.Default.Name.Should().Be( "common" );
            sut.Schemas.Default.Objects.Should().BeEmpty();
            sut.Schemas.Default.Objects.Schema.Should().BeSameAs( sut.Schemas.Default );

            sut.Dialect.Should().BeSameAs( SqlDialectMock.Instance );
            sut.ServerVersion.Should().Be( "0.0.0" );

            sut.Changes.Database.Should().BeSameAs( sut );
            sut.Changes.Mode.Should().Be( SqlDatabaseCreateMode.DryRun );
            sut.Changes.IsAttached.Should().BeTrue();
            sut.Changes.ActiveObject.Should().BeSameAs( sut.Schemas.Default );
            sut.Changes.ActiveObjectExistenceState.Should().Be( SqlObjectExistenceState.Created );
            sut.Changes.IsActive.Should().BeTrue();
            sut.Changes.GetPendingActions().ToArray().Select( a => a.Sql ).Should().BeSequentiallyEqualTo( "CREATE [Schema] common;" );

            ((ISqlDatabaseBuilder)sut).DataTypes.Should().BeSameAs( sut.DataTypes );
            ((ISqlDatabaseBuilder)sut).TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            ((ISqlDatabaseBuilder)sut).NodeInterpreters.Should().BeSameAs( sut.NodeInterpreters );
            ((ISqlDatabaseBuilder)sut).QueryReaders.Should().BeSameAs( sut.QueryReaders );
            ((ISqlDatabaseBuilder)sut).ParameterBinders.Should().BeSameAs( sut.ParameterBinders );
            ((ISqlDatabaseBuilder)sut).Schemas.Should().BeSameAs( sut.Schemas );
            ((ISqlDatabaseBuilder)sut).Changes.Should().BeSameAs( sut.Changes );
            ((ISqlSchemaBuilderCollection)sut.Schemas).Default.Should().BeSameAs( sut.Schemas.Default );
            ((ISqlSchemaBuilderCollection)sut.Schemas).Database.Should().BeSameAs( sut.Schemas.Database );
            ((ISqlDatabaseChangeTracker)sut.Changes).Database.Should().BeSameAs( sut.Changes.Database );
            ((ISqlDatabaseChangeTracker)sut.Changes).ActiveObject.Should().BeSameAs( sut.Changes.ActiveObject );
        }
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldNotThrow()
    {
        ISqlDatabaseBuilder sut = SqlDatabaseBuilderMock.Create();
        var result = sut.AddConnectionChangeCallback( _ => { } );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenObjectIsNotReferenced()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj );
        result.Should().BeEmpty();
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenAllReferencesAreFilteredOut()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var obj = schema.Objects.CreateTable( "T" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj, _ => false );

        result.Should().BeEmpty();
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenAllObjectsAreIncluded()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C1" );
        var obj = table.Columns.Create( "C2" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( column ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj );

        result.Should().BeSequentiallyEqualTo( schema, table, column );
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenSomeObjectsAreFilteredOut()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C1" );
        var obj = table.Columns.Create( "C2" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( column ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj, o => o.Source.Object.Type != SqlObjectType.Schema );

        result.Should().BeSequentiallyEqualTo( table, column );
    }
}
