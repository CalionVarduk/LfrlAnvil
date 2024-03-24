using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests;

public class MySqlForeignKeyTests : TestsBase
{
    [Theory]
    [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Restrict )]
    [InlineData( ReferenceBehavior.Values.SetNull, ReferenceBehavior.Values.NoAction )]
    [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.NoAction, ReferenceBehavior.Values.SetNull )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WhenForeignKeyIsSelfReference(
        ReferenceBehavior.Values onDelete,
        ReferenceBehavior.Values onUpdate)
    {
        var onDeleteBehavior = ReferenceBehavior.GetBehavior( onDelete );
        var onUpdateBehavior = ReferenceBehavior.GetBehavior( onUpdate );

        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).MarkAsNullable().Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 )
            .SetName( "FK_TEST" )
            .SetOnDeleteBehavior( onDeleteBehavior )
            .SetOnUpdateBehavior( onUpdateBehavior );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        var sut = schema.Objects.GetForeignKey( "FK_TEST" );
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
    [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Restrict )]
    [InlineData( ReferenceBehavior.Values.SetNull, ReferenceBehavior.Values.NoAction )]
    [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.NoAction, ReferenceBehavior.Values.SetNull )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WhenForeignKeyReferencesOtherTableFromTheSameSchema(
        ReferenceBehavior.Values onDelete,
        ReferenceBehavior.Values onUpdate)
    {
        var onDeleteBehavior = ReferenceBehavior.GetBehavior( onDelete );
        var onUpdateBehavior = ReferenceBehavior.GetBehavior( onUpdate );

        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder1 = schemaBuilder.Objects.CreateTable( "T1" );
        tableBuilder1.Constraints.SetPrimaryKey( tableBuilder1.Columns.Create( "C1" ).Asc() );
        var indexBuilder1 = tableBuilder1.Constraints.CreateIndex( tableBuilder1.Columns.Create( "C2" ).MarkAsNullable().Asc() );
        var tableBuilder2 = schemaBuilder.Objects.CreateTable( "T2" );
        var indexBuilder2 = tableBuilder2.Constraints.SetPrimaryKey( tableBuilder2.Columns.Create( "C3" ).Asc() ).Index;
        tableBuilder1.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 )
            .SetName( "FK_TEST" )
            .SetOnDeleteBehavior( onDeleteBehavior )
            .SetOnUpdateBehavior( onUpdateBehavior );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        var sut = schema.Objects.GetForeignKey( "FK_TEST" );
        var index = schema.Objects.GetIndex( "IX_T1_C2A" );
        var refIndex = schema.Objects.GetIndex( "UIX_T2_C3A" );

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
    [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Restrict )]
    [InlineData( ReferenceBehavior.Values.SetNull, ReferenceBehavior.Values.NoAction )]
    [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.NoAction, ReferenceBehavior.Values.SetNull )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WhenForeignKeyReferencesOtherTableFromAnotherSchema(
        ReferenceBehavior.Values onDelete,
        ReferenceBehavior.Values onUpdate)
    {
        var onDeleteBehavior = ReferenceBehavior.GetBehavior( onDelete );
        var onUpdateBehavior = ReferenceBehavior.GetBehavior( onUpdate );

        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        var schemaBuilder1 = dbBuilder.Schemas.Create( "foo" );
        var tableBuilder1 = schemaBuilder1.Objects.CreateTable( "T1" );
        tableBuilder1.Constraints.SetPrimaryKey( tableBuilder1.Columns.Create( "C1" ).Asc() );
        var indexBuilder1 = tableBuilder1.Constraints.CreateIndex( tableBuilder1.Columns.Create( "C2" ).MarkAsNullable().Asc() );
        var schemaBuilder2 = dbBuilder.Schemas.Create( "bar" );
        var tableBuilder2 = schemaBuilder2.Objects.CreateTable( "T2" );
        var indexBuilder2 = tableBuilder2.Constraints.SetPrimaryKey( tableBuilder2.Columns.Create( "C3" ).Asc() ).Index;
        tableBuilder1.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 )
            .SetName( "FK_TEST" )
            .SetOnDeleteBehavior( onDeleteBehavior )
            .SetOnUpdateBehavior( onUpdateBehavior );

        var db = MySqlDatabaseMock.Create( dbBuilder );
        var schema1 = db.Schemas.Get( "foo" );
        var schema2 = db.Schemas.Get( "bar" );

        var sut = schema1.Objects.GetForeignKey( "FK_TEST" );
        var index = schema1.Objects.GetIndex( "IX_T1_C2A" );
        var refIndex = schema2.Objects.GetIndex( "UIX_T2_C3A" );

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
