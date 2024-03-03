using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlForeignKeyTests : TestsBase
{
    [Theory]
    [InlineData( "CASCADE", "RESTRICT" )]
    [InlineData( "RESTRICT", "CASCADE" )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WhenForeignKeyIsSelfReference(string onDelete, string onUpdate)
    {
        var onDeleteBehavior = onDelete == "CASCADE" ? ReferenceBehavior.Cascade : ReferenceBehavior.Restrict;
        var onUpdateBehavior = onUpdate == "CASCADE" ? ReferenceBehavior.Cascade : ReferenceBehavior.Restrict;

        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 )
            .SetName( "FK_TEST" )
            .SetOnDeleteBehavior( onDeleteBehavior )
            .SetOnUpdateBehavior( onUpdateBehavior );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        ISqlForeignKey sut = schema.Objects.GetForeignKey( "FK_TEST" );
        var index = schema.Objects.GetIndex( "IX_T_C1A" );
        var refIndex = schema.Objects.GetIndex( "UIX_T_C2A" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.OriginIndex.Should().BeSameAs( index );
            sut.ReferencedIndex.Should().BeSameAs( refIndex );
            sut.OnDeleteBehavior.Should().BeSameAs( onDeleteBehavior );
            sut.OnUpdateBehavior.Should().BeSameAs( onUpdateBehavior );
            sut.Type.Should().Be( SqlObjectType.ForeignKey );
            sut.Name.Should().Be( "FK_TEST" );
            sut.ToString().Should().Be( "[ForeignKey] foo.FK_TEST" );
            sut.IsSelfReference().Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( "CASCADE", "RESTRICT" )]
    [InlineData( "RESTRICT", "CASCADE" )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WhenForeignKeyReferencesOtherTableFromTheSameSchema(
        string onDelete,
        string onUpdate)
    {
        var onDeleteBehavior = onDelete == "CASCADE" ? ReferenceBehavior.Cascade : ReferenceBehavior.Restrict;
        var onUpdateBehavior = onUpdate == "CASCADE" ? ReferenceBehavior.Cascade : ReferenceBehavior.Restrict;

        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder1 = schemaBuilder.Objects.CreateTable( "T1" );
        var indexBuilder1 = tableBuilder1.Constraints.SetPrimaryKey( tableBuilder1.Columns.Create( "C1" ).Asc() ).Index;
        var tableBuilder2 = schemaBuilder.Objects.CreateTable( "T2" );
        var indexBuilder2 = tableBuilder2.Constraints.SetPrimaryKey( tableBuilder2.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder1.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 )
            .SetName( "FK_TEST" )
            .SetOnDeleteBehavior( onDeleteBehavior )
            .SetOnUpdateBehavior( onUpdateBehavior );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        ISqlForeignKey sut = schema.Objects.GetForeignKey( "FK_TEST" );
        var index = schema.Objects.GetIndex( "UIX_T1_C1A" );
        var refIndex = schema.Objects.GetIndex( "UIX_T2_C2A" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.OriginIndex.Should().BeSameAs( index );
            sut.ReferencedIndex.Should().BeSameAs( refIndex );
            sut.OnDeleteBehavior.Should().BeSameAs( onDeleteBehavior );
            sut.OnUpdateBehavior.Should().BeSameAs( onUpdateBehavior );
            sut.Type.Should().Be( SqlObjectType.ForeignKey );
            sut.Name.Should().Be( "FK_TEST" );
            sut.ToString().Should().Be( "[ForeignKey] foo.FK_TEST" );
            sut.IsSelfReference().Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( "CASCADE", "RESTRICT" )]
    [InlineData( "RESTRICT", "CASCADE" )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WhenForeignKeyReferencesOtherTableFromAnotherSchema(
        string onDelete,
        string onUpdate)
    {
        var onDeleteBehavior = onDelete == "CASCADE" ? ReferenceBehavior.Cascade : ReferenceBehavior.Restrict;
        var onUpdateBehavior = onUpdate == "CASCADE" ? ReferenceBehavior.Cascade : ReferenceBehavior.Restrict;

        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var schemaBuilder1 = dbBuilder.Schemas.Create( "foo" );
        var tableBuilder1 = schemaBuilder1.Objects.CreateTable( "T1" );
        var indexBuilder1 = tableBuilder1.Constraints.SetPrimaryKey( tableBuilder1.Columns.Create( "C1" ).Asc() ).Index;
        var schemaBuilder2 = dbBuilder.Schemas.Create( "bar" );
        var tableBuilder2 = schemaBuilder2.Objects.CreateTable( "T2" );
        var indexBuilder2 = tableBuilder2.Constraints.SetPrimaryKey( tableBuilder2.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder1.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 )
            .SetName( "FK_TEST" )
            .SetOnDeleteBehavior( onDeleteBehavior )
            .SetOnUpdateBehavior( onUpdateBehavior );

        var db = SqlDatabaseMock.Create( dbBuilder );
        var schema1 = db.Schemas.Get( "foo" );
        var schema2 = db.Schemas.Get( "bar" );

        ISqlForeignKey sut = schema1.Objects.GetForeignKey( "FK_TEST" );
        var index = schema1.Objects.GetIndex( "UIX_T1_C1A" );
        var refIndex = schema2.Objects.GetIndex( "UIX_T2_C2A" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.OriginIndex.Should().BeSameAs( index );
            sut.ReferencedIndex.Should().BeSameAs( refIndex );
            sut.OnDeleteBehavior.Should().BeSameAs( onDeleteBehavior );
            sut.OnUpdateBehavior.Should().BeSameAs( onUpdateBehavior );
            sut.Type.Should().Be( SqlObjectType.ForeignKey );
            sut.Name.Should().Be( "FK_TEST" );
            sut.ToString().Should().Be( "[ForeignKey] foo.FK_TEST" );
            sut.IsSelfReference().Should().BeFalse();
        }
    }
}
