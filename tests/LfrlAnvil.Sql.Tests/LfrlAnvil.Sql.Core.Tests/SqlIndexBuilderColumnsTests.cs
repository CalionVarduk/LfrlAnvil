using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlIndexBuilderColumnsTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnCorrectResult()
    {
        var sut = SqlIndexBuilderColumns<SqlColumnBuilderMock>.Empty;
        sut.Expressions.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );

        var expressions = new[] { c1.Asc(), c2.Desc(), (c1.Node + c2.Node).Asc() };
        var sut = new SqlIndexBuilderColumns<SqlColumnBuilderMock>( expressions );

        sut.Expressions.TestSequence( expressions ).Go();
    }

    [Fact]
    public void TryGet_ShouldReturnColumnBuilder_WhenExpressionIsColumn()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );

        var expressions = new[] { c1.Asc(), c2.Desc(), (c1.Node + c2.Node).Asc() };
        var sut = new SqlIndexBuilderColumns<SqlColumnBuilderMock>( expressions );

        var result = sut.TryGet( 1 );

        result.TestRefEquals( c2 ).Go();
    }

    [Fact]
    public void TryGet_ShouldReturnNull_WhenExpressionIsNotColumn()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );

        var expressions = new[] { c1.Asc(), c2.Desc(), (c1.Node + c2.Node).Asc() };
        var sut = new SqlIndexBuilderColumns<SqlColumnBuilderMock>( expressions );

        var result = sut.TryGet( 2 );

        result.TestNull().Go();
    }

    [Fact]
    public void IsExpression_ShouldReturnFalse_WhenExpressionIsColumn()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );

        var expressions = new[] { c1.Asc(), c2.Desc(), (c1.Node + c2.Node).Asc() };
        var sut = new SqlIndexBuilderColumns<SqlColumnBuilderMock>( expressions );

        var result = sut.IsExpression( 1 );

        result.TestFalse().Go();
    }

    [Fact]
    public void IsExpression_ShouldReturnTrue_WhenExpressionIsNotColumn()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );

        var expressions = new[] { c1.Asc(), c2.Desc(), (c1.Node + c2.Node).Asc() };
        var sut = new SqlIndexBuilderColumns<SqlColumnBuilderMock>( expressions );

        var result = sut.IsExpression( 2 );

        result.TestTrue().Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );

        var expressions = new[] { c1.Asc(), c2.Desc(), (c1.Node + c2.Node).Asc() };
        var sut = new SqlIndexBuilderColumns<SqlColumnBuilderMock>( expressions );

        var result = new List<SqlColumnBuilderMock?>();
        foreach ( var column in sut ) result.Add( column );

        result.TestSequence( [ c1, c2, null ] ).Go();
    }
}
